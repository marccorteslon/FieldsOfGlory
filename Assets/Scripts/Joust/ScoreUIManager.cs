using UnityEngine;
using UnityEngine.UI;

public class ScoreUIManager : MonoBehaviour
{
    [Header("References")]
    public ScoreManager scoreManager;
    public WinManager winManager;

    [Header("Bars (Superpuestas)")]
    // La barra general de progreso (acumulado anterior)
    public Slider baseBar;   
    
    // El progreso específico de la ronda actual
    public Slider currentRoundBar;  

    [Header("Min Win Indicator")]
    [Tooltip("Indicador visual de la marca mínima para ganar en la barra de progreso.")]
    public Slider minWinSlider;

    private int lastRoundScore = 0;
    private int basePoints = 0;
    private bool lockCurrentRoundBar = false;

    void Start()
    {
        if (winManager == null) winManager = FindFirstObjectByType<WinManager>();
        if (scoreManager == null) scoreManager = FindFirstObjectByType<ScoreManager>();

        if (winManager != null)
        {
            if (baseBar != null) baseBar.maxValue = winManager.winPoints;
            if (currentRoundBar != null) currentRoundBar.maxValue = winManager.winPoints;
            if (minWinSlider != null) minWinSlider.maxValue = winManager.winPoints;
        }

        if (baseBar != null) baseBar.value = 0;
        if (currentRoundBar != null) currentRoundBar.value = 0;
        if (minWinSlider != null) minWinSlider.value = 0;
    }

    void Update()
    {
        UpdateCurrentRoundProgress();
        UpdateMinWinSliderInstant();
    }

    void UpdateCurrentRoundProgress()
    {
        if (lockCurrentRoundBar || scoreManager == null || winManager == null)
            return;

        int currentScore = scoreManager.GetScore();
        if (currentScore == lastRoundScore)
            return;

        lastRoundScore = currentScore;
        
        if (currentRoundBar != null)
        {
            currentRoundBar.value = Mathf.Min(currentScore, winManager.winPoints);
        }
    }

    public void ConsolidateRound()
    {
        if (scoreManager == null || winManager == null) return;

        int currentScore = scoreManager.GetScore();
        basePoints = Mathf.Min(currentScore, winManager.winPoints);

        if (baseBar != null) baseBar.value = basePoints;
        if (currentRoundBar != null) currentRoundBar.value = basePoints;
        
        lastRoundScore = 0;
    }

    public void PrepareNextRound()
    {
        lastRoundScore = 0;
        if (currentRoundBar != null)
        {
            currentRoundBar.value = basePoints;
        }
    }

    public void ResetAll()
    {
        basePoints = 0;
        lastRoundScore = 0;
        
        if (baseBar != null) baseBar.value = 0;
        if (currentRoundBar != null) currentRoundBar.value = 0;
        if (minWinSlider != null) minWinSlider.value = 0;
    }

    void UpdateMinWinSliderInstant()
    {
        if (minWinSlider == null || winManager == null) return;

        // Ahora la marca de victoria se posiciona siempre al 100% del total necesario (al final de la barra)
        minWinSlider.value = winManager.winPoints;
    }
}
