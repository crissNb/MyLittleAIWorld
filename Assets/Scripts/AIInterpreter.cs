using UnityEngine;

public class AIInterpreter : MonoBehaviour
{
    [SerializeField] private Personality _personality;
    [SerializeField] private BasePrompt _basePrompt;

    private string BuildPrompt()
    {
        string prompt = _basePrompt.Intro + _personality.Name + _personality.Description + "\n" + _basePrompt.AfterDesc;
        Debug.Log(_personality.Name + " is performing prompt:\n" + prompt);
        return prompt;
    }
}