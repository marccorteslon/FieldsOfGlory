using UnityEngine;

public class LoadoutStatsComponent : MonoBehaviour
{
    // Componente que recalcula automaticamente las stats finales cuando cambia el equipo.
    public EquipmentManager equipment;
    public LoadoutStats stats = new LoadoutStats();

    void Awake()
    {
        if (equipment == null) equipment = GetComponent<EquipmentManager>();
        if (equipment != null) equipment.OnEquipmentChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (equipment != null) equipment.OnEquipmentChanged -= Refresh;
    }

    public void Refresh()
    {
        if (equipment == null) return;
        
        System.Collections.Generic.List<StatModifier> allMods = new System.Collections.Generic.List<StatModifier>(equipment.GetAllModifiers());
        
        ProgressManager progress = FindFirstObjectByType<ProgressManager>();
        if (progress != null && progress.data != null && progress.data.permanentBoosts != null)
        {
            allMods.AddRange(progress.data.permanentBoosts);
        }

        stats.Recalculate(allMods);
    }
}
