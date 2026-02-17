using UnityEngine;
using TMPro;

//RESUMEN SCRIPT: Controla la fase de defensa de la Justa usando texto para mostrar el ataque.
public class DefensePart_Joust : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI attackText;          // Texto en UI que indica el lado del ataque
    public JoustManager joustManager;           // Referencia al manager

    [Header("Settings")]
    public float defenseTime = 2f;              // Tiempo máximo para reaccionar al ataque

    private string[] attackSides = { "Izquierda", "Derecha", "Arriba", "Abajo" };
    private string currentAttackSide;
    private bool awaitingDefense = false;
    private float timer = 0f;

    void OnEnable()
    {
        // Al iniciar, ocultamos el texto hasta que empiece la defensa
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
            Debug.Log("Defensa correcta!");
            EndDefense();
        }
        else if (timer >= defenseTime)
        {
            Debug.Log("Has fallado la defensa!");
            EndDefense();
        }
    }

    void StartNewAttack()
    {
        // Elegir lado aleatorio
        int index = Random.Range(0, attackSides.Length);
        currentAttackSide = attackSides[index];

        // Mostrar en la UI solo al empezar la defensa
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
        // Teclado
        bool correctKey = false;
        switch (currentAttackSide)
        {
            case "Izquierda":
                correctKey = Input.GetKey(KeyCode.LeftArrow);
                break;
            case "Derecha":
                correctKey = Input.GetKey(KeyCode.RightArrow);
                break;
            case "Arriba":
                correctKey = Input.GetKey(KeyCode.UpArrow);
                break;
            case "Abajo":
                correctKey = Input.GetKey(KeyCode.DownArrow);
                break;
        }

        // Joystick izquierdo
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        bool correctStick = false;
        switch (currentAttackSide)
        {
            case "Izquierda":
                correctStick = horizontal < -0.5f;
                break;
            case "Derecha":
                correctStick = horizontal > 0.5f;
                break;
            case "Arriba":
                correctStick = vertical > 0.5f;
                break;
            case "Abajo":
                correctStick = vertical < -0.5f;
                break;
        }

        return correctKey || correctStick;
    }

    void EndDefense()
    {
        awaitingDefense = false;
        joustManager.EndDefensePhase();

        // Limpiar y ocultar texto de UI
        if (attackText != null)
        {
            attackText.text = "";
            attackText.gameObject.SetActive(false);
        }
    }
}
