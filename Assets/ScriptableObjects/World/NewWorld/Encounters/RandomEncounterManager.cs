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

        ProgressManager progressManager = FindFirstObjectByType<ProgressManager>();
        
        string encounterId = PickWeightedEncounter(node, progressManager);
        if (string.IsNullOrEmpty(encounterId))
        {
            Debug.Log($"[Encounter] Todos los encuentros de este nodo ya estÃ¡n completados.");
            return;
        }

        int roll = Random.Range(1, 101);

        if (roll > node.dangerIndex)
        {
            Debug.Log($"[Encounter] No ocurre encuentro ({roll}/{node.dangerIndex})");
            return;
        }

        RandomEncounterDefinition encounter = encounterDatabase.GetById(encounterId);

        if (encounter == null)
        {
            Debug.LogWarning("Encounter no encontrado: " + encounterId);
            return;
        }

        if (progressManager != null)
            progressManager.MarkEncounterCompleted(encounterId);

        popupController.OpenEncounter(encounter);
    }

    string PickWeightedEncounter(MapNodeDefinition node, ProgressManager progressManager)
    {
        int totalWeight = 0;

        foreach (var entry in node.possibleEncounters)
        {
            if (progressManager != null && progressManager.IsEncounterCompleted(entry.encounterId))
                continue;
            
            totalWeight += Mathf.Max(0, entry.weight);
        }

        if (totalWeight <= 0)
            return null; // All possible encounters are completed

        int roll = Random.Range(0, totalWeight);
        int current = 0;

        foreach (var entry in node.possibleEncounters)
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