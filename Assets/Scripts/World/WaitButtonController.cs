using UnityEngine;

public class WaitButtonController : MonoBehaviour
{
    [Header("Refs")]
    public ProgressManager progressManager;
    public CalendarPanelController calendarPanelController;
    public TournamentAvailabilityButton tournamentAvailabilityButton;

    void Awake()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (calendarPanelController == null)
            calendarPanelController = FindFirstObjectByType<CalendarPanelController>();
    }

    public void WaitOneDay()
    {
        if (progressManager == null)
        {
            Debug.LogError("WaitButtonController: no hay ProgressManager.");
            return;
        }

        progressManager.AdvanceDays(1);

        if (calendarPanelController != null)
            calendarPanelController.RefreshCalendar();

        if (tournamentAvailabilityButton != null)
            tournamentAvailabilityButton.RefreshVisibility();

        Debug.Log("Has esperado 1 día.");
    }
}