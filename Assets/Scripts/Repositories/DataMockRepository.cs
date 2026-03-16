using System;
using UnityEngine;

public class DataMockRepository : IDataRepository
{
    public void GetCityById(string id, Action<CityDefinition> onSuccess, Action<Exception> onFailure)
    {
        var city = ScriptableObject.CreateInstance<CityDefinition>();
        city.cityId = id;
        city.displayName = "Mock City";
        city.shopId = "shop_mock";
        city.tavernId = "tavern_mock";
        onSuccess(city);
    }

    public void GetShopById(string id, Action<ShopDefinition> onSuccess, Action<Exception> onFailure)
    {
        var shop = ScriptableObject.CreateInstance<ShopDefinition>();
        shop.shopId = id;
        shop.itemIds.Add("horse_basic");
        shop.itemIds.Add("lance_basic");
        shop.itemIds.Add("shield_basic");
        shop.itemIds.Add("armor_basic");
        onSuccess(shop);
    }

    public void GetTavernById(string id, Action<TavernDefinition> onSuccess, Action<Exception> onFailure)
    {
        var tavern = ScriptableObject.CreateInstance<TavernDefinition>();
        tavern.tavernId = id;
        tavern.description = "Mock Tavern";
        onSuccess(tavern);
    }
}