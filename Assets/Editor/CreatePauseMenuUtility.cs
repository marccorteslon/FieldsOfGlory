using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Events;
using UnityEngine.Events;

public class CreatePauseMenuUtility
{
    public static void CreateMenu()
    {
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No hay un Canvas en la escena. Asegúrate de estar en la escena de World.");
            return;
        }

        // Crear Panel Principal
        GameObject pausePanel = new GameObject("PauseMenuPanel", typeof(RectTransform), typeof(Image));
        pausePanel.transform.SetParent(canvas.transform, false);
        
        RectTransform rt = pausePanel.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        Image img = pausePanel.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.9f);

        // Añadir el script PauseMenuController
        PauseMenuController controller = pausePanel.AddComponent<PauseMenuController>();
        controller.pausePanelObject = pausePanel;

        CalendarPanelController calController = Object.FindFirstObjectByType<CalendarPanelController>(FindObjectsInactive.Include);
        if (calController != null)
        {
            controller.calendarController = calController;
        }

        // Botón Volver al Menú
        GameObject btnMenu = CreateButton("Btn_ReturnToMenu", "Volver al Menú Principal", pausePanel.transform, new Vector2(0, 100));
        Button bMenu = btnMenu.GetComponent<Button>();
        UnityAction actionMenu = new UnityAction(controller.ReturnToMainMenu);
        UnityEventTools.AddVoidPersistentListener(bMenu.onClick, actionMenu);

        // Botón Calendario
        GameObject btnCal = CreateButton("Btn_OpenCalendar", "Abrir Calendario", pausePanel.transform, new Vector2(0, -20));
        Button bCal = btnCal.GetComponent<Button>();
        UnityAction actionCal = new UnityAction(controller.OpenCalendar);
        UnityEventTools.AddVoidPersistentListener(bCal.onClick, actionCal);

        // Botón Reanudar
        GameObject btnResume = CreateButton("Btn_Resume", "Reanudar Partida", pausePanel.transform, new Vector2(0, -140));
        Button bResume = btnResume.GetComponent<Button>();
        UnityAction actionResume = new UnityAction(controller.TogglePauseMenu);
        UnityEventTools.AddVoidPersistentListener(bResume.onClick, actionResume);

        // Apagar el panel para que no moleste al principio
        pausePanel.SetActive(false);

        // Guardar escena
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        
        Selection.activeGameObject = pausePanel;
        Debug.Log("¡Menú de pausa instalado con éxito! No olvides darle a Guardar Escena (Ctrl+S).");
    }

    private static GameObject CreateButton(string name, string text, Transform parent, Vector2 anchoredPos)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(400, 80);
        rt.anchoredPosition = anchoredPos;

        GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        
        RectTransform textRt = textObj.GetComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 32;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.black;

        return btnObj;
    }
}
