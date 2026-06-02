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

    [Header("World Spawn")]
    [Tooltip("Punto donde aparecerá el WalkingPlayer al entrar a este pueblo (pulsar X en el mapa).")]
    public TownObjectSpawn townObjectSpawn;

    public void EnterTown()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (progressManager == null)
        {
            Debug.LogError("TownNode: no se encontró ProgressManager.");
            return;
        }

        // SOLO puedes abrir el pueblo en el que estás
        if (progressManager.CurrentCityId != cityId)
        {
            Debug.Log($"Primero debes viajar a {cityId}.");
            return;
        }

        // Teletransportar al WalkingPlayer al punto de spawn del pueblo
        TeleportPlayerToSpawn();

        if (mapButtonsObject != null)
            mapButtonsObject.SetActive(false);

        // Bloquear el input de navegación del mapa mientras estemos en el pueblo
        WorldMapManager.SetInTown(true);

        GameManager.dataRepository.GetCityById(
            cityId,
            OnCityLoaded,
            OnError
        );
    }

    void TeleportPlayerToSpawn()
    {
        if (townObjectSpawn == null)
            return;

        Transform player = playerMovement != null ? playerMovement.transform : null;

        if (player == null)
        {
            Debug.LogWarning("TownNode: no hay WalkingPlayer asignado para teletransportar.");
            return;
        }

        // El CharacterController bloquea la teleportación; hay que desactivarlo un frame
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = townObjectSpawn.transform.position;
        player.rotation = townObjectSpawn.transform.rotation;

        if (cc != null) cc.enabled = true;

        Debug.Log($"[TownNode] WalkingPlayer teletransportado al spawn de '{cityId}' en {townObjectSpawn.transform.position}");
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

        // Reanudar el input de navegación del mapa
        WorldMapManager.SetInTown(false);

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


