using UnityEngine;
using System.Collections.Generic;

public class AIInterpreter : MonoBehaviour
{
    [SerializeField]
    private AIController _aiController;

    [SerializeField]
    private Personality _personality;

    [SerializeField]
    private BasePrompt _basePrompt;

    private LLMAction _previousAction;

    private OpponentLLMAction _previousOpponentAction;

    private Dictionary<string, string> _relationships;

    private string _currentLocation;

    public Personality Personality
    {
        get { return _personality; }
    }

    [System.Serializable]
    private class OpponentLLMAction
    {
        public string name;
        public LLMAction action;
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
        LLMAPIHandler.Instance.SendChatRequest(prompt, InterpretResponseCallback, ErrorCallback);
    }

    private void ErrorCallback(string error)
    {
        Debug.LogError("Error: " + error);
        // Retry
        SendRequest();
    }

    private void InterpretResponseCallback(string response)
    {
        Debug.Log("Interpreting response: " + response);

        try
        {
            LLMAction action = JsonUtility.FromJson<LLMAction>(response);

            Debug.Log("Interpreted action: " + action.action);

            string param = action.target;
            string content = action.content;

            switch (action.action.ToLower())
            {
                case "talk":
                    _aiController.Talk(param, content);
                    break;
                case "goto":
                    _aiController.GoTo(param);
                    break;
                default:
                    // Invalid action, request new action
                    SendRequest();
                    break;
            }

            _previousAction = action;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error interpreting response: " + e.Message);
            SendRequest();
        }
    }

    private string BuildPrompt()
    {
        string prompt =
            _basePrompt.Intro
            + _personality.Name
            + _personality.Description
            + "\n"
            + _basePrompt.AfterDesc
            + "\n"
            + _basePrompt.CurrentRelationshipText
            + "\n"
            + BuildCurrentRelationshipText();

        if (_previousOpponentAction != null)
        {
            string previousOpponentActionJSON = JsonUtility.ToJson(_previousOpponentAction);
            prompt +=
                _basePrompt.PreviousOpponentText
                + _previousOpponentAction.name
                + _basePrompt.PreviousActionOpponent
                + "\n"
                + previousOpponentActionJSON;
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
}
