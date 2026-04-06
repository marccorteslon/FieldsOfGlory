using UnityEngine;

public class JoustTutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject horseTutorialPanel;
    public GameObject attackTutorialPanel;
    public GameObject defenseTutorialPanel;

    private GameObject currentPanel;

    private const string TutorialEnabledKey = "JoustTutorialEnabled";

    void Awake()
    {
        HideAllTutorialPanelsImmediate();
        Time.timeScale = 1f;
    }

    public void EnableTutorial()
    {
        PlayerPrefs.SetInt(TutorialEnabledKey, 1);
        PlayerPrefs.Save();
    }

    public void DisableTutorial()
    {
        PlayerPrefs.SetInt(TutorialEnabledKey, 0);
        PlayerPrefs.Save();
    }

    public bool ShouldShowTutorial()
    {
        return PlayerPrefs.GetInt(TutorialEnabledKey, 0) == 1;
    }

    public bool IsTutorialOpen()
    {
        return currentPanel != null && currentPanel.activeSelf;
    }

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

        HideAllTutorialPanelsImmediate();

        currentPanel = panel;
        currentPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void CloseTutorial()
    {
        if (currentPanel != null)
            currentPanel.SetActive(false);

        currentPanel = null;
        Time.timeScale = 1f;
    }

    void HideAllTutorialPanelsImmediate()
    {
        if (horseTutorialPanel != null)
            horseTutorialPanel.SetActive(false);

        if (attackTutorialPanel != null)
            attackTutorialPanel.SetActive(false);

        if (defenseTutorialPanel != null)
            defenseTutorialPanel.SetActive(false);

        currentPanel = null;
    }

    void Update()
    {
        if (currentPanel == null || !currentPanel.activeSelf)
            return;

        bool closeWithController = Input.GetKeyDown(KeyCode.JoystickButton1);
        bool closeWithKeyboard = Input.GetKeyDown(KeyCode.X);

        if (closeWithController || closeWithKeyboard)
        {
            CloseTutorial();
        }
    }
}