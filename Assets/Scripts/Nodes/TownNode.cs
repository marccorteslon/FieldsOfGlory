using System;
using UnityEngine;

public class TownNode : MonoBehaviour
{
    [Header("Town Data")]
    public string cityId;

    [Header("UI Refs")]
    public ShopPanelController shopPanel;
    public GameObject shopPanelObject;   // opcional, por si quieres activar el panel
    public GameObject tavernPanelObject; // opcional, por si luego quieres activar la taberna

    public void EnterTown()
    {
        GameManager.dataRepository.GetCityById(
            cityId,
            OnCityLoaded,
            OnError
        );
    }

    void OnCityLoaded(CityDefinition city)
    {
        GameManager.dataRepository.GetShopById(
            city.shopId,
            shop =>
            {
                Debug.Log("Shop loaded: " + shop.shopId);

                if (shopPanel == null)
                {
                    Debug.LogError("TownNode: shopPanel no asignado.");
                    return;
                }

                // Abrir panel de tienda si has asignado un GO
                if (shopPanelObject != null)
                    shopPanelObject.SetActive(true);

                // Rellenar exactamente 4 slots
                for (int i = 0; i < shopPanel.itemIds.Length; i++)
                    shopPanel.itemIds[i] = string.Empty;

                int count = Mathf.Min(shop.itemIds.Count, shopPanel.itemIds.Length);
                for (int i = 0; i < count; i++)
                    shopPanel.itemIds[i] = shop.itemIds[i];

                // Refrescar UI
                shopPanel.RefreshMoneyUI();
                shopPanel.RefreshShopUI();
            },
            OnError
        );

        GameManager.dataRepository.GetTavernById(
            city.tavernId,
            tavern =>
            {
                Debug.Log("Tavern loaded: " + tavern.tavernId);

                // De momento solo lo deja preparado por si quieres activar panel luego
                if (tavernPanelObject != null)
                {
                    // No lo activo automáticamente para no pisar la tienda.
                    // Cuando quieras usar la taberna, aquí conectas su UI.
                }
            },
            OnError
        );
    }

    void OnError(Exception ex)
    {
        Debug.LogError(ex.Message);
    }
}