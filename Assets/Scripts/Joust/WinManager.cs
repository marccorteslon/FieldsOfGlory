using UnityEngine;

public class WinManager : MonoBehaviour
{
    public int winPoints = 30;
    public int currentWinPoints = 0;
    [Range(0f, 1f)] public float minPointsFraction = 1f / 3f;

    public ScoreManager scoreManager;
    public JoustManager joustManager;

    public int roundNumber = 1;

    // ---------------- Llamar al final de cada ronda ----------------
    public void ProcessRoundEnd()
    {
        if (scoreManager == null || joustManager == null)
        {
            Debug.LogError("WinManager: ScoreManager o JoustManager no asignado.");
            return;
        }

        int roundScore = scoreManager.GetScore();
        currentWinPoints += roundScore;

        Debug.Log($"[Ronda {roundNumber} Finalizada] Puntos de esta ronda: {roundScore} | Puntos totales: {currentWinPoints}/{winPoints}");

        int minPointsThisRound = Mathf.CeilToInt(winPoints * minPointsFraction);

        if (roundScore >= minPointsThisRound)
        {
            if (currentWinPoints >= winPoints)
            {
                WinGame();
            }
            else
            {
                roundNumber++;
                StartNextRound();
            }
        }
        else
        {
            LoseGame();
        }
    }

    void StartNextRound()
    {
        Debug.Log("Empezando la siguiente ronda...");

        // Reset de fases
        joustManager.horsePartIsOn = true;
        joustManager.attackPartIsOn = false;
        joustManager.defensePartIsOn = false;
        joustManager.UpdatePhases();

        // Reset de score de la ronda
        scoreManager.totalScore = 0;

        // Reset cámara
        if (joustManager.mainCamera != null && joustManager.horseCameraPoint != null)
        {
            joustManager.mainCamera.transform.position = joustManager.horseCameraPoint.position;
            joustManager.mainCamera.transform.rotation = joustManager.horseCameraPoint.rotation;
        }

        // Si tu HorsePart_Joust tiene indicadores en movimiento, reinicia su estado
        if (joustManager.horsePart != null)
        {
            joustManager.horsePart.ResetHorsePhase();
        }

        // Aquí puedes resetear UI, sliders o retículas del ataque/defensa si hace falta
    }

    void WinGame()
    {
        Debug.Log("¡Has alcanzado los puntos necesarios! ¡Has ganado la partida!");
        // Mostrar UI de victoria o pasar a menú
    }

    void LoseGame()
    {
        Debug.Log("No alcanzaste los puntos mínimos de esta ronda. Has perdido.");
        // Mostrar UI de derrota o reiniciar partida
    }
}