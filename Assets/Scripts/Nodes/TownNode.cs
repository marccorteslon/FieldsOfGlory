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

    [Header("Town Objects")]
    public GameObject townObjectsRoot;

    public PlayerMovement playerMovement;

    public void EnterTown()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (progressManager == null)
        {
            Debug.LogError("TownNode: no se encontrÃ¯Â¿Â½ ProgressManager.");
            return;
        }

        // SOLO puedes abrir el pueblo en el que estÃ¯Â¿Â½s
        if (progressManager.CurrentCityId != cityId)
        {
            Debug.Log($"Primero debes viajar a {cityId}.");
            return;
        }

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
        if (playerMovement != null)
            playerMovement.canMove = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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

        //shopPanel.SetOriginTownPanel(townPanelObject);
        shopPanel.SetOriginTownPanel(townObjectsRoot != null ? townObjectsRoot : townPanelObject);

        if (shopPanel.shopTitleText != null && currentCity != null)
        {
            shopPanel.shopTitleText.text = $"{currentCity.displayName}'s Shop";
        }

        if (townPanelObject != null)
            townPanelObject.SetActive(false);

        if (townObjectsRoot != null)
            townObjectsRoot.SetActive(false);

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

        if (townObjectsRoot != null)
            townObjectsRoot.SetActive(false);

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

        if (townObjectsRoot != null)
            townObjectsRoot.SetActive(false);

        if (mapButtonsObject != null)
            mapButtonsObject.SetActive(true);

        currentCity = null;
        currentShop = null;
        currentTavern = null;

        if (playerMovement != null)
            playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        //if (townPanelObject != null)
        //    townPanelObject.SetActive(true);

        if (townPanelObject != null)
            townPanelObject.SetActive(false);

        if (townObjectsRoot != null)
            townObjectsRoot.SetActive(true);

        if (shopPanelObject != null)
            shopPanelObject.SetActive(false);

        if (tavernPanelObject != null)
            tavernPanelObject.SetActive(false);

        if (travelUI != null)
            travelUI.RefreshTravelOptions(city.cityId);
    }

    public void ExitShop()
    {
        if (shopPanelObject != null)
            shopPanelObject.SetActive(false);

        if (townObjectsRoot != null)
            townObjectsRoot.SetActive(true);

        if (townPanelObject != null)
            townPanelObject.SetActive(true);

        if (playerMovement != null)
            playerMovement.canMove = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnError(Exception ex)
    {
        Debug.LogError(ex.Message);
    }
}


