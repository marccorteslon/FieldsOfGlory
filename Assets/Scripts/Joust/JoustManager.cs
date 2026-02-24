using UnityEngine;
using UnityEngine.SceneManagement;

// RESUMEN SCRIPT: Centro neuralgico de los scripts de la Justa, controla fases, cámara y conexión con WinManager
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
    public Camera mainCamera;             // Cámara principal
    public Transform horseCameraPoint;    // Posición/rotación de fase caballo
    public Transform attackCameraPoint;   // Posición/rotación de fase ataque
    public Transform defenseCameraPoint;  // Posición/rotación de fase defensa

    [Header("Camera Movement")]
    public float cameraTransitionSpeed = 2f; // Velocidad de transición
    private bool movingCamera = false;
    private Transform targetCameraPoint;

    [Header("Camera Follow")]
    public float followSpeed = 5f;

    private Transform currentCameraPoint;

    [Header("Win System")]
    public WinManager winManager; // Referencia al manager de puntos acumulativos

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

    // ---------------- Posiciones iniciales para reset de ronda ----------------
    [HideInInspector] public Vector3 initialPlayerPos;
    [HideInInspector] public Quaternion initialPlayerRot;

    [HideInInspector] public Vector3 initialEnemyPos;
    [HideInInspector] public Quaternion initialEnemyRot;

    [HideInInspector] public Vector3 initialCameraPos;
    [HideInInspector] public Quaternion initialCameraRot;

    void Start()
    {
        // Guardar posiciones iniciales
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

        if (mainCamera != null)
        {
            initialCameraPos = mainCamera.transform.position;
            initialCameraRot = mainCamera.transform.rotation;
        }

        // Inicializar la cámara en el punto de caballo
        if (mainCamera != null && horseCameraPoint != null)
        {
            mainCamera.transform.position = horseCameraPoint.position;
            mainCamera.transform.rotation = horseCameraPoint.rotation;
        }

        UpdatePhases();
        currentSpeed = horsePhaseSpeed;
        horseTimerRunning = true;
        currentCameraPoint = horseCameraPoint;
    }

    void Update()
    {
        MoveJousters();
        HandleHorseTimer();
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

    void MoveJousters()
    {
        if (player != null)
            player.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        if (enemy != null)
            enemy.Translate(Vector3.back * currentSpeed * Time.deltaTime);
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

    // ---------------------- Activar/Desactivar fases según estado ----------------------
    public void UpdatePhases()
    {
        if (horsePart != null)
            horsePart.gameObject.SetActive(horsePartIsOn);

        if (attackPart != null)
            attackPart.gameObject.SetActive(attackPartIsOn);

        if (defensePart != null)
            defensePart.gameObject.SetActive(defensePartIsOn);
    }

    // ---------------------- Terminar fase caballo ----------------------
    public void EndHorsePhase()
    {
        horsePartIsOn = false;
        attackPartIsOn = true;

        currentSpeed = combatPhaseSpeed;
        currentCameraPoint = attackCameraPoint;

        UpdatePhases();
    }

    // ---------------------- Terminar fase ataque ----------------------
    public void EndAttackPhase()
    {
        attackPartIsOn = false;
        defensePartIsOn = true;

        currentCameraPoint = defenseCameraPoint;

        UpdatePhases();
    }

    // ---------------------- Terminar fase defensa ----------------------
    public void EndDefensePhase()
    {
        defensePartIsOn = false;
        UpdatePhases();

        Debug.Log("La justa ha terminado.");

        // Procesar puntos acumulativos y decidir victoria/derrota
        if (winManager != null)
        {
            winManager.ProcessRoundEnd();
        }
    }

    // ---------------------- Reset completo de posiciones al iniciar nueva ronda ----------------------
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

        if (mainCamera != null)
        {
            mainCamera.transform.position = horseCameraPoint.position;
            mainCamera.transform.rotation = horseCameraPoint.rotation;
            currentCameraPoint = horseCameraPoint;
        }

        currentSpeed = horsePhaseSpeed;
        horseTimer = 0f;
        horseTimerRunning = true;
    }
}