using UnityEngine;

public class LoadoutStatsComponent : MonoBehaviour
{
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
        stats.Recalculate(equipment.GetAllModifiers());
    }
}
