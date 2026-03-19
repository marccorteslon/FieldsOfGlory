using UnityEngine;

public class TournamentAvailabilityButton : MonoBehaviour
{
    [Header("Tournament Data")]
    public string cityId;
    public int day = 1;
    public int month = 1;

    [Header("Refs")]
    public ProgressManager progressManager;
    public GameObject targetObject; 

    [Header("Options")]
    public bool requireCurrentCityMatch = true;

    void Awake()
    {
        if (progressManager == null)
            progressManager = FindObjectOfType<ProgressManager>();

        if (targetObject == null)
            targetObject = gameObject;
    }

    void OnEnable()
    {
        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (progressManager == null || targetObject == null)
            return;

        bool correctDate =
            progressManager.CurrentDay == day &&
            progressManager.CurrentMonth == month;

        bool correctCity = true;

        if (requireCurrentCityMatch)
            correctCity = progressManager.CurrentCityId == cityId;

        targetObject.SetActive(correctDate && correctCity);
    }
}