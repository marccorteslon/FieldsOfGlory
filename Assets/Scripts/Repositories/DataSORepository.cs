using System;
using UnityEngine;

public class DataSORepository : IDataRepository
{
    private readonly CityDatabase cityDatabase;
    private readonly ShopDatabase shopDatabase;
    private readonly TavernDatabase tavernDatabase;

    public DataSORepository(CityDatabase cityDb, ShopDatabase shopDb, TavernDatabase tavernDb)
    {
        cityDatabase = cityDb;
        shopDatabase = shopDb;
        tavernDatabase = tavernDb;

        cityDatabase.BuildLookup();
        shopDatabase.BuildLookup();
        tavernDatabase.BuildLookup();
    }

    public void GetCityById(string id, Action<CityDefinition> onSuccess, Action<Exception> onFailure)
    {
        var city = cityDatabase.GetById(id);
        if (city != null) onSuccess(city);
        else onFailure(new Exception($"City not found: {id}"));
    }

    public void GetShopById(string id, Action<ShopDefinition> onSuccess, Action<Exception> onFailure)
    {
        var shop = shopDatabase.GetById(id);
        if (shop != null) onSuccess(shop);
        else onFailure(new Exception($"Shop not found: {id}"));
    }

    public void GetTavernById(string id, Action<TavernDefinition> onSuccess, Action<Exception> onFailure)
    {
        var tavern = tavernDatabase.GetById(id);
        if (tavern != null) onSuccess(tavern);
        else onFailure(new Exception($"Tavern not found: {id}"));
    }
}