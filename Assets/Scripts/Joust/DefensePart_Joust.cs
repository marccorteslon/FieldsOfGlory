using UnityEngine;
using UnityEngine.UI;

public class DefensePart_Joust : MonoBehaviour
{
    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Loadout (Ghost Player)")]
    public LoadoutStatsComponent loadout;

    [Header("Fallback Shield Stat")]
    public int fallbackBB = 2;

    [Header("UI Defensa")]
    public RectTransform defenseCircle;
    public RectTransform attackIndicator;
    public Image attackIndicatorImage;

    [Header("Attack Settings")]
    public float circleRadius = 120f;
    public float angleTolerance = 30f;
    public Color indicatorColor = Color.red;

    [Header("Keyboard Input (WASD)")]
    public string keyboardHorizontalAxis = "Horizontal";
    public string keyboardVerticalAxis = "Vertical";

    [Header("Gamepad Input")]
    public string leftStickHorizontalAxis = "LeftStickHorizontal";
    public string leftStickVerticalAxis = "LeftStickVertical";

    [Header("Input Settings")]
    public float minimumInputMagnitude = 0.65f;

    [Header("Joystick Visual")]
    public RectTransform joystickVisual;
    public float joystickVisualRadius = 100f;

    private bool awaitingDefense = false;
    private bool defenseStarted = false;
    private Vector2 targetDirection;

    void Awake()
    {
        if (loadout == null)
            loadout = FindObjectOfType<LoadoutStatsComponent>();
    }

    void OnEnable()
    {
        awaitingDefense = false;
        defenseStarted = false;
        ShowDefenseUI(false);
        ResetJoystickVisual();
    }

    void Update()
    {
        if (!joustManager.defensePartIsOn)
        {
            ShowDefenseUI(false);
            ResetJoystickVisual();
            return;
        }

        if (!defenseStarted)
        {
            defenseStarted = true;
            StartNewAttack();
        }

        UpdateJoystickVisual();

        if (!awaitingDefense)
            return;

        if (CheckDefenseInput())
            EndDefense(true);
    }

    int GetBB()
    {
        if (loadout == null) return fallbackBB;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.BB));
    }

    void StartNewAttack()
    {
        float randomAngle = Random.Range(0f, 360f);
        float radians = randomAngle * Mathf.Deg2Rad;

        targetDirection = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)).normalized;

        UpdateAttackIndicatorVisual();
        ShowDefenseUI(true);
        awaitingDefense = true;
    }

    void UpdateAttackIndicatorVisual()
    {
        if (attackIndicator == null)
            return;

        attackIndicator.anchoredPosition = targetDirection * circleRadius;

        if (attackIndicatorImage != null)
            attackIndicatorImage.color = indicatorColor;
    }

    Vector2 GetCurrentDefenseInput()
    {
        float horizontal;
        float vertical;

        if (InputModeManager.IsGamepad())
        {
            horizontal = Input.GetAxis(leftStickHorizontalAxis);
            vertical = Input.GetAxis(leftStickVerticalAxis);
        }
        else
        {
            horizontal = Input.GetAxis(keyboardHorizontalAxis);
            vertical = Input.GetAxis(keyboardVerticalAxis);
        }

        Vector2 input = new Vector2(horizontal, vertical);

        if (input.magnitude > 1f)
            input.Normalize();

        return input;
    }

    bool CheckDefenseInput()
    {
        Vector2 input = GetCurrentDefenseInput();

        if (input.magnitude < minimumInputMagnitude)
            return false;

        Vector2 inputDirection = input.normalized;
        float angleDifference = Vector2.Angle(inputDirection, targetDirection);

        Debug.Log("Modo input actual: " + InputModeManager.CurrentInputMode +
                  " | Input defensa: " + inputDirection +
                  " | Target: " + targetDirection +
                  " | Angle: " + angleDifference);

        return angleDifference <= angleTolerance;
    }

    void ShowDefenseUI(bool show)
    {
        if (defenseCircle != null)
            defenseCircle.gameObject.SetActive(show);

        if (attackIndicator != null)
            attackIndicator.gameObject.SetActive(show);

        if (joystickVisual != null)
            joystickVisual.gameObject.SetActive(show);
    }

    public void ForceEndDefense(bool blockedCorrectly)
    {
        if (!awaitingDefense)
            return;

        EndDefense(blockedCorrectly);
    }

    void UpdateJoystickVisual()
    {
        if (joystickVisual == null) return;

        Vector2 input = GetCurrentDefenseInput();
        joystickVisual.anchoredPosition = input * joystickVisualRadius;
    }

    void ResetJoystickVisual()
    {
        if (joystickVisual != null)
            joystickVisual.anchoredPosition = Vector2.zero;
    }

    void EndDefense(bool blockedCorrectly)
    {
        awaitingDefense = false;

        ShowDefenseUI(false);
        ResetJoystickVisual();

        scoreManager.ApplyDefense(blockedCorrectly, GetBB());
        joustManager.EndDefensePhase();
    }
}