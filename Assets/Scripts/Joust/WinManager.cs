using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    public ProgressManager progressManager;
    public int winPoints = 30;
    public int currentWinPoints = 0;
    [Range(0f, 1f)] public float minPointsFraction = 1f / 3f;

    public ScoreManager scoreManager;
    public JoustManager joustManager;

    public int roundNumber = 1;

    [Header("UI Panels")]
    public GameObject roundWinPanel;
    public GameObject roundLosePanel;
    public GameObject gameWinPanel;

    [Header("UI Timing")]
    public float panelDisplayTime = 3f;

    [Header("Scene Settings")]
    public string nextSceneName = "Shop";

    private bool gameEnded = false;

    // ---------------- Llamar al final de cada ronda ----------------
    public void ProcessRoundEnd()
    {
        if (scoreManager == null || joustManager == null)
        {
            Debug.LogError("WinManager: ScoreManager o JoustManager no asignado.");
            return;
        }
        if (progressManager == null)
            progressManager = FindObjectOfType<ProgressManager>();

        if (gameEnded) return; // ya terminó la partida, no procesar más

        int roundScore = scoreManager.GetScore();
        currentWinPoints += roundScore;

        Debug.Log($"[Ronda {roundNumber} Finalizada] Puntos de esta ronda: {roundScore} | Puntos totales: {currentWinPoints}/{winPoints}");

        int minPointsThisRound = Mathf.CeilToInt(winPoints * minPointsFraction);

        if (roundScore >= minPointsThisRound)
        {
            if (currentWinPoints >= winPoints)
            {
                StartCoroutine(ShowGameWinPanel()); // victoria de la PARTIDA
            }
            else
            {
                StartCoroutine(ShowRoundWinPanel()); // victoria de RONDA
            }
        }
        else
        {
            StartCoroutine(ShowRoundLosePanel()); // derrota de ronda

        }
    }

    IEnumerator ShowRoundWinPanel()
    {
        if (roundWinPanel != null)
            roundWinPanel.SetActive(true);

        yield return new WaitForSeconds(panelDisplayTime);

        if (roundWinPanel != null)
            roundWinPanel.SetActive(false);

        roundNumber++;
        StartNextRound();
    }

    IEnumerator ShowRoundLosePanel()
    {
        if (roundLosePanel != null)
            roundLosePanel.SetActive(true);

        yield return new WaitForSeconds(panelDisplayTime);

        if (roundLosePanel != null)
            roundLosePanel.SetActive(false);

        LoseGame();
    }

    IEnumerator ShowGameWinPanel()
    {
        gameEnded = true;

        if (gameWinPanel != null)
            gameWinPanel.SetActive(true);

        Debug.Log("¡Has ganado la partida completa!");
        WinGame();

        // Espera 3 segundos antes de cambiar de escena
        yield return new WaitForSeconds(3f);

        // Cambiar de escena
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("WinManager: nextSceneName no asignado.");
        }
    }

    void StartNextRound()
    {
        Debug.Log("Empezando la siguiente ronda...");

        // Reset fases
        joustManager.horsePartIsOn = true;
        joustManager.attackPartIsOn = false;
        joustManager.defensePartIsOn = false;
        joustManager.UpdatePhases();

        // Reset score de la ronda
        scoreManager.totalScore = 0;

        // Reset indicadores de fase caballo
        if (joustManager.horsePart != null)
        {
            joustManager.horsePart.ResetHorsePhase();
        }

        // ---------------- Reset posiciones jugador, enemigo y cámara ----------------
        joustManager.ResetPositions();
    }

    void WinGame()
    {
        Debug.Log("¡Has alcanzado los puntos necesarios! ¡Has ganado la partida!");

        if (progressManager == null)
            progressManager = FindObjectOfType<ProgressManager>();

        if (progressManager != null)
        {
            int reward = progressManager.CalculateReward(winPoints, roundNumber);

            progressManager.AddMoney(reward);

            Debug.Log($"[REWARD] HP enemigo: {winPoints} | Ronda: {roundNumber} | Dinero ganado: {reward}");
        }
        else
        {
            Debug.LogError("No se encontró ProgressManager en la escena.");
        }
    }

    void LoseGame()
    {
        Debug.Log("No alcanzaste los puntos mínimos de esta ronda. Has perdido.");
        
       
    }
}