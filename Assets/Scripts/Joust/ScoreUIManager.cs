using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [Header("References")]
    public ScoreManager scoreManager;
    public WinManager winManager;

    [Header("Bars (Superpuestas)")]

    // La barra general de progreso
    public Slider baseBar;   
    
    // El progreso específico de la ronda
    public Slider currentRoundBar;  

    [Header("Min Win Indicator")]
    public Slider minWinSlider;

    private int lastRoundScore = 0;
    private int basePoints = 0;

    private bool lockCurrentRoundBar = false;

    void Start()
    {
        baseBar.maxValue = winManager.winPoints;
        currentRoundBar.maxValue = winManager.winPoints;

        baseBar.value = 0;
        currentRoundBar.value = 0;

        if (minWinSlider != null)
        {
            minWinSlider.maxValue = winManager.winPoints;
            minWinSlider.value = 0;
        }
    }

    void Update()
    {
        UpdateCurrentRoundProgress();
        UpdateMinWinSliderInstant();
    }

    void UpdateCurrentRoundProgress()
    {
        if (lockCurrentRoundBar)
            return;

        int currentScore = scoreManager.GetScore();

        if (currentScore == lastRoundScore)
            return;

        lastRoundScore = currentScore;

        currentRoundBar.value = Mathf.Min(basePoints + currentScore, winManager.winPoints);
    }

    public void ConsolidateRound()
    {
        basePoints += lastRoundScore;
        basePoints = Mathf.Min(basePoints, winManager.winPoints);

        baseBar.value = basePoints;
        currentRoundBar.value = basePoints;

        lockCurrentRoundBar = true;

        lastRoundScore = 0;
    }

    public void PrepareNextRound()
    {
        // El score de la nueva ronda empieza desde 0, pero visualmente la barra current arranca desde la base ya consolidada
        lastRoundScore = 0;
        currentRoundBar.value = basePoints;
        lockCurrentRoundBar = false;
    }

    public void ResetAll()
    {
        basePoints = 0;
        lastRoundScore = 0;
        lockCurrentRoundBar = false;

        baseBar.value = 0;
        currentRoundBar.value = 0;

        if (minWinSlider != null)
            minWinSlider.value = 0;
    }

    void UpdateMinWinSliderInstant() // Actualizar el minimo de ronda para ganar
    {
        if (minWinSlider == null || winManager == null) return;

        float targetValue = basePoints + (winManager.winPoints * winManager.minPointsFraction);
        targetValue = Mathf.Min(targetValue, winManager.winPoints);

        minWinSlider.value = targetValue;
    }
}