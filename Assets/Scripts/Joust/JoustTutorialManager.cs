using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class JoustTutorialManager : MonoBehaviour
{
    // TUTORIAL SCENE MODE

    [Header("Tutorial Scene Mode")]
    [Tooltip("Activar en la escena NewTutorial. Usa un panel ya creado en la escena.")]
    public bool isTutorialScene = false;

    [Header("Manual Tutorial Panel")]
    public GameObject tutorialPanel;
    public RawImage videoRawImage;
    public TextMeshProUGUI titleLabel;
    public TextMeshProUGUI descLabel;
    public TextMeshProUGUI buttonLabel;
    public Button nextButton;

    [Header("Video Clips")]
    public VideoClip horseTutorialClip;
    public VideoClip attackTutorialClip;
    public VideoClip defenseTutorialClip;

    [Header("Tutorial Texts")]
    [TextArea(2, 4)] public string horseTutorialTitle = "FASE 1: CABALLO";
    [TextArea(2, 4)] public string horseTutorialDesc = "Pulsa X (Mando) o Click Izquierdo cuando el indicador esté en la zona verde para cargar tu caballo al máximo.";

    [TextArea(2, 4)] public string attackTutorialTitle = "FASE 2: ATAQUE";
    [TextArea(2, 4)] public string attackTutorialDesc = "Apunta con el ratón o el stick derecho hacia el enemigo. Mantén R2/Click Izquierdo para cargar tu lanza y suelta para atacar.";

    [TextArea(2, 4)] public string defenseTutorialTitle = "FASE 3: DEFENSA";
    [TextArea(2, 4)] public string defenseTutorialDesc = "Usa el stick izquierdo o las teclas WASD para mover tu escudo y bloquear el ataque enemigo siguiendo el indicador rojo.";

    // LEGACY PANEL MODE

    [Header("Legacy Tutorial Panels")]
    public GameObject horseTutorialPanel;
    public GameObject attackTutorialPanel;
    public GameObject defenseTutorialPanel;

    // RUNTIME

    private GameObject currentPanel;

    private VideoPlayer videoPlayer;
    private RenderTexture videoRenderTexture;

    private float timeScaleBeforeTutorial = 1f;
    private bool tutorialPausedTime = false;
    private bool isShowingTutorial = false;
    private string currentPhase = "";

    private const string TutorialEnabledKey = "JoustTutorialEnabled";

    // LIFECYCLE

    void Awake()
    {
        // Asegurar que el cursor esté visible y desbloqueado en la escena de la justa/tutorial
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (isTutorialScene)
        {
            SetupTutorialPanel();
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
        if (videoPlayer != null)
            videoPlayer.Stop();

        if (videoRenderTexture != null)
        {
            videoRenderTexture.Release();
            Destroy(videoRenderTexture);
        }
    }

    void Update()
    {
        if (!IsTutorialOpen()) return;

        bool closeWithController = Input.GetKeyDown(KeyCode.JoystickButton1);
        bool closeWithKeyboard = Input.GetKeyDown(KeyCode.X);
        bool closeWithSpace = Input.GetKeyDown(KeyCode.Space);

        if (closeWithController || closeWithKeyboard || closeWithSpace)
            CloseTutorial();
    }

    // PUBLIC API

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

    // TUTORIAL SCENE

    void SetupTutorialPanel()
    {
        if (tutorialPanel == null)
            return;

        videoRenderTexture = new RenderTexture(1280, 720, 0);
        videoRenderTexture.Create();

        videoPlayer = gameObject.AddComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoRenderTexture;
        videoPlayer.isLooping = true;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;

        if (videoRawImage != null)
            videoRawImage.texture = videoRenderTexture;

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(CloseTutorial);
        }

        if (buttonLabel != null)
            buttonLabel.text = "CONTINUAR";
    }

    void ShowTutorialVideo(string phase, VideoClip clip, string title, string description)
    {
        if (tutorialPanel == null) return;

        if (!tutorialPausedTime)
        {
            timeScaleBeforeTutorial = Time.timeScale;
            tutorialPausedTime = true;
        }

        Time.timeScale = 0f;

        isShowingTutorial = true;
        currentPhase = phase;

        if (titleLabel != null)
            titleLabel.text = title;

        if (descLabel != null)
            descLabel.text = description;

        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            if (clip != null)
            {
                videoPlayer.clip = clip;
                videoPlayer.Play();
            }
        }

        if (videoRawImage != null)
            videoRawImage.color = clip != null ? Color.white : new Color(0.1f, 0.1f, 0.1f, 1f);

        tutorialPanel.SetActive(true);
    }

    void CloseTutorialVideo()
    {
        bool wasShowingAttack = currentPhase == "attack";

        if (videoPlayer != null)
            videoPlayer.Stop();

        HideTutorialUI();

        isShowingTutorial = false;
        currentPhase = "";

        if (wasShowingAttack && ShouldShowTutorial())
        {
            ShowTutorialVideo("defense", defenseTutorialClip, defenseTutorialTitle, defenseTutorialDesc);
            return;
        }

        if (tutorialPausedTime)
        {
            Time.timeScale = timeScaleBeforeTutorial;
            tutorialPausedTime = false;
        }
    }

    void HideTutorialUI()
    {
        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    // LEGACY PANEL SYSTEM

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
}