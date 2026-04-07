using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    public ScoreUIManager scoreUIManager;
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
    private bool tutorialDisabledAfterJoust = false;

    public void ProcessRoundEnd()
    {
        if (scoreManager == null || joustManager == null)
        {
            Debug.LogError("WinManager: ScoreManager o JoustManager no asignado.");
            return;
        }

        if (progressManager == null)
            progressManager = FindObjectOfType<ProgressManager>();

        if (gameEnded) return;

        int roundScore = scoreManager.GetScore();
        currentWinPoints += roundScore;

        if (scoreUIManager != null)
        {
            scoreUIManager.ConsolidateRound();
        }

        DisableTutorialAfterThisJoust();

        Debug.Log($"[Ronda {roundNumber} Finalizada] Puntos de esta ronda: {roundScore} | Puntos totales: {currentWinPoints}/{winPoints}");

        int minPointsThisRound = Mathf.CeilToInt(winPoints * minPointsFraction);

        if (roundScore >= minPointsThisRound)
        {
            if (currentWinPoints >= winPoints)
            {
                StartCoroutine(ShowGameWinPanel());
            }
            else
            {
                StartCoroutine(ShowRoundWinPanel());
            }
        }
        else
        {
            StartCoroutine(ShowRoundLosePanel());
        }
    }

    void DisableTutorialAfterThisJoust()
    {
        if (tutorialDisabledAfterJoust)
            return;

        if (joustManager != null && joustManager.tutorialManager != null)
        {
            joustManager.tutorialManager.DisableTutorial();
            tutorialDisabledAfterJoust = true;
            Debug.Log("Tutorial desactivado tras completar la justa.");
        }
        else
        {
            PlayerPrefs.SetInt("JoustTutorialEnabled", 0);
            PlayerPrefs.Save();
            tutorialDisabledAfterJoust = true;
            Debug.Log("Tutorial desactivado tras completar la justa (fallback con PlayerPrefs).");
        }
    }

    // Mostrar los distintos paneles
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

        LoseGame();

        yield return new WaitForSeconds(5f);

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("WinManager: nextSceneName no asignado.");
        }
    }

    IEnumerator ShowGameWinPanel()
    {
        gameEnded = true;

        if (gameWinPanel != null)
            gameWinPanel.SetActive(true);

        Debug.Log("¡Has ganado la partida completa!");
        WinGame();

        yield return new WaitForSeconds(3f);

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

        scoreManager.totalScore = 0;

        if (scoreUIManager != null)
        {
            scoreUIManager.PrepareNextRound();
        }

        joustManager.horsePartIsOn = true;
        joustManager.attackPartIsOn = false;
        joustManager.defensePartIsOn = false;
        joustManager.UpdatePhases();

        if (joustManager.horsePart != null)
        {
            joustManager.horsePart.ResetHorsePhase();
        }

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