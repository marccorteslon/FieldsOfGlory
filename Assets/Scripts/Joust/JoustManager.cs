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

    [Header("Win System")]
    public WinManager winManager; // Referencia al manager de puntos acumulativos

    void Start()
    {
        // Inicializar la cámara en el punto de caballo
        if (mainCamera != null && horseCameraPoint != null)
        {
            mainCamera.transform.position = horseCameraPoint.position;
            mainCamera.transform.rotation = horseCameraPoint.rotation;
        }

        UpdatePhases();
    }

    void LateUpdate()
    {
        // Lerp de cámara si estamos moviéndonos
        if (movingCamera && mainCamera != null && targetCameraPoint != null)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetCameraPoint.position,
                Time.deltaTime * cameraTransitionSpeed
            );

            mainCamera.transform.rotation = Quaternion.Slerp(
                mainCamera.transform.rotation,
                targetCameraPoint.rotation,
                Time.deltaTime * cameraTransitionSpeed
            );

            // Comprobar si llegó al destino
            if (Vector3.Distance(mainCamera.transform.position, targetCameraPoint.position) < 0.01f)
            {
                mainCamera.transform.position = targetCameraPoint.position;
                mainCamera.transform.rotation = targetCameraPoint.rotation;
                movingCamera = false;
            }
        }
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

        // Mover cámara a punto de ataque
        if (attackCameraPoint != null)
        {
            targetCameraPoint = attackCameraPoint;
            movingCamera = true;
        }

        UpdatePhases();
    }

    // ---------------------- Terminar fase ataque ----------------------
    public void EndAttackPhase()
    {
        attackPartIsOn = false;
        defensePartIsOn = true;

        // Mover cámara a punto de defensa
        if (defenseCameraPoint != null)
        {
            targetCameraPoint = defenseCameraPoint;
            movingCamera = true;
        }

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
}