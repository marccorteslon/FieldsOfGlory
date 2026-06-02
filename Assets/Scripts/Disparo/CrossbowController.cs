using UnityEngine;
using UnityEngine.UI;
using Unity.Cinemachine;

public class CrossbowController : MonoBehaviour
{
    [Header("Referencias de Cámara (Cinemachine)")]
    public CinemachineCamera fpVirtualCamera; // Cámara virtual de Cinemachine para primera persona
    public Camera firstPersonCamera;
    public Transform playerRoot; // El caballo o cuerpo del jugador que se mueve adelante

    [Header("Visuales de la Ballesta")]
    public GameObject crossbowPrefab;
    public Transform crossbowAttachPoint; // Punto acoplado a la cámara para sostener la ballesta
    public Vector3 weaponPositionOffset = new Vector3(-0.2406f, 0.2399f, -0.4831f);
    public Vector3 weaponRotationOffset = new Vector3(17.502f, -110.179f, 0f);
    public Vector3 weaponScale = new Vector3(0.3f, 0.3f, 0.3f);

    [Header("Ajustes del Pivote de la Ballesta")]
    [Tooltip("Offset de posición local de la ballesta respecto al pivote para re-alinear su punto de rotación.")]
    public Vector3 weaponPivotOffset = Vector3.zero;
    [Tooltip("Offset de rotación local de la ballesta respecto al pivote para corregir la orientación del prefab.")]
    public Vector3 weaponPivotRotationOffset = Vector3.zero;

    [Header("Ajustes de Puntería (Físicas del Peso)")]
    public float inputSensitivity = 1.5f;
    public float baseSwayDamping = 0.08f; // Peso/retraso de la ballesta al apuntar
    public float cameraMoveMultiplier = 1f;

    [Header("Límites de Rotación")]
    public float minPitch = -35f;
    public float maxPitch = 45f;
    public float minYaw = -85f;
    public float maxYaw = 85f;

    [Header("Corrección de Orientación y Ejes")]
    [Tooltip("Rotación Y base en grados. Si la cámara mira hacia atrás (al revés) debido a la orientación de tu modelo de caballo, cámbialo a 180.")]
    public float baseYawOffset = 0f;
    [Tooltip("Invierte el eje horizontal del apuntado.")]
    public bool invertAimX = false;
    [Tooltip("Invierte el eje vertical del apuntado.")]
    public bool invertAimY = false;

    [Header("Configuración del Disparo")]
    public GameObject boltPrefab;
    public float shootForce = 140f; // Aumentado para un disparo tenso, rápido y preciso
    public float reloadDuration = 1.5f;
    public int maxBolts = 20;
    [HideInInspector] public int remainingBolts;

    [Header("Efectos de Retroceso Visual (Recoil)")]
    public float recoilBackForce = 0.2f;
    public float recoilUpForce = 8f;
    public float recoilRecoverSpeed = 5f;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip dryFireSound; // Sin munición

    [Header("UI del Crosshair")]
    public Image crosshairImage;
    public Color crosshairNormalColor = Color.white;
    public Color crosshairHitColor = Color.green;

    // Estados internos
    private Vector3 targetAimAngles;
    private Vector3 currentAimAngles;
    private Vector3 aimAnglesVelocity;

    private float reloadTimer = 0f;
    private bool isReloading = false;
    private float mousePreviousX, mousePreviousY;

    // Vectores para retroceso procedimental
    private Vector3 recoilPosOffset;
    private float recoilRotOffset;

    // Referencia al objeto de la ballesta instanciado y su contenedor de pivote
    private GameObject spawnedCrossbow;
    private GameObject weaponPivotWrapper;
    private DisparoGameplayManager gameplayManager;

    void Start()
    {
        gameplayManager = FindFirstObjectByType<DisparoGameplayManager>();
        remainingBolts = maxBolts;

        if (fpVirtualCamera == null)
        {
            fpVirtualCamera = FindFirstObjectByType<CinemachineCamera>();
            if (fpVirtualCamera == null && playerRoot != null)
            {
                fpVirtualCamera = playerRoot.GetComponentInChildren<CinemachineCamera>();
            }
        }

        if (firstPersonCamera == null)
            firstPersonCamera = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // Bloquear y ocultar el cursor para mejor experiencia FPS
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        mousePreviousX = Input.mousePosition.x;
        mousePreviousY = Input.mousePosition.y;

        // Instanciar la ballesta
        SpawnCrossbow();

        // Inicializar ángulos al frente
        targetAimAngles = Vector3.zero;
        currentAimAngles = Vector3.zero;
    }

