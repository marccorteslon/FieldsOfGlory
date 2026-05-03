using UnityEngine;

public class RandomEncounterManager : MonoBehaviour
{
    [Header("Data")]
    public RandomEncounterDatabase encounterDatabase;

    [Header("UI")]
    public EncounterPopupController popupController;

    public void TryTriggerEncounter(MapNodeDefinition node)
    {
        if (node == null) return;
        if (encounterDatabase == null) return;
        if (popupController == null) return;

        if (node.possibleEncounters == null || node.possibleEncounters.Count == 0)
            return;

        int roll = Random.Range(1, 101);

        if (roll > node.dangerIndex)
        {
            Debug.Log($"[Encounter] No ocurre encuentro ({roll}/{node.dangerIndex})");
            return;
        }

        string encounterId = PickWeightedEncounter(node);
        RandomEncounterDefinition encounter = encounterDatabase.GetById(encounterId);

        if (encounter == null)
        {
            Debug.LogWarning("Encounter no encontrado: " + encounterId);
            return;
        }

        popupController.OpenEncounter(encounter);
    }

    string PickWeightedEncounter(MapNodeDefinition node)
    {
        int totalWeight = 0;

        foreach (var entry in node.possibleEncounters)
            totalWeight += Mathf.Max(0, entry.weight);

        if (totalWeight <= 0)
            return node.possibleEncounters[0].encounterId;

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var entry in node.possibleEncounters)
        {
            current += Mathf.Max(0, entry.weight);

            if (roll < current)
                return entry.encounterId;
        }

        return node.possibleEncounters[0].encounterId;
    }
}