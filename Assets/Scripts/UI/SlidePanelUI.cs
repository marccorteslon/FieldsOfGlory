using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SlidePanelUI : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;

    [Tooltip("Pestaña visible")]
    public RectTransform visibleHandle;

    public float slideDuration = 0.25f;

    [Header("Calendar")]
    public CalendarPanelController calendarPanelController;

    [Header("UI Button")]
    public Button toggleButton;
    public TMP_Text buttonText;

    private bool isActive = false;
    private Coroutine slideCoroutine;

    private float hiddenX;
    private float visibleX;

    void Start()
    {
        if (calendarPanelController == null)
            calendarPanelController = FindFirstObjectByType<CalendarPanelController>();

        CalculatePositions();

        SetPanelPosition(false);

        if (toggleButton != null)
            toggleButton.onClick.AddListener(TogglePanel);

        UpdateButtonText();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            TogglePanel();
    }

    void CalculatePositions()
    {
        visibleX = panel.rect.width * panel.pivot.x;

        Vector3[] corners = new Vector3[4];
        visibleHandle.GetWorldCorners(corners);

        Vector3 rightWorld = corners[2];
        Vector3 panelWorld = panel.position;

        float difference = rightWorld.x - panelWorld.x;
        hiddenX = -difference;
    }

    public void TogglePanel()
    {
        isActive = !isActive;

        if (isActive && calendarPanelController != null)
            calendarPanelController.RefreshCalendar();

        if (slideCoroutine != null)
            StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlidePanel(isActive));
        UpdateButtonText();
    }

    IEnumerator SlidePanel(bool show)
    {
        Vector2 startPos = panel.anchoredPosition;
        Vector2 targetPos = new Vector2(show ? visibleX : hiddenX, startPos.y);

        float time = 0f;

        while (time < slideDuration)
        {
            time += Time.unscaledDeltaTime;
            float t = time / slideDuration;

            panel.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            yield return null;
        }

        panel.anchoredPosition = targetPos;
    }

    void SetPanelPosition(bool show)
    {
        Vector2 pos = panel.anchoredPosition;
        pos.x = show ? visibleX : hiddenX;
        panel.anchoredPosition = pos;
    }

    void UpdateButtonText()
    {
        if (buttonText != null)
            buttonText.text = isActive ? "<" : ">";
    }
}