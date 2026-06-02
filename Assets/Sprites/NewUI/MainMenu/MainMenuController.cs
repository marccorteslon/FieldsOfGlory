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
    public float timeToHidePanels = 10f; 
    private float inactivityTimer = 0f;
    private Vector3 lastMousePosition;
    private GameObject lastSelectedObject;

    [Header("Controller Focus")]
    public GameObject firstSelectedMainMenu; 
    public GameObject firstSelectedSettingsTab; 
    public GameObject firstSelectedCredits;
    public GameObject firstSelectedControls;
    public GameObject firstSelectedSaveLoad;

    void Start()
    {
        if (soundSettingsPanel != null && soundSettingsPanel.GetComponent<AudioSettingsController>() == null)
        {
            soundSettingsPanel.AddComponent<AudioSettingsController>();
        }

        if (gameSettingsPanel != null && gameSettingsPanel.GetComponent<SensitivitySettingsController>() == null)
        {
            gameSettingsPanel.AddComponent<SensitivitySettingsController>();
        }

        CloseAllRightPanels();
        
        SetSelected(firstSelectedMainMenu);
    }

    void Update()
    {
        bool hasActivity = false;

        if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != lastSelectedObject)
        {
            hasActivity = true;
            lastSelectedObject = EventSystem.current.currentSelectedGameObject;
        }

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
        catch { } 

        if (hasActivity)
        {
            inactivityTimer = 0f; 
        }
        else
        {
            if ((settingsPanel != null && settingsPanel.activeSelf) ||
                (creditsPanel != null && creditsPanel.activeSelf) ||
                (controlsPanel != null && controlsPanel.activeSelf) ||
                (saveLoadPanel != null && saveLoadPanel.activeSelf))
            {
                inactivityTimer += Time.deltaTime;

                if (inactivityTimer >= timeToHidePanels)
                {
                    CloseAllRightPanels();
                    SetSelected(firstSelectedMainMenu); 
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

    public void CloseActivePanel()
    {
        CloseAllRightPanels();
        SetSelected(firstSelectedMainMenu);
    }

    private void SetSelected(GameObject obj)
    {
        if (obj != null)
        {
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }


    public void OpenSettings()
    {
        CloseAllRightPanels();
        if (settingsPanel) settingsPanel.SetActive(true);
        
        OpenGameSettings();
        
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


    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting Game...");
        Application.Quit();
    }

    public void DeleteSaveData()
    {
        ProgressSaveSystem.DeleteMainSave();
        
        // Si hay un ProgressManager en esta escena, lo reiniciamos
        ProgressManager progress = FindFirstObjectByType<ProgressManager>();
        if (progress != null)
        {
            progress.data = new ProgressSaveData
            {
                money = 0,
                equippedHorseId = "Farm_Horse",
                equippedLanceId = "Training_Lance",
                equippedShieldId = "Training_Shield",
                equippedArmorId = "Training_Armor",
                currentCityId = "city_senderopomar",
                currentNodeId = "node_chozapapa",
                currentDay = 1,
                currentMonth = 1
            };
            progress.SaveProgress();
        }
        
        Debug.Log("[MainMenuController] Progreso borrado satisfactoriamente.");
    }
}