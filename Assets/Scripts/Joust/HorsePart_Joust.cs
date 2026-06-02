using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HorsePart_Joust : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform sliderArea;
    public RectTransform movingIndicatorPrefab;
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("UI Feedback")]
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI counterText;

    [Header("Loadout (Ghost Player)")]
    public LoadoutStatsComponent loadout;

    [Header("Drop Animation")]
    public float dropSpeed = 1800f;

    [Header("Horse Animation")]
    public Animator horseAnimator;
    public ParticleSystem gallopSmoke;
    [Header("Opponent Horse Animation")]
    public Animator opponentHorseAnimator;
    public ParticleSystem opponentGallopSmoke;

    [Header("Animation Settings")]
    public float gallopSpeedThreshold = 500f;
    public float animationBlendSpeed = 5f;

    private float currentAnimSpeed = 0f;
    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    private bool isDropping = false;

    [Header("Zone Proportions")]
    [Range(0f, 0.5f)] public float yellowProportion = 0.15f;
    [Range(0f, 1f)] public float greenProportion = 0.25f;

    [Header("Zone Position")]
    [Range(0f, 1f)] public float goodZoneCenter = 0.75f;

    [Header("Movement")]
    public float moveSpeed = 300f;
    public float speedIncreasePerHit = 75f;
    public float maxMoveSpeed = 900f;

    [Header("Camera FOV")]
    public CinemachineCamera virtualCamera;
    public float yellowFovIncrease = 3f;
    public float greenFovIncrease = 6f;
    public float fovSmoothSpeed = 6f;

    private float originalFOV;
    private float targetFOV;
    private bool hasOriginalFOV = false;
    private bool fovWasModified = false;
    private bool waitingForCameraChangeToRestoreFOV = false;

    [Header("Post Processing")]
    [Tooltip("Intensidad máxima del Motion Blur al acertar.")]
    [Range(0f, 1f)] public float maxBlurIntensity = 0.65f;
    [Tooltip("Intensidad máxima del Vignette al acertar.")]
    [Range(0f, 1f)] public float maxVignetteIntensity = 0.45f;
    [Tooltip("Velocidad a la que el efecto se desvanece después de acertar (más alto = más rápido).")]
    [Range(0.1f, 5f)] public float ppFadeSpeed = 1f;

    // Auto-creados en runtime
    private Volume horseEffectVolume;
    private MotionBlur motionBlur;
    private Vignette vignette;
    private float ppIntensity = 0f; // 0 = sin efecto, 1 = máximo

    [Header("Speed Lines")]
    [Tooltip("Se crea automáticamente si no se asigna.")]
    public HorseSpeedLinesEffect speedLinesEffect;
    [Tooltip("Número máximo de líneas (Verde). Amarillo usa la mitad.")]
    public int speedLineCount = 24;
    [Tooltip("Radio interior — dónde empieza cada línea (0…0.49 del ancho de pantalla).")]
    [Range(0f, 0.49f)] public float speedLineInnerRadius = 0.12f;
    [Tooltip("Radio exterior — dónde termina cada línea (0…0.49 del ancho de pantalla).")]
    [Range(0f, 0.49f)] public float speedLineOuterRadius = 0.46f;
    [Tooltip("Grosor de cada línea (0…0.05 del ancho de pantalla).")]
    [Range(0.001f, 0.05f)] public float speedLineWidth = 0.012f;
    [Tooltip("Tiempo de desvanecimiento en segundos.")]
    public float speedLineFadeDuration = 0.3f;
    [Tooltip("Color de las líneas al acertar en zona Verde.")]
    public Color speedLineGreenColor = new Color(0.45f, 1f, 0.45f, 1f);
    [Tooltip("Color de las líneas al acertar en zona Amarilla.")]
    public Color speedLineYellowColor = new Color(1f, 0.85f, 0.15f, 1f);

    [Header("Colors")]
    public Color redColor = Color.red;
    public Color yellowColor = Color.yellow;
    public Color greenColor = Color.green;
    public Color indicatorColor = Color.white;

    [Header("Fallback Horse Values")]
    public int fallbackMV = 3;
    public int fallbackV = 1;

    [Header("Sounds")]
    public AudioSource audioSource;
    public AudioClip redSound;
    public AudioClip yellowSound;
    public AudioClip greenSound;

    [Header("Pitch")]
    public float pitchIncrease = 0.1f;
    public float maxPitch = 2f;

    private string lastSoundZone = "";
    private int consecutiveHits = 0;

    [Header("Extra")]
    public GameObject objectToDisableOnEnd;

    private RectTransform movingIndicator;
    private float sliderHeight;
    private int pressCount = 0;
    private bool pointsAwardedThisPhase = false;
    private string lastScoredZone = "Rojo";
    private bool isActive = true;
    private bool hasResolved = false;
    private float currentMoveSpeed;

    private float yellowBottomMin;
    private float greenMin;
    private float greenMax;
    private float yellowTopMax;

    void Awake()
    {
        if (loadout == null)
        {
            GameObject ghost = GameObject.Find("GhostPlayer");
            if (ghost != null)
                loadout = ghost.GetComponent<LoadoutStatsComponent>();
            else
                loadout = FindFirstObjectByType<LoadoutStatsComponent>();
        }

        if (joustManager == null)
            joustManager = FindFirstObjectByType<JoustManager>();

        if (scoreManager == null)
            scoreManager = FindFirstObjectByType<ScoreManager>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (virtualCamera == null)
            virtualCamera = FindFirstObjectByType<CinemachineCamera>();

        // Crear el efecto de speed lines automáticamente si no se asignó en el Inspector
        if (speedLinesEffect == null)
        {
            speedLinesEffect = gameObject.AddComponent<HorseSpeedLinesEffect>();

            var brain = FindFirstObjectByType<CinemachineBrain>();
            if (brain != null)
                speedLinesEffect.targetCamera = brain.GetComponent<Camera>();

            if (speedLinesEffect.targetCamera == null)
                speedLinesEffect.targetCamera = Camera.main;
        }

        // Pasar parámetros del Inspector al efecto
        speedLinesEffect.maxLines     = speedLineCount;
        speedLinesEffect.innerRadius  = speedLineInnerRadius;
        speedLinesEffect.outerRadius  = speedLineOuterRadius;
        speedLinesEffect.lineWidth    = speedLineWidth;
        speedLinesEffect.fadeDuration = speedLineFadeDuration;
        speedLinesEffect.greenColor   = speedLineGreenColor;
        speedLinesEffect.yellowColor  = speedLineYellowColor;
    }

    void Start()
    {
        sliderHeight = sliderArea.rect.height;
        currentMoveSpeed = moveSpeed;

        SaveOriginalFOV();
        InitPostProcessing();

        CalculateZones();
        DrawZones();
        CreateIndicator();
        InitializeUI();
    }

    void Update()
    {
        UpdateCameraFOV();
        UpdatePostProcessing();

        if (joustManager == null) return;

        UpdateHorseAnimation();

        if (!joustManager.horsePartIsOn)
        {
            RestoreOriginalFOVWhenCameraChanged();
            HideUI();
            return;
        }

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        if (!isActive) return;

        if (isDropping)
            DropIndicator();
        else
            MoveIndicator();

        HandleInput();
    }

    void UpdateHorseAnimation()
    {
        float targetAnimSpeed = 0f;

        if (joustManager != null)
        {
            if ((joustManager.horsePartIsOn && isActive) || joustManager.attackPartIsOn || joustManager.defensePartIsOn)
            {
                // Mapeamos de forma dinámica la velocidad física [300, 900] a tu BlendTree [1.0, 3.0]
                float speedPercentage = Mathf.InverseLerp(moveSpeed, maxMoveSpeed, currentMoveSpeed);
                targetAnimSpeed = Mathf.Lerp(1.0f, 3.0f, speedPercentage);
            }
            // Si no está en ninguna fase activa, targetAnimSpeed = 0f (se lerpea suave a Idle)
        }

        currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, animationBlendSpeed * Time.deltaTime);

        if (horseAnimator != null)
            horseAnimator.SetFloat(SpeedParam, currentAnimSpeed);

        if (opponentHorseAnimator != null)
            opponentHorseAnimator.SetFloat(SpeedParam, currentAnimSpeed);

        if (gallopSmoke != null)
        {
            if (targetAnimSpeed >= 2f && !gallopSmoke.isPlaying)
                gallopSmoke.Play();
            else if (targetAnimSpeed < 2f && gallopSmoke.isPlaying)
                gallopSmoke.Stop();
        }

        if (opponentGallopSmoke != null)
        {
            if (targetAnimSpeed >= 2f && !opponentGallopSmoke.isPlaying)
                opponentGallopSmoke.Play();
            else if (targetAnimSpeed < 2f && opponentGallopSmoke.isPlaying)
                opponentGallopSmoke.Stop();
        }
    }

    void SaveOriginalFOV()
    {
        if (virtualCamera == null) return;

        originalFOV = virtualCamera.Lens.FieldOfView;
        targetFOV = originalFOV;
        hasOriginalFOV = true;
        fovWasModified = false;
        waitingForCameraChangeToRestoreFOV = false;
    }

    void IncreaseCameraFOV(string zone)
    {
        if (virtualCamera == null) return;

        if (!hasOriginalFOV)
            SaveOriginalFOV();

        if (zone == "Amarillo")
        {
            targetFOV += yellowFovIncrease;
            fovWasModified = true;
        }
        else if (zone == "Verde")
        {
            targetFOV += greenFovIncrease;
            fovWasModified = true;
        }
    }

    void UpdateCameraFOV()
    {
        if (virtualCamera == null) return;
        if (!hasOriginalFOV) return;

        if (fovWasModified && !waitingForCameraChangeToRestoreFOV)
        {
            virtualCamera.Lens.FieldOfView = Mathf.Lerp(
                virtualCamera.Lens.FieldOfView,
                targetFOV,
                fovSmoothSpeed * Time.deltaTime
            );
        }
    }

    void RestoreOriginalFOVWhenCameraChanged()
    {
        if (virtualCamera == null) return;
        if (!hasOriginalFOV) return;
        if (!fovWasModified) return;

        waitingForCameraChangeToRestoreFOV = true;

        if (!virtualCamera.IsLive)
        {
            virtualCamera.Lens.FieldOfView = originalFOV;
            targetFOV = originalFOV;
            fovWasModified = false;
            waitingForCameraChangeToRestoreFOV = false;
        }
    }

    void RestoreOriginalFOV()
    {
        if (virtualCamera == null) return;
        if (!hasOriginalFOV) return;
        if (!fovWasModified) return;

        virtualCamera.Lens.FieldOfView = originalFOV;
        targetFOV = originalFOV;
        fovWasModified = false;
        waitingForCameraChangeToRestoreFOV = false;
        ResetPostProcessing();
    }

    // ---------------------------------------------------------------
    // POST PROCESSING
    // ---------------------------------------------------------------

    void InitPostProcessing()
    {
        // Buscar un Volume existente llamado "HorseEffectVolume" o crear uno nuevo
        var existingGO = GameObject.Find("HorseEffectVolume");
        if (existingGO != null)
            horseEffectVolume = existingGO.GetComponent<Volume>();

        if (horseEffectVolume == null)
        {
            var go = new GameObject("HorseEffectVolume");
            horseEffectVolume = go.AddComponent<Volume>();
            horseEffectVolume.isGlobal = true;
            horseEffectVolume.weight = 1f;
            horseEffectVolume.priority = 100f;
            horseEffectVolume.profile = ScriptableObject.CreateInstance<VolumeProfile>();
        }

        var profile = horseEffectVolume.profile;

        // Motion Blur
        if (!profile.TryGet(out motionBlur))
        {
            motionBlur = profile.Add<MotionBlur>();
            motionBlur.intensity.overrideState = true;
            motionBlur.quality.overrideState = true;
            motionBlur.quality.value = MotionBlurQuality.Low;
        }
        motionBlur.intensity.value = 0f;

        // Vignette
        if (!profile.TryGet(out vignette))
        {
            vignette = profile.Add<Vignette>();
            vignette.intensity.overrideState = true;
            vignette.smoothness.overrideState = true;
            vignette.smoothness.value = 0.4f;
        }
        vignette.intensity.value = 0f;
    }

    void UpdatePostProcessing()
    {
        if (motionBlur == null || vignette == null) return;

        // Fade out gradual
        if (ppIntensity > 0f)
        {
            ppIntensity -= Time.deltaTime * ppFadeSpeed;
            ppIntensity = Mathf.Max(ppIntensity, 0f);
        }

        motionBlur.intensity.value = maxBlurIntensity * ppIntensity;
        vignette.intensity.value   = maxVignetteIntensity * ppIntensity;
    }

    /// <summary>Sube el efecto al máximo. Llamar al acertar.</summary>
    void PulsePostProcessing(string zone)
    {
        switch (zone)
        {
            case "Verde":    ppIntensity = 1f;   break;
            case "Amarillo": ppIntensity = 0.6f; break;
            default: return;
        }
    }

    void ResetPostProcessing()
    {
        ppIntensity = 0f;
        if (motionBlur != null) motionBlur.intensity.value = 0f;
        if (vignette != null)   vignette.intensity.value   = 0f;
    }

    void CalculateZones()
    {
        float totalGoodZoneSize = greenProportion + yellowProportion * 2f;
        float halfSize = totalGoodZoneSize / 2f;

        float start = goodZoneCenter - halfSize;
        float end = goodZoneCenter + halfSize;

        if (start < 0f)
        {
            end -= start;
            start = 0f;
        }

        if (end > 1f)
        {
            start -= end - 1f;
            end = 1f;
        }

        start = Mathf.Clamp01(start);
        end = Mathf.Clamp01(end);

        yellowBottomMin = start;
        greenMin = yellowBottomMin + yellowProportion;
        greenMax = greenMin + greenProportion;
        yellowTopMax = end;
    }

    void InitializeUI()
    {
        ShowHorseBarUI();

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = "0";
        }
    }

    void ShowHorseBarUI()
    {
        if (sliderArea != null)
            sliderArea.gameObject.SetActive(true);

        if (movingIndicator != null)
            movingIndicator.gameObject.SetActive(true);

        if (counterText != null)
            counterText.gameObject.SetActive(true);
    }

    void HideUI()
    {
        if (sliderArea != null)
            sliderArea.gameObject.SetActive(false);

        if (movingIndicator != null)
            movingIndicator.gameObject.SetActive(false);

        if (resultText != null)
            resultText.gameObject.SetActive(false);

        if (counterText != null)
            counterText.gameObject.SetActive(false);
    }

    void DrawZones()
    {
        CreateZone("Red_Bottom", 0f, yellowBottomMin, redColor);
        CreateZone("Yellow_Bottom", yellowBottomMin, greenMin, yellowColor);
        CreateZone("Green", greenMin, greenMax, greenColor);
        CreateZone("Yellow_Top", greenMax, yellowTopMax, yellowColor);
        CreateZone("Red_Top", yellowTopMax, 1f, redColor);
    }

    void CreateZone(string zoneName, float min, float max, Color color)
    {
        if (max <= min) return;

        GameObject zone = new GameObject(zoneName, typeof(Image));
        zone.transform.SetParent(sliderArea, false);

        Image img = zone.GetComponent<Image>();
        img.color = color;

        RectTransform rt = zone.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, min);
        rt.anchorMax = new Vector2(1f, max);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void CreateIndicator()
    {
        movingIndicator = Instantiate(movingIndicatorPrefab, sliderArea);
        movingIndicator.GetComponent<Image>().color = indicatorColor;
        ResetIndicatorToBottom();
    }

    void MoveIndicator()
    {
        Vector2 pos = movingIndicator.anchoredPosition;
        float topLimit = sliderHeight / 2f;

        pos.y += currentMoveSpeed * Time.deltaTime;

        if (pos.y >= topLimit)
        {
            pos.y = topLimit;
            isDropping = true;
        }

        movingIndicator.anchoredPosition = pos;
    }

    void DropIndicator()
    {
        Vector2 pos = movingIndicator.anchoredPosition;
        float bottomLimit = -sliderHeight / 2f;

        pos.y -= dropSpeed * Time.deltaTime;

        if (pos.y <= bottomLimit)
        {
            pos.y = bottomLimit;
            isDropping = false;
        }

        movingIndicator.anchoredPosition = pos;
    }

    void HandleInput()
    {
        bool mouseClick = Input.GetMouseButtonDown(0);
        bool xboxX = Input.GetKeyDown(KeyCode.JoystickButton2);

        if (mouseClick || xboxX)
            TryHorseHit();
    }

    void TryHorseHit()
    {
        if (isDropping) return;

        string zone = GetCurrentZone();

        PlayZoneSound(zone);
        ShowResult(zone);

        if (zone != "Rojo")
        {
            pressCount++;
            pointsAwardedThisPhase = true;
            lastScoredZone = zone;

            IncreaseCameraFOV(zone);
            speedLinesEffect?.PlayBurst(zone);
            PulsePostProcessing(zone);

            if (scoreManager != null)
            {
                scoreManager.AddHorsePhaseScore(zone, GetMV(), GetV());
            }
            else
            {
                Debug.LogWarning("[Caballo] No hay ScoreManager asignado; no se pueden sumar puntos.");
            }

            currentMoveSpeed = Mathf.Min(currentMoveSpeed + speedIncreasePerHit, maxMoveSpeed);

            if (counterText != null)
                counterText.text = pressCount.ToString();
        }

        isDropping = true;
    }

    void ResetIndicatorToBottom()
    {
        if (movingIndicator != null)
            movingIndicator.anchoredPosition = new Vector2(0f, -sliderHeight / 2f);
    }

    string GetCurrentZone()
    {
        float y = movingIndicator.anchoredPosition.y;
        float normalized = (y + sliderHeight / 2f) / sliderHeight;

        if (normalized >= greenMin && normalized <= greenMax)
            return "Verde";

        if ((normalized >= yellowBottomMin && normalized < greenMin) ||
            (normalized > greenMax && normalized <= yellowTopMax))
            return "Amarillo";

        return "Rojo";
    }

    int GetMV()
    {
        if (loadout == null) return fallbackMV;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.MV));
    }

    int GetV()
    {
        if (loadout == null) return fallbackV;
        return Mathf.RoundToInt(loadout.stats.Get(StatType.V));
    }

    void EvaluateZone()
    {
        if (hasResolved) return;

        string zone = GetCurrentZone();

        if (!pointsAwardedThisPhase)
        {
            Debug.Log("[Caballo] La fase terminó sin aciertos válidos. +0 puntos.");
        }

        ShowResult(pointsAwardedThisPhase ? lastScoredZone : zone);

        hasResolved = true;
        isActive = false;
    }

    void PlayZoneSound(string zone)
    {
        if (audioSource == null) return;

        AudioClip clip = null;

        switch (zone)
        {
            case "Rojo":
                clip = redSound;
                break;

            case "Amarillo":
                clip = yellowSound;
                break;

            case "Verde":
                clip = greenSound;
                break;
        }

        if (clip == null) return;

        // Si repites color, aumenta pitch
        if (zone == lastSoundZone)
        {
            consecutiveHits++;
        }
        else
        {
            consecutiveHits = 0;
            lastSoundZone = zone;
        }

        float pitch = 1f + (consecutiveHits * pitchIncrease);
        pitch = Mathf.Clamp(pitch, 1f, maxPitch);

        audioSource.pitch = pitch;
        audioSource.volume = AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 1f;
        audioSource.PlayOneShot(clip);
    }

    void ShowResult(string zone)
    {
        if (resultText == null) return;

        resultText.gameObject.SetActive(true);

        switch (zone)
        {
            case "Rojo":
                resultText.text = "Bad";
                resultText.color = redColor;
                break;

            case "Amarillo":
                resultText.text = "Good";
                resultText.color = yellowColor;
                break;

            case "Verde":
                resultText.text = "Perfect";
                resultText.color = greenColor;
                break;
        }
    }

    public void ForceEndHorsePhase()
    {
        EvaluateZone();
        RestoreOriginalFOVWhenCameraChanged();
        ResetPostProcessing();
        HideUI();

        if (objectToDisableOnEnd != null)
            objectToDisableOnEnd.SetActive(false);
    }

    public void ResetHorsePhase()
    {
        RestoreOriginalFOV();
        SaveOriginalFOV();

        isActive = true;
        hasResolved = false;
        pressCount = 0;
        pointsAwardedThisPhase = false;
        lastScoredZone = "Rojo";
        currentMoveSpeed = moveSpeed;

        ShowHorseBarUI();
        ResetIndicatorToBottom();

        if (objectToDisableOnEnd != null)
            objectToDisableOnEnd.SetActive(true);

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = "0";
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }
}