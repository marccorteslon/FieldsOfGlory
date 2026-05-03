using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/World/Shop Database")]
public class ShopDatabase : ScriptableObject
{
    public List<ShopDefinition> shops = new();

    private Dictionary<string, ShopDefinition> lookup;

    public void BuildLookup()
    {
        lookup = new Dictionary<string, ShopDefinition>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var shop in shops)
        {
            if (shop == null || string.IsNullOrWhiteSpace(shop.shopId)) continue;
            
            string id = shop.shopId.Trim();
            if (!lookup.ContainsKey(id))
                lookup.Add(id, shop);
        }
    }

    public ShopDefinition GetById(string id)
    {
        if (lookup == null) BuildLookup();
        if (string.IsNullOrWhiteSpace(id)) return null;
        return lookup.TryGetValue(id.Trim(), out var shop) ? shop : null;
    }
}