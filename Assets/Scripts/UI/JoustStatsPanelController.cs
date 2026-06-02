using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JoustStatsPanelController : MonoBehaviour
{
    [Header("Core Refs")]
    public GameObject panelObject;
    public string nextSceneName = "World";

    [Header("UI Result Header")]
    public TMP_Text resultTitleText;

    [Header("UI Phase Scores")]
    public TMP_Text horseScoreText;
    public TMP_Text attackScoreText;
    public TMP_Text defenseScoreText;
    public TMP_Text totalScoreText;

    [Header("UI Equipped Items")]
    public TMP_Text horseEquippedText;
    public TMP_Text lanceEquippedText;
    public TMP_Text shieldEquippedText;
    public TMP_Text armorEquippedText;

    [Header("UI Loadout Stats")]
    public TMP_Text statBFText;  // Fuerza
    public TMP_Text statBLText;  // Lanza
    public TMP_Text statMText;   // Maniobrabilidad
    public TMP_Text statBBText;  // Bloqueo / Escudo
    public TMP_Text statMVText;  // Vel. Caballo

    [Header("UI Rewards")]
    public TMP_Text rewardsGoldText;
    public TMP_Text rewardsItemText;

    [Header("UI Finish Button")]
    public Button finishButton;

    void Awake()
    {
        if (panelObject == null)
        {
            // Si el componente está en un hijo (como JoustStatsPanel dentro de StatsPanelBorder),
            // el panelObject real que debemos activar es el padre para que todo el marco sea visible.
            if (transform.parent != null)
            {
                panelObject = transform.parent.gameObject;
            }
            else
            {
                panelObject = this.gameObject;
            }
        }

        if (finishButton != null)
        {
            finishButton.onClick.RemoveAllListeners();
            finishButton.onClick.AddListener(FinishTournament);
        }
    }

    public void PopulateAndShow(bool won, int goldEarned, string itemEarnedName)
    {
        if (panelObject != null)
            panelObject.SetActive(true);

        // Forzar visibilidad y desbloqueo del cursor al mostrar el panel final
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isTutorialScene = currentSceneName.ToLower().Contains("tutorial");

        WinManager winManager = FindFirstObjectByType<WinManager>();
        bool matchDecided = false;
        if (winManager != null)
        {
            matchDecided = (winManager.playerRoundWins >= 2 || winManager.enemyRoundWins >= 2) || 
                            (winManager.joustManager != null && winManager.joustManager.isTutorialMode) ||
                            isTutorialScene;
        }

        // 1. Mostrar Resultado
        if (resultTitleText != null)
        {
            if (winManager != null)
            {
                if (matchDecided)
                {
                    resultTitleText.text = won
                        ? $"<color=#48e085>¡VICTORIA EN EL TORNEO ({winManager.playerRoundWins} - {winManager.enemyRoundWins})!</color>"
                        : $"<color=#ef5350>¡DERROTA EN EL TORNEO ({winManager.playerRoundWins} - {winManager.enemyRoundWins})!</color>";
                }
                else
                {
                    resultTitleText.text = won
                        ? $"<color=#48e085>¡RONDA {winManager.roundNumber} GANADA!</color>"
                        : $"<color=#ef5350>¡RONDA {winManager.roundNumber} PERDIDA!</color>";
                }
            }
            else
            {
                resultTitleText.text = won ? "<color=#48e085>¡VICTORIA EN LA JUSTA!</color>" : "<color=#ef5350>¡DERROTA EN LA JUSTA!</color>";
            }
        }

        // 2. Poblar puntuaciones por fase
        ScoreManager score = FindFirstObjectByType<ScoreManager>();
        if (score != null)
        {
            if (horseScoreText != null) horseScoreText.text = $"+{score.horsePhaseScore} Ptos";
            if (attackScoreText != null) attackScoreText.text = $"+{score.attackPhaseScore} Ptos";
            if (defenseScoreText != null) defenseScoreText.text = $"{score.defensePhaseScore} Ptos"; // puede ser penalización negativa

            if (winManager != null && winManager.playerWonRoundsScores.Count > 0)
            {
                int totalWonScores = 0;
                foreach (int s in winManager.playerWonRoundsScores)
                {
                    totalWonScores += s;
                }
                if (totalScoreText != null) totalScoreText.text = $"{totalWonScores} Ptos";
            }
            else
            {
                if (totalScoreText != null) totalScoreText.text = $"{score.totalScore} Ptos";
            }
        }

        // 3. Poblar Equipamiento Actual
        ProgressManager progress = FindFirstObjectByType<ProgressManager>();
        if (progress != null && progress.equipment != null)
        {
            var eq = progress.equipment;
            if (horseEquippedText != null) horseEquippedText.text = eq.GetEquipped(EquipmentSlot.Horse)?.displayName ?? "Sin caballo";
            if (lanceEquippedText != null) lanceEquippedText.text = eq.GetEquipped(EquipmentSlot.Lance)?.displayName ?? "Sin lanza";
            if (shieldEquippedText != null) shieldEquippedText.text = eq.GetEquipped(EquipmentSlot.Shield)?.displayName ?? "Sin escudo";
            if (armorEquippedText != null) armorEquippedText.text = eq.GetEquipped(EquipmentSlot.Armor)?.displayName ?? "Sin armadura";
        }

        // 4. Poblar Estadísticas de Combate
        LoadoutStatsComponent loadout = FindFirstObjectByType<LoadoutStatsComponent>();
        if (loadout != null)
        {
            if (statBFText != null) statBFText.text = $"{Mathf.RoundToInt(loadout.stats.Get(StatType.BF))}";
            if (statBLText != null) statBLText.text = $"{Mathf.RoundToInt(loadout.stats.Get(StatType.BL))}";
            if (statMText != null) statMText.text = $"{Mathf.RoundToInt(loadout.stats.Get(StatType.M))}";
            if (statBBText != null) statBBText.text = $"{Mathf.RoundToInt(loadout.stats.Get(StatType.BB))}";
            if (statMVText != null) statMVText.text = $"{Mathf.RoundToInt(loadout.stats.Get(StatType.MV))}";
        }

        // 5. Poblar Recompensas
        if (rewardsGoldText != null)
        {
            if (matchDecided)
            {
                rewardsGoldText.text = won ? $"+{goldEarned} Monedas" : "+0 Monedas (Derrota)";
            }
            else
            {
                rewardsGoldText.text = "+0 Monedas (Torneo en curso)";
            }
        }

        if (rewardsItemText != null)
        {
            rewardsItemText.text = !string.IsNullOrEmpty(itemEarnedName) ? $"¡{itemEarnedName}!" : "Ninguno";
        }

        // Poner focus en el botón y cambiar su texto dinámicamente
        if (finishButton != null)
        {
            finishButton.gameObject.SetActive(true);

            TMP_Text btnTxtComp = finishButton.GetComponentInChildren<TMP_Text>();
            if (btnTxtComp == null)
            {
                btnTxtComp = finishButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (btnTxtComp != null)
            {
                btnTxtComp.text = matchDecided ? "REGRESAR AL MAPA" : "SIGUIENTE RONDA";
            }

            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(finishButton.gameObject);
        }
    }

    public void FinishTournament()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        bool isTutorialScene = currentSceneName.ToLower().Contains("tutorial");

        WinManager winManager = FindFirstObjectByType<WinManager>();
        bool matchDecided = false;
        if (winManager != null)
        {
            matchDecided = (winManager.playerRoundWins >= 2 || winManager.enemyRoundWins >= 2) || 
                            (winManager.joustManager != null && winManager.joustManager.isTutorialMode) ||
                            isTutorialScene;
        }

        if (matchDecided)
        {
            // Limpiar efectos activos
            EffectManager effectManager = FindFirstObjectByType<EffectManager>();
            if (effectManager != null)
                effectManager.DisableAllEffects();

            // Marcar que al llegar al mundo debemos entrar directamente al pueblo en 1ª persona
            ProgressManager.ReturnToTownFirstPerson = true;

            // Determinar la escena correcta: World o TutorialWorld según el nodo actual
            string sceneToLoad = GetReturnSceneName();
            Debug.Log($"[Tournament] Finalizando justa. Regresando a: '{sceneToLoad}' en modo 1ª persona.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            Debug.Log("[Tournament] Botón Siguiente Ronda pulsado. Avanzando...");

            if (panelObject != null)
                panelObject.SetActive(false);

            if (winManager != null)
                winManager.StartNextRoundFromStatsPanel();
        }
    }

    string GetReturnSceneName()
    {
        if (!string.IsNullOrEmpty(ProgressManager.ReturnSceneName))
        {
            return ProgressManager.ReturnSceneName;
        }

        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentSceneName.ToLower().Contains("tutorial"))
        {
            return "TutorialWorld";
        }

        WinManager winManager = FindFirstObjectByType<WinManager>();
        if (winManager != null && winManager.joustManager != null && winManager.joustManager.isTutorialMode)
        {
            return "TutorialWorld";
        }

        ProgressManager pm = FindFirstObjectByType<ProgressManager>();
        if (pm != null)
        {
            // Si el nodo actual contiene "tutorial", volvemos a TutorialWorld
            string nodeId = pm.CurrentNodeId ?? "";
            if (nodeId.ToLower().Contains("tutorial"))
                return "TutorialWorld";
        }

        // Por defecto y para todos los torneos normales, volvemos a World
        return string.IsNullOrEmpty(nextSceneName) ? "World" : nextSceneName;
    }
}

