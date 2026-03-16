using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public List<ShopDefinition> shops = new();

    private Dictionary<string, ShopDefinition> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, ShopDefinition>();

        foreach (var shop in shops)
        {
            if (shop == null || string.IsNullOrWhiteSpace(shop.shopId)) continue;

            if (!lookup.ContainsKey(shop.shopId))
                lookup.Add(shop.shopId, shop);
        }
    }

    public ShopDefinition GetById(string id)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(id, out var shop) ? shop : null;
    }
}