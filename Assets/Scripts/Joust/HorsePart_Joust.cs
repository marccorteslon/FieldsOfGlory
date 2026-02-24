using UnityEngine;
using UnityEngine.UI;

//RESUMEN SCRIPT: Controla la fase del caballo, detecta la zona del indicador y suma puntos usando MV y V del loadout.

public class HorsePart_Joust : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform sliderArea;
    public RectTransform movingIndicatorPrefab;
    public JoustManager joustManager;
    public ScoreManager scoreManager;

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
    }

    void Update()
    {
        if (!joustManager.horsePartIsOn || !isActive) return;

        MoveIndicator();
        HandleInput();
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
        if (pos.y > limit || pos.y < -limit) direction *= -1f;

        movingIndicator.anchoredPosition = pos;
    }

    void HandleInput()
    {
        bool spacePressed = Input.GetKeyDown(KeyCode.Space);
        bool ps4X = Input.GetKeyDown(KeyCode.JoystickButton2);

        if (spacePressed || ps4X)
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

        if (normalized <= redProportion || normalized >= 1f - redProportion) zone = "Rojo";
        else if (normalized <= redProportion + yellowProportion || normalized >= 1f - (redProportion + yellowProportion)) zone = "Amarillo";
        else zone = "Verde";

        // Saca MV y V del equipo del GhostPlayer
        scoreManager.AddHorsePhaseScore(zone, GetMV(), GetV());

        clickCount++;
        if (clickCount >= 3)
        {
            isActive = false;
        }
    }

    public void ResetHorsePhase()
    {
        isActive = true;
        clickCount = 0;
        direction = 1f;
        movingIndicator.anchoredPosition = new Vector2(0f, -sliderHeight / 2f);
    }
}