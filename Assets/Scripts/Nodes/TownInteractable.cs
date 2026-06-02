using UnityEngine;

public class TownInteractable : MonoBehaviour
{
    public enum TownInteractionType
    {
        Shop,
        Tavern,
        Travel,
        TournamentJoust,
        Wait,
        ExitTown,
        TogglePanel,
        PracticeJoust,
        Disparo,
        MainMenu
    }

    [Header("Interaction")]
    public TownInteractionType interactionType;
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

    [Header("Practice Joust Settings")]
    [Tooltip("Only used if InteractionType is PracticeJoust")]
    public JoustDifficulty practiceDifficulty = JoustDifficulty.Normal;

    [Header("Refs")]
    public TownNode townNode;
    public TownTravelUI travelUI;
    public SceneChanger sceneChanger;
    public WaitButtonController waitButtonController;
    public PanelController panelController;

    private bool playerInside;

    void Awake()
    {
        if (townNode == null)
            townNode = FindFirstObjectByType<TownNode>();

        if (travelUI == null)
            travelUI = FindFirstObjectByType<TownTravelUI>();

        if (waitButtonController == null)
            waitButtonController = FindFirstObjectByType<WaitButtonController>();
    }

    void OnEnable()
    {
        RefreshVisibility();
    }

    public void RefreshVisibility()
    {
        if (interactionType == TownInteractionType.TournamentJoust)
        {
            TournamentManager tm = FindFirstObjectByType<TournamentManager>();
            if (tm != null)
            {
                gameObject.SetActive(tm.HasTournamentInCurrentCityToday());
            }
        }
    }

    void Update()
    {
        if (!playerInside)
            return;

        if (Input.GetKeyDown(interactKey))
            Interact();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrWhiteSpace(playerTag) && !other.CompareTag(playerTag))
            return;

        playerInside = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrWhiteSpace(playerTag) && !other.CompareTag(playerTag))
            return;

        playerInside = false;
    }

    public void Interact()
    {
        switch (interactionType)
        {
            case TownInteractionType.Shop:
                if (townNode != null)
                    townNode.EnterShop();
                break;

            case TownInteractionType.Tavern:
                if (townNode != null)
                    townNode.EnterTavern();
                break;

            case TownInteractionType.Travel:
                if (travelUI != null)
                    travelUI.TravelSelected();
                break;

            case TownInteractionType.TournamentJoust:
                TournamentManager tm = FindFirstObjectByType<TournamentManager>();
                ProgressManager pmInst = FindFirstObjectByType<ProgressManager>();
                if (tm != null && pmInst != null)
                {
                    var todayTournament = tm.GetTournamentForCityAndDate(
                        pmInst.CurrentCityId,
                        pmInst.CurrentDay,
                        pmInst.CurrentMonth
                    );
                    if (todayTournament != null)
                    {
                        ProgressManager.PracticeDifficultyOverride = todayTournament.difficulty;
                        Debug.Log($"[TownInteractable] Dificultad del torneo de hoy '{todayTournament.difficulty}' asignada como override estático.");
                    }
                }
                ProgressManager.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (sceneChanger != null)
                    sceneChanger.ChangeScene();
                break;

            case TownInteractionType.PracticeJoust:
                ProgressManager.PracticeDifficultyOverride = practiceDifficulty;
                ProgressManager.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (sceneChanger != null)
                    sceneChanger.ChangeScene();
                break;

            case TownInteractionType.Disparo:
                ProgressManager.ReturnSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (sceneChanger != null)
                    sceneChanger.ChangeScene();
                break;

            case TownInteractionType.Wait:
                if (waitButtonController != null)
                    waitButtonController.WaitOneDay();
                break;

            case TownInteractionType.ExitTown:
                if (townNode != null)
                    townNode.ExitTown();
                break;

            case TownInteractionType.TogglePanel:
                if (panelController != null)
                    panelController.TogglePanel();
                break;

            case TownInteractionType.MainMenu:
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                break;
        }
    }
}