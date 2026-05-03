using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Tavern Database")]
public class TavernDatabase : ScriptableObject
{
    public List<TavernDefinition> taverns = new();

    private Dictionary<string, TavernDefinition> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, TavernDefinition>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var tavern in taverns)
        {
            if (tavern == null || string.IsNullOrWhiteSpace(tavern.tavernId)) continue;
            
            string id = tavern.tavernId.Trim();
            if (!lookup.ContainsKey(id))
                lookup.Add(id, tavern);
        }
    }

    public TavernDefinition GetById(string id)
    {
        if (lookup == null) BuildLookup();
        if (string.IsNullOrWhiteSpace(id)) return null;
        return lookup.TryGetValue(id.Trim(), out var tavern) ? tavern : null;
    }
}