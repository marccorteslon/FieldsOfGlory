using UnityEngine;

public class RPGEventNode : MonoBehaviour
{
    [Header("Event Data")]
    public string nodeId;
    public EventNodeDefinition initialEvent;

    [Header("UI Refs")]
    public ProgressManager progressManager;
    public EventPanelController eventPanel;
    public GameObject mapButtonsObject;

    public void EnterEventNode()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (progressManager == null)
        {
            Debug.LogError("RPGEventNode: No se encontro ProgressManager.");
            return;
        }

        // SOLO puedes abrir el evento si estas en el nodo correcto
        if (progressManager.CurrentCityId != nodeId)
        {
            Debug.Log($"Primero debes viajar a {nodeId}.");
            return;
        }

        if (initialEvent == null)
        {
            Debug.LogError("RPGEventNode: No hay evento inicial asignado.");
            return;
        }

        if (mapButtonsObject != null)
            mapButtonsObject.SetActive(false);

        if (eventPanel != null)
        {
            eventPanel.gameObject.SetActive(true);
            eventPanel.LoadEvent(initialEvent, this);
            Debug.Log($"Iniciando evento narrativo: {initialEvent.title}");
        }
        else
        {
            Debug.LogError("RPGEventNode: No se ha asignado EventPanelController.");
        }
    }

    public void ExitEventNode()
    {
        if (eventPanel != null)
            eventPanel.gameObject.SetActive(false);

        if (mapButtonsObject != null)
            mapButtonsObject.SetActive(true);
            
        Debug.Log($"Saliendo del evento narrativo en el nodo {nodeId}.");
    }
}
