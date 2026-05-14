using UnityEngine;

public class PhysicalLanceController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto vacío que hace de 'Mano' (Eje). Debe estar fuera de cámara.")]
    public Transform lancePivot;
    public LoadoutStatsComponent loadout;

    [Header("Sensación de Peso (Inercia)")]
    public float mass = 5f;
    public float springForce = 50f;
    public float damping = 3f;

    [Header("Input y Límites")]
    public float inputSensitivity = 2f;
    public Vector2 maxAimAngles = new Vector2(30f, 20f);

    [Header("Carga del Golpe")]
    public float maxChargeTime = 2f;
    private float currentCharge = 0f;
    private bool isCharging = false;

    // Físicas
    private Vector2 targetAimPosition;
    private Vector2 actualLancePosition;
    private Vector2 lanceVelocity;
    private float shakeTimer = 0f;

    [Header("Fallback Stats")]
    public int fallbackBL = 2; // Stat de maniobrabilidad/Lanza
    public int fallbackBF = 4; // Stat de Fuerza/Daño

    void Update()
    {
        if (lancePivot == null) return;

        HandleInput();
        HandleCharge();
        ApplyPhysics();
    }

    void HandleInput()
    {
        // RATÓN: Usa movimiento Delta
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        targetAimPosition.x += mouseX * inputSensitivity;
        targetAimPosition.y += mouseY * inputSensitivity;

        // Limitar la mira para que no rompas el cuello
        targetAimPosition.x = Mathf.Clamp(targetAimPosition.x, -maxAimAngles.x, maxAimAngles.x);
        targetAimPosition.y = Mathf.Clamp(targetAimPosition.y, -maxAimAngles.y, maxAimAngles.y);
    }

    void HandleCharge()
    {
        // Cargar con Clic Izquierdo (Ratón)
        bool attackInput = Input.GetMouseButton(0);

        if (attackInput)
        {
            isCharging = true;
            currentCharge = Mathf.Min(currentCharge + Time.deltaTime, maxChargeTime);
        }
        else
        {
            isCharging = false;
            if (currentCharge > 0)
            {
                currentCharge -= Time.deltaTime * 2f; // Pierdes fuerza lentamente si dejas de cargar
                currentCharge = Mathf.Max(0, currentCharge);
            }
        }
    }

    void ApplyPhysics()
    {
        // 1. Físicas de muelle (Spring Physics)
        Vector2 force = (targetAimPosition - actualLancePosition) * springForce;
        Vector2 acceleration = force / mass;

        lanceVelocity += acceleration * Time.deltaTime;
        lanceVelocity *= (1f - damping * Time.deltaTime); // Freno natural

        actualLancePosition += lanceVelocity * Time.deltaTime;

        // 2. Temblor (Sway)
        float maniobrabilidad = loadout != null ? loadout.stats.Get(StatType.BL) : fallbackBL;
        float chargePercent = currentCharge / maxChargeTime;
        
        // A más carga, más tiembla. A más maniobrabilidad, menos tiembla.
        float shakeAmount = (chargePercent * 5f) / Mathf.Max(0.5f, maniobrabilidad);

        shakeTimer += Time.deltaTime * (10f + chargePercent * 5f);
        float shakeX = (Mathf.PerlinNoise(shakeTimer, 0) - 0.5f) * shakeAmount;
        float shakeY = (Mathf.PerlinNoise(0, shakeTimer) - 0.5f) * shakeAmount;

        Vector2 finalPosition = actualLancePosition + new Vector2(shakeX, shakeY);

        // Aplicamos la rotación final al pivote (invirtiendo Y para que arriba sea arriba)
        lancePivot.localRotation = Quaternion.Euler(-finalPosition.y, finalPosition.x, 0);
    }

    public float GetImpactDamage()
    {
        float damageBase = loadout != null ? loadout.stats.Get(StatType.BF) : fallbackBF;
        float totalDamage = damageBase * (1f + currentCharge);
        
        // Reseteo por impacto
        currentCharge = 0f; 
        isCharging = false;
        
        // Retroceso visual en la cámara/lanza (opcional, simulado con velocidad de inercia)
        lanceVelocity = new Vector2(Random.Range(-30f, 30f), Random.Range(30f, 60f)); 

        return totalDamage;
    }
}
