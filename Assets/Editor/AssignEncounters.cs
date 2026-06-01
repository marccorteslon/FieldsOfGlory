using UnityEngine;
using UnityEditor;

public static class AssignEncounters
{
    [MenuItem("Tools/Assign World Encounters To Nodes")]
    public static void Assign()
    {
        AssignEncounterToNode("World_Muggdrassil", "Node_Muggdrassil");
        AssignEncounterToNode("World_ChateauBlanc", "Node_ChateauBlanc");
        AssignEncounterToNode("World_Mitonar", "Node_Mitonar");
        AssignEncounterToNode("World_PuertasDragon", "Node_PuertaDragon");
        AssignEncounterToNode("World_Tumulos", "Node_Tumulos");
        AssignEncounterToNode("World_Thurich", "Node_RuinasThurich");
        AssignEncounterToNode("World_Maulkin", "Node_HaciendaMaulkin");
        AssignEncounterToNode("World_PuenteNegro", "Node_PuenteNegro");
        AssignEncounterToNode("World_BosqueNorte", "Node_BosqueNorte");
        AssignEncounterToNode("World_ValleReyes", "Node_ValleReyes");
        AssignEncounterToNode("World_BocaDemonio", "Node_BocaDemonio");
        AssignEncounterToNode("World_TribuCielo", "Node_TribusCielo");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Encounters successfully assigned to Nodes!");
    }

    private static void AssignEncounterToNode(string encounterFileName, string nodeFileName)
    {
        string encounterPath = $"Assets/ScriptableObjects/World/NewWorld/Encounters/World/{encounterFileName}.asset";
        string nodePath = $"Assets/ScriptableObjects/World/NewWorld/Nodos/{nodeFileName}.asset";

        var encounter = AssetDatabase.LoadAssetAtPath<RandomEncounterDefinition>(encounterPath);
        var node = AssetDatabase.LoadAssetAtPath<MapNodeDefinition>(nodePath);

        if (encounter == null)
        {
            Debug.LogError($"Could not find encounter: {encounterPath}");
            return;
        }
        if (node == null)
        {
            Debug.LogError($"Could not find node: {nodePath}");
            return;
        }

        // Check if it already exists to prevent duplicates
        bool exists = false;
        foreach (var entry in node.possibleEncounters)
        {
            if (entry != null && entry.encounterId == encounter.encounterId)
            {
                exists = true;
                break;
            }
        }

        if (!exists)
        {
            node.possibleEncounters.Add(new WeightedEncounterEntry { encounterId = encounter.encounterId, weight = 100 });
            EditorUtility.SetDirty(node);
            Debug.Log($"Assigned {encounterFileName} to {nodeFileName}");
        }
    }
}
