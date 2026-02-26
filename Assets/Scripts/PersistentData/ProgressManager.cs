using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [Header("Refs")]
    public EquipmentManager equipment;
    public ItemDatabase itemDatabase;

    public ProgressSaveData data;

    private bool _equipmentLoaded;

    void Awake()
    {
        data = ProgressSaveSystem.Load();

        if (equipment == null) equipment = FindObjectOfType<EquipmentManager>();

        // Prepara DB
        if (itemDatabase != null)
            itemDatabase.BuildLookup();

        // Carga equipamiento guardado una vez
        LoadEquipped();

        // Auto-guardado cuando cambie el equipo
        if (equipment != null)
            equipment.OnEquipmentChanged += SaveEquipped;
    }

    void OnDestroy()
    {
        if (equipment != null)
            equipment.OnEquipmentChanged -= SaveEquipped;
    }

    public int Money => data.money;

    public void AddMoney(int amount)
    {
        data.money += Mathf.Max(0, amount);
        ProgressSaveSystem.Save(data);
        Debug.Log($"[Progress] Dinero actual: {data.money}");
    }

    public int CalculateReward(int enemyHpWinPoints, int winRoundNumber)
    {
        float mult = winRoundNumber switch
        {
            1 => 2.0f,
            2 => 1.5f,
            _ => 1.0f
        };

        return Mathf.RoundToInt(enemyHpWinPoints * mult);
    }

    public void SaveEquipped()
    {
        if (equipment == null) return;

        data.equippedHorseId = equipment.GetEquipped(EquipmentSlot.Horse)?.id;
        data.equippedLanceId = equipment.GetEquipped(EquipmentSlot.Lance)?.id;
        data.equippedShieldId = equipment.GetEquipped(EquipmentSlot.Shield)?.id;
        data.equippedArmorId = equipment.GetEquipped(EquipmentSlot.Armor)?.id;

        ProgressSaveSystem.Save(data);
        Debug.Log("[Progress] Equipamiento guardado en JSON.");
    }

    public void LoadEquipped()
    {
        if (_equipmentLoaded) return; // evita duplicados
        _equipmentLoaded = true;

        if (equipment == null) return;

        if (itemDatabase == null)
        {
            Debug.LogWarning("[Progress] No hay ItemDatabase asignada, no se puede cargar equipamiento.");
            return;
        }

        var horse = itemDatabase.GetById(data.equippedHorseId);
        var lance = itemDatabase.GetById(data.equippedLanceId);
        var shield = itemDatabase.GetById(data.equippedShieldId);
        var armor = itemDatabase.GetById(data.equippedArmorId);

        if (horse != null) equipment.Equip(horse);
        if (lance != null) equipment.Equip(lance);
        if (shield != null) equipment.Equip(shield);
        if (armor != null) equipment.Equip(armor);

        Debug.Log("[Progress] Equipamiento cargado desde JSON.");
    }
    public bool TrySpendMoney(int cost)
    {
        if (cost < 0) cost = 0;

        if (data.money < cost)
            return false;

        data.money -= cost;
        ProgressSaveSystem.Save(data);
        Debug.Log($"[Progress] Gastados {cost}. Dinero actual: {data.money}");
        return true;
    }
}