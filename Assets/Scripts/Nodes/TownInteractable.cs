using UnityEngine;

public class TownInteractable : MonoBehaviour
{
    public enum TownInteractionType
    {
        Shop,
        Tavern,
        Travel,
        Joust,
        Wait,
        ExitTown,
        TogglePanel
    }

    [Header("Interaction")]
    public TownInteractionType interactionType;
    public KeyCode interactKey = KeyCode.E;
    public string playerTag = "Player";

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

            case TownInteractionType.Joust:
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
        }
    }
}