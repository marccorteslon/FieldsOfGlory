using UnityEngine;
using UnityEngine.UI;

public class JoustTutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject horseTutorialPanel;
    public GameObject attackTutorialPanel;
    public GameObject defenseTutorialPanel;

    [Header("UI Toggle")]
    public Toggle tutorialToggle; // Asignar desde el inspector

    private GameObject currentPanel;

    private const string TutorialEnabledKey = "JoustTutorialEnabled";

    // ---------------- INIT ----------------

    void Start()
    {
        // Cargar estado del toggle (por defecto ON)
        bool enabled = PlayerPrefs.GetInt(TutorialEnabledKey, 1) == 1;

        if (tutorialToggle != null)
        {
            tutorialToggle.isOn = enabled;
            tutorialToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    // ---------------- TOGGLE ----------------

    void OnToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt(TutorialEnabledKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool ShouldShowTutorial()
    {
        return PlayerPrefs.GetInt(TutorialEnabledKey, 1) == 1;
    }

    // ---------------- CONTROL ----------------

    public bool IsTutorialOpen()
    {
        return currentPanel != null && currentPanel.activeSelf;
    }

    // ---------------- SHOW ----------------

    public void ShowHorseTutorial()
    {
        if (!ShouldShowTutorial()) return;
        ShowPanel(horseTutorialPanel);
    }

    public void ShowAttackTutorial()
    {
        if (!ShouldShowTutorial()) return;
        ShowPanel(attackTutorialPanel);
    }

    public void ShowDefenseTutorial()
    {
        if (!ShouldShowTutorial()) return;
        ShowPanel(defenseTutorialPanel);
    }

    void ShowPanel(GameObject panel)
    {
        if (panel == null) return;

        currentPanel = panel;
        panel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseTutorial()
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        currentPanel = null;
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (currentPanel != null &&
            (Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.X)))
        {
            CloseTutorial();
        }
    }
}