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
    public Renderer horseRenderer;
    public Transform lanceAttachment;
    [Tooltip("Opcional: Después de spawnearse en lanceAttachment, se convertirá en hijo de este objeto.")]
    public Transform finalLanceParent;
    public Transform shieldAttachment;
    public Transform armorAttachment;

    private Material originalHorseMaterial;
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
                if (horseRenderer != null && item.visualMaterial != null)
                {
                    if (originalHorseMaterial == null) originalHorseMaterial = horseRenderer.sharedMaterial;
                    horseRenderer.material = item.visualMaterial;
                }
                break;
            case EquipmentSlot.Lance: 
                lance = item; 
                UpdateVisual(item, lanceAttachment, ref currentLanceVisual, finalLanceParent);
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
                if (horseRenderer != null && originalHorseMaterial != null)
                {
                    horseRenderer.material = originalHorseMaterial;
                }
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

    private void UpdateVisual(EquipmentDefinition item, Transform attachment, ref GameObject currentVisual, Transform finalParent = null)
    {
        ClearVisual(ref currentVisual);

        if (item.visualPrefab == null)
        {
            Debug.LogWarning($"[EquipmentManager] No se pudo spawnear {item.name}: El campo 'visualPrefab' está vacío en el ScriptableObject.");
            return;
        }

        if (attachment == null)
        {
            Debug.LogWarning($"[EquipmentManager] No se pudo spawnear {item.name}: El 'Attachment' correspondiente está vacío en el Inspector del EquipmentManager.");
            return;
        }

        currentVisual = Instantiate(item.visualPrefab, attachment);
        
        // Aplicamos los ajustes manuales definidos en el ScriptableObject
        currentVisual.transform.localPosition = item.positionOffset;
        currentVisual.transform.localRotation = Quaternion.Euler(item.rotationOffset);
        currentVisual.transform.localScale = item.scaleOffset;
        
        // Si hay un padre final, lo re-emparentamos manteniendo su posición y tamaño exactos en el mundo 3D
        if (finalParent != null)
        {
            currentVisual.transform.SetParent(finalParent, true);
        }
        
        Debug.Log($"[EquipmentManager] Spawneado con éxito el visual para {item.name} en {attachment.name}.");
        
        OnVisualInstantiated?.Invoke(item.slot, currentVisual);
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
