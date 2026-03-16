using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Tavern Database")]
public class TavernDatabase : ScriptableObject
{
    public List<TavernDefinition> taverns = new();

    private Dictionary<string, TavernDefinition> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, TavernDefinition>();

        foreach (var tavern in taverns)
        {
            if (tavern == null || string.IsNullOrWhiteSpace(tavern.tavernId)) continue;

            if (!lookup.ContainsKey(tavern.tavernId))
                lookup.Add(tavern.tavernId, tavern);
        }
    }

    public TavernDefinition GetById(string id)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(id, out var tavern) ? tavern : null;
    }
}