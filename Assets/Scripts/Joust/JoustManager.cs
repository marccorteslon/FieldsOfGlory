using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class JoustManager : MonoBehaviour
{
    [Header("Phase States")]
    public bool horsePartIsOn = true;
    public bool attackPartIsOn = false;
    public bool defensePartIsOn = false;

    [Header("Phase Scripts")]
    public HorsePart_Joust horsePart;
    public AttackPart_Joust attackPart;
    public DefensePart_Joust defensePart;

    [Header("Camera References")]
    public Camera mainCamera;
    public Transform horseCameraPoint;
    public Transform attackCameraPoint;
    public Transform defenseCameraPoint;

    [Header("Camera Follow")]
    public float followSpeed = 5f;
    private Transform currentCameraPoint;

    [Header("Win System")]
    public WinManager winManager;

    [Header("Joust Movement")]
    public Transform player;
    public Transform enemy;

    public float horsePhaseSpeed = 10f;
    public float combatPhaseSpeed = 4f;
    private float currentSpeed;

    [Header("Horse Phase Timer")]
    public float horsePhaseDuration = 5f;
    private float horseTimer = 0f;
    private bool horseTimerRunning = false;

    [Header("Transition")]
    public float delayBetweenHorseAndAttack = 0.5f;
    private float transitionTimer = 0f;
    private bool waitingToStartCombat = false;

    [Header("Attack Timer")]
    public float attackDuration = 3f;
    private float attackTimer = 0f;
    private bool attackTimerRunning = false;

    [Header("Defense Timer")]
    public float defenseDuration = 2f;
    private float defenseTimer = 0f;
    private bool defenseTimerRunning = false;

    private bool attackResolved = false;
    private bool defenseResolved = false;

    [Header("Controls UI")]
    public TextMeshProUGUI controlsText;

    [Header("Tutorial")]
    public JoustTutorialManager tutorialManager;

    [HideInInspector] public Vector3 initialPlayerPos;
    [HideInInspector] public Quaternion initialPlayerRot;
    [HideInInspector] public Vector3 initialEnemyPos;
    [HideInInspector] public Quaternion initialEnemyRot;

    void Start()
    {
        if (player != null)
        {
            initialPlayerPos = player.position;
            initialPlayerRot = player.rotation;
        }

        if (enemy != null)
        {
            initialEnemyPos = enemy.position;
            initialEnemyRot = enemy.rotation;
        }

        if (mainCamera != null && horseCameraPoint != null)
        {
            mainCamera.transform.position = horseCameraPoint.position;
            mainCamera.transform.rotation = horseCameraPoint.rotation;
        }

        currentCameraPoint = horseCameraPoint;
        currentSpeed = horsePhaseSpeed;
        horseTimerRunning = true;

        UpdatePhases();

        if (tutorialManager != null && tutorialManager.ShouldShowTutorial())
        {
            tutorialManager.ShowHorseTutorial();
        }
    }

    void Update()
    {
        MoveJousters();
        HandleHorseTimer();
        HandleTransitionTimer();
        HandleAttackTimer();
        HandleDefenseTimer();
    }

    void LateUpdate()
    {
        if (mainCamera == null || currentCameraPoint == null) return;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            currentCameraPoint.position,
            Time.deltaTime * followSpeed
        );

        mainCamera.transform.rotation = Quaternion.Slerp(
            mainCamera.transform.rotation,
            currentCameraPoint.rotation,
            Time.deltaTime * followSpeed
        );
    }

    void MoveJousters()
    {
        if (player != null)
            player.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        if (enemy != null)
            enemy.Translate(Vector3.back * currentSpeed * Time.deltaTime);
    }

    void HandleHorseTimer()
    {
        if (!horseTimerRunning) return;

        horseTimer += Time.deltaTime;

        if (horseTimer >= horsePhaseDuration)
        {
            horseTimerRunning = false;
            EndHorsePhase();
        }
    }

    void HandleTransitionTimer()
    {
        if (!waitingToStartCombat) return;

        transitionTimer += Time.deltaTime;

        if (transitionTimer >= delayBetweenHorseAndAttack)
        {
            waitingToStartCombat = false;
            StartCombatPhase();
        }
    }

    void HandleAttackTimer()
    {
        if (!attackTimerRunning) return;

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackDuration)
        {
            attackTimerRunning = false;

            if (attackPart != null)
            {
                attackPart.ForceAttack();
            }
        }
    }

    void HandleDefenseTimer()
    {
        if (!defenseTimerRunning) return;

        defenseTimer += Time.deltaTime;

        if (defenseTimer >= defenseDuration)
        {
            defenseTimerRunning = false;

            if (defensePart != null)
            {
                defensePart.ForceEndDefense(false);
            }
        }
    }

    public void UpdatePhases()
    {
        if (horsePart != null)
            horsePart.gameObject.SetActive(horsePartIsOn);

        if (attackPart != null)
            attackPart.gameObject.SetActive(attackPartIsOn);

        if (defensePart != null)
            defensePart.gameObject.SetActive(defensePartIsOn);

        UpdateControlsUI();
    }

    void UpdateControlsUI()
    {
        if (controlsText == null) return;

        if (horsePartIsOn)
        {
            controlsText.text = "X (Mando) -> Cargar caballo";
        }
        else if (attackPartIsOn && defensePartIsOn)
        {
            controlsText.text =
                "ATAQUE: Ratón + Mantener/Soltar Click Izq / Stick Der + R2\n" +
                "DEFENSA: Stick Izq -> Bloquear dirección";
        }
        else if (attackPartIsOn)
        {
            controlsText.text =
                "PC: Ratón + Mantener/Soltar Click Izq\n" +
                "Mando: Stick Der + R2\n";
        }
        else if (defensePartIsOn)
        {
            controlsText.text =
                "Stick Izq -> Bloquear direccion";
        }
        else
        {
            controlsText.text = "";
        }
    }

    public void EndHorsePhase()
    {
        if (horsePart != null)
            horsePart.ForceEndHorsePhase();

        horsePartIsOn = false;
        attackPartIsOn = false;
        defensePartIsOn = false;

        waitingToStartCombat = true;
        transitionTimer = 0f;

        UpdatePhases();
    }

    void StartCombatPhase()
    {
        attackPartIsOn = true;
        defensePartIsOn = true;
        attackResolved = false;
        defenseResolved = false;

        currentSpeed = combatPhaseSpeed;
        currentCameraPoint = attackCameraPoint;

        attackTimer = 0f;
        attackTimerRunning = true;

        defenseTimer = 0f;
        defenseTimerRunning = true;

        UpdatePhases();

        if (tutorialManager != null && tutorialManager.ShouldShowTutorial())
            tutorialManager.ShowAttackTutorial();
    }

    public void EndAttackPhase()
    {
        if (attackResolved) return;

        attackResolved = true;
        attackTimerRunning = false;
        attackPartIsOn = false;

        UpdatePhases();
        TryEndCombatPhase();
    }

    public void EndDefensePhase()
    {
        if (defenseResolved) return;

        defenseResolved = true;
        defenseTimerRunning = false;
        defensePartIsOn = false;

        UpdatePhases();
        TryEndCombatPhase();
    }

    void TryEndCombatPhase()
    {
        if (!attackResolved || !defenseResolved)
            return;

        Debug.Log("La justa ha terminado.");

        if (winManager != null)
        {
            winManager.ProcessRoundEnd();
        }
    }

    public void ResetPositions()
    {
        if (player != null)
        {
            player.position = initialPlayerPos;
            player.rotation = initialPlayerRot;
        }

        if (enemy != null)
        {
            enemy.position = initialEnemyPos;
            enemy.rotation = initialEnemyRot;
        }

        if (mainCamera != null && horseCameraPoint != null)
        {
            mainCamera.transform.position = horseCameraPoint.position;
            mainCamera.transform.rotation = horseCameraPoint.rotation;
            currentCameraPoint = horseCameraPoint;
        }

        horsePartIsOn = true;
        attackPartIsOn = false;
        defensePartIsOn = false;

        attackResolved = false;
        defenseResolved = false;

        waitingToStartCombat = false;
        transitionTimer = 0f;

        currentSpeed = horsePhaseSpeed;
        horseTimer = 0f;
        horseTimerRunning = true;

        attackTimer = 0f;
        attackTimerRunning = false;

        defenseTimer = 0f;
        defenseTimerRunning = false;

        UpdatePhases();

        if (horsePart != null)
            horsePart.ResetHorsePhase();
    }
}