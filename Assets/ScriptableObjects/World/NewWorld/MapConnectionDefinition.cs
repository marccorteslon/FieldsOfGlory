using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Map Connection")]
public class MapConnectionDefinition : ScriptableObject
{
    [Header("Nodes")]
    public string nodeAId;
    public string nodeBId;

    [Header("Input Direction")]
    public MapDirection directionFromA;
    public MapDirection directionFromB;
}