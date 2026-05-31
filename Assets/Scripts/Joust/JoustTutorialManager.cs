using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class JoustTutorialManager : MonoBehaviour
{
    // ---------------------------------------------------------------
    // TUTORIAL SCENE MODE (VideoPlayer + UI auto-generada)
    // ---------------------------------------------------------------

    [Header("Tutorial Scene Mode")]
    [Tooltip("Activar en la escena NewTutorial. Usa VideoPlayer y UI auto-generada en lugar de paneles manuales.")]
    public bool isTutorialScene = false;

    [Header("Video Clips (Solo Tutorial Scene)")]
    [Tooltip("Clip en loop que muestra cómo jugar la fase del Caballo.")]
    public VideoClip horseTutorialClip;
    [Tooltip("Clip en loop que muestra cómo jugar la fase de Ataque.")]
    public VideoClip attackTutorialClip;
    [Tooltip("Clip en loop que muestra cómo jugar la fase de Defensa.")]
    public VideoClip defenseTutorialClip;

    [Header("Tutorial Texts")]
    [TextArea(2, 4)] public string horseTutorialTitle = "FASE 1: CABALLO";
    [TextArea(2, 4)] public string horseTutorialDesc = "Pulsa X (Mando) o Click Izquierdo cuando el indicador esté en la zona verde para cargar tu caballo al máximo.";

    [TextArea(2, 4)] public string attackTutorialTitle = "FASE 2: ATAQUE";
    [TextArea(2, 4)] public string attackTutorialDesc = "Apunta con el ratón o el stick derecho hacia el enemigo. Mantén R2/Click Izquierdo para cargar tu lanza y suelta para atacar.";

    [TextArea(2, 4)] public string defenseTutorialTitle = "FASE 3: DEFENSA";
    [TextArea(2, 4)] public string defenseTutorialDesc = "Usa el stick izquierdo o las teclas WASD para mover tu escudo y bloquear el ataque enemigo siguiendo el indicador rojo.";

    // ---------------------------------------------------------------
    // LEGACY PANEL MODE (Escena Justa normal)
    // ---------------------------------------------------------------

    [Header("Legacy Tutorial Panels (Escena Justa normal)")]
    public GameObject horseTutorialPanel;
    public GameObject attackTutorialPanel;
    public GameObject defenseTutorialPanel;

    // ---------------------------------------------------------------
    // RUNTIME STATE
    // ---------------------------------------------------------------

    private GameObject currentPanel; // legacy mode

    // Video tutorial UI (auto-generated)
    private GameObject tutorialOverlay;
    private RawImage videoRawImage;
    private TextMeshProUGUI titleLabel;
    private TextMeshProUGUI descLabel;
    private TextMeshProUGUI buttonLabel;
    private VideoPlayer videoPlayer;
    private RenderTexture videoRenderTexture;

    private float timeScaleBeforeTutorial = 1f;
    private bool tutorialPausedTime = false;
    private bool isShowingTutorial = false;
    private string currentPhase = ""; // "horse", "attack", "defense"

    private const string TutorialEnabledKey = "JoustTutorialEnabled";

    // ---------------------------------------------------------------
    // LIFECYCLE
    // ---------------------------------------------------------------

    void Awake()
    {
        if (isTutorialScene)
        {
            BuildTutorialUI();
            HideTutorialUI();
        }
        else
        {
            HideAllLegacyPanels();
        }

        Time.timeScale = 1f;
    }

    void OnDestroy()
    {
        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
        }
    }

    void Update()
    {
        if (!IsTutorialOpen()) return;

        // Input para cerrar tutorial (funciona con Time.timeScale = 0)
        bool closeWithController = Input.GetKeyDown(KeyCode.JoystickButton1); // B en Xbox
        bool closeWithKeyboard = Input.GetKeyDown(KeyCode.X);
        bool closeWithSpace = Input.GetKeyDown(KeyCode.Space);

        if (closeWithController || closeWithKeyboard || closeWithSpace)
            CloseTutorial();
    }

    // ---------------------------------------------------------------
    // PUBLIC API (usada por JoustManager y otros)
    // ---------------------------------------------------------------

    public bool ShouldShowTutorial()
    {
        if (isTutorialScene) return true;
        return PlayerPrefs.GetInt(TutorialEnabledKey, 0) == 1;
    }

    public bool IsTutorialOpen()
    {
        if (isTutorialScene)
            return isShowingTutorial;
        return currentPanel != null && currentPanel.activeSelf;
    }

    public void ShowHorseTutorial()
    {
        if (!ShouldShowTutorial()) return;

        if (isTutorialScene)
            ShowTutorialVideo("horse", horseTutorialClip, horseTutorialTitle, horseTutorialDesc);
        else
            ShowLegacyPanel(horseTutorialPanel);
    }

    public void ShowAttackTutorial()
    {
        if (!ShouldShowTutorial()) return;

        if (isTutorialScene)
            ShowTutorialVideo("attack", attackTutorialClip, attackTutorialTitle, attackTutorialDesc);
        else
            ShowLegacyPanel(attackTutorialPanel);
    }

    public void ShowDefenseTutorial()
    {
        if (!ShouldShowTutorial()) return;

        if (isTutorialScene)
            ShowTutorialVideo("defense", defenseTutorialClip, defenseTutorialTitle, defenseTutorialDesc);
        else
            ShowLegacyPanel(defenseTutorialPanel);
    }

    public void CloseTutorial()
    {
        if (isTutorialScene)
            CloseTutorialVideo();
        else
            CloseLegacyTutorial();
    }

    public void EnableTutorial()
    {
        PlayerPrefs.SetInt(TutorialEnabledKey, 1);
        PlayerPrefs.Save();
    }

    public void DisableTutorial()
    {
        PlayerPrefs.SetInt(TutorialEnabledKey, 0);
        PlayerPrefs.Save();
    }

    // ---------------------------------------------------------------
    // VIDEO TUTORIAL SYSTEM (isTutorialScene = true)
    // ---------------------------------------------------------------

    void ShowTutorialVideo(string phase, VideoClip clip, string title, string description)
    {
        if (tutorialOverlay == null) return;

        // Pausar el juego
        if (!tutorialPausedTime)
        {
            timeScaleBeforeTutorial = Time.timeScale;
            tutorialPausedTime = true;
        }
        Time.timeScale = 0f;
        isShowingTutorial = true;
        currentPhase = phase;

        // Textos
        if (titleLabel != null) titleLabel.text = title;
        if (descLabel != null) descLabel.text = description;

        // Reproducir vídeo en loop
        if (videoPlayer != null && clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
        }

        // Mostrar si no hay clip (con placeholder negro)
        if (videoRawImage != null)
        {
            videoRawImage.color = clip != null ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f);
        }

        tutorialOverlay.SetActive(true);
    }

    void CloseTutorialVideo()
    {
        bool wasShowingAttack = (currentPhase == "attack");

        // Parar vídeo
        if (videoPlayer != null)
            videoPlayer.Stop();

        HideTutorialUI();
        isShowingTutorial = false;
        currentPhase = "";

        // Encadenar: después del tutorial de ataque → mostrar tutorial de defensa
        if (wasShowingAttack && ShouldShowTutorial())
        {
            ShowTutorialVideo("defense", defenseTutorialClip, defenseTutorialTitle, defenseTutorialDesc);
            return;
        }

        // Restaurar tiempo
        if (tutorialPausedTime)
        {
            Time.timeScale = timeScaleBeforeTutorial;
            tutorialPausedTime = false;
        }
    }

    void HideTutorialUI()
    {
        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);
    }

    // ---------------------------------------------------------------
    // LEGACY PANEL SYSTEM (isTutorialScene = false)
    // ---------------------------------------------------------------

    void ShowLegacyPanel(GameObject panel)
    {
        if (panel == null) return;

        HideAllLegacyPanels();

        currentPanel = panel;
        currentPanel.SetActive(true);

        if (!tutorialPausedTime)
        {
            timeScaleBeforeTutorial = Time.timeScale;
            tutorialPausedTime = true;
        }

        Time.timeScale = 0f;
    }

    void CloseLegacyTutorial()
    {
        bool closingAttackTutorial = currentPanel == attackTutorialPanel;

        if (currentPanel != null)
            currentPanel.SetActive(false);

        currentPanel = null;

        if (closingAttackTutorial && ShouldShowTutorial() && defenseTutorialPanel != null)
        {
            currentPanel = defenseTutorialPanel;
            currentPanel.SetActive(true);
            Time.timeScale = 0f;
            return;
        }

        if (tutorialPausedTime)
        {
            Time.timeScale = timeScaleBeforeTutorial;
            tutorialPausedTime = false;
        }
    }

    void HideAllLegacyPanels()
    {
        if (horseTutorialPanel != null)
            horseTutorialPanel.SetActive(false);

        if (attackTutorialPanel != null)
            attackTutorialPanel.SetActive(false);

        if (defenseTutorialPanel != null)
            defenseTutorialPanel.SetActive(false);

        currentPanel = null;
    }

    // ---------------------------------------------------------------
    // UI BUILDER — Crea la UI del tutorial en runtime
    // ---------------------------------------------------------------

    void BuildTutorialUI()
    {
        // Buscar Canvas existente en la escena
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("TutorialCanvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // RenderTexture para el VideoPlayer
        videoRenderTexture = new RenderTexture(1280, 720, 0);
        videoRenderTexture.Create();

        // VideoPlayer en este GameObject
        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        // ─── OVERLAY (fondo oscuro pantalla completa) ───
        tutorialOverlay = CreateUIElement("TutorialOverlay", canvas.transform);
        StretchFull(tutorialOverlay.GetComponent<RectTransform>());
        var overlayImg = tutorialOverlay.AddComponent<Image>();
        overlayImg.color = new Color(0f, 0f, 0f, 0.88f);

        // Canvas propio para asegurar que se renderiza encima de todo
        var overlayCanvas = tutorialOverlay.AddComponent<Canvas>();
        overlayCanvas.overrideSorting = true;
        overlayCanvas.sortingOrder = 200;
        tutorialOverlay.AddComponent<GraphicRaycaster>();

        // ─── CONTENT PANEL (centrado) ───
        GameObject contentPanel = CreateUIElement("TutorialContentPanel", tutorialOverlay.transform);
        var contentRT = contentPanel.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(920, 650);

        var contentImg = contentPanel.AddComponent<Image>();
        contentImg.color = new Color(0.06f, 0.06f, 0.1f, 0.96f);

        // Layout vertical
        var vlg = contentPanel.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(40, 40, 30, 25);
        vlg.spacing = 12;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        // ─── TÍTULO ───
        GameObject titleGO = CreateUIElement("TitleText", contentPanel.transform);
        titleLabel = titleGO.AddComponent<TextMeshProUGUI>();
        titleLabel.text = "TUTORIAL";
        titleLabel.fontSize = 38;
        titleLabel.fontStyle = FontStyles.Bold;
        titleLabel.color = new Color(1f, 0.84f, 0.25f, 1f); // Dorado
        titleLabel.alignment = TextAlignmentOptions.Center;
        var titleLE = titleGO.AddComponent<LayoutElement>();
        titleLE.preferredHeight = 55;

        // ─── SEPARADOR SUPERIOR ───
        CreateSeparator(contentPanel.transform, new Color(1f, 0.84f, 0.25f, 0.4f));

        // ─── VIDEO (RawImage) ───
        GameObject videoGO = CreateUIElement("VideoImage", contentPanel.transform);
        videoRawImage = videoGO.AddComponent<RawImage>();
        videoRawImage.texture = videoRenderTexture;
        videoRawImage.color = Color.white;
        var videoLE = videoGO.AddComponent<LayoutElement>();
        videoLE.preferredHeight = 340;

        // Borde del vídeo
        var videoOutline = videoGO.AddComponent<Outline>();
        videoOutline.effectColor = new Color(1f, 0.84f, 0.25f, 0.5f);
        videoOutline.effectDistance = new Vector2(2, 2);

        // ─── DESCRIPCIÓN ───
        GameObject descGO = CreateUIElement("DescText", contentPanel.transform);
        descLabel = descGO.AddComponent<TextMeshProUGUI>();
        descLabel.text = "";
        descLabel.fontSize = 22;
        descLabel.color = new Color(0.85f, 0.85f, 0.9f, 1f);
        descLabel.alignment = TextAlignmentOptions.Center;
        descLabel.enableWordWrapping = true;
        var descLE = descGO.AddComponent<LayoutElement>();
        descLE.preferredHeight = 65;

        // ─── SEPARADOR INFERIOR ───
        CreateSeparator(contentPanel.transform, new Color(0.4f, 0.4f, 0.5f, 0.3f));

        // ─── BOTÓN CONTINUAR ───
        GameObject btnGO = CreateUIElement("ContinueButton", contentPanel.transform);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.55f, 0.34f, 1f); // Verde elegante

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;

        // Hover color
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.22f, 0.65f, 0.4f, 1f);
        colors.pressedColor = new Color(0.14f, 0.45f, 0.28f, 1f);
        btn.colors = colors;

        var btnLE = btnGO.AddComponent<LayoutElement>();
        btnLE.preferredHeight = 55;

        // Texto del botón
        GameObject btnTextGO = CreateUIElement("ButtonText", btnGO.transform);
        StretchFull(btnTextGO.GetComponent<RectTransform>());
        buttonLabel = btnTextGO.AddComponent<TextMeshProUGUI>();
        buttonLabel.text = "CONTINUAR  [ X ]";
        buttonLabel.fontSize = 26;
        buttonLabel.fontStyle = FontStyles.Bold;
        buttonLabel.color = Color.white;
        buttonLabel.alignment = TextAlignmentOptions.Center;

        btn.onClick.AddListener(CloseTutorial);

        tutorialOverlay.SetActive(false);
    }

    void CreateSeparator(Transform parent, Color color)
    {
        GameObject sep = CreateUIElement("Separator", parent);
        var sepImg = sep.AddComponent<Image>();
        sepImg.color = color;
        var sepLE = sep.AddComponent<LayoutElement>();
        sepLE.preferredHeight = 2;
    }

    GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}