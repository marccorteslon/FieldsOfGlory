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

    [Header("Armor Mesh Swapping")]
    [Tooltip("SkinnedMeshRenderer de la armor del player. Si se asigna, se usará para el mesh swap.")]
    public SkinnedMeshRenderer armorSkinnedMeshRenderer;
    [Tooltip("MeshFilter de la armor del player (alternativa si no usa SkinnedMeshRenderer).")]
    public MeshFilter armorMeshFilter;

    private Material originalHorseMaterial;
    private Mesh originalArmorMesh;
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
                if (item is ArmorDefinition armorDef && armorDef.armorMesh != null)
                {
                    // Mesh swap directo: sin instanciar prefab
                    if (armorSkinnedMeshRenderer != null)
                    {
                        if (originalArmorMesh == null) originalArmorMesh = armorSkinnedMeshRenderer.sharedMesh;
                        armorSkinnedMeshRenderer.sharedMesh = armorDef.armorMesh;
                        Debug.Log($"[EquipmentManager] Armor mesh swapped (SkinnedMeshRenderer): {armorDef.armorMesh.name}");
                    }
                    else if (armorMeshFilter != null)
                    {
                        if (originalArmorMesh == null) originalArmorMesh = armorMeshFilter.sharedMesh;
                        armorMeshFilter.sharedMesh = armorDef.armorMesh;
                        Debug.Log($"[EquipmentManager] Armor mesh swapped (MeshFilter): {armorDef.armorMesh.name}");
                    }
                    else
                    {
                        Debug.LogWarning("[EquipmentManager] armorMesh definida pero no hay SkinnedMeshRenderer ni MeshFilter asignados.");
                        UpdateVisual(item, armorAttachment, ref currentArmorVisual);
                    }
                }
                else
                {
                    UpdateVisual(item, armorAttachment, ref currentArmorVisual);
                }
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
                // Restaurar mesh original si se hizo swap
                if (originalArmorMesh != null)
                {
                    if (armorSkinnedMeshRenderer != null)
                        armorSkinnedMeshRenderer.sharedMesh = originalArmorMesh;
                    else if (armorMeshFilter != null)
                        armorMeshFilter.sharedMesh = originalArmorMesh;
                    originalArmorMesh = null;
                    Debug.Log("[EquipmentManager] Armor mesh restaurada al original.");
                }
                ClearVisual(ref currentArmorVisual);
                armor = null;
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

        // 1. Instanciamos el prefab en la raíz de forma limpia
        currentVisual = Instantiate(item.visualPrefab);
        
        // 2. Determinamos cuál es el padre final y emparentamos directamente con 'keepWorldPosition = false'
        // Esto hace que el objeto adopte el sistema de coordenadas local limpio del padre (posición 0, rotación 0, escala 1)
        Transform targetParent = (finalParent != null) ? finalParent : attachment;
        currentVisual.transform.SetParent(targetParent, false);
        
        // 3. Ahora aplicamos las posiciones, rotaciones y escalas locales directamente en el espacio de su padre.
        // De esta forma, los valores en el Transform de la escena coincidirán EXACTAMENTE con los de tu ScriptableObject
        currentVisual.transform.localPosition = item.positionOffset;
        currentVisual.transform.localRotation = Quaternion.Euler(item.rotationOffset);
        currentVisual.transform.localScale = item.scaleOffset;
        
        Debug.Log($"[EquipmentManager] Spawneado con éxito el visual para {item.name} en {targetParent.name} con los valores exactos del ScriptableObject.");
        
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
