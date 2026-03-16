using System;
using UnityEngine;

public interface IDataRepository
{
    void GetCityById(string id, Action<CityDefinition> onSuccess, Action<Exception> onFailure);
    void GetShopById(string id, Action<ShopDefinition> onSuccess, Action<Exception> onFailure);
    void GetTavernById(string id, Action<TavernDefinition> onSuccess, Action<Exception> onFailure);
}