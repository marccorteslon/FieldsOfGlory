using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [Header("References")]
    public ScoreManager scoreManager;
    public WinManager winManager;

    [Header("Bars (Superpuestas)")]
    public Slider baseBar;          // Color Y (rondas anteriores)
    public Slider currentRoundBar;  // Color X (ronda actual)

    [Header("Min Win Indicator")]
    public Slider minWinSlider;     // Barra Slider para mostrar mínimo a alcanzar

    [Header("Animation")]
    public float minWinAnimSpeed = 2f; // velocidad de animación

    private int lastRoundScore = 0;
    private int basePoints = 0;     // puntos consolidados de rondas anteriores

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
        AnimateMinWinSlider();
    }

    void UpdateCurrentRoundProgress()
    {
        int currentScore = scoreManager.GetScore();

        if (currentScore == lastRoundScore) return;

        lastRoundScore = currentScore;

        currentRoundBar.value = basePoints + currentScore;
    }

    public void ConsolidateRound()
    {
        basePoints += lastRoundScore;

        baseBar.value = basePoints;
        currentRoundBar.value = basePoints;

        lastRoundScore = 0;
    }

    public void ResetAll()
    {
        basePoints = 0;
        lastRoundScore = 0;

        baseBar.value = 0;
        currentRoundBar.value = 0;

        if (minWinSlider != null)
            minWinSlider.value = 0;
    }

    void AnimateMinWinSlider()
    {
        if (minWinSlider == null || winManager == null) return;

        // mínimo a lograr = 1/3 de winPoints
        float minFraction = winManager.minPointsFraction;

        // progreso visual = basePoints + 1/3 de winPoints
        float targetValue = basePoints + winManager.winPoints * minFraction;

        // no exceder máximo
        targetValue = Mathf.Min(targetValue, winManager.winPoints);

        // animación suave
        minWinSlider.value = Mathf.Lerp(minWinSlider.value, targetValue, Time.deltaTime * minWinAnimSpeed);
    }
}