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

    [Header("Visual Attachments")]
    public Transform horseAttachment;
    public Transform lanceAttachment;
    public Transform shieldAttachment;
    public Transform armorAttachment;

    private GameObject currentHorseVisual;
    private GameObject currentLanceVisual;
    private GameObject currentShieldVisual;
    private GameObject currentArmorVisual;

    public delegate void EquipmentChanged();
    public event EquipmentChanged OnEquipmentChanged;

    public delegate void VisualInstantiated(EquipmentSlot slot, GameObject visualInstance);
    public event VisualInstantiated OnVisualInstantiated;

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
            case EquipmentSlot.Horse: 
                horse = item; 
                UpdateVisual(item, horseAttachment, ref currentHorseVisual);
                break;
            case EquipmentSlot.Lance: 
                lance = item; 
                UpdateVisual(item, lanceAttachment, ref currentLanceVisual);
                break;
            case EquipmentSlot.Shield: 
                shield = item; 
                UpdateVisual(item, shieldAttachment, ref currentShieldVisual);
                break;
            case EquipmentSlot.Armor: 
                armor = item; 
                UpdateVisual(item, armorAttachment, ref currentArmorVisual);
                break;
        }

        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Horse: 
                horse = null; 
                ClearVisual(ref currentHorseVisual);
                break;
            case EquipmentSlot.Lance: 
                lance = null; 
                ClearVisual(ref currentLanceVisual);
                break;
            case EquipmentSlot.Shield: 
                shield = null; 
                ClearVisual(ref currentShieldVisual);
                break;
            case EquipmentSlot.Armor: 
                armor = null; 
                ClearVisual(ref currentArmorVisual);
                break;
        }

        OnEquipmentChanged?.Invoke();
    }

    private void UpdateVisual(EquipmentDefinition item, Transform attachment, ref GameObject currentVisual)
    {
        ClearVisual(ref currentVisual);

        if (item.visualPrefab != null && attachment != null)
        {
            currentVisual = Instantiate(item.visualPrefab, attachment);
            currentVisual.transform.localPosition = Vector3.zero;
            currentVisual.transform.localRotation = Quaternion.identity;
            
            OnVisualInstantiated?.Invoke(item.slot, currentVisual);
        }
    }

    private void ClearVisual(ref GameObject currentVisual)
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
            currentVisual = null;
        }
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
