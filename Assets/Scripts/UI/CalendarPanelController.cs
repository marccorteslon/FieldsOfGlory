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
            currentDateText.text = "Día " + progressManager.CurrentDay + " - Mes " + progressManager.CurrentMonth;

        if (tournamentsListText == null)
            return;

        var tournaments = tournamentManager.GetTournamentsForMonth(progressManager.CurrentMonth);

        if (tournaments.Count == 0)
        {
            tournamentsListText.text = "No hay torneos este mes.";
            return;
        }

        StringBuilder sb = new StringBuilder();

        foreach (var tournament in tournaments)
        {
            if (tournament == null) continue;

            string cityName = tournament.cityId; // Por defecto la ID
            
            if (GameManager.dataRepository != null)
            {
                GameManager.dataRepository.GetCityById(tournament.cityId, 
                    city => { cityName = city.displayName; },
                    err => { }
                );
            }

            sb.AppendLine(cityName + " - Día " + tournament.day + " - Mes " + progressManager.CurrentMonth);
        }

        tournamentsListText.text = sb.ToString();
    }
}
