using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/City Database")]
public class CityDatabase : ScriptableObject
{
    public List<CityDefinition> cities = new();

    private Dictionary<string, CityDefinition> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, CityDefinition>();

        foreach (var city in cities)
        {
            if (city == null || string.IsNullOrWhiteSpace(city.cityId)) continue;

            if (!lookup.ContainsKey(city.cityId))
                lookup.Add(city.cityId, city);
        }
    }

    public CityDefinition GetById(string id)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(id, out var city) ? city : null;
    }
}