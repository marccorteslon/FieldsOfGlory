using UnityEngine;
using TMPro;

//RESUMEN SCRIPT: Controla la fase de defensa y aplica mitigación/puntos usando BB del loadout.

public class DefensePart_Joust : MonoBehaviour
{
    public TextMeshProUGUI attackText;
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Loadout (Ghost Player)")]
    public LoadoutStatsComponent loadout;

    [Header("Defense")]
    public float defenseTime = 2f;

    private string[] attackSides = { "Izquierda", "Derecha", "Arriba", "Abajo" };
    private string currentAttackSide;
    private bool awaitingDefense = false;
    private float timer = 0f;

    [Header("Fallback Shield Stat (si no hay loadout)")]
    public int fallbackBB = 2;

    void Awake()
    {
        if (loadout == null)
            loadout = FindObjectOfType<LoadoutStatsComponent>();
    }

    void OnEnable()
    {
        if (attackText != null)
            attackText.gameObject.SetActive(false);
        StartNewAttack();
    }

    void Update()
    {
        if (!joustManager.defensePartIsOn || !awaitingDefense) return;

        timer += Time.deltaTime;

        if (CheckDefenseInput())
        {
            EndDefense(true);
        }
        else if (timer >= defenseTime)
        {
            EndDefense(false);
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
        timer = 0f;
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

    void EndDefense(bool blockedCorrectly)
    {
        awaitingDefense = false;

        // BB desde el loadout
        scoreManager.ApplyDefense(blockedCorrectly, GetBB());

        joustManager.EndDefensePhase();

        if (attackText != null)
        {
            attackText.text = "";
            attackText.gameObject.SetActive(false);
        }
    }
}