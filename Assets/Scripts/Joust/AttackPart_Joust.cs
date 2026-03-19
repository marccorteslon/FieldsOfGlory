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
    public string rightStickHorizontalAxis = "RightStickHorizontal";
    public string rightStickVerticalAxis = "RightStickVertical";

    [Header("Gamepad Attack")]
    public string attackAxis = "Attack";
    public float attackPressThreshold = 0.2f;

    [Header("Fallback Lance Stats")]
    public int fallbackBF = 4;
    public int fallbackBL = 2;

    private Vector2 crosshairPos;
    private bool previousAttackState = false;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float currentShakeAmount;
    private float shakeTime;
    private bool gamepadAttackWasHeldLastFrame = false;

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
        crosshairPos = Vector2.zero;
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
                crosshairPos = Vector2.zero;
                crosshair.anchoredPosition = crosshairPos;
                shakeTime = Random.Range(0f, 100f);
            }
        }

        if (!attackStarted) return;

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
        bool startCharge = false;
        bool holdCharge = false;
        bool releaseCharge = false;

        if (InputModeManager.IsGamepad())
        {
            float attackValue = Input.GetAxis(attackAxis);
            bool held = attackValue > attackPressThreshold;

            startCharge = held && !gamepadAttackWasHeldLastFrame && !isCharging;
            holdCharge = held;
            releaseCharge = !held && gamepadAttackWasHeldLastFrame && isCharging;

            gamepadAttackWasHeldLastFrame = held;
        }
        else
        {
            startCharge = Input.GetMouseButtonDown(0) && !isCharging;
            holdCharge = Input.GetMouseButton(0);
            releaseCharge = Input.GetMouseButtonUp(0) && isCharging;
        }

        if (startCharge)
        {
            isCharging = true;
            chargeTimer = 0f;
            powerSlider.gameObject.SetActive(true);
        }

        if (isCharging && holdCharge)
        {
            chargeTimer += Time.deltaTime;

            float percent = Mathf.Clamp01(chargeTimer / maxChargeTime);
            powerSlider.value = percent * 100f;
            currentShakeAmount = baseShakeAmount + (baseShakeAmount * percent);
        }

        if (releaseCharge)
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
        currentShakeAmount = baseShakeAmount;
        gamepadAttackWasHeldLastFrame = false;

        if (powerSlider != null)
            powerSlider.value = 0f;
    }

    void UpdateCrosshair()
    {
        if (InputModeManager.IsGamepad())
            UpdateCrosshairWithGamepad();
        else
            UpdateCrosshairWithMouse();

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

    void UpdateCrosshairWithMouse()
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localPoint))
        {
            crosshairPos = localPoint;
        }
    }

    void UpdateCrosshairWithGamepad()
    {
        float horizontal = Input.GetAxis(rightStickHorizontalAxis);
        float vertical = Input.GetAxis(rightStickVerticalAxis);

        Vector2 stickInput = new Vector2(horizontal, vertical);
        crosshairPos += stickInput * joystickSpeed * Time.deltaTime;

        ClampCrosshairToCanvas();
    }

    void ClampCrosshairToCanvas()
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Rect rect = canvasRect.rect;

        float halfWidth = rect.width * 0.5f;
        float halfHeight = rect.height * 0.5f;

        crosshairPos.x = Mathf.Clamp(crosshairPos.x, -halfWidth, halfWidth);
        crosshairPos.y = Mathf.Clamp(crosshairPos.y, -halfHeight, halfHeight);
    }

    void PerformAttack()
    {
        Vector3 screenPoint = RectTransformUtility.WorldToScreenPoint(
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            crosshair.position
        );

        Ray ray = cam.ScreenPointToRay(screenPoint);

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