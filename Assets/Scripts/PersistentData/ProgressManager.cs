using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    [Header("Refs")]
    public EquipmentManager equipment;
    public ItemDatabase itemDatabase;

    public ProgressSaveData data;

    private bool equipmentLoaded;

    void Awake()
    {
        data = ProgressSaveSystem.Load();

        if (equipment == null)
            equipment = FindFirstObjectByType<EquipmentManager>();

        if (itemDatabase != null)
            itemDatabase.BuildLookup();

        FixMissingSaveValues();

        LoadEquipped();

        if (equipment != null)
            equipment.OnEquipmentChanged += SaveEquipped;
    }

    void OnDestroy()
    {
        if (equipment != null)
            equipment.OnEquipmentChanged -= SaveEquipped;
    }

    void FixMissingSaveValues()
    {
        bool changed = false;

        if (data.currentDay <= 0)
        {
            data.currentDay = 1;
            changed = true;
        }

        if (data.currentMonth <= 0)
        {
            data.currentMonth = 1;
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(data.currentCityId))
        {
            data.currentCityId = "city_senderopomar";
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(data.currentNodeId))
        {
            data.currentNodeId = "node_chozapapa";
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(data.equippedHorseId))
        {
            data.equippedHorseId = "Farm_Horse";
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(data.equippedLanceId))
        {
            data.equippedLanceId = "Training_Lance";
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(data.equippedShieldId))
        {
            data.equippedShieldId = "Training_Shield";
            changed = true;
        }

        if (string.IsNullOrWhiteSpace(data.equippedArmorId))
        {
            data.equippedArmorId = "Training_Armor";
            changed = true;
        }

        if (changed)
            SaveProgress();
    }

    public int Money => data.money;
    public string CurrentCityId => data.currentCityId;
    public string CurrentNodeId => data.currentNodeId;
    public int CurrentDay => data.currentDay;
    public int CurrentMonth => data.currentMonth;

    public JoustDifficulty? PracticeDifficultyOverride { get; set; }

    public void AddMoney(int amount)
    {
        data.money += Mathf.Max(0, amount);
        SaveProgress();
        Debug.Log($"[Progress] Dinero actual: {data.money}");
    }

    public bool TrySpendMoney(int cost)
    {
        if (cost < 0) cost = 0;

        if (data.money < cost)
            return false;

        data.money -= cost;
        SaveProgress();
        Debug.Log($"[Progress] Gastados {cost}. Dinero actual: {data.money}");
        return true;
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

        SaveProgress();
        Debug.Log("[Progress] Equipamiento guardado en JSON.");
    }

    public void LoadEquipped()
    {
        if (equipmentLoaded) return;
        equipmentLoaded = true;

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

    public void SetCurrentCity(string cityId)
    {
        if (string.IsNullOrWhiteSpace(cityId))
            return;

        data.currentCityId = cityId;
        SaveProgress();
        Debug.Log($"[Progress] Ciudad actual guardada: {cityId}");
    }

    public void SetCurrentNode(string nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
            return;

        data.currentNodeId = nodeId;
        SaveProgress();
        Debug.Log($"[Progress] Nodo actual guardado: {nodeId}");
    }

    public void AdvanceDays(int days)
    {
        if (days <= 0) return;

        data.currentDay += days;

        while (data.currentDay > 30)
        {
            data.currentDay -= 30;
            data.currentMonth++;

            if (data.currentMonth > 12)
                data.currentMonth = 1;
        }

        SaveProgress();
        Debug.Log($"[Progress] Fecha actual: D�a {data.currentDay}, Mes {data.currentMonth}");
    }

    public void SetDate(int day, int month)
    {
        data.currentDay = Mathf.Clamp(day, 1, 30);
        data.currentMonth = Mathf.Clamp(month, 1, 12);

        SaveProgress();
        Debug.Log($"[Progress] Fecha fijada: D�a {data.currentDay}, Mes {data.currentMonth}");
    }

    public void SaveProgress()
    {
        ProgressSaveSystem.Save(data);
    }
    public bool IsEncounterCompleted(string encounterId)
    {
        if (data.completedEncounters == null) return false;
        return data.completedEncounters.Contains(encounterId);
    }

    public void MarkEncounterCompleted(string encounterId)
    {
        if (data.completedEncounters == null)
            data.completedEncounters = new System.Collections.Generic.List<string>();

        if (!data.completedEncounters.Contains(encounterId))
        {
            data.completedEncounters.Add(encounterId);
            SaveProgress();
        }
    }

    public bool HasFlag(string flagName)
    {
        if (data.storyFlags == null) return false;
        return data.storyFlags.Contains(flagName);
    }

    public void SetFlag(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName)) return;

        if (data.storyFlags == null)
            data.storyFlags = new System.Collections.Generic.List<string>();

        if (!data.storyFlags.Contains(flagName))
        {
            data.storyFlags.Add(flagName);
            SaveProgress();
        }
    }

    public void RemoveFlag(string flagName)
    {
        if (string.IsNullOrWhiteSpace(flagName)) return;

        if (data.storyFlags != null && data.storyFlags.Contains(flagName))
        {
            data.storyFlags.Remove(flagName);
            SaveProgress();
        }
    }

    public void AddPermanentBoost(StatType stat, float value, StatModType modType = StatModType.Flat)
    {
        if (data.permanentBoosts == null)
            data.permanentBoosts = new System.Collections.Generic.List<StatModifier>();

        data.permanentBoosts.Add(new StatModifier { stat = stat, value = value, type = modType });
        SaveProgress();

        if (equipment != null)
        {
            var loadout = equipment.GetComponent<LoadoutStatsComponent>();
            if (loadout != null)
                loadout.Refresh();
        }
        
        Debug.Log($"[Progress] Permanent Boost Added: {stat} +{value} ({modType})");
    }
}