    void SpawnCrossbow()
    {
        if (crossbowPrefab == null) return;

        // Si no se asignó un punto de acople, lo creamos debajo de la cámara
        if (crossbowAttachPoint == null && firstPersonCamera != null)
        {
            GameObject attach = new GameObject("CrossbowAttachPoint");
            attach.transform.SetParent(firstPersonCamera.transform, false);
            crossbowAttachPoint = attach.transform;
        }

        if (crossbowAttachPoint != null)
        {
            // 1. Crear el contenedor Pivot Wrapper como hijo del punto de acople (sin Animator para evitar anulaciones)
            GameObject wrapperObj = new GameObject("WeaponPivotWrapper");
            wrapperObj.transform.SetParent(crossbowAttachPoint, false);
            weaponPivotWrapper = wrapperObj;
            
            // 2. Aplicar el offset de posición y rotación de alineación directamente en el wrapper
            weaponPivotWrapper.transform.localPosition = weaponPivotOffset;
            weaponPivotWrapper.transform.localRotation = Quaternion.Euler(weaponPivotRotationOffset);
            weaponPivotWrapper.transform.localScale = Vector3.one;

            // 3. Instanciar la ballesta bajo el wrapper (su transform local se mantiene limpio a 0)
            spawnedCrossbow = Instantiate(crossbowPrefab);
            spawnedCrossbow.transform.SetParent(weaponPivotWrapper.transform, false);
            spawnedCrossbow.transform.localPosition = Vector3.zero;
            spawnedCrossbow.transform.localRotation = Quaternion.identity;
            spawnedCrossbow.transform.localScale = Vector3.one;

            // Remover componentes Rigidbodies o Colliders que pueda tener el prefab en sus raíces para evitar conflictos físicos
            Rigidbody rb = spawnedCrossbow.GetComponent<Rigidbody>();
            if (rb != null) Destroy(rb);

            Collider col = spawnedCrossbow.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }
    }

    void Update()
    {
        // Comprobar si el gameplay está activo
        if (gameplayManager == null || !gameplayManager.isGameplayActive)
        {
            // Desbloquear cursor al terminar
            if (gameplayManager != null && gameplayManager.isGameEnded)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            return;
        }

        // Si está en pausa, no hacer nada y desbloquear cursor
        if (PauseMenuController.IsPaused)
        {
            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            return;
        }
        else
        {
            // Si el gameplay está activo y NO está pausado, asegurar que el cursor esté bloqueado y oculto
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        HandleAimInput();
        HandleShooting();
        ApplyAimRotation();
        UpdateProceduralRecoil();
        UpdateWeaponSway();
    }

    void HandleAimInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        float joyX = 0f;
        float joyY = 0f;

        // Soporte de mando (Joystick derecho) con envoltura de seguridad (try-catch)
        // para evitar ArgumentException si las entradas no están configuradas en el Input Manager de Unity
        try
        {
            joyX = Input.GetAxis("Joystick 2 X");
            joyY = Input.GetAxis("Joystick 2 Y");
        }
        catch (System.ArgumentException)
        {
            // Intentar con nombres estándar si Joystick 2 falla
            try
            {
                joyX = Input.GetAxis("RightStickHorizontal");
                joyY = Input.GetAxis("RightStickVertical");
            }
            catch (System.ArgumentException)
            {
                // Sin joystick configurado, omitir silenciosamente y usar solo ratón
            }
        }

        float horizontalInput = Mathf.Abs(mouseX) > 0.01f ? mouseX : joyX * 2f;
        float verticalInput = Mathf.Abs(mouseY) > 0.01f ? mouseY : joyY * 2f;

        if (invertAimX) horizontalInput = -horizontalInput;
        if (invertAimY) verticalInput = -verticalInput;

        float effectiveSensitivity = inputSensitivity * 2f;

        // Modificar los ángulos objetivo
        targetAimAngles.y += horizontalInput * effectiveSensitivity;
        targetAimAngles.x -= verticalInput * effectiveSensitivity;

        // Aplicar límites de rotación
        targetAimAngles.x = Mathf.Clamp(targetAimAngles.x, minPitch, maxPitch);
        targetAimAngles.y = Mathf.Clamp(targetAimAngles.y, minYaw, maxYaw);
    }

