using UnityEngine;

public class ShootingTarget : MonoBehaviour
{
    public enum TargetType
    {
        Standard,
        Golden,
        Moving
    }

    [Header("Configuración General")]
    public TargetType targetType = TargetType.Standard;
    public int scorePoints = 15;
    public float activationDistance = 35f;

    [Header("Animación de Aparición (Pop-up)")]
    [Tooltip("El pivote o el objeto que rotará/se moverá para aparecer.")]
    public Transform animatedVisual;
    public Vector3 hiddenLocalRotation = new Vector3(90f, 0f, 0f);
    public Vector3 activeLocalRotation = new Vector3(0f, 0f, 0f);
    public float popUpSpeed = 5f;

    [Header("Animación de Impacto")]
    public Vector3 hitLocalRotation = new Vector3(-85f, 0f, 0f);
    public float knockDownSpeed = 10f;

    [Header("Movimiento (Solo para tipo Moving)")]
    public float moveRange = 4f;
    public float moveSpeed = 2f;
    public Vector3 moveDirection = Vector3.right;

    [Header("Audio")]
    public AudioClip targetHitSound;
    [Range(0f, 1f)]
    public float hitVolume = 1f;

    private bool isPoppedUp = false;
    [HideInInspector] public bool isHit = false;
    private Vector3 initialLocalPosition;
    private Transform playerTransform;
    private DisparoGameplayManager gameplayManager;

    void Start()
    {
        gameplayManager = FindFirstObjectByType<DisparoGameplayManager>();
        
        if (animatedVisual == null)
            animatedVisual = transform;

        // Iniciar oculto
        animatedVisual.localRotation = Quaternion.Euler(hiddenLocalRotation);
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (isHit)
        {
            // Caer/Abatirse suavemente
            animatedVisual.localRotation = Quaternion.Slerp(
                animatedVisual.localRotation,
                Quaternion.Euler(hitLocalRotation),
                Time.deltaTime * knockDownSpeed
            );
            return;
        }

        // Buscar al jugador si no está asignado
        if (playerTransform == null)
        {
            if (gameplayManager != null && gameplayManager.player != null)
                playerTransform = gameplayManager.player;
            else
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                    playerTransform = playerObj.transform;
            }
        }

        if (playerTransform == null) return;

        // Comprobar distancia en el eje Z (o distancia real)
        float distanceZ = transform.position.z - playerTransform.position.z;
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // Activarse si el jugador está lo suficientemente cerca
        // Usamos la distancia real pero permitimos activarse solo si está enfrente (Z positivo)
        if (!isPoppedUp && distance <= activationDistance && distanceZ > 0f)
        {
            isPoppedUp = true;
        }

        // Animar Pop-up
        if (isPoppedUp)
        {
            animatedVisual.localRotation = Quaternion.Slerp(
                animatedVisual.localRotation,
                Quaternion.Euler(activeLocalRotation),
                Time.deltaTime * popUpSpeed
            );

            // Si es un objetivo móvil, oscilar a los lados usando una onda seno
            if (targetType == TargetType.Moving)
            {
                float offset = Mathf.Sin(Time.time * moveSpeed) * moveRange;
                transform.localPosition = initialLocalPosition + moveDirection.normalized * offset;
            }
        }
    }

    public void OnHit(CrossbowBolt bolt)
    {
        if (isHit) return;

        isHit = true;

        if (targetHitSound != null)
        {
            AudioSource.PlayClipAtPoint(
                targetHitSound,
                transform.position,
                hitVolume
            );
        }

        Debug.Log($"¡Diana golpeada! Tipo: {targetType} | +{scorePoints} puntos.");

        if (gameplayManager != null)
        {
            gameplayManager.AddScore(scorePoints, targetType);
        }
    }

    // Resetear objetivo para reintentos de ronda
    public void ResetTarget()
    {
        isHit = false;
        isPoppedUp = false;
        
        if (animatedVisual != null)
        {
            animatedVisual.localRotation = Quaternion.Euler(hiddenLocalRotation);
        }
        
        transform.localPosition = initialLocalPosition;
    }
}
