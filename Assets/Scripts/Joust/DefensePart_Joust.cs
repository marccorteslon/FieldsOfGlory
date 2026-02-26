using UnityEngine;
using TMPro;

// Controla input y resultado de la defensa.
// El temporizador ahora lo gestiona JoustManager.

public class DefensePart_Joust : MonoBehaviour
{
    public TextMeshProUGUI attackText;
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Loadout (Ghost Player)")]
    public LoadoutStatsComponent loadout;

    [Header("Fallback Shield Stat (si no hay loadout)")]
    public int fallbackBB = 2;

    private string[] attackSides = { "Izquierda", "Derecha", "Arriba", "Abajo" };
    private string currentAttackSide;

    private bool awaitingDefense = false;
    private bool defenseStarted = false;

    void Awake()
    {
        if (loadout == null)
            loadout = FindObjectOfType<LoadoutStatsComponent>();
    }

    void OnEnable()
    {
        if (attackText != null)
            attackText.gameObject.SetActive(false);

        awaitingDefense = false;
        defenseStarted = false;
    }

    void Update()
    {
        if (!joustManager.defensePartIsOn)
            return;

        // Primer frame real de defensa
        if (!defenseStarted)
        {
            defenseStarted = true;
            StartNewAttack();
        }

        if (!awaitingDefense)
            return;

        if (CheckDefenseInput())
        {
            EndDefense(true);
        }
    }

    int GetBB()
    {
        if (loadout == null) return fallbackBB;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.BB));
    }

    void StartNewAttack()
    {
        currentAttackSide = attackSides[Random.Range(0, attackSides.Length)];

        if (attackText != null)
        {
            attackText.text = $"¡Enemigo ataca {currentAttackSide}!";
            attackText.gameObject.SetActive(true);
        }

        awaitingDefense = true;
    }

    bool CheckDefenseInput()
    {
        bool correctKey = currentAttackSide switch
        {
            "Izquierda" => Input.GetKey(KeyCode.LeftArrow),
            "Derecha" => Input.GetKey(KeyCode.RightArrow),
            "Arriba" => Input.GetKey(KeyCode.UpArrow),
            "Abajo" => Input.GetKey(KeyCode.DownArrow),
            _ => false
        };

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool correctStick = currentAttackSide switch
        {
            "Izquierda" => horizontal < -0.5f,
            "Derecha" => horizontal > 0.5f,
            "Arriba" => vertical > 0.5f,
            "Abajo" => vertical < -0.5f,
            _ => false
        };

        return correctKey || correctStick;
    }

    public void ForceEndDefense(bool blockedCorrectly)
    {
        if (!awaitingDefense)
            return;

        EndDefense(blockedCorrectly);
    }

    void EndDefense(bool blockedCorrectly)
    {
        awaitingDefense = false;

        scoreManager.ApplyDefense(blockedCorrectly, GetBB());

        if (attackText != null)
        {
            attackText.text = "";
            attackText.gameObject.SetActive(false);
        }

        joustManager.EndDefensePhase();
    }
}