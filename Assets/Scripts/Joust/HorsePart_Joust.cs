using UnityEngine;
using UnityEngine.UI;
using TMPro;

// RESUMEN SCRIPT: Controla la fase del caballo, detecta la zona del indicador,
// suma puntos usando MV y V del loadout
// y muestra feedback visual + contador de clicks.

public class HorsePart_Joust : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform sliderArea;
    public RectTransform movingIndicatorPrefab;
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("UI Feedback")]
    public TextMeshProUGUI resultText;   // Texto que muestra Rojo/Amarillo/Verde
    public TextMeshProUGUI counterText;  // Texto tipo 0/3

    [Header("Loadout (Ghost Player)")]
    public LoadoutStatsComponent loadout;

    [Header("Zone Proportions (0-1)")]
    [Range(0f, 0.5f)] public float redProportion = 0.15f;
    [Range(0f, 0.5f)] public float yellowProportion = 0.15f;
    [Range(0f, 1f)] public float greenProportion = 0.4f;

    [Header("Movement")]
    public float moveSpeed = 300f;

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
    private int clickCount = 0;
    private bool isActive = true;

    private const int maxClicks = 3;

    void Awake()
    {
        if (loadout == null)
            loadout = FindObjectOfType<LoadoutStatsComponent>();
    }

    void Start()
    {
        sliderHeight = sliderArea.rect.height;

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
        if (!joustManager.horsePartIsOn)
        {
            HideUI();
            return;
        }

        // Bloquear totalmente el input y la lógica mientras haya tutorial abierto
        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        if (!isActive) return;

        MoveIndicator();
        HandleInput();
    }

    void InitializeUI()
    {
        if (resultText != null)
            resultText.gameObject.SetActive(false);

        if (counterText != null)
            counterText.text = "0/" + maxClicks;
    }

    void HideUI()
    {
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
        pos.y += direction * moveSpeed * Time.deltaTime;

        float limit = sliderHeight / 2f;
        if (pos.y > limit || pos.y < -limit)
            direction *= -1f;

        movingIndicator.anchoredPosition = pos;
    }

    void HandleInput()
    {
        bool mouseClick = Input.GetMouseButtonDown(0);

        // Botón X mando Xbox
        bool xboxX = Input.GetKeyDown(KeyCode.JoystickButton2);

        if (mouseClick || xboxX)
            EvaluateZone();
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

        clickCount++;

        if (counterText != null)
            counterText.text = clickCount + "/" + maxClicks;

        if (clickCount >= maxClicks)
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

    public void ResetHorsePhase()
    {
        isActive = true;
        clickCount = 0;
        direction = 1f;

        movingIndicator.anchoredPosition = new Vector2(0f, -sliderHeight / 2f);

        if (counterText != null)
        {
            counterText.gameObject.SetActive(true);
            counterText.text = "0/" + maxClicks;
        }

        if (resultText != null)
            resultText.gameObject.SetActive(false);
    }
}