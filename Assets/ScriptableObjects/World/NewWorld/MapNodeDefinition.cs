using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Map Node")]
public class MapNodeDefinition : ScriptableObject
{
    [Header("Identity")]
    public string nodeId;
    public string displayName;

    [Header("Travel")]
    public int travelDaysCost = 1;
    [Range(0, 100)] public int dangerIndex = 0;

    [Header("Town")]
    public bool isTown;
    public string cityId;

    [Header("Random Encounters")]
    public List<WeightedEncounterEntry> possibleEncounters = new();
}