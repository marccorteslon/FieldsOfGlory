using System;
using UnityEngine;

[Serializable]
public class EncounterOptionDefinition
{
    [TextArea]
    public string optionText;

    [Header("Stat Check")]
    public StatType statToCheck;
    public int difficulty = 10;

    [Header("Result Text")]
    [TextArea] public string successText;
    [TextArea] public string failureText;

    [Header("Effects")]
    public EncounterEffect[] successEffects;
    public EncounterEffect[] failureEffects;

    [Header("Branching Narrative (Optional)")]
    public RandomEncounterDefinition nextEncounterOnSuccess;
    public RandomEncounterDefinition nextEncounterOnFailure;

    [Header("Story Flags")]
    [Tooltip("If set, this option will only appear if the player has this flag.")]
    public string requiredFlag;
    
    [Tooltip("If set, this option will NOT appear if the player has this flag.")]
    public string forbiddenFlag;
}