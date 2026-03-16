using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [Header("Core Refs")]
    public ItemDatabase itemDatabase;
    public ProgressManager progress;
    public EquipmentManager equipment;

    [Header("Navigation")]
    public GameObject shopPanelObject;

    [Header("Money UI")]
    public TMP_Text moneyText;

    [Header("Shop Items (4) - IDs")]
    public string[] itemIds = new string[4];

    [System.Serializable]
    public class ShopSlotUI
    {
        public TMP_Text nameText;
        public TMP_Text modifiersText;
        public Image iconImage;
        public Button purchaseButton;
        public TMP_Text priceText;
    }

    [Header("UI Slots (4)")]
    public ShopSlotUI[] slots = new ShopSlotUI[4];

    private EquipmentDefinition[] resolved = new EquipmentDefinition[4];
    private GameObject currentTownPanel;

    void Awake()
    {
        if (progress == null) progress = FindObjectOfType<ProgressManager>();
        if (equipment == null) equipment = FindObjectOfType<EquipmentManager>();
    }

    void Start()
    {
        RefreshMoneyUI();
        RefreshShopUI();
        HookButtons();
    }

    public void SetOriginTownPanel(GameObject townPanel)
    {
        currentTownPanel = townPanel;
    }

    public void ExitShop()
    {
        if (shopPanelObject != null)
            shopPanelObject.SetActive(false);

        if (currentTownPanel != null)
            currentTownPanel.SetActive(true);
    }

    public void RefreshMoneyUI()
    {
        if (moneyText != null && progress != null)
            moneyText.text = progress.Money.ToString();
    }

    public void RefreshShopUI()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("[Shop] ItemDatabase no asignada.");
            return;
        }

        itemDatabase.BuildLookup();

        for (int i = 0; i < 4; i++)
        {
            var ui = slots[i];
            var id = (itemIds != null && i < itemIds.Length) ? itemIds[i] : null;

            EquipmentDefinition item = null;
            if (!string.IsNullOrEmpty(id))
                item = itemDatabase.GetById(id);

            resolved[i] = item;

            if (item == null)
            {
                if (ui.nameText) ui.nameText.text = "(Empty)";
                if (ui.modifiersText) ui.modifiersText.text = "";
                if (ui.iconImage)
                {
                    ui.iconImage.sprite = null;
                    ui.iconImage.enabled = false;
                }
                if (ui.priceText) ui.priceText.text = "";
                if (ui.purchaseButton) ui.purchaseButton.interactable = false;
                continue;
            }

            if (ui.nameText) ui.nameText.text = item.displayName;

            if (ui.iconImage)
            {
                ui.iconImage.sprite = item.icon;
                ui.iconImage.enabled = item.icon != null;
            }

            if (ui.priceText) ui.priceText.text = item.price.ToString();

            if (ui.modifiersText)
                ui.modifiersText.text = FormatModifiers(item);

            if (ui.purchaseButton)
                ui.purchaseButton.interactable = true;
        }
    }

    void HookButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            int index = i;
            var btn = slots[i].purchaseButton;
            if (btn == null) continue;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => Purchase(index));
        }
    }

    public void Purchase(int index)
    {
        if (index < 0 || index >= 4) return;

        var item = resolved[index];
        if (item == null)
        {
            Debug.LogWarning("[Shop] No hay item en ese slot.");
            return;
        }

        if (progress == null || equipment == null)
        {
            Debug.LogError("[Shop] Falta ProgressManager o EquipmentManager.");
            return;
        }

        int cost = item.price;

        if (!progress.TrySpendMoney(cost))
        {
            Debug.Log("[Shop] No tienes dinero suficiente.");
            RefreshMoneyUI();
            return;
        }

        equipment.Equip(item);
        progress.SaveEquipped();
        RefreshMoneyUI();

        Debug.Log($"[Shop] Comprado y equipado: {item.displayName} ({item.id}) por {cost}.");
    }

    string FormatModifiers(EquipmentDefinition item)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(item.description))
        {
            sb.AppendLine(item.description.Trim());
            sb.AppendLine();
        }

        if (item.modifiers == null || item.modifiers.Count == 0)
        {
            sb.Append("Sin modificadores.");
            return sb.ToString();
        }

        foreach (var mod in item.modifiers)
        {
            string sign = mod.value >= 0 ? "+" : "";
            string val = mod.value.ToString("0.##");
            string type = mod.type == StatModType.Percent ? "%" : "";
            sb.AppendLine($"{sign}{val}{type} {mod.stat}");
        }

        return sb.ToString();
    }
}