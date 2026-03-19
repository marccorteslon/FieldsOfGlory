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
    public RectTransform defenseCircle;
    public RectTransform attackIndicator;
    public Image attackIndicatorImage;

    [Header("Attack Settings")]
    public float circleRadius = 120f;
    public float angleTolerance = 30f;
    public Color indicatorColor = Color.red;

    [Header("Input Settings")]
    public string leftStickHorizontalAxis = "LeftStickHorizontal";
    public string leftStickVerticalAxis = "LeftStickVertical";
    public float minimumStickMagnitude = 0.65f;

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
    }

    void Update()
    {
        if (!joustManager.defensePartIsOn)
        {
            ShowDefenseUI(false);
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
        {
            EndDefense(true);
        }
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

    bool CheckDefenseInput()
    {
        float horizontal = Input.GetAxis(leftStickHorizontalAxis);
        float vertical = Input.GetAxis(leftStickVerticalAxis);

        Vector2 stickInput = new Vector2(horizontal, vertical);

        if (stickInput.magnitude < minimumStickMagnitude)
            return false;

        Vector2 stickDirection = stickInput.normalized;

        float angleDifference = Vector2.Angle(stickDirection, targetDirection);

        Debug.Log($"Stick: {stickDirection} | Target: {targetDirection} | Angle: {angleDifference}");

        return angleDifference <= angleTolerance;
    }

    void ShowDefenseUI(bool show)
    {
        if (defenseCircle != null)
            defenseCircle.gameObject.SetActive(show);

        if (attackIndicator != null)
            attackIndicator.gameObject.SetActive(show);
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

        float horizontal = Input.GetAxis(leftStickHorizontalAxis);
        float vertical = Input.GetAxis(leftStickVerticalAxis);

        Vector2 stickInput = new Vector2(horizontal, vertical);

        // limitar a radio
        if (stickInput.magnitude > 1f)
            stickInput.Normalize();

        joystickVisual.anchoredPosition = stickInput * joystickVisualRadius;
    }

    void EndDefense(bool blockedCorrectly)
    {
        awaitingDefense = false;

        ShowDefenseUI(false);

        scoreManager.ApplyDefense(blockedCorrectly, GetBB());
        joustManager.EndDefensePhase();
    }
}