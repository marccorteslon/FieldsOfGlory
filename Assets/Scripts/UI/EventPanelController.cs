using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EventPanelController : MonoBehaviour
{
    [Header("Refs")]
    public ProgressManager progressManager;

    [Header("UI Elements")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Image eventImage;

    [System.Serializable]
    public class EventChoiceUI
    {
        public GameObject buttonObject;
        public TMP_Text choiceText;
        public Button buttonComponent;
    }

    [Header("Choices (Max 4)")]
    public EventChoiceUI[] choiceSlots = new EventChoiceUI[4];

    private EventNodeDefinition currentEvent;
    private RPGEventNode currentNode;

    void Awake()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();
    }

    public void LoadEvent(EventNodeDefinition ev, RPGEventNode node)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (ev == null)
        {
            ClosePanel();
            return;
        }

        currentEvent = ev;
        currentNode = node;

        if (titleText != null) titleText.text = ev.title;
        if (descriptionText != null) descriptionText.text = ev.description;
        
        if (eventImage != null)
        {
            if (ev.eventImage != null)
            {
                eventImage.sprite = ev.eventImage;
                eventImage.gameObject.SetActive(true);
            }
            else
            {
                eventImage.gameObject.SetActive(false);
            }
        }

        // Configurar botones de opciones
        for (int i = 0; i < choiceSlots.Length; i++)
        {
            if (i < ev.choices.Count)
            {
                choiceSlots[i].buttonObject.SetActive(true);
                choiceSlots[i].choiceText.text = ev.choices[i].choiceText;
                
                int index = i; // Captura para el closure del listener
                choiceSlots[i].buttonComponent.onClick.RemoveAllListeners();
                choiceSlots[i].buttonComponent.onClick.AddListener(() => OnChoiceSelected(index));
            }
            else
            {
                choiceSlots[i].buttonObject.SetActive(false);
            }
        }
    }

    private void OnChoiceSelected(int index)
    {
        if (currentEvent == null || index < 0 || index >= currentEvent.choices.Count)
            return;

        EventChoice choice = currentEvent.choices[index];

        // Aplicar recompensas o penalizaciones
        if (choice.moneyReward > 0)
        {
            if (progressManager != null)
                progressManager.AddMoney(choice.moneyReward);
        }
        else if (choice.moneyReward < 0)
        {
            if (progressManager != null)
            {
                // Convertir a valor absoluto para gastar
                progressManager.TrySpendMoney(Mathf.Abs(choice.moneyReward));
            }
        }

        // Navegar a la siguiente parte del evento o cerrar
        if (choice.nextEvent != null)
        {
            LoadEvent(choice.nextEvent, currentNode);
        }
        else
        {
            ClosePanel();
        }
    }

    private void ClosePanel()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (currentNode != null)
        {
            currentNode.ExitEventNode();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
