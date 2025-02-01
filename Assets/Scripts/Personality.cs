using UnityEngine;

[CreateAssetMenu(fileName = "Personality", menuName = "Scriptable Objects/Personality")]
public class Personality : ScriptableObject
{
    public string Name;
    [TextArea]
    public string Description;
}
