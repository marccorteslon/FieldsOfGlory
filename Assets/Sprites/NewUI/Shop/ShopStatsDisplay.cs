using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ShopStatsDisplay : MonoBehaviour
{
    [Header("Data Source")]
    public LoadoutStatsComponent loadoutStats;
    public EquipmentManager equipmentManager;

    [System.Serializable]
    public class StatUIDisplay
    {
        public StatType statType;
        public TMP_Text nameText;       
        public TMP_Text fullNameText;   
        public TMP_Text valueText;      
        public string customFullName;   
    }

    [Header("UI Grid Setup")]
    public List<StatUIDisplay> statDisplays;

    void Awake()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged += OnEquipmentChanged;
        }
    }

    void OnDestroy()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= OnEquipmentChanged;
        }
    }

    void OnEnable()
    {
        UpdateStatsUI();
    }

    private void OnEquipmentChanged()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(UpdateStatsNextFrame());
        }
    }

    private System.Collections.IEnumerator UpdateStatsNextFrame()
    {
        yield return null; 
        UpdateStatsUI();
    }

    public void UpdateStatsUI()
    {
        if (loadoutStats == null) return;

        foreach (var display in statDisplays)
        {
            if (display.nameText != null)
            {
                display.nameText.text = display.statType.ToString();
            }

            if (display.fullNameText != null)
            {
                display.fullNameText.text = display.customFullName;
            }

            if (display.valueText != null)
            {
                float val = loadoutStats.stats.Get(display.statType);
                display.valueText.text = val.ToString("0"); 
            }
        }
    }
}

