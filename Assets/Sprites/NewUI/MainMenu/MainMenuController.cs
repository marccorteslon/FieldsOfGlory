using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("Right-Side Panels")]
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject controlsPanel;
    public GameObject saveLoadPanel;

    [Header("Settings Sub-Panels")]
    public GameObject gameSettingsPanel;
    public GameObject videoSettingsPanel;
    public GameObject soundSettingsPanel;

    [Header("Inactivity Settings")]
    public float timeToHidePanels = 10f; // Seconds before closing right panels
    private float inactivityTimer = 0f;
    private Vector3 lastMousePosition;
    private GameObject lastSelectedObject;

    [Header("Controller Focus")]
    public GameObject firstSelectedMainMenu; 
    public GameObject firstSelectedSettingsTab; // e.g., the Game tab button
    public GameObject firstSelectedCredits;
    public GameObject firstSelectedControls;
    public GameObject firstSelectedSaveLoad;

    void Start()
    {
        // Start with the right side completely empty
        CloseAllRightPanels();
        
        // Ensure the left menu is focused so D-pad works instantly
        SetSelected(firstSelectedMainMenu);
    }

    void Update()
    {
        bool hasActivity = false;

        // 1. Check if the controller/keyboard navigated to a new button
        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != lastSelectedObject)
        {
            hasActivity = true;
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }

        // 2. Safely check for Mouse/Keyboard input (wrapped in try/catch in case you use the New Input System exclusively)
        try
        {
            if (Input.mousePosition != lastMousePosition)
            {
                hasActivity = true;
                lastMousePosition = Input.mousePosition;
            }

            if (Input.anyKey)
            {
                hasActivity = true;
            }
        }
        catch { } // Ignore errors if old Input system is disabled

        // 3. Handle the Timer
        if (hasActivity)
        {
            inactivityTimer = 0f; // Reset the timer immediately
        }
        else
        {
            // Only count down if at least one right-side panel is currently open
            if ((settingsPanel != null && settingsPanel.activeSelf) ||
                (creditsPanel != null && creditsPanel.activeSelf) ||
                (controlsPanel != null && controlsPanel.activeSelf) ||
                (saveLoadPanel != null && saveLoadPanel.activeSelf))
            {
                inactivityTimer += Time.deltaTime;

                if (inactivityTimer >= timeToHidePanels)
                {
                    CloseAllRightPanels();
                    SetSelected(firstSelectedMainMenu); // Snap controller back to the left menu
                    inactivityTimer = 0f;
                }
            }
        }
    }

    private void CloseAllRightPanels()
    {
        if (settingsPanel) settingsPanel.SetActive(false);
        if (creditsPanel) creditsPanel.SetActive(false);
        if (controlsPanel) controlsPanel.SetActive(false);
        if (saveLoadPanel) saveLoadPanel.SetActive(false);
    }

    private void SetSelected(GameObject obj)
    {
        if (obj != null)
        {
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }

    // --- Left Menu Button Actions ---

    public void OpenSettings()
    {
        CloseAllRightPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
        
        // Automatically open the Game settings tab by default when opening Settings
        OpenGameSettings();
        
        // Move controller focus to the right side (e.g. the Game/Video/Sound tabs)
        SetSelected(firstSelectedSettingsTab);
    }

    public void OpenCredits()
    {
        CloseAllRightPanels();
        if (creditsPanel) creditsPanel.SetActive(true);
        SetSelected(firstSelectedCredits);
    }

    public void OpenControls()
    {
        CloseAllRightPanels();
        if (controlsPanel) controlsPanel.SetActive(true);
        SetSelected(firstSelectedControls);
    }

    public void OpenSaveLoad()
    {
        CloseAllRightPanels();
        if (saveLoadPanel) saveLoadPanel.SetActive(true);
        SetSelected(firstSelectedSaveLoad);
    }

    // --- Settings Sub-Tabs Actions ---

    public void OpenGameSettings()
    {
        if (videoSettingsPanel) videoSettingsPanel.SetActive(false);
        if (soundSettingsPanel) soundSettingsPanel.SetActive(false);
        if (gameSettingsPanel) gameSettingsPanel.SetActive(true);
    }

    public void OpenVideoSettings()
    {
        if (gameSettingsPanel) gameSettingsPanel.SetActive(false);
        if (soundSettingsPanel) soundSettingsPanel.SetActive(false);
        if (videoSettingsPanel) videoSettingsPanel.SetActive(true);
    }

    public void OpenSoundSettings()
    {
        if (gameSettingsPanel) gameSettingsPanel.SetActive(false);
        if (videoSettingsPanel) videoSettingsPanel.SetActive(false);
        if (soundSettingsPanel) soundSettingsPanel.SetActive(true);
    }

    // --- Scene & Application Methods ---

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }
}
