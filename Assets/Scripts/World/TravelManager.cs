using System.Collections.Generic;
using UnityEngine;

public class TravelManager : MonoBehaviour
{
    [Header("Travel Data")]
    public TravelDatabase travelDatabase;

    [Header("Refs")]
    public ProgressManager progress;

    public string CurrentCityId
    {
        get
        {
            if (progress == null) return null;
            return progress.CurrentCityId;
        }
    }

    void Awake()
    {
        if (progress == null)
            progress = FindObjectOfType<ProgressManager>();
    }

    public List<TravelRouteDefinition> GetAvailableRoutes()
    {
        if (travelDatabase == null)
        {
            Debug.LogError("TravelManager: no hay TravelDatabase asignada.");
            return new List<TravelRouteDefinition>();
        }

        if (string.IsNullOrWhiteSpace(CurrentCityId))
        {
            Debug.LogWarning("TravelManager: no hay currentCityId cargada.");
            return new List<TravelRouteDefinition>();
        }

        return travelDatabase.GetRoutesFromCity(CurrentCityId);
    }

    public string GetDestinationCityId(TravelRouteDefinition route)
    {
        if (route == null) return null;
        return travelDatabase.GetOtherCity(CurrentCityId, route);
    }

    public int GetTravelDays(TravelRouteDefinition route)
    {
        if (route == null) return 0;
        return route.travelDays;
    }

    public bool TravelTo(string destinationCityId)
    {
        var routes = GetAvailableRoutes();

        foreach (var route in routes)
        {
            string otherCity = GetDestinationCityId(route);

            if (otherCity == destinationCityId)
            {
                progress.SetCurrentCity(destinationCityId);
                progress.AdvanceDays(route.travelDays);

                Debug.Log($"Viaje realizado a {destinationCityId} en {route.travelDays} días.");
                return true;
            }
        }

        Debug.LogWarning($"No existe ruta desde {CurrentCityId} hasta {destinationCityId}");
        return false;
    }
}