using UnityEngine;
using UnityEngine.UI;

public class DefensePart_Joust : MonoBehaviour
{
    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Loadout (Ghost Player)")]
    public LoadoutStatsComponent loadout;

    [Header("Fallback Shield Stat (si no hay loadout)")]
    public int fallbackBB = 2;

    [Header("UI Defensa")]
    [Tooltip("Panel principal que engloba toda la interfaz de defensa. Se activará cuando empiece la fase.")]
    public GameObject defensePanel;
    public RectTransform defenseCircle;
    public RectTransform attackIndicator;
    public Image attackIndicatorImage;

    [Header("UI Tracking")]
    public Image joystickImage;
    public Color trackingColor = Color.green;
    public Color defaultColor = Color.white;

    [Header("Attack Settings (Difficulty Scaled)")]
    public float circleRadius = 120f;
    public Color indicatorColor = Color.red;
    public float attackMoveSpeed = 1f;
    public float captureDistanceTolerance = 40f;
    public float requiredCaptureTime = 1f;

    [Header("Input Settings")]
    public string leftStickHorizontalAxis = "LeftStickHorizontal";
    public string leftStickVerticalAxis = "LeftStickVertical";
    public float minimumStickMagnitude = 0.2f;

    [Header("Joystick Visual")]
    public RectTransform joystickVisual;
    public float joystickVisualRadius = 120f;
    public float pointerMoveSpeed = 600f; // Velocidad del puntero del escudo

    private bool awaitingDefense = false;
    private bool defenseStarted = false;

    private float attackAngle = 0f;
    private float captureProgress = 0f;

    private float timeSinceAttackStart = 0f;
    private float directionMultiplier = -1f; // -1 = Clockwise, 1 = Counter-Clockwise
    private bool willFlipDirection = false;
    private bool hasFlippedDirection = false;
    private float flipTime = 0f;

    void Awake()
    {
        if (loadout == null)
        {
            GameObject ghost = GameObject.Find("GhostPlayer");
            if (ghost != null)
                loadout = ghost.GetComponent<LoadoutStatsComponent>();
            else
                loadout = FindFirstObjectByType<LoadoutStatsComponent>();
        }
        
        if (joystickImage == null && joystickVisual != null)
            joystickImage = joystickVisual.GetComponent<Image>();
    }

    void OnEnable()
    {
        awaitingDefense = false;
        defenseStarted = false;
        ShowDefenseUI(false);
    }

    void Update()
    {
        if (!joustManager.defensePartIsOn)
        {
            ShowDefenseUI(false);
            return;
        }

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        if (!defenseStarted)
        {
            defenseStarted = true;
            StartNewAttack();
        }

        if (!awaitingDefense)
            return;

        UpdateAttackMovement();
        UpdateJoystickVisual();
        CheckCaptureLogic();
    }

    int GetBB()
    {
        if (loadout == null) return fallbackBB;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.BB));
    }

    void StartNewAttack()
    {
        attackAngle = Random.Range(0f, Mathf.PI * 2f);
        captureProgress = 0f;

        timeSinceAttackStart = 0f;
        directionMultiplier = -1f; // Siempre empieza clockwise
        hasFlippedDirection = false;
        
        // 40% de probabilidad de que decida darse la vuelta a medias
        willFlipDirection = Random.value < 0.4f; 
        // El giro ocurrirá entre 0.4s y 1.2s después de empezar
        flipTime = Random.Range(0.4f, 1.2f);

        ShowDefenseUI(true);
        awaitingDefense = true;
    }

    void UpdateAttackMovement()
    {
        if (attackIndicator == null) return;

        timeSinceAttackStart += Time.unscaledDeltaTime;

        if (willFlipDirection && !hasFlippedDirection && timeSinceAttackStart >= flipTime)
        {
            directionMultiplier = 1f; // Cambia a counter-clockwise
            hasFlippedDirection = true;
        }

        // Movimiento constante en la dirección actual
        attackAngle += directionMultiplier * attackMoveSpeed * Time.unscaledDeltaTime * 3f;

        Vector2 newPos = new Vector2(Mathf.Cos(attackAngle), Mathf.Sin(attackAngle)) * circleRadius;
        attackIndicator.anchoredPosition = newPos;

        if (attackIndicatorImage != null)
            attackIndicatorImage.color = indicatorColor;
    }

    void UpdateJoystickVisual()
    {
        if (joystickVisual == null) return;

        float horizontal = Input.GetAxisRaw(leftStickHorizontalAxis);
        float vertical = Input.GetAxisRaw(leftStickVerticalAxis);

        // Soporte de fallback para teclado en Editor
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) horizontal = -1f;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) horizontal = 1f;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) vertical = 1f;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) vertical = -1f;

        Vector2 stickInput = new Vector2(horizontal, vertical);
        if (stickInput.magnitude > 1f) stickInput.Normalize();

        Vector2 targetPosition = Vector2.zero;
        if (stickInput.magnitude >= minimumStickMagnitude)
        {
            targetPosition = stickInput * joystickVisualRadius;
        }

        // Interpolamos la posición suavemente usando MoveTowards con unscaledDeltaTime
        joystickVisual.anchoredPosition = Vector2.MoveTowards(
            joystickVisual.anchoredPosition, 
            targetPosition, 
            Time.unscaledDeltaTime * pointerMoveSpeed
        );
    }

    void CheckCaptureLogic()
    {
        if (joystickVisual == null || attackIndicator == null) return;

        float distance = Vector2.Distance(joystickVisual.anchoredPosition, attackIndicator.anchoredPosition);

        if (distance <= captureDistanceTolerance)
        {
            captureProgress += Time.unscaledDeltaTime;
            if (joystickImage != null) joystickImage.color = trackingColor;

            if (captureProgress >= requiredCaptureTime)
            {
                EndDefense(true); // Bloqueo completado con éxito
            }
        }
        else
        {
            if (joystickImage != null) joystickImage.color = defaultColor;
        }
    }

    void ShowDefenseUI(bool show)
    {
        if (defensePanel != null) defensePanel.SetActive(show);
        if (defenseCircle != null) defenseCircle.gameObject.SetActive(show);
        if (attackIndicator != null) attackIndicator.gameObject.SetActive(show);
    }

    public void ForceEndDefense(bool blockedCorrectly)
    {
        if (!awaitingDefense) return;
        EndDefense(blockedCorrectly);
    }

    void EndDefense(bool blockedCorrectly)
    {
        awaitingDefense = false;
        ShowDefenseUI(false);

        scoreManager.ApplyDefense(blockedCorrectly, GetBB());
        joustManager.EndDefensePhase();
    }
}