    void ApplyAimRotation()
    {
        // Interpolación fluida para simular inercia y peso físico
        currentAimAngles = Vector3.SmoothDamp(
            currentAimAngles,
            targetAimAngles,
            ref aimAnglesVelocity,
            baseSwayDamping,
            Mathf.Infinity,
            Time.deltaTime
        );

        // Rotar la cámara en primera persona (eje local) con compensación Y de yaw base
        // Si usamos Cinemachine, rotamos la cámara virtual de Cinemachine
        if (fpVirtualCamera != null)
        {
            fpVirtualCamera.transform.localRotation = Quaternion.Euler(currentAimAngles.x, currentAimAngles.y + baseYawOffset, 0f);
        }
        else if (firstPersonCamera != null)
        {
            firstPersonCamera.transform.localRotation = Quaternion.Euler(currentAimAngles.x, currentAimAngles.y + baseYawOffset, 0f);
        }
    }

    void HandleShooting()
    {
        if (isReloading)
        {
            reloadTimer -= Time.deltaTime;
            if (reloadTimer <= 0f)
            {
                isReloading = false;
                PlaySound(reloadSound);
            }
            return;
        }

        bool click = Input.GetMouseButtonDown(0);
        bool controllerRT = Input.GetKeyDown(KeyCode.JoystickButton5) || Input.GetKeyDown(KeyCode.JoystickButton10); // Teclas de disparo mando comunes

        if (click || controllerRT)
        {
            if (remainingBolts > 0)
            {
                FireBolt();
            }
            else
            {
                PlaySound(dryFireSound);
            }
        }
    }

