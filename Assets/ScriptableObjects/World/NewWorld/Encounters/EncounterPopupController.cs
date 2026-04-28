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

    private RandomEncounterDefinition currentEncounter;
    private bool resultShown = false;

    void Awake()
    {
        if (loadout == null)
            loadout = FindFirstObjectByType<LoadoutStatsComponent>();

        Close();
    }

    public void OpenEncounter(RandomEncounterDefinition encounter)
    {
        currentEncounter = encounter;
        resultShown = false;

        panelObject.SetActive(true);

        titleText.text = encounter.title;
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
            var option = currentEncounter.options[i];

            optionButtonTexts[i].text = option.optionText;

            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => ChooseOption(index));
        }
    }

    void ChooseOption(int index)
    {
        if (resultShown) return;

        var option = currentEncounter.options[index];

        int stat = GetStat(option.statToCheck);
        int roll = Random.Range(1, 21);
        int total = roll + stat;

        bool success = total >= option.difficulty;

        descriptionText.text = success
            ? option.successText
            : option.failureText;

        Debug.Log($"[Encounter] {option.statToCheck}: {roll}+{stat} vs {option.difficulty}");

        ShowContinueOnly();
        resultShown = true;
    }

    void ShowContinueOnly()
    {
        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool isContinue = i == 0;

            optionButtons[i].gameObject.SetActive(isContinue);
            optionButtons[i].onClick.RemoveAllListeners();

            if (!isContinue) continue;

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

        panelObject.SetActive(false);
    }
}