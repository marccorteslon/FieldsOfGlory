using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EncounterPopupController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject panelObject;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image encounterImageUI;

    [Header("Buttons")]
    public Button[] optionButtons;
    public TMP_Text[] optionButtonTexts;

    [Header("Stats")]
    public LoadoutStatsComponent loadout;

    [Header("Managers")]
    public ProgressManager progressManager;
    public WorldMapManager worldMapManager;
    public EquipmentManager equipmentManager;

    private RandomEncounterDefinition currentEncounter;
    private bool resultShown = false;
    private string rootEncounterId;
    private bool rootHasRequiredFlagOptions = false;
    private bool choseRequiredFlagOption = false;

    void Awake()
    {
        if (loadout == null)
            loadout = FindFirstObjectByType<LoadoutStatsComponent>();

        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (worldMapManager == null)
            worldMapManager = FindFirstObjectByType<WorldMapManager>();

        if (equipmentManager == null)
            equipmentManager = FindFirstObjectByType<EquipmentManager>();

        Close();
    }

    public void OpenEncounter(RandomEncounterDefinition encounter)
    {
        rootEncounterId = encounter != null ? encounter.encounterId : null;
        rootHasRequiredFlagOptions = false;
        choseRequiredFlagOption = false;
        OpenEncounterInternal(encounter);
    }

    private void OpenEncounterInternal(RandomEncounterDefinition encounter)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        currentEncounter = encounter;
        resultShown = false;

        if (encounter != null && encounter.options != null)
        {
            foreach (var opt in encounter.options)
            {
                if (!string.IsNullOrEmpty(opt.requiredFlag))
                {
                    rootHasRequiredFlagOptions = true;
                    break;
                }
            }
        }

        if (panelObject != null)
            panelObject.SetActive(true);

        if (titleText != null)
            titleText.text = encounter.title;

        if (descriptionText != null)
            descriptionText.text = encounter.description;

        if (encounterImageUI != null)
        {
            if (encounter.encounterImage != null)
            {
                encounterImageUI.sprite = encounter.encounterImage;
                encounterImageUI.gameObject.SetActive(true);
            }
            else
            {
                encounterImageUI.gameObject.SetActive(false);
            }
        }

        RefreshOptions();
    }

    void RefreshOptions()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        int buttonIndex = 0;

        if (currentEncounter != null && currentEncounter.options != null)
        {
            for (int i = 0; i < currentEncounter.options.Count; i++)
            {
                EncounterOptionDefinition option = currentEncounter.options[i];
                
                if (!string.IsNullOrWhiteSpace(option.requiredFlag) && progressManager != null && !progressManager.HasFlag(option.requiredFlag))
                    continue;

                if (!string.IsNullOrWhiteSpace(option.forbiddenFlag) && progressManager != null && progressManager.HasFlag(option.forbiddenFlag))
                    continue;

                if (buttonIndex < optionButtons.Length)
                {
                    optionButtons[buttonIndex].gameObject.SetActive(true);
                    
                    if (optionButtonTexts != null && buttonIndex < optionButtonTexts.Length)
                        optionButtonTexts[buttonIndex].text = option.optionText;

                    int capturedIndex = i;
                    optionButtons[buttonIndex].onClick.RemoveAllListeners();
                    optionButtons[buttonIndex].onClick.AddListener(() => ChooseOption(capturedIndex));
                    
                    buttonIndex++;
                }
            }
        }

        for (int i = buttonIndex; i < optionButtons.Length; i++)
        {
            optionButtons[i].gameObject.SetActive(false);
        }
    }

    void ChooseOption(int index)
    {
        if (resultShown) return;
        if (currentEncounter == null) return;
        if (index < 0 || index >= currentEncounter.options.Count) return;

        EncounterOptionDefinition option = currentEncounter.options[index];

        if (!string.IsNullOrEmpty(option.requiredFlag))
        {
            choseRequiredFlagOption = true;
            Debug.Log($"[Encounter] Player chose an option with required flag: {option.requiredFlag}");
        }

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

        ShowContinueOnly(option, success);
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

                case EncounterEffectType.AddItem:
                    if (effect.itemReward != null && equipmentManager != null && progressManager != null)
                    {
                        equipmentManager.Equip(effect.itemReward);
                        progressManager.SaveEquipped();
                        Debug.Log($"[Encounter] Acquired and equipped item: {effect.itemReward.displayName}");
                    }
                    break;
                
                case EncounterEffectType.SetFlag:
                    if (progressManager != null && !string.IsNullOrWhiteSpace(effect.flagName))
                    {
                        progressManager.SetFlag(effect.flagName);
                        Debug.Log($"[Encounter] Story Flag set: {effect.flagName}");
                    }
                    break;

                case EncounterEffectType.RemoveFlag:
                    if (progressManager != null && !string.IsNullOrWhiteSpace(effect.flagName))
                    {
                        progressManager.RemoveFlag(effect.flagName);
                        Debug.Log($"[Encounter] Story Flag removed: {effect.flagName}");
                    }
                    break;

                case EncounterEffectType.AddPermanentStat:
                    if (progressManager != null)
                    {
                        progressManager.AddPermanentBoost(effect.statToBoost, effect.statBoostValue, StatModType.Flat);
                    }
                    break;
            }
        }
    }

    void ShowContinueOnly(EncounterOptionDefinition option, bool success)
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool isContinue = i == 0;

            optionButtons[i].gameObject.SetActive(isContinue);
            optionButtons[i].onClick.RemoveAllListeners();

            if (!isContinue) continue;

            if (optionButtonTexts != null && i < optionButtonTexts.Length)
                optionButtonTexts[i].text = "Continue";

            optionButtons[i].onClick.AddListener(() => OnContinueClicked(option, success));
        }
    }

    void OnContinueClicked(EncounterOptionDefinition option, bool success)
    {
        RandomEncounterDefinition nextEvent = success ? option.nextEncounterOnSuccess : option.nextEncounterOnFailure;
        
        if (nextEvent != null)
        {
            OpenEncounterInternal(nextEvent);
        }
        else
        {
            if (progressManager != null && !string.IsNullOrEmpty(rootEncounterId))
            {
                bool shouldMarkCompleted = !rootHasRequiredFlagOptions || choseRequiredFlagOption;
                if (shouldMarkCompleted)
                {
                    progressManager.MarkEncounterCompleted(rootEncounterId);
                    Debug.Log($"[Encounter] Root encounter '{rootEncounterId}' marked as completed.");
                }
                else
                {
                    Debug.Log($"[Encounter] Root encounter '{rootEncounterId}' NOT marked as completed because the player did not complete the required flag option.");
                }
            }
            Close();
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

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}