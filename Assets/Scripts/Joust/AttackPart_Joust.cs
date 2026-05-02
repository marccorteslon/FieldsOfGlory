using UnityEngine;
using UnityEngine.UI;

public class AttackPart_Joust : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;
    public Canvas canvas;
    public Image powerRadial;

    [Header("Settings")]
    public float maxChargeTime = 2f;
    public float baseShakeAmount = 200f;
    public float shakeSpeed = 25f;
    public bool enableShake = true;

    [Header("Crosshair Scale")]
    public float crosshairStartScale = 1f;
    public float crosshairCriticalScale = 0.35f;

    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Loadout")]
    public LoadoutStatsComponent loadout;

    [Header("Cinematics")]
    public JoustCinematicManager cinematicManager;

    [Header("Controller Aim")]
    public float joystickSpeed = 800f;
    public float stickDeadzone = 0.2f;

    private Vector2 crosshairPos;

    [Header("Fallback Lance Stats")]
    public int fallbackBF = 4;
    public int fallbackBL = 2;

    [Header("Enemy Ragdoll")]
    public EnemyRagdollController enemyRagdoll;

    private bool hasLastHit;
    private Vector3 lastHitPoint;
    private Vector3 lastHitDirection;

    [Header("Timing Bonus")]
    public bool enableTimingBonus = true;
    public float timingCountdown = 1.2f;
    public float timingWindowDuration = 0.25f;
    public int timingBonusPoints = 5;
    public ParticleSystem timingWindowParticles;
    public ParticleSystem timingSuccessParticles;

    private float timingTimer;
    private float timingWindowTimer;
    private bool timingWindowOpen;
    private bool timingWindowConsumed;

    private bool previousAttackState = false;
    private bool attackCameraStartedForThisPhase = false;
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
        if (powerRadial != null)
        {
            powerRadial.gameObject.SetActive(false);
            powerRadial.fillAmount = 0f;
        }

        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(false);
            crosshairPos = crosshair.anchoredPosition;
            crosshair.localScale = Vector3.one * crosshairStartScale;
        }

        SetParticlesActive(timingWindowParticles, false);
        SetParticlesActive(timingSuccessParticles, false);

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
                StartTimingBonusTimer();

                if (crosshair != null)
                    crosshair.localScale = Vector3.one * crosshairStartScale;

                shakeTime = Random.Range(0f, 100f);

                attackCameraStartedForThisPhase = false;
                TryStartAttackCamera();

                if (crosshair != null)
                    crosshairPos = crosshair.anchoredPosition;
            }
            else
            {
                attackCameraStartedForThisPhase = false;
                CloseTimingWindow();
            }
        }

        if (!attackStarted) return;

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        TryStartAttackCamera();

        UpdateTimingBonusTimer();
        UpdateCrosshairScale();
        UpdateCrosshair();
        HandleChargeInput();
    }


    void TryStartAttackCamera()
    {
        if (attackCameraStartedForThisPhase) return;

        if (joustManager != null && joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        attackCameraStartedForThisPhase = true;

        if (cinematicManager != null)
            cinematicManager.StartAttackPhaseCamera();
    }

    void UpdateCrosshairScale()
    {
        if (crosshair == null) return;

        if (!enableTimingBonus)
        {
            crosshair.localScale = Vector3.one * crosshairStartScale;
            return;
        }

        if (timingWindowOpen)
        {
            crosshair.localScale = Vector3.one * crosshairCriticalScale;
            return;
        }

        if (timingWindowConsumed)
            return;

        float t = 1f - Mathf.Clamp01(timingTimer / timingCountdown);
        float scale = Mathf.Lerp(crosshairStartScale, crosshairCriticalScale, t);

        crosshair.localScale = Vector3.one * scale;
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
        bool mouseDown = Input.GetMouseButtonDown(0);
        bool mouseHeld = Input.GetMouseButton(0);
        bool mouseUp = Input.GetMouseButtonUp(0);

        float r2Axis = Input.GetAxis("Attack");
        bool controllerHeld = r2Axis > 0.2f;
        bool controllerDown = controllerHeld && !isCharging;
        bool controllerUp = !controllerHeld && isCharging && currentInputMode == InputMode.Controller;

        if (!isCharging)
        {
            if (mouseDown)
                StartCharge(InputMode.Mouse);
            else if (controllerDown)
                StartCharge(InputMode.Controller);
        }

        if (!isCharging)
            return;

        bool keepCharging;
        bool releaseAttack;

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

            if (powerRadial != null)
                powerRadial.fillAmount = percent * 0.5f;

            currentShakeAmount = baseShakeAmount + (baseShakeAmount * percent);
        }

        if (releaseAttack)
        {
            isCharging = false;

            if (powerRadial != null)
                powerRadial.gameObject.SetActive(false);

            if (cinematicManager != null)
                cinematicManager.OnAttackInputReleased();

            bool timingBonusSuccess = ConsumeTimingBonus();

            PerformAttack(timingBonusSuccess);
        }
    }

    void StartCharge(InputMode mode)
    {
        currentInputMode = mode;
        isCharging = true;
        chargeTimer = 0f;
        currentShakeAmount = baseShakeAmount;

        if (powerRadial != null)
        {
            powerRadial.gameObject.SetActive(true);
            powerRadial.fillAmount = 0f;
        }
    }

    void ResetCharge()
    {
        isCharging = false;
        chargeTimer = 0f;
        currentShakeAmount = baseShakeAmount;

        if (powerRadial != null)
        {
            powerRadial.fillAmount = 0f;
            powerRadial.gameObject.SetActive(false);
        }
    }

    void StartTimingBonusTimer()
    {
        timingTimer = timingCountdown;
        timingWindowTimer = timingWindowDuration;
        timingWindowOpen = false;
        timingWindowConsumed = false;

        SetParticlesActive(timingWindowParticles, false);
        SetParticlesActive(timingSuccessParticles, false);
    }

    void UpdateTimingBonusTimer()
    {
        if (!enableTimingBonus || timingWindowConsumed)
            return;

        if (!timingWindowOpen)
        {
            timingTimer -= Time.deltaTime;

            if (timingTimer <= 0f)
                OpenTimingWindow();
        }
        else
        {
            timingWindowTimer -= Time.deltaTime;

            if (timingWindowTimer <= 0f)
                CloseTimingWindow();
        }
    }

    void OpenTimingWindow()
    {
        timingWindowOpen = true;
        timingWindowTimer = timingWindowDuration;

        if (crosshair != null)
            crosshair.localScale = Vector3.one * crosshairCriticalScale;

        PlayParticles(timingWindowParticles);

        Debug.Log("[Attack Timing] Ventana de bonus abierta.");
    }

    void CloseTimingWindow()
    {
        timingWindowOpen = false;
        SetParticlesActive(timingWindowParticles, false);
    }

    bool ConsumeTimingBonus()
    {
        if (!enableTimingBonus || timingWindowConsumed)
            return false;

        bool success = timingWindowOpen;

        timingWindowConsumed = true;
        timingWindowOpen = false;

        SetParticlesActive(timingWindowParticles, false);

        if (success)
        {
            PlayParticles(timingSuccessParticles);
            Debug.Log($"[Attack Timing] Bonus conseguido: +{timingBonusPoints}");
        }

        return success;
    }

    void PlayParticles(ParticleSystem particles)
    {
        if (particles == null) return;

        particles.gameObject.SetActive(true);
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.Play();
    }

    void SetParticlesActive(ParticleSystem particles, bool active)
    {
        if (particles == null) return;

        if (!active)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        particles.gameObject.SetActive(active);
    }

    void UpdateCrosshair()
    {
        if (crosshair == null || canvas == null) return;

        float horizontal = Input.GetAxis("RightStickHorizontal");
        float vertical = -Input.GetAxis("RightStickVertical");
        Vector2 stickInput = new Vector2(horizontal, vertical);

        if (stickInput.magnitude > stickDeadzone)
            currentInputMode = InputMode.Controller;

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
            currentInputMode = InputMode.Mouse;

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

    void PerformAttack(bool timingBonusSuccess = false)
    {
        if (cam == null || crosshair == null || scoreManager == null || joustManager == null)
            return;

        Ray ray = cam.ScreenPointToRay(crosshair.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / maxChargeTime) * 100f;

            scoreManager.AddAttackScore(hit.collider.tag, GetBF(), GetBL(), chargePercent, 0, 0);

            if (timingBonusSuccess)
            {
                scoreManager.totalScore += timingBonusPoints;
                Debug.Log($"[Attack Timing] Bonus aplicado: +{timingBonusPoints}");
            }

            hasLastHit = true;
            lastHitPoint = hit.point;
            lastHitDirection = (hit.point - cam.transform.position).normalized;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayLanceHit();
        }

        joustManager.EndAttackPhase();
    }

    public void ApplyEnemyImpact(int roundScore, bool fightWon)
    {
        if (enemyRagdoll == null)
            return;

        if (!hasLastHit)
            return;

        enemyRagdoll.PlayImpact(
            lastHitPoint,
            lastHitDirection,
            roundScore,
            fightWon
        );

        hasLastHit = false;
    }

    public void ResetEnemyRagdoll()
    {
        if (enemyRagdoll != null)
            enemyRagdoll.ResetRagdoll();

        hasLastHit = false;
    }

    public void ForceAttack()
    {
        if (isCharging)
            isCharging = false;

        if (powerRadial != null)
            powerRadial.gameObject.SetActive(false);

        if (cinematicManager != null)
            cinematicManager.OnAttackInputReleased();

        PerformAttack(false);
    }
}