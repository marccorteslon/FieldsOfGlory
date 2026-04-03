using UnityEngine;

public class JoustTutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject horseTutorialPanel;
    public GameObject attackTutorialPanel;
    public GameObject defenseTutorialPanel;

    private GameObject currentPanel;

    private const string TutorialSeenKey = "JoustTutorialSeen";

    // ---------------- CONTROL ESTADO ----------------

    public bool ShouldShowTutorial()
    {
        return PlayerPrefs.GetInt(TutorialSeenKey, 0) == 0;
    }

    public void MarkTutorialAsSeen()
    {
        PlayerPrefs.SetInt(TutorialSeenKey, 1);
        PlayerPrefs.Save();
    }

    public void ResetTutorialProgress()
    {
        PlayerPrefs.SetInt(TutorialSeenKey, 0);
        PlayerPrefs.Save();
    }

    // Saber si hay un panel de tutorial abierto
    public bool IsTutorialOpen()
    {
        return currentPanel != null && currentPanel.activeSelf;
    }

    // Botón UI
    public void ReactivateTutorialFromUI()
    {
        ResetTutorialProgress();
    }

    // Opcional: activarlo y mostrarlo ya
    public void ReactivateAndShowNow()
    {
        ResetTutorialProgress();
        ShowHorseTutorial();
    }

    // ---------------- SHOW ----------------

    public void ShowHorseTutorial()
    {
        ShowPanel(horseTutorialPanel);
    }

    public void ShowAttackTutorial()
    {
        ShowPanel(attackTutorialPanel);
    }

    public void ShowDefenseTutorial()
    {
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
        // B (mando) o X (teclado)
        if (currentPanel != null &&
            (Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.X)))
        {
            CloseTutorial();
        }
    }
}