using System.Collections;
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
    public JoustCinematicManager cinematicManager;
    public Transform horseCameraPoint;
    public Transform attackCameraPoint;
    public Transform defenseCameraPoint;

    [Header("Camera Follow")]
    public float followSpeed = 5f;
    private Transform currentCameraPoint;
    [Header("Camera Control")]
    public bool lockCameraToPoints = true;

    [Header("Win System")]
    public WinManager winManager;

    [Header("Joust Movement")]
    public Transform player;
    public Transform enemy;

    public float horsePhaseSpeed = 10f;
    public float combatPhaseSpeed = 4f;
    private float currentSpeed;

    [Header("Effects")]
    public EffectManager effectManager;

    [Header("Pre Joust Intro")]
    public bool usePreJoustIntro = true;

    [Tooltip("Duracion de la Overview Cam antes de que empiece a caminar el jugador.")]
    public float overviewCamDuration = 4f;

    [Tooltip("Punto inicial desde el que se movera la Overview Cam durante la intro.")]
    public Transform overviewCamStartPoint;

    [Tooltip("Punto final hasta el que se movera la Overview Cam durante la intro.")]
    public Transform overviewCamEndPoint;

    [Tooltip("Duracion del movimiento del jugador mientras se muestra la WalkingPlayerCam.")]
    public float preJoustMoveDuration = 4f;

    public float preJoustFinalPause = 1f;

    public Transform[] playerPreJoustWaypoints;

    public bool snapPlayerToFirstWaypoint = true;

    private bool preJoustIntroRunning = false;
    private bool joustStarted = false;
    private Coroutine preJoustIntroCoroutine;

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

        [Header("Difficulty Settings")]
    public JoustDifficulty difficulty = JoustDifficulty.Normal;

    void Start()
    {
        ApplyDifficulty();
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

        PrepareBeforeJoustStarts();

        if (usePreJoustIntro)
            preJoustIntroCoroutine = StartCoroutine(PreJoustIntroSequence());
        else
            StartJoustNormally();
    }

    
        void ApplyDifficulty()
    {
        int basePoints = 10;

        switch (difficulty)
        {
            case JoustDifficulty.Easy:
                horsePhaseDuration = 6f;
                attackDuration = 4f;
                defenseDuration = 2.5f;
                horsePhaseSpeed = 9f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints;
                break;
            case JoustDifficulty.Normal:
                horsePhaseDuration = 5f;
                attackDuration = 3f;
                defenseDuration = 2f;
                horsePhaseSpeed = 12f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints + 10; // 50
                break;
            case JoustDifficulty.Hard:
                horsePhaseDuration = 4f;
                attackDuration = 2f;
                defenseDuration = 1.5f;
                horsePhaseSpeed = 13f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints + 20; // 80
                break;
            case JoustDifficulty.Epic:
                horsePhaseDuration = 3.5f;
                attackDuration = 1.5f;
                defenseDuration = 1f;
                horsePhaseSpeed = 15f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints + 30; // 120
                break;
        }
    }

    void Update()
    {
        if (!joustStarted || preJoustIntroRunning)
            return;

        MoveJousters();
        HandleHorseTimer();
        HandleTransitionTimer();
        HandleAttackTimer();
        HandleDefenseTimer();
    }

    void LateUpdate()
    {
        if (preJoustIntroRunning && cinematicManager != null) return;
        
        if (!lockCameraToPoints) return;
        
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

    void PrepareBeforeJoustStarts()
    {
        joustStarted = false;
        preJoustIntroRunning = false;

        horsePartIsOn = false;
        attackPartIsOn = false;
        defensePartIsOn = false;

        horseTimerRunning = false;
        attackTimerRunning = false;
        defenseTimerRunning = false;
        waitingToStartCombat = false;

        UpdatePhases();
    }

    IEnumerator PreJoustIntroSequence()
    {
        preJoustIntroRunning = true;

        if (player != null && snapPlayerToFirstWaypoint && playerPreJoustWaypoints != null && playerPreJoustWaypoints.Length > 0 && playerPreJoustWaypoints[0] != null)
        {
            player.position = playerPreJoustWaypoints[0].position;
            player.rotation = playerPreJoustWaypoints[0].rotation;
        }

        yield return StartCoroutine(PlayOverviewCamIntro());

        if (cinematicManager != null)
            cinematicManager.StartWalkingPlayerCamera();

        yield return StartCoroutine(MovePlayerThroughPreJoustWaypoints());

        if (preJoustFinalPause > 0f)
            yield return new WaitForSeconds(preJoustFinalPause);

        preJoustIntroRunning = false;
        StartJoustNormally();
    }

    IEnumerator PlayOverviewCamIntro()
    {
        if (cinematicManager != null)
            cinematicManager.StartOverviewCamera();

        if (cinematicManager == null || cinematicManager.OverviewCam == null || overviewCamDuration <= 0f)
            yield break;

        Transform overviewCamTransform = cinematicManager.OverviewCam.transform;

        Vector3 startPosition = overviewCamStartPoint != null ? overviewCamStartPoint.position : overviewCamTransform.position;
        Quaternion startRotation = overviewCamStartPoint != null ? overviewCamStartPoint.rotation : overviewCamTransform.rotation;

        Vector3 endPosition = overviewCamEndPoint != null ? overviewCamEndPoint.position : startPosition;
        Quaternion endRotation = overviewCamEndPoint != null ? overviewCamEndPoint.rotation : startRotation;

        overviewCamTransform.position = startPosition;
        overviewCamTransform.rotation = startRotation;

        float elapsed = 0f;
        while (elapsed < overviewCamDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / overviewCamDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            overviewCamTransform.position = Vector3.Lerp(startPosition, endPosition, smoothT);
            overviewCamTransform.rotation = Quaternion.Slerp(startRotation, endRotation, smoothT);

            yield return null;
        }

        overviewCamTransform.position = endPosition;
        overviewCamTransform.rotation = endRotation;
    }

    IEnumerator MovePlayerThroughPreJoustWaypoints()
    {
        if (player == null || playerPreJoustWaypoints == null || playerPreJoustWaypoints.Length == 0 || preJoustMoveDuration <= 0f)
            yield break;

        Transform[] validWaypoints = GetValidPreJoustWaypoints();
        if (validWaypoints.Length == 0)
            yield break;

        if (validWaypoints.Length == 1)
        {
            player.position = validWaypoints[0].position;
            player.rotation = validWaypoints[0].rotation;
            yield break;
        }

        float totalDistance = GetTotalWaypointDistance(validWaypoints);
        float fallbackSegmentDuration = preJoustMoveDuration / (validWaypoints.Length - 1);

        for (int i = 0; i < validWaypoints.Length - 1; i++)
        {
            Transform startPoint = validWaypoints[i];
            Transform endPoint = validWaypoints[i + 1];

            Vector3 startPosition = player.position;
            Quaternion startRotation = player.rotation;

            Vector3 endPosition = endPoint.position;
            Quaternion endRotation = endPoint.rotation;

            float segmentDistance = Vector3.Distance(startPoint.position, endPoint.position);
            float segmentDuration = totalDistance > 0f
                ? preJoustMoveDuration * (segmentDistance / totalDistance)
                : fallbackSegmentDuration;

            if (segmentDuration <= 0f)
            {
                player.position = endPosition;
                player.rotation = endRotation;
                continue;
            }

            float elapsed = 0f;
            while (elapsed < segmentDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segmentDuration);
                float smoothT = Mathf.SmoothStep(0f, 1f, t);

                player.position = Vector3.Lerp(startPosition, endPosition, smoothT);
                player.rotation = Quaternion.Slerp(startRotation, endRotation, smoothT);

                yield return null;
            }

            player.position = endPosition;
            player.rotation = endRotation;
        }
    }

    Transform[] GetValidPreJoustWaypoints()
    {
        int count = 0;

        for (int i = 0; i < playerPreJoustWaypoints.Length; i++)
        {
            if (playerPreJoustWaypoints[i] != null)
                count++;
        }

        Transform[] validWaypoints = new Transform[count];
        int index = 0;

        for (int i = 0; i < playerPreJoustWaypoints.Length; i++)
        {
            if (playerPreJoustWaypoints[i] != null)
            {
                validWaypoints[index] = playerPreJoustWaypoints[i];
                index++;
            }
        }

        return validWaypoints;
    }

    float GetTotalWaypointDistance(Transform[] waypoints)
    {
        float totalDistance = 0f;

        for (int i = 0; i < waypoints.Length - 1; i++)
            totalDistance += Vector3.Distance(waypoints[i].position, waypoints[i + 1].position);

        return totalDistance;
    }

    void StartJoustNormally()
    {
        joustStarted = true;

        if (effectManager != null)
            effectManager.ChooseRandomEffect();

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

        currentCameraPoint = horseCameraPoint;

        if (cinematicManager != null)
            cinematicManager.StartHorsePhaseCamera();

        UpdatePhases();

        if (horsePart != null)
            horsePart.ResetHorsePhase();

        if (tutorialManager != null && tutorialManager.ShouldShowTutorial())
            tutorialManager.ShowHorseTutorial();
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

        if (preJoustIntroRunning)
        {
            controlsText.text = "";
        }
        else if (horsePartIsOn)
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
        if (preJoustIntroCoroutine != null)
        {
            StopCoroutine(preJoustIntroCoroutine);
            preJoustIntroCoroutine = null;
        }

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

        PrepareBeforeJoustStarts();

        if (horsePart != null)
            horsePart.ResetHorsePhase();

        if (usePreJoustIntro)
            preJoustIntroCoroutine = StartCoroutine(PreJoustIntroSequence());
        else
            StartJoustNormally();
    }
}

