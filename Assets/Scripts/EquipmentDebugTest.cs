using UnityEngine;

public class EquipmentDebugTest : MonoBehaviour
// Script de prueba para equipar/desequipar items con teclas y mostrar las stats por consola.
{
    public EquipmentManager eq;
    public LoadoutStatsComponent loadout;

    [Header("Test items (assets)")]
    public EquipmentDefinition testHorse;
    public EquipmentDefinition testLance;
    public EquipmentDefinition testShield;
    public EquipmentDefinition testArmor;

    void Awake()
    {
        if (eq == null) eq = GetComponent<EquipmentManager>();
        if (loadout == null) loadout = GetComponent<LoadoutStatsComponent>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) eq.Equip(testHorse);
        if (Input.GetKeyDown(KeyCode.Alpha2)) eq.Equip(testLance);
        if (Input.GetKeyDown(KeyCode.Alpha3)) eq.Equip(testShield);
        if (Input.GetKeyDown(KeyCode.Alpha4)) eq.Equip(testArmor);

        if (Input.GetKeyDown(KeyCode.Q)) eq.Unequip(EquipmentSlot.Horse);
        if (Input.GetKeyDown(KeyCode.W)) eq.Unequip(EquipmentSlot.Lance);
        if (Input.GetKeyDown(KeyCode.E)) eq.Unequip(EquipmentSlot.Shield);
        if (Input.GetKeyDown(KeyCode.R)) eq.Unequip(EquipmentSlot.Armor);

        if (Input.GetKeyDown(KeyCode.P))
        {
            var s = loadout.stats;
            Debug.Log($"MV:{s.Get(StatType.MV)} V:{s.Get(StatType.V)} | BF:{s.Get(StatType.BF)} BL:{s.Get(StatType.BL)} M:{s.Get(StatType.M)} | BB:{s.Get(StatType.BB)}");
        }
    }
}
