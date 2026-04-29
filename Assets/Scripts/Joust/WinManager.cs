using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class WinManager : MonoBehaviour
{
    public ScoreUIManager scoreUIManager;
    public ProgressManager progressManager;

    public int winPoints = 30;
    public int currentWinPoints = 0;

    // Se mantiene porque ScoreUIManager lo usa para pintar el indicador mínimo.
    // En una sola pasada, el mínimo para ganar es el total.
    [Range(0f, 1f)] public float minPointsFraction = 1f;

    public ScoreManager scoreManager;
    public JoustManager joustManager;

    [Header("Ragdoll")]
    public AttackPart_Joust attackPart;

    [Header("Cinematics")]
    public JoustCinematicManager cinematicManager;

    // Se mantiene por compatibilidad con ProgressManager/recompensas.
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

        // Ahora la justa solo tiene una pasada.
        currentWinPoints = roundScore;

        if (scoreUIManager != null)
            scoreUIManager.ConsolidateRound();

        DisableTutorialAfterThisJoust();

        bool fightWon = roundScore >= winPoints;

        Debug.Log($"[Justa finalizada] Puntos: {roundScore}/{winPoints} | Resultado: {(fightWon ? "Victoria" : "Derrota")}");

        StartCoroutine(ProcessJoustEndSequence(roundScore, fightWon));
    }

    IEnumerator ProcessJoustEndSequence(int roundScore, bool fightWon)
    {
        if (attackPart != null)
            attackPart.ApplyEnemyImpact(roundScore, fightWon);

        if (cinematicManager != null)
            yield return StartCoroutine(cinematicManager.PlayEnemyImpactSequence(fightWon));

        if (fightWon)
            StartCoroutine(ShowGameWinPanel());
        else
            StartCoroutine(ShowRoundLosePanel());
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

    IEnumerator ShowRoundLosePanel()
    {
        gameEnded = true;

        if (roundLosePanel != null)
            roundLosePanel.SetActive(true);

        LoseGame();

        yield return new WaitForSeconds(5f);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("WinManager: nextSceneName no asignado.");
    }

    IEnumerator ShowGameWinPanel()
    {
        gameEnded = true;

        if (gameWinPanel != null)
            gameWinPanel.SetActive(true);

        Debug.Log("¡Has ganado la justa!");
        WinGame();

        yield return new WaitForSeconds(3f);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("WinManager: nextSceneName no asignado.");
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
        Debug.Log("No alcanzaste los puntos necesarios. Has perdido.");
    }
}