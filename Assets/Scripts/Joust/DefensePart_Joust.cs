using UnityEngine;
using TMPro;

//RESUMEN SCRIPT: Controla la fase de defensa de la Justa usando texto para mostrar el ataque.

public class DefensePart_Joust : MonoBehaviour
{
    public TextMeshProUGUI attackText;
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    public float defenseTime = 2f;
    private string[] attackSides = { "Izquierda", "Derecha", "Arriba", "Abajo" };
    private string currentAttackSide;
    private bool awaitingDefense = false;
    private float timer = 0f;
    public int BB = 2; // Penalización si bloqueas bien

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
        scoreManager.ApplyDefense(blockedCorrectly, BB);
        joustManager.EndDefensePhase();

        if (attackText != null)
        {
            attackText.text = "";
            attackText.gameObject.SetActive(false);
        }
    }
}