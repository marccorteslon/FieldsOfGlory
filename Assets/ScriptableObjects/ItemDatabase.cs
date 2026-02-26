using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "FieldsOfGlory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<EquipmentDefinition> allEquipment = new();

    private Dictionary<string, EquipmentDefinition> _lookup;

    public void BuildLookup()
    {
        _lookup = new Dictionary<string, EquipmentDefinition>();

        foreach (var item in allEquipment)
        {
            if (item == null) continue;
            if (string.IsNullOrEmpty(item.id)) continue;

            if (!_lookup.ContainsKey(item.id))
                _lookup.Add(item.id, item);
            else
                Debug.LogWarning($"[ItemDatabase] ID duplicada: {item.id}");
        }
    }

    public EquipmentDefinition GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        if (_lookup == null) BuildLookup();
        return _lookup.TryGetValue(id, out var item) ? item : null;
    }
}