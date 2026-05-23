using UnityEngine;
using UnityEngine.UI;

public class PhysicalLanceController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform lancePivot;
    public LoadoutStatsComponent loadout;
    public JoustManager joustManager;

    [Header("Hit Marker (UI Ayuda)")]
    public Transform hitMarker;
    public Color colorNada = Color.red;
    public Color colorCuerpoEscudo = Color.yellow;
    public Color colorCabeza = Color.green;

    [Header("Input")]
    public float inputSensitivity = 2f;

    [Header("Físicas (Peso de la Lanza)")]
    [Tooltip("El retraso o 'peso' base de la lanza al moverse.")]
    public float baseSwayDamping = 0.05f;
    [Tooltip("Cuánto retraso extra se añade por cada punto de M. Si M es alto, más pesada se sentirá.")]
    public float swayDampingPerM = 0.005f;
    [Tooltip("Cuánto retraso extra se añade si la lanza está cargada de fuerza al máximo (100%).")]
    public float swayDampingFromCharge = 0.05f;

    [Header("Carga del Golpe")]
    public float maxChargeTime = 2f;
    public float currentCharge = 0f;
    [Tooltip("Asigna aquí el Slider de UI que mostrará la barra de carga.")]
    public Slider chargeSlider;

    [Header("Cámara (Paneo 2D)")]
    public bool moveCamera = true;
    [Tooltip("El punto de la cámara o la propia cámara que se moverá. Si lo dejas vacío, usará el attackCameraPoint del JoustManager.")]
    public Transform customCameraPoint;
    [Range(0f, 1f)] public float cameraMoveMultiplier = 0.3f;

    [Header("Visual Lance")]
    public Transform lance3DModel;
    public Vector3 lancePositionOffset = Vector3.zero;
    public Vector3 lanceRotationOffset;

    [Header("Fallback Stats")]
    public int fallbackBL = 2; // Stat de Lanza
    public int fallbackBF = 4; // Stat de Fuerza/Daño
    public int fallbackM = 2;  // Stat de Maniobrabilidad

    private Vector3 currentAimAngles;
    private Vector3 targetAimAngles;
    private Vector3 aimAnglesVelocity;
    
    private float mousePreviousX, mousePreviousY;
    private Renderer markerRenderer;
    private Image markerImage;
    private bool wasPlaying = false;
    private Quaternion initialCameraPointRot;

    void Awake()
    {
        if (loadout == null)
        {
            GameObject ghost = GameObject.Find("GhostPlayer");
            if (ghost != null)
            {
                loadout = ghost.GetComponent<LoadoutStatsComponent>();
            }
            else
            {
                loadout = FindFirstObjectByType<LoadoutStatsComponent>();
            }
        }
    }

    void Start()
    {
        if (joustManager == null) joustManager = FindFirstObjectByType<JoustManager>();

        Transform camPoint = customCameraPoint != null ? customCameraPoint : (joustManager != null ? joustManager.attackCameraPoint : null);
        if (camPoint != null)
        {
            initialCameraPointRot = camPoint.localRotation;
        }

        if (hitMarker != null)
        {
            markerRenderer = hitMarker.GetComponentInChildren<Renderer>();
            markerImage = hitMarker.GetComponentInChildren<Image>();
        }

        mousePreviousX = Input.mousePosition.x;
        mousePreviousY = Input.mousePosition.y;
    }

    void Update()
    {
        if (lancePivot == null) 
        {
            Debug.LogWarning("⚠️ Falta asignar 'Lance Pivot' en PhysicalLanceController.");
            return;
        }

        bool isPlaying = joustManager != null && joustManager.attackPartIsOn;

        if (isPlaying)
        {
            if (!wasPlaying)
            {
                mousePreviousX = Input.mousePosition.x;
                mousePreviousY = Input.mousePosition.y;
                wasPlaying = true;
            }

            HandleInput();
            HandleCharge();
            ApplyMovement();

            if (hitMarker != null && !hitMarker.gameObject.activeSelf) hitMarker.gameObject.SetActive(true);
            
            if (chargeSlider != null && !chargeSlider.gameObject.activeSelf)
            {
                chargeSlider.gameObject.SetActive(true);
            }

            UpdateHitMarker();
        }
        else
        {
            if (wasPlaying)
            {
                wasPlaying = false;
                if (hitMarker != null && hitMarker.gameObject.activeSelf) hitMarker.gameObject.SetActive(false);
                if (chargeSlider != null && chargeSlider.gameObject.activeSelf) chargeSlider.gameObject.SetActive(false);
            }

            // Retorno suave a la posición original/reposo (cero)
            targetAimAngles = Vector3.zero;
            ApplyMovement();
        }

        // Siempre mantenemos la lanza pegada a la mano visualmente con los ángulos actualizados
        UpdateLanceVisuals();
    }

    private void OnApplicationFocus(bool focus)
    {
        mousePreviousX = Input.mousePosition.x;
        mousePreviousY = Input.mousePosition.y;
    }

    void HandleInput()
    {
        float mouseX = Input.mousePosition.x;
        float mouseY = Input.mousePosition.y;

        // Escalamos la sensibilidad para que valores como "2" en el Inspector sean suaves
        // y no giren la lanza 100 grados en un solo frame.
        float effectiveSensitivity = inputSensitivity * 0.05f;
        Vector2 inputDelta = new Vector2(mouseX - mousePreviousX, mouseY - mousePreviousY) * effectiveSensitivity;

        mousePreviousX = mouseX;
        mousePreviousY = mouseY;

        // Sumamos el movimiento del ratón a la rotación OBJETIVO
        targetAimAngles += new Vector3(-inputDelta.y, inputDelta.x, 0f);
    }

    void HandleCharge()
    {
        if (Input.GetMouseButton(0))
        {
            currentCharge = Mathf.Min(currentCharge + Time.deltaTime, maxChargeTime);
        }
        else
        {
            if (currentCharge > 0)
            {
                currentCharge -= Time.deltaTime * 2f;
                currentCharge = Mathf.Max(0, currentCharge);
            }
        }

        // Actualizar la barra visual de UI
        if (chargeSlider != null)
        {
            chargeSlider.value = currentCharge / maxChargeTime;
        }
    }

    void ApplyMovement()
    {
        // NO rotamos lancePivot en absoluto. Se queda como un punto estático puro (la mano).
        // Calculamos cuánto tarda en seguir al ratón dependiendo de la maniobrabilidad (M) y la fuerza
        float chargePercent = currentCharge / maxChargeTime;
        float currentDamping = baseSwayDamping + (GetM() * swayDampingPerM) + (chargePercent * swayDampingFromCharge);

        // Interpolación fluida usando unscaledDeltaTime para que la cámara lenta no afecte la responsividad del ratón
        currentAimAngles = Vector3.SmoothDamp(
            currentAimAngles, 
            targetAimAngles, 
            ref aimAnglesVelocity, 
            currentDamping, 
            Mathf.Infinity, 
            Time.unscaledDeltaTime
        );
    }

    void UpdateHitMarker()
    {
        if (hitMarker == null) return;

        // La rotación virtual generada por el ratón
        Quaternion localAimRotation = Quaternion.Euler(currentAimAngles);
        Quaternion worldAimRotation = lancePivot.rotation * localAimRotation;

        Ray ray = new Ray(lancePivot.position, worldAimRotation * Vector3.forward);
        Vector3 targetWorldPos;
        Vector3 targetNormal;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (!hitMarker.gameObject.activeSelf) hitMarker.gameObject.SetActive(true);
            
            targetWorldPos = hit.point;
            targetNormal = hit.normal;

            string hitTag = hit.collider.tag;
            if (hitTag == "Head")
            {
                SetMarkerColor(colorCabeza);
            }
            else if (hitTag == "Body" || hitTag == "Shield")
            {
                SetMarkerColor(colorCuerpoEscudo);
            }
            else 
            {
                SetMarkerColor(colorNada);
            }
        }
        else
        {
            if (!hitMarker.gameObject.activeSelf) hitMarker.gameObject.SetActive(true);
            targetWorldPos = ray.GetPoint(50f);
            targetNormal = -ray.direction;
            SetMarkerColor(colorNada);
        }

        RectTransform rt = hitMarker.GetComponent<RectTransform>();
        Canvas canvas = hitMarker.GetComponentInParent<Canvas>();

        if (rt != null && canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            if (Camera.main != null)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(targetWorldPos);
                if (screenPos.z > 0)
                {
                    rt.position = screenPos;
                }
            }
        }
        else
        {
            hitMarker.position = targetWorldPos + targetNormal * 0.05f;
            hitMarker.rotation = Quaternion.LookRotation(targetNormal);
        }

    }

    void UpdateLanceVisuals()
    {
        if (lance3DModel != null)
        {
            Quaternion localAimRotation = Quaternion.Euler(currentAimAngles);
            Quaternion worldAimRotation = lancePivot.rotation * localAimRotation;

            // Combinamos la rotación de la mira con el offset de Blender
            Quaternion finalLanceRotation = worldAimRotation * Quaternion.Euler(lanceRotationOffset);
            
            // La lanza orbita mágicamente sobre el lancePivot estático
            lance3DModel.position = lancePivot.position + finalLanceRotation * lancePositionOffset;
            lance3DModel.rotation = finalLanceRotation;
        }
    }

    void SetMarkerColor(Color color)
    {
        if (markerRenderer != null) markerRenderer.material.color = color;
        if (markerImage != null) markerImage.color = color;
    }

    public float GetChargePercent()
    {
        return (currentCharge / maxChargeTime) * 100f;
    }

    public int GetBF()
    {
        if (loadout == null) return fallbackBF;
        int val = Mathf.RoundToInt(loadout.stats.Get(StatType.BF));
        return val > 0 ? val : fallbackBF;
    }

    public int GetBL()
    {
        if (loadout == null) return fallbackBL;
        int val = Mathf.RoundToInt(loadout.stats.Get(StatType.BL));
        return val > 0 ? val : fallbackBL;
    }

    public int GetM()
    {
        if (loadout == null) return fallbackM;
        int val = Mathf.RoundToInt(loadout.stats.Get(StatType.M));
        return val > 0 ? val : fallbackM;
    }

    void LateUpdate()
    {
        Transform camPoint = customCameraPoint != null ? customCameraPoint : (joustManager != null ? joustManager.attackCameraPoint : null);

        if (moveCamera && joustManager != null && joustManager.attackPartIsOn && camPoint != null)
        {
            // Paneo puro en 2D (Arriba/Abajo, Izquierda/Derecha) siguiendo los ángulos de la lanza
            Quaternion cameraPan = Quaternion.Euler(currentAimAngles.x * cameraMoveMultiplier, currentAimAngles.y * cameraMoveMultiplier, 0);
            
            // Aplicamos la rotación relativa al punto inicial de la cámara
            camPoint.localRotation = initialCameraPointRot * cameraPan;
        }
    }
}
