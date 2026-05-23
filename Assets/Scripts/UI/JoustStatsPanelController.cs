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

        // 1. Mostrar Resultado
        if (resultTitleText != null)
        {
            resultTitleText.text = won ? "<color=#48e085>¡VICTORIA EN LA JUSTA!</color>" : "<color=#ef5350>¡DERROTA EN LA JUSTA!</color>";
        }

        // 2. Poblar puntuaciones por fase
        ScoreManager score = FindFirstObjectByType<ScoreManager>();
        if (score != null)
        {
            if (horseScoreText != null) horseScoreText.text = $"+{score.horsePhaseScore} Ptos";
            if (attackScoreText != null) attackScoreText.text = $"+{score.attackPhaseScore} Ptos";
            if (defenseScoreText != null) defenseScoreText.text = $"{score.defensePhaseScore} Ptos"; // puede ser penalización negativa
            if (totalScoreText != null) totalScoreText.text = $"{score.totalScore} Ptos";
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
            rewardsGoldText.text = won ? $"+{goldEarned} Monedas" : "+0 Monedas (Derrota)";
        }

        if (rewardsItemText != null)
        {
            rewardsItemText.text = !string.IsNullOrEmpty(itemEarnedName) ? $"¡{itemEarnedName}!" : "Ninguno";
        }
        
        // Poner focus en el botón de finalizar torneo
        if (finishButton != null)
        {
            finishButton.gameObject.SetActive(true);
            UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(finishButton.gameObject);
        }
    }

    public void FinishTournament()
    {
        Debug.Log($"[Tournament] Finalizando justa. Cargando siguiente escena: {nextSceneName}");
        
        // Limpiamos los efectos climatológicos y de cartas al salir de la escena
        EffectManager effectManager = FindFirstObjectByType<EffectManager>();
        if (effectManager != null)
        {
            effectManager.DisableAllEffects();
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
