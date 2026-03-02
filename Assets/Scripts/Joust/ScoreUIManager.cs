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

    private int lastRoundScore = 0;
    private int basePoints = 0;     // puntos consolidados de rondas anteriores

    void Start()
    {
        baseBar.maxValue = winManager.winPoints;
        currentRoundBar.maxValue = winManager.winPoints;

        baseBar.value = 0;
        currentRoundBar.value = 0;
    }

    void Update()
    {
        UpdateCurrentRoundProgress();
    }

    void UpdateCurrentRoundProgress()
    {
        int currentScore = scoreManager.GetScore();

        if (currentScore == lastRoundScore) return;

        lastRoundScore = currentScore;

        // La barra de ronda actual es:
        // puntos base + puntos actuales de la ronda
        currentRoundBar.value = basePoints + currentScore;
    }

    // -------- Llamado desde WinManager al acabar la ronda --------
    public void ConsolidateRound()
    {
        basePoints += lastRoundScore;

        baseBar.value = basePoints;
        currentRoundBar.value = basePoints;

        lastRoundScore = 0;
    }

    // -------- Si quieres resetear completamente la partida --------
    public void ResetAll()
    {
        basePoints = 0;
        lastRoundScore = 0;

        baseBar.value = 0;
        currentRoundBar.value = 0;
    }
}