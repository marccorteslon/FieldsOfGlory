using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Random Encounter")]
public class RandomEncounterDefinition : ScriptableObject
{
    public string encounterId;
    public string title;

    [Header("Illustration")]
    public Sprite encounterImage;

    [TextArea(4, 8)]
    public string description;

    public List<EncounterOptionDefinition> options = new();
}