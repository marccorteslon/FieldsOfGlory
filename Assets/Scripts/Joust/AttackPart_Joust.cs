using UnityEngine;
using UnityEngine.UI;

public class AttackPart_Joust : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;
    public Canvas canvas;
    public Slider powerSlider;

    [Header("Settings")]
    public float maxChargeTime = 2f;
    public float baseShakeAmount = 200f;
    public float shakeSpeed = 25f;
    public bool enableShake = true;

    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Loadout")]
    public LoadoutStatsComponent loadout;

    [Header("Controller Aim")]
    public float joystickSpeed = 800f;
    public float stickDeadzone = 0.2f;

    private Vector2 crosshairPos;

    [Header("Fallback Lance Stats")]
    public int fallbackBF = 4;
    public int fallbackBL = 2;

    private bool previousAttackState = false;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float currentShakeAmount;
    private float shakeTime;

    private enum InputMode
    {
        Mouse,
        Controller
    }

    private InputMode currentInputMode = InputMode.Mouse;

    void Awake()
    {
        if (loadout == null)
            loadout = FindObjectOfType<LoadoutStatsComponent>();
    }

    void Start()
    {
        // La carga del ataque
        if (powerSlider != null)
        {
            powerSlider.gameObject.SetActive(false);
            powerSlider.minValue = 0;
            powerSlider.maxValue = 100;
            powerSlider.value = 0;
        }

        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
            crosshairPos = crosshair.anchoredPosition;
        }

        currentShakeAmount = baseShakeAmount;
    }

    void Update()
    {
        if (joustManager == null) return;

        bool attackStarted = joustManager.attackPartIsOn;

        if (attackStarted != previousAttackState)
        {
            if (crosshair != null)
                crosshair.gameObject.SetActive(attackStarted);

            previousAttackState = attackStarted;

            if (attackStarted)
            {
                ResetCharge();
                shakeTime = Random.Range(0f, 100f);

                if (crosshair != null)
                    crosshairPos = crosshair.anchoredPosition;
            }
        }

        if (!attackStarted) return;

        // Bloquear totalmente mientras el tutorial esté abierto
        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        UpdateCrosshair();
        HandleChargeInput();
    }

    int GetBF()
    {
        if (loadout == null) return fallbackBF;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.BF));
    }

    int GetBL()
    {
        if (loadout == null) return fallbackBL;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.BL));
    }

    void HandleChargeInput()
    {
        // PC: Click izquierdo
        bool mouseDown = Input.GetMouseButtonDown(0);
        bool mouseHeld = Input.GetMouseButton(0);
        bool mouseUp = Input.GetMouseButtonUp(0);

        // Mando: eje Attack / R2
        float r2Axis = Input.GetAxis("Attack");
        bool controllerHeld = r2Axis > 0.2f;
        bool controllerDown = controllerHeld && !isCharging;
        bool controllerUp = !controllerHeld && isCharging && currentInputMode == InputMode.Controller;

        // Si el jugador empieza con ratón, carga con ratón
        if (!isCharging)
        {
            if (mouseDown)
            {
                StartCharge(InputMode.Mouse);
            }
            else if (controllerDown)
            {
                StartCharge(InputMode.Controller);
            }
        }

        if (!isCharging)
            return;

        bool keepCharging = false;
        bool releaseAttack = false;

        if (currentInputMode == InputMode.Mouse)
        {
            keepCharging = mouseHeld;
            releaseAttack = mouseUp;
        }
        else
        {
            keepCharging = controllerHeld;
            releaseAttack = controllerUp;
        }

        if (keepCharging)
        {
            chargeTimer += Time.deltaTime;

            float percent = Mathf.Clamp01(chargeTimer / maxChargeTime);

            if (powerSlider != null)
                powerSlider.value = percent * 100f;

            currentShakeAmount = baseShakeAmount + (baseShakeAmount * percent);
        }

        if (releaseAttack)
        {
            isCharging = false;

            if (powerSlider != null)
                powerSlider.gameObject.SetActive(false);

            PerformAttack();
        }
    }

    void StartCharge(InputMode mode) // Empezar a hacer la carga
    {
        currentInputMode = mode;
        isCharging = true;
        chargeTimer = 0f;
        currentShakeAmount = baseShakeAmount;

        if (powerSlider != null)
        {
            powerSlider.gameObject.SetActive(true);
            powerSlider.value = 0f;
        }
    }

    void ResetCharge() // Al inicio de cada ronda, reiniciar
    {
        isCharging = false;
        chargeTimer = 0f;
        currentShakeAmount = baseShakeAmount;

        if (powerSlider != null)
        {
            powerSlider.value = 0f;
            powerSlider.gameObject.SetActive(false);
        }
    }

    void UpdateCrosshair() // Movimiento del crosshair
    {
        if (crosshair == null || canvas == null) return;

        float horizontal = Input.GetAxis("RightStickHorizontal");
        float vertical = -Input.GetAxis("RightStickVertical");
        Vector2 stickInput = new Vector2(horizontal, vertical);

        if (stickInput.magnitude > stickDeadzone)
        {
            currentInputMode = InputMode.Controller;
        }

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            currentInputMode = InputMode.Mouse;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (currentInputMode == InputMode.Controller)
        {
            if (stickInput.magnitude > stickDeadzone)
            {
                Vector2 filteredInput = stickInput.normalized * ((stickInput.magnitude - stickDeadzone) / (1f - stickDeadzone));
                filteredInput = Vector2.ClampMagnitude(filteredInput, 1f);

                crosshairPos += filteredInput * joystickSpeed * Time.deltaTime;
            }
        }
        else
        {
            Vector2 localPoint;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                Input.mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
                out localPoint))
            {
                crosshairPos = localPoint;
            }
        }

        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 halfSize = canvasSize * 0.5f;

        crosshairPos.x = Mathf.Clamp(crosshairPos.x, -halfSize.x, halfSize.x);
        crosshairPos.y = Mathf.Clamp(crosshairPos.y, -halfSize.y, halfSize.y);

        Vector2 finalPosition = crosshairPos;

        if (enableShake)
        {
            shakeTime += Time.deltaTime * shakeSpeed;

            float offsetX = Mathf.PerlinNoise(shakeTime, 0f) - 0.5f;
            float offsetY = Mathf.PerlinNoise(0f, shakeTime) - 0.5f;

            finalPosition += new Vector2(offsetX, offsetY) * currentShakeAmount;
        }

        crosshair.anchoredPosition = finalPosition;
    }

    void PerformAttack() // Atacar al soltar
    {
        if (cam == null || crosshair == null || scoreManager == null || joustManager == null)
            return;

        Ray ray = cam.ScreenPointToRay(crosshair.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / maxChargeTime) * 100f;
            scoreManager.AddAttackScore(hit.collider.tag, GetBF(), GetBL(), chargePercent, 0, 0);
        }

        joustManager.EndAttackPhase();
    }

    public void ForceAttack() // Atacar si te quedas sin tiempo
    {
        if (isCharging)
        {
            isCharging = false;
        }

        if (powerSlider != null)
            powerSlider.gameObject.SetActive(false);

        PerformAttack();
    }
}