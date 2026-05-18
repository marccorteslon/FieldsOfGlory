using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    }

    void Start()
    {
        sliderHeight = sliderArea.rect.height;
        currentMoveSpeed = moveSpeed;

        CalculateZones();
        DrawZones();
        CreateIndicator();
        InitializeUI();
    }

    void Update()
    {
        if (joustManager == null) return;

        UpdateHorseAnimation();

        if (!joustManager.horsePartIsOn)
        {
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

        if (joustManager != null && joustManager.horsePartIsOn && isActive)
        {
            // 1f = Trote, 2f = Galope
            targetAnimSpeed = currentMoveSpeed >= gallopSpeedThreshold ? 2f : 1f;
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
    HideUI();

    if (objectToDisableOnEnd != null)
        objectToDisableOnEnd.SetActive(false);
}

    public void ResetHorsePhase()
    {
        isActive = true;
        hasResolved = false;
        pressCount = 0;
        pointsAwardedThisPhase = false;
        lastScoredZone = "Rojo";
        currentMoveSpeed = moveSpeed;

        ShowHorseBarUI();
        ResetIndicatorToBottom();

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = "0";
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }
}