using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Travel Database")]
public class TravelDatabase : ScriptableObject
{
    public List<TravelRouteDefinition> routes = new();

    public List<TravelRouteDefinition> GetRoutesFromCity(string cityId)
    {
        List<TravelRouteDefinition> result = new();

        foreach (var route in routes)
        {
            if (route == null) continue;

            if (route.cityAId == cityId || route.cityBId == cityId)
                result.Add(route);
        }

        return result;
    }

    public string GetOtherCity(string currentCityId, TravelRouteDefinition route)
    {
        if (route.cityAId == currentCityId) return route.cityBId;
        if (route.cityBId == currentCityId) return route.cityAId;
        return null;
    }
}