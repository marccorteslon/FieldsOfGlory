using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menu References")]
    public GameObject pausePanelObject;
    public CalendarPanelController calendarController;

    [Header("Input")]
    public KeyCode keyboardPauseKey = KeyCode.Escape;
    public KeyCode joystickPauseKey = KeyCode.JoystickButton7; // Botón Start/Menu

    public static bool IsPaused { get; private set; }

    void Start()
    {
        if (pausePanelObject != null)
        {
            pausePanelObject.SetActive(false);
        }
        IsPaused = false;
        Time.timeScale = 1f;
    }

    void Update()
    {
        // Detectar botón de pausa
        if (Input.GetKeyDown(keyboardPauseKey) || Input.GetKeyDown(joystickPauseKey))
        {
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (pausePanelObject == null)
            return;

        bool isActivating = !pausePanelObject.activeSelf;
        pausePanelObject.SetActive(isActivating);
        IsPaused = isActivating;

        if (isActivating)
        {
            // Pausar tiempo del juego
            Time.timeScale = 0f;
        }
        else
        {
            // Reanudar tiempo del juego
            Time.timeScale = 1f;

            // Asegurarnos de que el calendario se cierra si cerramos el menú de pausa
            if (calendarController != null && calendarController.panelObject != null && calendarController.panelObject.activeSelf)
            {
                calendarController.CloseCalendar();
            }
        }
    }

    public void OpenCalendar()
    {
        if (calendarController != null)
        {
            calendarController.OpenCalendar();
        }
        else
        {
            Debug.LogWarning("PauseMenuController: No hay CalendarPanelController asignado.");
        }
    }

    public void ReturnToMainMenu()
    {
        // Asegurarnos de restaurar el tiempo antes de cambiar de escena
        Time.timeScale = 1f;
        IsPaused = false;
        
        SceneManager.LoadScene("MainMenu");
    }
}
