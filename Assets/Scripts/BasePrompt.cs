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
}
