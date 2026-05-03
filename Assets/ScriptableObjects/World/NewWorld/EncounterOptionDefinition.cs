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
}