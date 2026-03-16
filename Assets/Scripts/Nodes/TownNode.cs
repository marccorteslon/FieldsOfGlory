using System;
using UnityEngine;

public class TownNode : MonoBehaviour
{
    public string cityId;

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
                // aqui bres la UI de tienda
            },
            OnError
        );

        GameManager.dataRepository.GetTavernById(
            city.tavernId,
            tavern =>
            {
                Debug.Log("Tavern loaded: " + tavern.tavernId);
                // aqui bres la UI de taberna
            },
            OnError
        );
    }

    void OnError(Exception ex)
    {
        Debug.LogError(ex.Message);
    }
}