using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [Header("References")]
    public ScoreManager scoreManager;
    public WinManager winManager;

    [Header("Bars (Superpuestas)")]
    public Slider baseBar;          // Puntos consolidados
    public Slider currentRoundBar;  // Base + progreso actual de la ronda

    [Header("Min Win Indicator")]
    public Slider minWinSlider;

    private int lastRoundScore = 0;
    private int basePoints = 0;

    // Evita que currentRoundBar siga recalculándose
    // después de consolidar la ronda anterior
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

        // Actualizamos ambas barras al mismo tiempo
        baseBar.value = basePoints;
        currentRoundBar.value = basePoints;

        // Bloqueamos la current para que no vuelva a sumarle
        // el score viejo de la ronda recién terminada
        lockCurrentRoundBar = true;

        lastRoundScore = 0;
    }

    public void PrepareNextRound()
    {
        // El score de la nueva ronda empieza desde 0,
        // pero visualmente la barra current arranca desde la base ya consolidada
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

    void UpdateMinWinSliderInstant()
    {
        if (minWinSlider == null || winManager == null) return;

        float targetValue = basePoints + (winManager.winPoints * winManager.minPointsFraction);
        targetValue = Mathf.Min(targetValue, winManager.winPoints);

        minWinSlider.value = targetValue;
    }
}