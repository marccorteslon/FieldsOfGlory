using System.Collections.Generic;
using UnityEngine;

public class TournamentManager : MonoBehaviour
{
    public TournamentDatabase tournamentDatabase;
    public ProgressManager progressManager;

    void Awake()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();
    }

    public List<TournamentDefinition> GetAllTournaments()
    {
        if (tournamentDatabase == null)
            return new List<TournamentDefinition>();

        return tournamentDatabase.tournaments;
    }

    public List<TournamentDefinition> GetTournamentsForMonth(int month)
    {
        List<TournamentDefinition> result = new();

        if (tournamentDatabase == null)
            return result;

        foreach (var tournament in tournamentDatabase.tournaments)
        {
            if (tournament == null) continue;
            if (tournament.month == month)
                result.Add(tournament);
        }

        return result;
    }

    public TournamentDefinition GetTournamentForCityAndDate(string cityId, int day, int month)
    {
        if (tournamentDatabase == null)
            return null;

        foreach (var tournament in tournamentDatabase.tournaments)
        {
            if (tournament == null) continue;

            if (tournament.cityId == cityId &&
                tournament.day == day &&
                tournament.month == month)
            {
                return tournament;
            }
        }

        return null;
    }

    public bool HasTournamentInCurrentCityToday()
    {
        if (progressManager == null)
            return false;

        return GetTournamentForCityAndDate(
            progressManager.CurrentCityId,
            progressManager.CurrentDay,
            progressManager.CurrentMonth
        ) != null;
    }
}