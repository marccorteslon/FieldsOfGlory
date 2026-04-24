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
    private DefenseDirection targetDefenseDirection;

    private enum DefenseDirection
    {
        Up,
        Down,
        Left,
        Right
    }

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

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

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
        targetDefenseDirection = (DefenseDirection)Random.Range(0, 4);
        targetDirection = GetVectorFromDirection(targetDefenseDirection);

        UpdateAttackIndicatorVisual();
        ShowDefenseUI(true);

        awaitingDefense = true;
    }

    Vector2 GetVectorFromDirection(DefenseDirection direction)
    {
        switch (direction)
        {
            case DefenseDirection.Up:
                return Vector2.up;
            case DefenseDirection.Down:
                return Vector2.down;
            case DefenseDirection.Left:
                return Vector2.left;
            case DefenseDirection.Right:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
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
        if (Input.GetKeyDown(KeyCode.UpArrow))
            return targetDefenseDirection == DefenseDirection.Up;

        if (Input.GetKeyDown(KeyCode.DownArrow))
            return targetDefenseDirection == DefenseDirection.Down;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            return targetDefenseDirection == DefenseDirection.Left;

        if (Input.GetKeyDown(KeyCode.RightArrow))
            return targetDefenseDirection == DefenseDirection.Right;

        float horizontal = Input.GetAxisRaw(leftStickHorizontalAxis);
        float vertical = Input.GetAxisRaw(leftStickVerticalAxis);
        Vector2 stickInput = new Vector2(horizontal, vertical);

        if (stickInput.magnitude < minimumStickMagnitude)
            return false;

        DefenseDirection inputDirection = GetDirectionFromInput(stickInput);
        return inputDirection == targetDefenseDirection;
    }

    DefenseDirection GetDirectionFromInput(Vector2 input)
    {
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return input.x > 0f ? DefenseDirection.Right : DefenseDirection.Left;

        return input.y > 0f ? DefenseDirection.Up : DefenseDirection.Down;
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

        float horizontal = Input.GetAxisRaw(leftStickHorizontalAxis);
        float vertical = Input.GetAxisRaw(leftStickVerticalAxis);
        Vector2 stickInput = new Vector2(horizontal, vertical);

        if (stickInput.magnitude < minimumStickMagnitude)
        {
            joystickVisual.anchoredPosition = Vector2.zero;
            return;
        }

        Vector2 snappedDirection = GetVectorFromDirection(GetDirectionFromInput(stickInput));
        joystickVisual.anchoredPosition = snappedDirection * joystickVisualRadius;
    }

    void EndDefense(bool blockedCorrectly)
    {
        awaitingDefense = false;

        ShowDefenseUI(false);

        scoreManager.ApplyDefense(blockedCorrectly, GetBB());
        joustManager.EndDefensePhase();
    }
}
