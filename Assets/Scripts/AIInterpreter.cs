using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class AIInterpreter : MonoBehaviour
{
    [SerializeField]
    private bool _debugMode;

    [SerializeField]
    private AIController _aiController;

    [SerializeField]
    private Personality _personality;

    [SerializeField]
    private BasePrompt _basePrompt;

    private LLMAction _previousAction;

    private OpponentAction _previousOpponentAction;

    private Dictionary<string, string> _relationships;

    private string _currentLocation;
    private string _previousFailReason;

    public Personality Personality
    {
        get { return _personality; }
    }

    [System.Serializable]
    private class LLMAction
    {
        public string action;
        public string target;
        public string content;
    }

    private void Start()
    {
        _relationships = new Dictionary<string, string>();

        string[] personalities = AIRepository.Instance.GetAllPersonalities();

        foreach (string personality in personalities)
        {
            _relationships.Add(personality, "");
        }
    }

    public void SendRequest()
    {
        string prompt = BuildPrompt();
        DebugPrint("Sending request: " + prompt);
        LLMAPIHandler.Instance.SendChatRequest(prompt, InterpretResponseCallback, ErrorCallback);
    }

    public void SendRequest(string failReason)
    {
        string prompt = BuildPrompt(failReason);
        DebugPrint("Sending request: " + prompt);
        LLMAPIHandler.Instance.SendChatRequest(prompt, InterpretResponseCallback, ErrorCallback);
    }

    public void RetryRequest()
    {
        SendRequest(_previousFailReason);
    }

    private void ErrorCallback(string error = "")
    {
        Debug.LogError("Error: " + error);
        _previousFailReason = error;
        // Retry
        SendRequest(error);
    }

    public void SetOpponentAction(OpponentAction opponentAction)
    {
        _previousOpponentAction = opponentAction;
    }

    private void InterpretResponseCallback(string response)
    {
        DebugPrint("Interpreting response: " + response);

        try
        {
            // If response contains ```json at the start, remove it and also remove the closing ```
            if (response.StartsWith("```json"))
            {
                response = response[7..];
                response = response[..^3];
            }

            // Add missing quotes around JSON keys
            string[] jsonKeys = { "action", "target", "content" };
            foreach (string key in jsonKeys)
            {
                string pattern = $@"([{{,]\s*){key}(\s*:)";
                string replacement = $"$1\"{key}\"$2";
                response = Regex.Replace(response, pattern, replacement);
            }

            LLMAction action = JsonUtility.FromJson<LLMAction>(response);
            _previousAction = action;

            DebugPrint("Interpreted action: " + action.action);

            string param = action.target;
            string content = action.content;
            string result = "";
            result = action.action.ToLower() switch
            {
                "talk" => _aiController.Talk(param, content),
                "goto" => _aiController.GoTo(param),
                _ => "Invalid action: " + action.action, // Invalid action, request new action
            };
            if (!string.IsNullOrEmpty(result))
            {
                ErrorCallback(result);
            }
            else
            {
                _previousFailReason = "";
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error interpreting response: " + e.Message);
            RetryRequest();
        }
    }

    private string BuildPrompt(string failReason = "")
    {
        string prompt =
            _basePrompt.Intro
            + _personality.Name
            + ". "
            + _personality.Description
            + "\n"
            + _basePrompt.AfterDesc
            + "\n"
            + _basePrompt.CurrentRelationshipText
            + "\n"
            + BuildCurrentRelationshipText();

        if (_previousOpponentAction != null)
        {
            prompt +=
                _basePrompt.PreviousOpponentText
                + _previousOpponentAction.from
                + _basePrompt.PreviousActionOpponent
                + "\n"
                + _previousOpponentAction.actionDescription;
        }

        if (!string.IsNullOrEmpty(failReason))
        {
            prompt += _basePrompt.PreviousFailed + failReason + "\n";
        }

        string previousActionJSON = JsonUtility.ToJson(_previousAction);

        prompt +=
            _basePrompt.PreviousActionText
            + previousActionJSON
            + "\n"
            + _basePrompt.LocationText
            + _currentLocation
            + "\n"
            + _basePrompt.ActionText
            + "\n"
            + _basePrompt.RespondInstructions;

        return prompt;
    }

    private string BuildCurrentRelationshipText()
    {
        string relationshipText = "";
        foreach (KeyValuePair<string, string> relationship in _relationships)
        {
            relationshipText += relationship.Key + ": " + relationship.Value + "\n";
        }

        return relationshipText;
    }

    public void UpdateRelationship(string personality, string relationship)
    {
        if (!_relationships.ContainsKey(personality))
        {
            Debug.LogError("Personality not found: " + personality);
            return;
        }

        _relationships[personality] = relationship;
    }

    public string SetLocation(string location)
    {
        _currentLocation = location;
        return _currentLocation;
    }

    private void DebugPrint(string message)
    {
        if (_debugMode)
        {
            Debug.Log(message);
        }
    }
}

[System.Serializable]
public class OpponentAction
{
    public string from;
    public string actionDescription;

    public OpponentAction(string from, string actionDescription)
    {
        this.from = from;
        this.actionDescription = actionDescription;
    }
}
