using System.Collections.Generic;
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

    [Header("Random Encounters (Route)")]
    [Range(0, 100)] public int dangerIndex = 0;
    public List<WeightedEncounterEntry> possibleEncounters = new();
}