    void FireBolt()
    {
        remainingBolts--;
        isReloading = true;
        reloadTimer = reloadDuration;

        // Reproducir sonido
        PlaySound(shootSound);

        // Aplicar fuerza de retroceso procedimental
        recoilPosOffset = new Vector3(0f, 0f, -recoilBackForce);
        recoilRotOffset = -recoilUpForce;

        // Instanciar el proyectil virote
        if (boltPrefab != null)
        {
            // Calcular la dirección de apuntado real combinando la rotación horizontal base del caballo y nuestros ángulos de mira
            float horseYaw = playerRoot != null ? playerRoot.eulerAngles.y : 0f;
            Quaternion aimRot = Quaternion.Euler(currentAimAngles.x, horseYaw + currentAimAngles.y + baseYawOffset, 0f);
            Vector3 shootDir = aimRot * Vector3.forward;

            // Determinar el punto de origen (la cámara principal o la virtual)
            Transform camTransform = firstPersonCamera != null ? firstPersonCamera.transform : (fpVirtualCamera != null ? fpVirtualCamera.transform : transform);
            Vector3 spawnPos = camTransform.position + shootDir * 1.5f;
            
            // Compensamos la punta de la flecha que mira hacia arriba rotándola 90 grados en X
            Quaternion spawnRot = aimRot * Quaternion.Euler(90f, 0f, 0f);

            GameObject bolt = Instantiate(boltPrefab, spawnPos, spawnRot);
            
            // Desactivar Animator en el proyectil si tuviese, para evitar que bloquee el transform físico
            Animator anim = bolt.GetComponentInChildren<Animator>();
            if (anim != null)
            {
                anim.enabled = false;
            }

            // Añadir trail dinámicamente si no lo tiene para mejorar visual
            TrailRenderer trail = bolt.GetComponentInChildren<TrailRenderer>();
            if (trail == null)
            {
                trail = bolt.AddComponent<TrailRenderer>();
                trail.time = 0.5f;
                trail.startWidth = 0.08f;
                trail.endWidth = 0.01f;
                trail.material = new Material(Shader.Find("Sprites/Default"));
                trail.startColor = new Color(1f, 0.9f, 0.6f, 0.8f);
                trail.endColor = new Color(1f, 0.4f, 0f, 0f);
            }

            // Obtener o añadir Rigidbody de forma dinámica para garantizar movimiento físico
            Rigidbody rb = bolt.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = bolt.AddComponent<Rigidbody>();
            }

            // Configurar Rigidbody
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            // Congelar la rotación física del Rigidbody para que la fricción angular de Unity no haga girar la flecha
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            
            // Aplicar velocidad física de forma universal en la dirección del apuntado real calculado
            rb.linearVelocity = shootDir * shootForce;

            // Asegurar que el proyectil tenga un Collider para detectar colisiones con las dianas
            Collider boltCol = bolt.GetComponent<Collider>();
            if (boltCol == null)
            {
                boltCol = bolt.GetComponentInChildren<Collider>();
                if (boltCol == null)
                {
                    // Si no tiene ningún colisionador, le creamos un BoxCollider por defecto
                    boltCol = bolt.AddComponent<BoxCollider>();
                }
            }
            // Asegurar que TODOS los colisionadores en el virote y sus hijos sean triggers para evitar rebotes físicos
            Collider[] allBoltColliders = bolt.GetComponentsInChildren<Collider>();
            foreach (var bc in allBoltColliders)
            {
                if (bc != null)
                {
                    bc.isTrigger = true;
                    bc.enabled = true;
                }
            }

            // Desactivar colisiones con la propia ballesta equipada (incluyendo hijos)
            if (spawnedCrossbow != null)
            {
                Collider[] weaponColliders = spawnedCrossbow.GetComponentsInChildren<Collider>();
                Collider[] arrowColliders = bolt.GetComponentsInChildren<Collider>();
                foreach (var wCol in weaponColliders)
                {
                    foreach (var aCol in arrowColliders)
                    {
                        if (wCol != null && aCol != null)
                        {
                            Physics.IgnoreCollision(aCol, wCol, true);
                        }
                    }
                }
            }

            // Desactivar colisiones con el jugador/caballo para evitar self-collisions inmediatas que claven la flecha al nacer
            if (playerRoot != null)
            {
                Collider[] playerColliders = playerRoot.GetComponentsInChildren<Collider>();
                Collider[] arrowColliders = bolt.GetComponentsInChildren<Collider>();
                foreach (var pCol in playerColliders)
                {
                    foreach (var aCol in arrowColliders)
                    {
                        if (pCol != null && aCol != null)
                        {
                            Physics.IgnoreCollision(aCol, pCol, true);
                        }
                    }
                }
            }

            // Asegurar que tenga el script de comportamiento
            CrossbowBolt boltScript = bolt.GetComponent<CrossbowBolt>();
            if (boltScript == null)
            {
                boltScript = bolt.AddComponent<CrossbowBolt>();
            }
        }

        // Feedback visual en la interfaz del HUD
        if (gameplayManager != null)
        {
            gameplayManager.UpdateAmmoUI(remainingBolts);
        }
    }

    void UpdateProceduralRecoil()
    {
        // Recuperación progresiva del retroceso mediante Lerp hacia cero
        recoilPosOffset = Vector3.Lerp(recoilPosOffset, Vector3.zero, Time.deltaTime * recoilRecoverSpeed);
        recoilRotOffset = Mathf.Lerp(recoilRotOffset, 0f, Time.deltaTime * recoilRecoverSpeed);
    }

    void UpdateWeaponSway()
    {
        if (crossbowAttachPoint == null) return;

        // Posición base de la ballesta + retroceso posicional + oscilación del galope del caballo (efecto bobbing)
        float bobbingX = Mathf.Sin(Time.time * 8f) * 0.01f;
        float bobbingY = Mathf.Cos(Time.time * 16f) * 0.015f;
        
        Vector3 targetPos = weaponPositionOffset + recoilPosOffset + new Vector3(bobbingX, bobbingY, 0f);
        Quaternion targetRot = Quaternion.Euler(weaponRotationOffset.x + recoilRotOffset, weaponRotationOffset.y, weaponRotationOffset.z);

        // Suavizar colocación final del crossbowAttachPoint (que actúa como el pivot directo)
        crossbowAttachPoint.localPosition = Vector3.Lerp(crossbowAttachPoint.localPosition, targetPos, Time.deltaTime * 15f);
        crossbowAttachPoint.localRotation = Quaternion.Slerp(crossbowAttachPoint.localRotation, targetRot, Time.deltaTime * 15f);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ResetAmmo()
    {
        remainingBolts = maxBolts;
        isReloading = false;
        reloadTimer = 0f;
    }
}
