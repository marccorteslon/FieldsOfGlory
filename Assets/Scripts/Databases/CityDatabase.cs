using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/City Database")]
public class CityDatabase : ScriptableObject
{
    public List<CityDefinition> cities = new();

    private Dictionary<string, CityDefinition> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, CityDefinition>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var city in cities)
        {
            if (city == null || string.IsNullOrWhiteSpace(city.cityId)) continue;
            
            string id = city.cityId.Trim();
            if (!lookup.ContainsKey(id))
                lookup.Add(id, city);
        }
    }

    public CityDefinition GetById(string id)
    {
        if (lookup == null) BuildLookup();
        if (string.IsNullOrWhiteSpace(id)) return null;
        return lookup.TryGetValue(id.Trim(), out var city) ? city : null;
    }
}