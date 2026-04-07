using UnityEngine;

public class TournamentAvailabilityButton : MonoBehaviour
{
    public TournamentManager tournamentManager;
    public GameObject targetObject;

    void Awake()
    {
        if (tournamentManager == null)
            tournamentManager = FindFirstObjectByType<TournamentManager>();

        if (targetObject == null)
            targetObject = gameObject;
    }

    void OnEnable()
    {
        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (tournamentManager == null || targetObject == null)
            return;

        bool shouldShow = tournamentManager.HasTournamentInCurrentCityToday();
        targetObject.SetActive(shouldShow);
    }
}