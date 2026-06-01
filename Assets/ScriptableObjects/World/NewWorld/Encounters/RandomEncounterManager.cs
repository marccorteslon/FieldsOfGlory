using UnityEngine;

public class RandomEncounterManager : MonoBehaviour
{
    [Header("Data")]
    public RandomEncounterDatabase encounterDatabase;

    [Header("UI")]
    public EncounterPopupController popupController;

    public void TryTriggerEncounter(MapConnectionDefinition connection, MapNodeDefinition destinationNode)
    {
        if (encounterDatabase == null) return;
        if (popupController == null) return;

        ProgressManager progressManager = FindFirstObjectByType<ProgressManager>();

        // 1. Try Route Encounter First
        if (connection != null && connection.possibleEncounters != null && connection.possibleEncounters.Count > 0)
        {
            if (TryRollEncounter(connection.possibleEncounters, connection.dangerIndex, progressManager))
                return; // Encounter triggered on route, don't trigger node encounter
        }

        // 2. Try Node Encounter Second
        if (destinationNode != null && !destinationNode.isCrossroad && destinationNode.possibleEncounters != null && destinationNode.possibleEncounters.Count > 0)
        {
            TryRollEncounter(destinationNode.possibleEncounters, destinationNode.dangerIndex, progressManager);
        }
    }

    private bool TryRollEncounter(System.Collections.Generic.List<WeightedEncounterEntry> possibleEncounters, int dangerIndex, ProgressManager progressManager)
    {
        string encounterId = PickWeightedEncounter(possibleEncounters, progressManager);
        if (string.IsNullOrEmpty(encounterId))
        {
            Debug.Log($"[Encounter] Todos los encuentros de esta lista ya están completados.");
            return false;
        }

        int roll = Random.Range(1, 101);

        if (roll > dangerIndex)
        {
            Debug.Log($"[Encounter] No ocurre encuentro ({roll}/{dangerIndex})");
            return false;
        }

        RandomEncounterDefinition encounter = encounterDatabase.GetById(encounterId);

        if (encounter == null)
        {
            Debug.LogWarning("Encounter no encontrado: " + encounterId);
            return false;
        }

        if (progressManager != null)
            progressManager.MarkEncounterCompleted(encounterId);

        popupController.OpenEncounter(encounter);
        return true;
    }

    string PickWeightedEncounter(System.Collections.Generic.List<WeightedEncounterEntry> possibleEncounters, ProgressManager progressManager)
    {
        int totalWeight = 0;

        foreach (var entry in possibleEncounters)
        {
            if (progressManager != null && progressManager.IsEncounterCompleted(entry.encounterId))
                continue;
            
            totalWeight += Mathf.Max(0, entry.weight);
        }

        if (totalWeight <= 0)
            return null; // All possible encounters are completed

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var entry in possibleEncounters)
        {
            if (progressManager != null && progressManager.IsEncounterCompleted(entry.encounterId))
                continue;

            current += Mathf.Max(0, entry.weight);

            if (roll < current)
                return entry.encounterId;
        }

        return null;
    }
}