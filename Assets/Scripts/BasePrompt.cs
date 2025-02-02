using UnityEngine;

[CreateAssetMenu(fileName = "BasePrompt", menuName = "Scriptable Objects/BasePrompt")]
public class BasePrompt : ScriptableObject
{
    [TextArea]
    public string Intro;

    [TextArea]
    public string AfterDesc;

    [TextArea]
    public string CurrentRelationshipText;

    [TextArea]
    public string PreviousOpponentText;

    [TextArea]
    public string PreviousActionOpponent;

    [TextArea]
    public string PreviousActionText;

    [TextArea]
    public string LocationText;

    [TextArea]
    public string ActionText;

    [TextArea]
    public string RespondInstructions;
}
