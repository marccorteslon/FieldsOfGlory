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

    [Header("Zone Proportions (0-1)")]
    [Range(0f, 0.5f)] public float redProportion = 0.15f;
    [Range(0f, 0.5f)] public float yellowProportion = 0.15f;
    [Range(0f, 1f)] public float greenProportion = 0.4f;

    [Header("Movement")]
    public float moveSpeed = 300f;
    public float speedIncreasePerPress = 75f;
    public float maxMoveSpeed = 900f;

    [Header("Colors")]
    public Color redColor = Color.red;
    public Color yellowColor = Color.yellow;
    public Color greenColor = Color.green;
    public Color indicatorColor = Color.white;

    [Header("Fallback Horse Values (si no hay loadout)")]
    public int fallbackMV = 3;
    public int fallbackV = 1;

    private RectTransform movingIndicator;
    private float sliderHeight;
    private float direction = 1f;
    private int pressCount = 0;
    private bool isActive = true;
    private bool hasResolved = false;
    private float currentMoveSpeed;

    void Awake()
    {
        if (loadout == null)
            loadout = FindObjectOfType<LoadoutStatsComponent>();
    }

    void Start()
    {
        sliderHeight = sliderArea.rect.height;
        currentMoveSpeed = moveSpeed;

        float total = 2 * redProportion + 2 * yellowProportion + greenProportion;
        if (total != 1f)
        {
            float factor = 1f / total;
            redProportion *= factor;
            yellowProportion *= factor;
            greenProportion *= factor;
        }

        DrawZones();
        CreateIndicator();
        InitializeUI();
    }

    void Update()
    {
        if (joustManager == null) return;

        if (!joustManager.horsePartIsOn)
        {
            HideUI();
            return;
        }

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        if (!isActive) return;

        MoveIndicator();
        HandleInput();
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
        float currentYMin = 0f;
        float[] proportions = { redProportion, yellowProportion, greenProportion, yellowProportion, redProportion };
        Color[] colors = { redColor, yellowColor, greenColor, yellowColor, redColor };

        for (int i = 0; i < proportions.Length; i++)
        {
            GameObject zone = new GameObject("Zone_" + i, typeof(Image));
            zone.transform.SetParent(sliderArea, false);

            Image img = zone.GetComponent<Image>();
            img.color = colors[i];

            RectTransform rt = zone.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, currentYMin);
            rt.anchorMax = new Vector2(1f, currentYMin + proportions[i]);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            currentYMin += proportions[i];
        }
    }

    void CreateIndicator()
    {
        movingIndicator = Instantiate(movingIndicatorPrefab, sliderArea);
        movingIndicator.GetComponent<Image>().color = indicatorColor;
        movingIndicator.anchoredPosition = new Vector2(0f, -sliderHeight / 2f);
    }

    void MoveIndicator()
    {
        Vector2 pos = movingIndicator.anchoredPosition;
        pos.y += direction * currentMoveSpeed * Time.deltaTime;

        float limit = sliderHeight / 2f;
        if (pos.y > limit || pos.y < -limit)
        {
            direction *= -1f;
            pos.y = Mathf.Clamp(pos.y, -limit, limit);
        }

        movingIndicator.anchoredPosition = pos;
    }

    void HandleInput()
    {
        bool mouseClick = Input.GetMouseButtonDown(0);
        bool xboxX = Input.GetKeyDown(KeyCode.JoystickButton2);

        if (mouseClick || xboxX)
            IncreaseBarSpeed();
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

    void IncreaseBarSpeed()
    {
        pressCount++;
        currentMoveSpeed = Mathf.Min(currentMoveSpeed + speedIncreasePerPress, maxMoveSpeed);

        if (counterText != null)
            counterText.text = pressCount.ToString();
    }

    void EvaluateZone()
    {
        if (hasResolved) return;

        float y = movingIndicator.anchoredPosition.y;
        float normalized = (y + sliderHeight / 2f) / sliderHeight;
        string zone;

        if (normalized <= redProportion || normalized >= 1f - redProportion)
            zone = "Rojo";
        else if (normalized <= redProportion + yellowProportion || normalized >= 1f - (redProportion + yellowProportion))
            zone = "Amarillo";
        else
            zone = "Verde";

        scoreManager.AddHorsePhaseScore(zone, GetMV(), GetV());

        ShowResult(zone);

        hasResolved = true;
        isActive = false;
    }

    void ShowResult(string zone)
    {
        if (resultText == null) return;

        resultText.gameObject.SetActive(true);

        switch (zone)
        {
            case "Rojo":
                resultText.text = "VERY BAD!";
                resultText.color = redColor;
                break;

            case "Amarillo":
                resultText.text = "GOOD!";
                resultText.color = yellowColor;
                break;

            case "Verde":
                resultText.text = "PERFECT!";
                resultText.color = greenColor;
                break;
        }
    }

    public void ForceEndHorsePhase()
    {
        EvaluateZone();
        HideUI();
    }

    public void ResetHorsePhase()
    {
        isActive = true;
        hasResolved = false;
        pressCount = 0;
        direction = 1f;
        currentMoveSpeed = moveSpeed;

        ShowHorseBarUI();

        if (movingIndicator != null)
            movingIndicator.anchoredPosition = new Vector2(0f, -sliderHeight / 2f);

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = "0";
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }
}