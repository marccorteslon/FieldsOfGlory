using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterPopupController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject panelObject;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    [Header("Buttons")]
    public Button[] optionButtons;
    public TMP_Text[] optionButtonTexts;

    [Header("Stats")]
    public LoadoutStatsComponent loadout;

    [Header("Managers")]
    public ProgressManager progressManager;
    public WorldMapManager worldMapManager;

    private RandomEncounterDefinition currentEncounter;
    private bool resultShown = false;

    void Awake()
    {
        if (loadout == null)
            loadout = FindFirstObjectByType<LoadoutStatsComponent>();

        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (worldMapManager == null)
            worldMapManager = FindFirstObjectByType<WorldMapManager>();

        Close();
    }

    public void OpenEncounter(RandomEncounterDefinition encounter)
    {
        currentEncounter = encounter;
        resultShown = false;

        if (panelObject != null)
            panelObject.SetActive(true);

        if (titleText != null)
            titleText.text = encounter.title;

        if (descriptionText != null)
            descriptionText.text = encounter.description;

        RefreshOptions();
    }

    void RefreshOptions()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool hasOption = currentEncounter != null && i < currentEncounter.options.Count;

            optionButtons[i].gameObject.SetActive(hasOption);

            if (!hasOption) continue;

            int index = i;
            EncounterOptionDefinition option = currentEncounter.options[i];

            if (optionButtonTexts != null && i < optionButtonTexts.Length)
                optionButtonTexts[i].text = option.optionText;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => ChooseOption(index));
        }
    }

    void ChooseOption(int index)
    {
        if (resultShown) return;
        if (currentEncounter == null) return;
        if (index < 0 || index >= currentEncounter.options.Count) return;

        EncounterOptionDefinition option = currentEncounter.options[index];

        int statValue = GetStat(option.statToCheck);
        int roll = Random.Range(1, 21);
        int total = roll + statValue;

        bool success = total >= option.difficulty;

        if (descriptionText != null)
        {
            descriptionText.text = success
                ? option.successText
                : option.failureText;
        }

        ApplyEffects(success ? option.successEffects : option.failureEffects);

        Debug.Log($"[Encounter] {option.statToCheck}: {roll}+{statValue} = {total} vs {option.difficulty} ? {(success ? "SUCCESS" : "FAIL")}");

        ShowContinueOnly();
        resultShown = true;
    }

    void ApplyEffects(EncounterEffect[] effects)
    {
        if (effects == null) return;

        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (worldMapManager == null)
            worldMapManager = FindFirstObjectByType<WorldMapManager>();

        foreach (var effect in effects)
        {
            if (effect == null) continue;

            switch (effect.type)
            {
                case EncounterEffectType.AddMoney:
                    if (progressManager != null)
                        progressManager.AddMoney(effect.value);
                    break;

                case EncounterEffectType.LoseMoney:
                    if (progressManager != null)
                        progressManager.TrySpendMoney(effect.value);
                    break;

                case EncounterEffectType.AddDays:
                    if (progressManager != null)
                        progressManager.AdvanceDays(effect.value);
                    break;

                case EncounterEffectType.MoveToNode:
                    if (!string.IsNullOrWhiteSpace(effect.targetNodeId))
                    {
                        if (progressManager != null)
                            progressManager.SetCurrentNode(effect.targetNodeId);

                        if (worldMapManager != null)
                            worldMapManager.ForceMoveToNode(effect.targetNodeId);
                    }
                    break;
            }
        }
    }

    void ShowContinueOnly()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool isContinue = i == 0;

            optionButtons[i].gameObject.SetActive(isContinue);
            optionButtons[i].onClick.RemoveAllListeners();

            if (!isContinue) continue;

            if (optionButtonTexts != null && i < optionButtonTexts.Length)
                optionButtonTexts[i].text = "Continue";

            optionButtons[i].onClick.AddListener(Close);
        }
    }

    int GetStat(StatType statType)
    {
        if (loadout == null) return 0;
        return Mathf.RoundToInt(loadout.stats.Get(statType));
    }

    public void Close()
    {
        resultShown = false;
        currentEncounter = null;

        if (panelObject != null)
            panelObject.SetActive(false);
    }
}