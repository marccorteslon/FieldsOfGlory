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
        powerSlider.gameObject.SetActive(false);
        powerSlider.minValue = 0;
        powerSlider.maxValue = 100;

        crosshair.gameObject.SetActive(false);
        crosshairPos = crosshair.anchoredPosition;
        currentShakeAmount = baseShakeAmount;
    }

    void Update()
    {
        bool attackStarted = joustManager.attackPartIsOn;

        if (attackStarted != previousAttackState)
        {
            crosshair.gameObject.SetActive(attackStarted);
            previousAttackState = attackStarted;

            if (attackStarted)
            {
                ResetCharge();
                shakeTime = Random.Range(0f, 100f);

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
        float r2Axis = Input.GetAxis("Attack");
        bool controllerHeld = r2Axis > 0.2f;
        bool controllerDown = r2Axis > 0.2f && !isCharging;
        bool controllerUp = r2Axis <= 0.2f && isCharging;

        bool mouseHeld = Input.GetMouseButton(0);
        bool mouseDown = Input.GetMouseButtonDown(0) && !isCharging;
        bool mouseUp = Input.GetMouseButtonUp(0) && isCharging;

        if (controllerDown || mouseDown)
        {
            isCharging = true;
            chargeTimer = 0f;
            powerSlider.gameObject.SetActive(true);
        }

        if (isCharging && (controllerHeld || mouseHeld))
        {
            chargeTimer += Time.deltaTime;

            float percent = Mathf.Clamp01(chargeTimer / maxChargeTime);
            powerSlider.value = percent * 100f;
            currentShakeAmount = baseShakeAmount + (baseShakeAmount * percent);
        }

        if (controllerUp || mouseUp)
        {
            isCharging = false;
            powerSlider.gameObject.SetActive(false);
            PerformAttack();
        }
    }

    void ResetCharge()
    {
        isCharging = false;
        chargeTimer = 0f;
        powerSlider.value = 0;
        currentShakeAmount = baseShakeAmount;
    }

    void UpdateCrosshair()
    {
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

    void PerformAttack()
    {
        Ray ray = cam.ScreenPointToRay(crosshair.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / maxChargeTime) * 100f;
            scoreManager.AddAttackScore(hit.collider.tag, GetBF(), GetBL(), chargePercent, 0, 0);
        }

        joustManager.EndAttackPhase();
    }

    public void ForceAttack()
    {
        if (isCharging)
        {
            isCharging = false;
            powerSlider.gameObject.SetActive(false);
        }

        PerformAttack();
    }
}