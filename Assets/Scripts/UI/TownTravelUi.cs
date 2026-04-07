using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TownTravelUI : MonoBehaviour
{
    [Header("Refs")]
    public TravelManager travelManager;
    public ProgressManager progressManager;
    public TMP_Dropdown destinationDropdown;
    public TownNode townNode;

    private readonly List<string> destinationCityIds = new();

    void Awake()
    {
        if (travelManager == null)
            travelManager = FindFirstObjectByType<TravelManager>();

        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();
    }

    public void RefreshTravelOptions(string currentCityId)
    {
        if (travelManager == null)
        {
            Debug.LogError("TownTravelUI: no hay TravelManager.");
            return;
        }

        if (destinationDropdown == null)
        {
            Debug.LogError("TownTravelUI: no hay TMP_Dropdown asignado.");
            return;
        }

        destinationCityIds.Clear();
        destinationDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new();
        var routes = travelManager.GetAvailableRoutes();

        foreach (var route in routes)
        {
            string destinationCityId = travelManager.GetDestinationCityId(route);
            if (string.IsNullOrWhiteSpace(destinationCityId))
                continue;

            destinationCityIds.Add(destinationCityId);

            string label = $"{destinationCityId} ({route.travelDays} días)";
            options.Add(new TMP_Dropdown.OptionData(label));
        }

        destinationDropdown.AddOptions(options);
        destinationDropdown.RefreshShownValue();
    }

    public void TravelSelected()
    {
        if (travelManager == null || destinationDropdown == null)
            return;

        if (destinationCityIds.Count == 0)
        {
            Debug.LogWarning("TownTravelUI: no hay destinos disponibles.");
            return;
        }

        int index = destinationDropdown.value;

        if (index < 0 || index >= destinationCityIds.Count)
            return;

        string destinationCityId = destinationCityIds[index];

        bool success = travelManager.TravelTo(destinationCityId);

        if (!success)
            return;

        if (townNode != null)
            townNode.ExitTown();
    }
}