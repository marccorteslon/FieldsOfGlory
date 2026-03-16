using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Tavern")]
public class TavernDefinition : ScriptableObject
{
    public string tavernId;
    [TextArea(3, 6)]
    public string description;
}