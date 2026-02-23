using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
// Gestiona el equipo actual del personaje y notifica cuando cambia el equipamiento.
{
    [Header("Equipped (runtime)")]
    [SerializeField] private EquipmentDefinition horse;
    [SerializeField] private EquipmentDefinition lance;
    [SerializeField] private EquipmentDefinition shield;
    [SerializeField] private EquipmentDefinition armor;

    public delegate void EquipmentChanged();
    public event EquipmentChanged OnEquipmentChanged;

    public EquipmentDefinition GetEquipped(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Horse => horse,
        EquipmentSlot.Lance => lance,
        EquipmentSlot.Shield => shield,
        EquipmentSlot.Armor => armor,
        _ => null
    };

    public void Equip(EquipmentDefinition item)
    {
        if (item == null) return;

        switch (item.slot)
        {
            case EquipmentSlot.Horse: horse = item; break;
            case EquipmentSlot.Lance: lance = item; break;
            case EquipmentSlot.Shield: shield = item; break;
            case EquipmentSlot.Armor: armor = item; break;
        }

        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Horse: horse = null; break;
            case EquipmentSlot.Lance: lance = null; break;
            case EquipmentSlot.Shield: shield = null; break;
            case EquipmentSlot.Armor: armor = null; break;
        }

        OnEquipmentChanged?.Invoke();
    }

    public List<StatModifier> GetAllModifiers()
    {
        var mods = new List<StatModifier>();

        if (horse != null) mods.AddRange(horse.modifiers);
        if (lance != null) mods.AddRange(lance.modifiers);
        if (shield != null) mods.AddRange(shield.modifiers);
        if (armor != null) mods.AddRange(armor.modifiers);

        return mods;
    }
}
