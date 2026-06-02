using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

[System.Serializable]
public struct CityMapMapping
{
    public string cityId;
    public GameObject mapGameObject;
}

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

    [Header("Horse Customization")]
    public string horseChildName = "Horse";

    public float horsePhaseSpeed = 10f;
    public float combatPhaseSpeed = 4f;
    [HideInInspector] public float currentSpeed;

    [Header("Effects")]
    public EffectManager effectManager;

    [Header("Effect Choice")]
    public bool useEffectChoiceButtons = true;
    private bool waitingForEffectChoice = false;

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

    [Header("Pre Joust Horse Animation")]
    public Animator playerHorseAnimator;
    public string horseSpeedParameter = "Speed";
    public float preJoustRunAnimSpeed = 0.5f;
    public float preJoustIdleAnimSpeed = 0f;

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

    // Los timers ciegos de ataque y defensa han sido eliminados.
    // La fase de combate terminará de forma realista calculando la distancia Z entre los dos caballos.

    private bool attackResolved = false;
    private bool defenseResolved = false;

    [Header("Controls UI")]
    public TextMeshProUGUI controlsText;

    [Header("Tutorial")]
    public JoustTutorialManager tutorialManager;
    [Tooltip("Activar en la escena NewTutorial. El enemigo no se mueve y se salta la intro/torneo.")]
    public bool isTutorialMode = false;

    [HideInInspector] public Vector3 initialPlayerPos;
    [HideInInspector] public Quaternion initialPlayerRot;
    [HideInInspector] public Vector3 initialEnemyPos;
    [HideInInspector] public Quaternion initialEnemyRot;

    private Transform playerVisualHorse;
    private Transform enemyVisualHorse;

    [Header("Difficulty Settings")]
    public JoustDifficulty difficulty = JoustDifficulty.Normal;

    [Header("Enemy Meshes by Difficulty")]
    public Mesh easyEnemyMesh;
    public Mesh normalEnemyMesh;
    public Mesh hardEnemyMesh;
    public Mesh epicEnemyMesh;

    [Header("Enemy Mesh Customization References")]
    [Tooltip("El SkinnedMeshRenderer del oponente al que se le cambiará la malla.")]
    public SkinnedMeshRenderer enemyArmorRenderer;
    [Tooltip("Alternativa si el enemigo usa MeshFilter tradicional.")]
    public MeshFilter enemyArmorMeshFilter;

    public List<CityMapMapping> cityMaps = new();
    public GameObject defaultMap;

    void Start()
    {
        // Asegurar que el cursor esté visible y desbloqueado en la escena de la justa/tutorial
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (isTutorialMode)
        {
            // --- TUTORIAL MODE: saltar torneo/dificultad, usar valores fáciles ---
            difficulty = JoustDifficulty.Easy;
            usePreJoustIntro = false;
            useEffectChoiceButtons = false;

            // Activar solo el mapa por defecto
            {
                GameObject activeMapObject = defaultMap;
                HashSet<GameObject> processedMaps = new HashSet<GameObject>();

                foreach (var mapping in cityMaps)
                {
                    if (mapping.mapGameObject == null) continue;
                    if (processedMaps.Contains(mapping.mapGameObject)) continue;
                    processedMaps.Add(mapping.mapGameObject);

                    mapping.mapGameObject.SetActive(mapping.mapGameObject == activeMapObject);
                }

                if (defaultMap != null && !processedMaps.Contains(defaultMap))
                {
                    defaultMap.SetActive(defaultMap == activeMapObject);
                }
            }

            // Desactivar el Animator del Dummy para que no reproduzca "pushed" al inicio
            if (enemy != null)
            {
                Animator dummyAnimator = enemy.GetComponentInChildren<Animator>();
                if (dummyAnimator != null)
                {
                    dummyAnimator.enabled = false;
                    Debug.Log("[JoustManager] Animator del Dummy desactivado hasta que reciba un impacto.");
                }
            }

            Debug.Log("[JoustManager] Tutorial Mode activado. Dificultad: Easy, sin intro, sin cartas.");
        }
        else
        {
            // 1. Resolver dificultad y ciudad actual dinámicamente
            ProgressManager progressManager = FindFirstObjectByType<ProgressManager>();
            TournamentManager tournamentManager = FindFirstObjectByType<TournamentManager>();
            string activeCityId = "";

            if (progressManager != null)
            {
                activeCityId = progressManager.CurrentCityId;

                if (ProgressManager.PracticeDifficultyOverride.HasValue)
                {
                    difficulty = ProgressManager.PracticeDifficultyOverride.Value;
                    ProgressManager.PracticeDifficultyOverride = null;
                    Debug.Log($"[JoustManager] Difficulty override detected: {difficulty}");
                }
                else if (tournamentManager != null)
                {
                    var todayTournament = tournamentManager.GetTournamentForCityAndDate(
                        progressManager.CurrentCityId,
                        progressManager.CurrentDay,
                        progressManager.CurrentMonth
                    );

                    if (todayTournament != null)
                    {
                        difficulty = todayTournament.difficulty;
                        Debug.Log($"[JoustManager] Dificultad del torneo de hoy detectada: {difficulty}");
                    }
                    else
                    {
                        Debug.LogWarning($"[JoustManager] No hay torneo hoy en {progressManager.CurrentCityId}. Se mantiene dificultad: {difficulty}");
                    }
                }
            }

            // 2. Activar/Desactivar escenarios locales basados en la ciudad actual
            Debug.Log($"[JoustManager] Ciudad activa leída del ProgressManager: '{activeCityId}'");
            Debug.Log($"[JoustManager] Entradas en cityMaps: {cityMaps.Count}");
            for (int i = 0; i < cityMaps.Count; i++)
            {
                var m = cityMaps[i];
                Debug.Log($"[JoustManager]   [{i}] cityId='{m.cityId}' | GameObject={(m.mapGameObject != null ? m.mapGameObject.name : "NULL")}");
            }

            {
                // Primero encontramos qué GameObject debe activarse
                GameObject activeMapObject = null;
                if (!string.IsNullOrEmpty(activeCityId))
                {
                    foreach (var mapping in cityMaps)
                    {
                        if (mapping.mapGameObject != null &&
                            string.Equals(mapping.cityId, activeCityId, System.StringComparison.OrdinalIgnoreCase))
                        {
                            activeMapObject = mapping.mapGameObject;
                            Debug.Log($"[JoustManager] ✓ Escenario encontrado para '{activeCityId}' → '{activeMapObject.name}'");
                            break;
                        }
                    }
                }

                // Si no se encontró (o no hay ciudad), usar el mapa por defecto
                if (activeMapObject == null)
                {
                    activeMapObject = defaultMap;
                    if (activeMapObject != null)
                    {
                        if (!string.IsNullOrEmpty(activeCityId))
                            Debug.LogWarning($"[JoustManager] ⚠ No se encontró mapa para '{activeCityId}' en la lista cityMaps. Activado mapa por defecto: '{activeMapObject.name}'");
                        else
                            Debug.Log($"[JoustManager] No hay ciudad activa. Activado mapa por defecto: '{activeMapObject.name}'");
                    }
                }

                // Activar únicamente el mapa seleccionado y desactivar todos los demás
                HashSet<GameObject> processedMaps = new HashSet<GameObject>();

                // Procesar todos los mapas de la lista
                foreach (var mapping in cityMaps)
                {
                    if (mapping.mapGameObject == null) continue;
                    if (processedMaps.Contains(mapping.mapGameObject)) continue;
                    processedMaps.Add(mapping.mapGameObject);

                    mapping.mapGameObject.SetActive(mapping.mapGameObject == activeMapObject);
                }

                // Procesar el mapa por defecto si no estaba en la lista
                if (defaultMap != null && !processedMaps.Contains(defaultMap))
                {
                    defaultMap.SetActive(defaultMap == activeMapObject);
                }
            }

            // 3. Cambiar la malla del caballero oponente correspondiente a la dificultad (Mesh Swapping)
            Mesh chosenMesh = difficulty switch
            {
                JoustDifficulty.Easy => easyEnemyMesh,
                JoustDifficulty.Normal => normalEnemyMesh,
                JoustDifficulty.Hard => hardEnemyMesh,
                JoustDifficulty.Epic => epicEnemyMesh,
                _ => normalEnemyMesh
            };

            if (chosenMesh != null)
            {
                if (enemyArmorRenderer != null)
                {
                    enemyArmorRenderer.sharedMesh = chosenMesh;
                    Debug.Log($"[JoustManager] Malla '{chosenMesh.name}' aplicada al SkinnedMeshRenderer del enemigo para dificultad {difficulty}");
                }
                else if (enemyArmorMeshFilter != null)
                {
                    enemyArmorMeshFilter.sharedMesh = chosenMesh;
                    Debug.Log($"[JoustManager] Malla '{chosenMesh.name}' aplicada al MeshFilter del enemigo para dificultad {difficulty}");
                }
                else
                {
                    Debug.LogWarning("[JoustManager] No se pudo cambiar la malla del enemigo: 'enemyArmorRenderer' y 'enemyArmorMeshFilter' son null.");
                }
            }
            else
            {
                Debug.LogWarning($"[JoustManager] No se ha asignado ningún Mesh de caballero para la dificultad {difficulty}. Se usará la malla por defecto de la escena.");
            }
        }

        ApplyDifficulty();

        if (player != null)
        {
            initialPlayerPos = player.position;
            initialPlayerRot = player.rotation;
            playerVisualHorse = FindChildRecursive(player, horseChildName);
            if (playerVisualHorse == null) playerVisualHorse = player;
        }

        if (enemy != null)
        {
            initialEnemyPos = enemy.position;
            initialEnemyRot = enemy.rotation;
            enemyVisualHorse = FindChildRecursive(enemy, horseChildName);
            if (enemyVisualHorse == null) enemyVisualHorse = enemy;
        }

        if (playerHorseAnimator == null && player != null)
            playerHorseAnimator = player.GetComponentInChildren<Animator>();

        SetPreJoustHorseRunning(false);

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
            ShowEffectChoicesBeforeHorsePhase();
    }

    void ApplyDifficulty()
    {
        int basePoints = 10;

        switch (difficulty)
        {
            case JoustDifficulty.Easy:
                horsePhaseDuration = 6f;
                horsePhaseSpeed = 9f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints;
                if (defensePart != null)
                {
                    defensePart.attackMoveSpeed = 0.5f;
                    defensePart.captureDistanceTolerance = 60f;
                    defensePart.requiredCaptureTime = 0.5f;
                }
                break;

            case JoustDifficulty.Normal:
                horsePhaseDuration = 4.5f;
                horsePhaseSpeed = 11.7f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints + 10; // 50
                if (defensePart != null)
                {
                    defensePart.attackMoveSpeed = 1f;
                    defensePart.captureDistanceTolerance = 50f;
                    defensePart.requiredCaptureTime = 0.8f;
                }
                break;

            case JoustDifficulty.Hard:
                horsePhaseDuration = 4f;
                horsePhaseSpeed = 13f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints + 20; // 80
                if (defensePart != null)
                {
                    defensePart.attackMoveSpeed = 2f;
                    defensePart.captureDistanceTolerance = 40f;
                    defensePart.requiredCaptureTime = 1.0f;
                }
                break;

            case JoustDifficulty.Epic:
                horsePhaseDuration = 3.5f;
                horsePhaseSpeed = 15f;
                combatPhaseSpeed = 6f;
                if (winManager != null) winManager.winPoints = basePoints + 30; // 120
                if (defensePart != null)
                {
                    defensePart.attackMoveSpeed = 3f;
                    defensePart.captureDistanceTolerance = 30f;
                    defensePart.requiredCaptureTime = 1.2f;
                }
                break;
        }
    }

    void UpdateCursorState()
    {
        // El cursor debe estar libre (desbloqueado y visible) si:
        // - Estamos eligiendo efectos/cartas antes de la justa
        // - El tutorial está abierto
        // - El juego está en pausa
        bool needFreeCursor = waitingForEffectChoice || 
                              (tutorialManager != null && tutorialManager.IsTutorialOpen()) ||
                              PauseMenuController.IsPaused;

        if (!needFreeCursor)
        {
            WinManager win = FindFirstObjectByType<WinManager>();
            if (win != null && win.statsPanelController != null && win.statsPanelController.panelObject != null && win.statsPanelController.panelObject.activeInHierarchy)
            {
                needFreeCursor = true;
            }
        }

        if (needFreeCursor)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
        else
        {
            // Durante el gameplay activo (incluyendo cinemática pre-justa y las fases de carrera/combate)
            // bloqueamos y ocultamos el cursor para que no estorbe en pantalla y funcione correctamente en builds.
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void Update()
    {
        UpdateCursorState();

        if (!joustStarted || preJoustIntroRunning || waitingForEffectChoice)
            return;

        MoveJousters();
        HandleHorseTimer();
        HandleTransitionTimer();
        CheckCombatDistance();
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
        waitingForEffectChoice = false;

        horsePartIsOn = false;
        attackPartIsOn = false;
        defensePartIsOn = false;

        horseTimerRunning = false;
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
        ShowEffectChoicesBeforeHorsePhase();
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
            SetPreJoustHorseRunning(false);
            yield break;
        }

        SetPreJoustHorseRunning(true);

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

        SetPreJoustHorseRunning(false);
    }

    void SetPreJoustHorseRunning(bool isRunning)
    {
        int speedParam = Animator.StringToHash(horseSpeedParameter);

        if (playerHorseAnimator != null)
        {
            playerHorseAnimator.SetFloat(speedParam, isRunning ? preJoustRunAnimSpeed : preJoustIdleAnimSpeed);
        }

        // Sincronizar el caballo del oponente durante la cinemática pre-justa
        Animator opponentHorseAnim = (horsePart != null) ? horsePart.opponentHorseAnimator : null;
        if (opponentHorseAnim != null)
        {
            opponentHorseAnim.SetFloat(speedParam, isRunning ? preJoustRunAnimSpeed : preJoustIdleAnimSpeed);
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

    void ShowEffectChoicesBeforeHorsePhase()
    {
        if (!useEffectChoiceButtons || effectManager == null)
        {
            StartJoustNormally();
            return;
        }

        waitingForEffectChoice = true;

        effectManager.ShowEffectChoices(() =>
        {
            waitingForEffectChoice = false;
            StartJoustNormally();
        });

        UpdateControlsUI();
    }

    void StartJoustNormally()
    {
        SetPreJoustHorseRunning(false);

        if (player != null)
        {
            playerVisualHorse = FindChildRecursive(player, horseChildName);
            if (playerVisualHorse == null) playerVisualHorse = player;
        }

        if (enemy != null)
        {
            enemyVisualHorse = FindChildRecursive(enemy, horseChildName);
            if (enemyVisualHorse == null) enemyVisualHorse = enemy;
        }

        // Volver a aplicar la dificultad base para asegurar consistencia
        ApplyDifficulty();

        // Aplicar los modificadores de penalización de la carta seleccionada
        if (effectManager != null && effectManager.hasActiveCard)
        {
            effectManager.ApplyActiveNegativeModifiers();
        }

        joustStarted = true;

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

        // En modo tutorial el Dummy se queda quieto
        if (enemy != null && !isTutorialMode)
            enemy.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
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

    void CheckCombatDistance()
    {
        if (!attackPartIsOn && !defensePartIsOn) return;

        if (player != null && enemy != null)
        {
            Transform pTrans = playerVisualHorse != null ? playerVisualHorse : player;
            Transform eTrans = enemyVisualHorse != null ? enemyVisualHorse : enemy;

            Vector3 dirToEnemy = (eTrans.position - pTrans.position).normalized;
            float dot = Vector3.Dot(player.forward, dirToEnemy);

            // Si el dot product es muy negativo, el enemigo está firmemente detrás nuestro.
            // Usamos -0.2f en lugar de 0f para darle un margen de tiempo al motor de físicas
            // (FixedUpdate) de procesar las colisiones (OnTriggerEnter) antes de apagar la lanza.
            if (dot < -0.2f)
            {
                EndCombatPhase();
            }
        }
    }

    void EndCombatPhase()
    {
        if (attackPartIsOn)
        {
            if (attackPart != null)
                attackPart.ForceAttack();

            EndAttackPhase();
        }

        if (defensePartIsOn)
        {
            if (defensePart != null)
                defensePart.ForceEndDefense(false);

            // EndDefensePhase es llamado desde dentro de ForceEndDefense
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

        if (preJoustIntroRunning || waitingForEffectChoice)
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

        UpdatePhases();

        if (tutorialManager != null && tutorialManager.ShouldShowTutorial())
            tutorialManager.ShowAttackTutorial();
    }

    public void EndAttackPhase()
    {
        if (attackResolved) return;

        attackResolved = true;
        attackPartIsOn = false;

        UpdatePhases();
        TryEndCombatPhase();
    }

    public void EndDefensePhase()
    {
        if (defenseResolved) return;

        defenseResolved = true;
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
            SetPreJoustHorseRunning(false);
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
            ShowEffectChoicesBeforeHorsePhase();
    }

    public void StartJoustForSubsequentRounds()
    {
        if (preJoustIntroCoroutine != null)
        {
            StopCoroutine(preJoustIntroCoroutine);
            preJoustIntroCoroutine = null;
        }

        SetPreJoustHorseRunning(false);

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

        // Direct, fast round start: skip introduction and cards
        StartJoustNormally();

        // Update the rounds HUD visual state immediately
        if (winManager != null)
        {
            winManager.UpdateBestOf3UI();
        }
    }

    void UpdateSceneReferencesToNewEnemy(Transform oldEnemy, Transform newEnemy)
    {
        if (oldEnemy == null || newEnemy == null) return;

        // 1. Actualizar cámaras de Cinemachine
        var cinemachineCameras = FindObjectsByType<Unity.Cinemachine.CinemachineCamera>(FindObjectsSortMode.None);
        foreach (var vcam in cinemachineCameras)
        {
            if (vcam == null) continue;

            // Si la cámara seguía al viejo enemigo o a alguno de sus hijos
            if (vcam.Follow != null && (vcam.Follow == oldEnemy || vcam.Follow.IsChildOf(oldEnemy)))
            {
                Transform newTarget = FindChildRecursive(newEnemy, vcam.Follow.name);
                vcam.Follow = (newTarget != null) ? newTarget : newEnemy;
                Debug.Log($"[JoustManager] Cinemachine '{vcam.name}' Follow reasignado a: {vcam.Follow.name}");
            }

            // Si la cámara miraba al viejo enemigo o a alguno de sus hijos
            if (vcam.LookAt != null && (vcam.LookAt == oldEnemy || vcam.LookAt.IsChildOf(oldEnemy)))
            {
                Transform newTarget = FindChildRecursive(newEnemy, vcam.LookAt.name);
                vcam.LookAt = (newTarget != null) ? newTarget : newEnemy;
                Debug.Log($"[JoustManager] Cinemachine '{vcam.name}' LookAt reasignado a: {vcam.LookAt.name}");
            }
        }
    }

    Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent == null) return null;
        if (parent.name == childName) return parent;

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), childName);
            if (found != null) return found;
        }
        return null;
    }
}