using System;
using UnityEngine;

public class TownNode : MonoBehaviour
{
    [Header("Town Data")]
    public string cityId;

    [Header("UI Refs")]
    public ShopPanelController shopPanel;
    public ProgressManager progressManager;
    public TownTravelUI travelUI;
    public GameObject mapButtonsObject;
    public GameObject townPanelObject;
    public GameObject shopPanelObject;
    public GameObject tavernPanelObject;

    private CityDefinition currentCity;
    private ShopDefinition currentShop;
    private TavernDefinition currentTavern;

    public void EnterTown()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (progressManager != null)
            progressManager.SetCurrentCity(cityId);

        if (mapButtonsObject != null)
            mapButtonsObject.SetActive(false);

        GameManager.dataRepository.GetCityById(
            cityId,
            OnCityLoaded,
            OnError
        );
    }

    public void EnterShop()
    {
        if (currentShop == null)
        {
            Debug.LogError("TownNode: no hay tienda cargada para este pueblo.");
            return;
        }

        if (shopPanel == null)
        {
            Debug.LogError("TownNode: shopPanel no asignado.");
            return;
        }

        shopPanel.SetOriginTownPanel(townPanelObject);

        if (townPanelObject != null)
            townPanelObject.SetActive(false);

        if (shopPanelObject != null)
            shopPanelObject.SetActive(true);

        if (tavernPanelObject != null)
            tavernPanelObject.SetActive(false);

        for (int i = 0; i < shopPanel.itemIds.Length; i++)
            shopPanel.itemIds[i] = string.Empty;

        int count = Mathf.Min(currentShop.itemIds.Count, shopPanel.itemIds.Length);
        for (int i = 0; i < count; i++)
            shopPanel.itemIds[i] = currentShop.itemIds[i];

        shopPanel.RefreshMoneyUI();
        shopPanel.RefreshShopUI();
    }

    public void EnterTavern()
    {
        if (currentTavern == null)
        {
            Debug.LogError("TownNode: no hay taberna cargada para este pueblo.");
            return;
        }

        if (townPanelObject != null)
            townPanelObject.SetActive(false);

        if (shopPanelObject != null)
            shopPanelObject.SetActive(false);

        if (tavernPanelObject != null)
            tavernPanelObject.SetActive(true);

        Debug.Log("Tavern opened: " + currentTavern.tavernId);
    }

    public void ExitTown()
    {
        if (shopPanelObject != null)
            shopPanelObject.SetActive(false);

        if (tavernPanelObject != null)
            tavernPanelObject.SetActive(false);

        if (townPanelObject != null)
            townPanelObject.SetActive(false);

        if (mapButtonsObject != null)
            mapButtonsObject.SetActive(true);

        currentCity = null;
        currentShop = null;
        currentTavern = null;
    }

    void OnCityLoaded(CityDefinition city)
    {
        currentCity = city;

        GameManager.dataRepository.GetShopById(
            city.shopId,
            shop =>
            {
                currentShop = shop;
                Debug.Log("Shop loaded: " + shop.shopId);
            },
            OnError
        );

        GameManager.dataRepository.GetTavernById(
            city.tavernId,
            tavern =>
            {
                currentTavern = tavern;
                Debug.Log("Tavern loaded: " + tavern.tavernId);
            },
            OnError
        );

        if (townPanelObject != null)
            townPanelObject.SetActive(true);

        if (shopPanelObject != null)
            shopPanelObject.SetActive(false);

        if (tavernPanelObject != null)
            tavernPanelObject.SetActive(false);

        if (travelUI != null)
            travelUI.RefreshTravelOptions(city.cityId);
    }

    void OnError(Exception ex)
    {
        Debug.LogError(ex.Message);
    }
}