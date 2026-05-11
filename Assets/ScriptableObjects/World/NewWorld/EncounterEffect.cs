using System;
using UnityEngine;

[Serializable]
public class EncounterEffect
{
    public EncounterEffectType type;

    [Header("Money / Days")]
    public int value;

    [Header("Only for MoveToNode")]
    public string targetNodeId;

    [Header("Only for AddItem")]
    public EquipmentDefinition itemReward;
}