using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class WinManager : MonoBehaviour
{
    public ScoreUIManager scoreUIManager;
    public ProgressManager progressManager;

    public int winPoints = 30;
    public int currentWinPoints = 0;

    [Range(0f, 1f)] public float minPointsFraction = 1f;

    public ScoreManager scoreManager;
    public JoustManager joustManager;

    [Header("Ragdoll")]
    public AttackPart_Joust attackPart;

    [Header("Cinematics")]
    public JoustCinematicManager cinematicManager;

    public int roundNumber = 1;

    [Header("UI Panels")]
    public GameObject roundWinPanel;
    public GameObject roundLosePanel;
    public GameObject gameWinPanel;

    [Header("Victory UI Texts")]
    public TextMeshProUGUI victoryMoneyText;
    public TextMeshProUGUI victoryScoreText;

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
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (gameEnded) return;

        int roundScore = scoreManager.GetScore();

        currentWinPoints = roundScore;

        if (scoreUIManager != null)
            scoreUIManager.ConsolidateRound();

        DisableTutorialAfterThisJoust();

        bool hasEnoughPoints = roundScore >= winPoints;
        bool fightWon = hasEnoughPoints && scoreManager.hasLandedAttack;

        if (hasEnoughPoints && !scoreManager.hasLandedAttack)
        {
            Debug.Log("[Justa finalizada] Tenías los puntos, pero perdiste por no impactar con la lanza.");
        }

        Debug.Log($"[Justa finalizada] Puntos: {roundScore}/{winPoints} | Resultado: {(fightWon ? "Victoria" : "Derrota")}");

        StartCoroutine(ProcessJoustEndSequence(roundScore, fightWon));
    }

    IEnumerator ProcessJoustEndSequence(int roundScore, bool fightWon)
    {
        if (!fightWon)
        {
            // Si perdimos la justa, nos aseguramos de que el oponente NO esté en ragdoll
            EnemyRagdollController enemyRagdoll = FindFirstObjectByType<EnemyRagdollController>();
            if (enemyRagdoll != null)
            {
                enemyRagdoll.ResetRagdoll();
            }

            // Activamos el ragdoll del propio jugador para que salga volando!
            EnemyRagdollController playerRagdoll = GetPlayerRagdoll();
            if (playerRagdoll != null)
            {
                // El impacto viene en dirección frontal (hacia atrás para el jugador)
                Vector3 direction = Vector3.forward;
                Vector3 point = playerRagdoll.transform.position + Vector3.up;
                int force = winPoints; // Fuerza del golpe proporcional a los puntos mínimos requeridos
                playerRagdoll.PlayImpact(point, direction, force, true);
            }
        }

        if (cinematicManager != null)
            yield return StartCoroutine(cinematicManager.PlayEnemyImpactSequence(fightWon));

        if (fightWon)
            StartCoroutine(ShowGameWinPanel());
        else
            StartCoroutine(ShowRoundLosePanel());
    }

    private EnemyRagdollController GetPlayerRagdoll()
    {
        Transform playerRoot = (joustManager != null) ? joustManager.player : null;
        GameObject playerObj = playerRoot != null ? playerRoot.gameObject : null;

        if (playerObj == null)
        {
            playerObj = GameObject.FindGameObjectWithTag("Player");
        }

        if (playerObj == null)
        {
            playerObj = GameObject.Find("Player");
        }

        if (playerObj == null)
        {
            Debug.LogError("[WinManager] ERROR: ¡No se pudo encontrar el GameObject del Player por ninguna vía! Asegúrate de que el JoustManager tiene asignado el campo Player o que el objeto está tagged como 'Player'.");
            return null;
        }

        Debug.Log($"[WinManager] GameObject del Player localizado con éxito: '{playerObj.name}'");

        EnemyRagdollController controller = playerObj.GetComponentInChildren<EnemyRagdollController>();
        if (controller != null)
        {
            Debug.Log($"[WinManager] Se encontró un EnemyRagdollController preexistente en el Player: '{controller.gameObject.name}'");
            return controller;
        }

        // Si no tiene el componente, buscamos su Animator (jinete) para acoplar el Ragdoll ahí
        Animator anim = playerObj.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            controller = anim.gameObject.AddComponent<EnemyRagdollController>();
            Debug.Log($"[WinManager] Se añadió dinámicamente EnemyRagdollController al Animator del Player: '{anim.gameObject.name}'");
            
            // Forzar carga de rigidbodies e inicialización manual inmediata
            Rigidbody[] bodies = anim.gameObject.GetComponentsInChildren<Rigidbody>();
            controller.allBodies = bodies;
            controller.animator = anim;
            Debug.Log($"[WinManager] Inicialización del Ragdoll del Player: Encontrados {bodies.Length} Rigidbodies.");
            if (bodies.Length == 0)
            {
                Debug.LogWarning("[WinManager] ADVERTENCIA: El esqueleto del Player no tiene ningún Rigidbody. ¡El ragdoll no funcionará si no hay rigidbodies físicos creados en sus huesos!");
            }
        }
        else
        {
            controller = playerObj.AddComponent<EnemyRagdollController>();
            Rigidbody[] bodies = playerObj.GetComponentsInChildren<Rigidbody>();
            controller.allBodies = bodies;
            Debug.Log($"[WinManager] Se añadió EnemyRagdollController en la raíz del Player '{playerObj.name}' con {bodies.Length} Rigidbodies.");
        }

        return controller;
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

        int moneyEarned = WinGame();
        UpdateVictoryTexts(currentWinPoints, moneyEarned);

        yield return new WaitForSeconds(3f);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
        else
            Debug.LogWarning("WinManager: nextSceneName no asignado.");
    }

    int WinGame()
    {
        Debug.Log("¡Has alcanzado los puntos necesarios! ¡Has ganado la partida!");

        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        int reward = 0;

        if (progressManager != null)
        {
            reward = progressManager.CalculateReward(winPoints, roundNumber);
            progressManager.AddMoney(reward);

            Debug.Log($"[REWARD] HP enemigo: {winPoints} | Ronda: {roundNumber} | Dinero ganado: {reward}");
        }
        else
        {
            Debug.LogError("No se encontró ProgressManager en la escena.");
        }

        return reward;
    }

    void UpdateVictoryTexts(int roundScore, int moneyEarned)
    {
        if (victoryMoneyText != null)
            victoryMoneyText.text = $"Money earned: {moneyEarned}";

        if (victoryScoreText != null)
            victoryScoreText.text = $"Points: {roundScore}";
    }

    void LoseGame()
    {
        Debug.Log("No alcanzaste los puntos necesarios. Has perdido.");
    }
}
