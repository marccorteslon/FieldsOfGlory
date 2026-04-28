using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/NewWorld/Encounters/Random Encounter Database")]
public class RandomEncounterDatabase : ScriptableObject
{
    public List<RandomEncounterDefinition> encounters = new();

    public RandomEncounterDefinition GetById(string encounterId)
    {
        foreach (var encounter in encounters)
        {
            if (encounter != null && encounter.encounterId == encounterId)
                return encounter;
        }

        return null;
    }
}
