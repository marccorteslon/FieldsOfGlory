using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class WorldPauseMenuController : MonoBehaviour
{
    [Header("Main Panels")]
    public GameObject bookUI;             
    public GameObject mainPausePage;      
    public GameObject calendarPage;       
    public GameObject settingsPage;       
    public GameObject exitPromptPanel;   

    [Header("Settings Sub-Panels")]
    public GameObject gameSettingsPanel;
    public GameObject videoSettingsPanel;
    public GameObject soundSettingsPanel;

    [Header("Calendar Reference")]
    public CalendarPanelController calendarController;

    [Header("Animation")]
    public Animator bookAnimator;
    [Tooltip("Bool for book open state (slides book in/out)")]
    public string isPausedAnimBool = "IsPaused";
    [Tooltip("Trigger for turning page left/right")]
    public string turnPageAnimTrigger = "TurnPage";

    [Header("Sprite Animation Settings")]
    [Tooltip("How long the sprite animation takes to finish turning the page")]
    public float pageTurnDuration = 0.5f;
    private bool isTurningPage = false;

    [Header("Controller Navigation First Selected")]
    public GameObject firstSelectedMain;
    public GameObject firstSelectedCalendar;
    public GameObject firstSelectedSettings;
    public GameObject firstSelectedExitPrompt;

    private bool isPaused = false;
    
    [Header("Scenes")]
    public string mainMenuSceneName = "MainMenu"; 

    private void Start()
    {
        if (soundSettingsPanel != null && soundSettingsPanel.GetComponent<AudioSettingsController>() == null)
        {
            soundSettingsPanel.AddComponent<AudioSettingsController>();
        }

        if (gameSettingsPanel != null && gameSettingsPanel.GetComponent<SensitivitySettingsController>() == null)
        {
            gameSettingsPanel.AddComponent<SensitivitySettingsController>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (!isPaused)
            {
                PauseGame();
            }
            else
            {
                HandleBackNavigation();
            }
        }
    }

    private void HandleBackNavigation()
    {
        if (exitPromptPanel != null && exitPromptPanel.activeSelf)
        {
            CloseExitPrompt();
        }
        else if (calendarPage != null && calendarPage.activeSelf)
        {
            CloseCalendar();
        }
        else if (settingsPage != null && settingsPage.activeSelf)
        {
            CloseSettings();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        if (bookUI != null) bookUI.SetActive(true);

        if (mainPausePage != null) mainPausePage.SetActive(true);
        if (calendarPage != null) calendarPage.SetActive(false);
        if (settingsPage != null) settingsPage.SetActive(false);
        if (exitPromptPanel != null) exitPromptPanel.SetActive(false);

        if (bookAnimator != null)
        {
            bookAnimator.SetBool(isPausedAnimBool, true);
        }

        SetSelected(firstSelectedMain);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (bookAnimator != null)
        {
            bookAnimator.SetBool(isPausedAnimBool, false);
        }
        
    }

    public void OpenCalendar()
    {
        if (isTurningPage) return;
        StartCoroutine(TurnPageRoutine(mainPausePage, calendarPage, () => 
        {
            if (calendarController != null)
            {
                calendarController.OpenCalendar();
            }
            SetSelected(firstSelectedCalendar);
        }));
    }

    public void CloseCalendar()
    {
        if (isTurningPage) return;
        StartCoroutine(TurnPageRoutine(calendarPage, mainPausePage, () => 
        {
            SetSelected(firstSelectedMain);
        }));
    }

    public void OpenSettings()
    {
        if (isTurningPage) return;
        StartCoroutine(TurnPageRoutine(mainPausePage, settingsPage, () => 
        {
            OpenGameSettings();
            SetSelected(firstSelectedSettings);
        }));
    }

    public void CloseSettings()
    {
        if (isTurningPage) return;
        StartCoroutine(TurnPageRoutine(settingsPage, mainPausePage, () => 
        {
            SetSelected(firstSelectedMain);
        }));
    }

    private System.Collections.IEnumerator TurnPageRoutine(GameObject pageToHide, GameObject pageToShow, System.Action onComplete)
    {
        isTurningPage = true;

        if (pageToHide != null) pageToHide.SetActive(false);

        if (bookAnimator != null) bookAnimator.SetTrigger(turnPageAnimTrigger);

        yield return new WaitForSecondsRealtime(pageTurnDuration);

        if (pageToShow != null) pageToShow.SetActive(true);

        onComplete?.Invoke();

        isTurningPage = false;
    }

    public void OpenGameSettings()
    {
        if (gameSettingsPanel != null) gameSettingsPanel.SetActive(true);
        if (videoSettingsPanel != null) videoSettingsPanel.SetActive(false);
        if (soundSettingsPanel != null) soundSettingsPanel.SetActive(false);
    }

    public void OpenVideoSettings()
    {
        if (gameSettingsPanel != null) gameSettingsPanel.SetActive(false);
        if (videoSettingsPanel != null) videoSettingsPanel.SetActive(true);
        if (soundSettingsPanel != null) soundSettingsPanel.SetActive(false);
    }

    public void OpenSoundSettings()
    {
        if (gameSettingsPanel != null) gameSettingsPanel.SetActive(false);
        if (videoSettingsPanel != null) videoSettingsPanel.SetActive(false);
        if (soundSettingsPanel != null) soundSettingsPanel.SetActive(true);
    }

    public void OpenExitPrompt()
    {
        if (exitPromptPanel != null) exitPromptPanel.SetActive(true);
        SetSelected(firstSelectedExitPrompt);
    }

    public void CloseExitPrompt()
    {
        if (exitPromptPanel != null) exitPromptPanel.SetActive(false);
        SetSelected(firstSelectedMain);
    }

    public void ConfirmExit()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void SetSelected(GameObject obj)
    {
        if (obj != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }
}
