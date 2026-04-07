using System.Text;
using TMPro;
using UnityEngine;

public class CalendarPanelController : MonoBehaviour
{
    public ProgressManager progressManager;
    public TournamentManager tournamentManager;

    public TMP_Text currentDateText;
    public TMP_Text tournamentsListText;
    public GameObject panelObject;

    void Awake()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (tournamentManager == null)
            tournamentManager = FindFirstObjectByType<TournamentManager>();
    }

    public void OpenCalendar()
    {
        if (panelObject != null)
            panelObject.SetActive(true);

        RefreshCalendar();
    }

    public void CloseCalendar()
    {
        if (panelObject != null)
            panelObject.SetActive(false);
    }

    public void RefreshCalendar()
    {
        if (progressManager == null || tournamentManager == null)
            return;

        if (currentDateText != null)
            currentDateText.text = $"Día {progressManager.CurrentDay} - Mes {progressManager.CurrentMonth}";

        if (tournamentsListText == null)
            return;

        var tournaments = tournamentManager.GetTournamentsForMonth(progressManager.CurrentMonth);

        if (tournaments.Count == 0)
        {
            tournamentsListText.text = "No hay torneos este mes.";
            return;
        }

        StringBuilder sb = new();

        foreach (var tournament in tournaments)
        {
            if (tournament == null) continue;

            sb.AppendLine($"{tournament.displayName} - {tournament.cityId} - Día {tournament.day}");
        }

        tournamentsListText.text = sb.ToString();
    }
}