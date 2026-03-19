using UnityEngine;

public class JoustTutorialManager : MonoBehaviour
{
    [Header("Tutorial Panels")]
    public GameObject horseTutorialPanel;
    public GameObject attackTutorialPanel;
    public GameObject defenseTutorialPanel;

    private GameObject currentPanel;

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

        Time.timeScale = 1f;
    }

    void Update()
    {
        // Botón B mando Xbox o tecla X teclado
        if (currentPanel != null &&
            (Input.GetKeyDown(KeyCode.JoystickButton1) || Input.GetKeyDown(KeyCode.X)))
        {
            CloseTutorial();
        }
    }
}