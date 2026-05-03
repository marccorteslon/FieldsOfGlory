using System.Collections;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    [Header("Data")]
    public MapDatabase mapDatabase;

    [Header("Refs")]
    public ProgressManager progressManager;
    public CalendarPanelController calendarPanelController;
    public RandomEncounterManager randomEncounterManager;

    [Header("Player")]
    public Transform mapPlayerIcon;
    public float moveSpeed = 300f;

    [Header("Input")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public KeyCode confirmKey = KeyCode.JoystickButton0;
    public KeyCode keyboardConfirmKey = KeyCode.Return;
    public float inputDeadzone = 0.6f;

    private MapNodeView[] nodeViews;
    private MapConnectionView[] connectionViews;

    private MapConnectionDefinition selectedConnection;
    private MapConnectionView selectedConnectionView;

    private bool isMoving;

    void Awake()
    {
        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (randomEncounterManager == null)
            randomEncounterManager = FindFirstObjectByType<RandomEncounterManager>();

        nodeViews = FindObjectsByType<MapNodeView>(FindObjectsSortMode.None);
        connectionViews = FindObjectsByType<MapConnectionView>(FindObjectsSortMode.None);
    }

    void Start()
    {
        PlacePlayerAtCurrentNode();
        RefreshAvailableRoutes();
    }

    void Update()
    {
        if (isMoving)
            return;

        HandleDirectionInput();
        HandleConfirmInput();
    }

    void HandleDirectionInput()
    {
        MapDirection? direction = null;

        if (Input.GetKeyDown(KeyCode.W))
            direction = MapDirection.Up;
        else if (Input.GetKeyDown(KeyCode.S))
            direction = MapDirection.Down;
        else if (Input.GetKeyDown(KeyCode.A))
            direction = MapDirection.Left;
        else if (Input.GetKeyDown(KeyCode.D))
            direction = MapDirection.Right;

        if (direction == null)
        {
            float h = Input.GetAxisRaw(horizontalAxis);
            float v = Input.GetAxisRaw(verticalAxis);

            Vector2 input = new Vector2(h, v);

            if (input.magnitude < inputDeadzone)
                return;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                direction = input.x > 0 ? MapDirection.Right : MapDirection.Left;
            else
                direction = input.y > 0 ? MapDirection.Up : MapDirection.Down;
        }

        if (direction != null)
            SelectConnectionByDirection(direction.Value);
    }

    void HandleConfirmInput()
    {
        if (selectedConnection == null)
            return;

        if (Input.GetKeyDown(confirmKey) || Input.GetKeyDown(keyboardConfirmKey))
            StartCoroutine(TravelSelectedRoute());
    }

    void SelectConnectionByDirection(MapDirection direction)
    {
        if (mapDatabase == null || progressManager == null)
            return;

        string currentNodeId = progressManager.CurrentNodeId;
        var connections = mapDatabase.GetConnectionsFromNode(currentNodeId);

        foreach (var connection in connections)
        {
            MapDirection connectionDirection =
                mapDatabase.GetDirectionFromNode(currentNodeId, connection);

            if (connectionDirection == direction)
            {
                SelectConnection(connection);
                return;
            }
        }
    }

    void SelectConnection(MapConnectionDefinition connection)
    {
        selectedConnection = connection;

        foreach (var view in connectionViews)
            view.SetSelected(false);

        selectedConnectionView = GetConnectionView(connection);

        if (selectedConnectionView != null)
            selectedConnectionView.SetSelected(true);

        string destinationId = mapDatabase.GetOtherNodeId(progressManager.CurrentNodeId, connection);
        MapNodeDefinition destinationNode = mapDatabase.GetNodeById(destinationId);

        if (destinationNode != null)
        {
            Debug.Log($"Ruta seleccionada: {destinationNode.displayName} | Días: {destinationNode.travelDaysCost} | Peligro: {destinationNode.dangerIndex}");
        }
    }

    IEnumerator TravelSelectedRoute()
    {
        if (selectedConnection == null || selectedConnectionView == null)
            yield break;

        isMoving = true;

        string currentNodeId = progressManager.CurrentNodeId;
        string destinationNodeId = mapDatabase.GetOtherNodeId(currentNodeId, selectedConnection);

        MapNodeDefinition destinationNode = mapDatabase.GetNodeById(destinationNodeId);

        if (destinationNode == null)
        {
            Debug.LogError("Destino no encontrado: " + destinationNodeId);
            isMoving = false;
            yield break;
        }

        Transform[] path = selectedConnectionView.waypoints;

        for (int i = 0; i < path.Length; i++)
            yield return MoveToPoint(path[i].position);

        MapNodeView destinationView = GetNodeView(destinationNodeId);

        if (destinationView != null)
        {
            Transform stopPoint = destinationView.playerStopPoint != null
                ? destinationView.playerStopPoint
                : destinationView.transform;

            yield return MoveToPoint(stopPoint.position);
        }

        progressManager.SetCurrentNode(destinationNode.nodeId);
        progressManager.AdvanceDays(destinationNode.travelDaysCost);

        if (destinationNode.isTown)
            progressManager.SetCurrentCity(destinationNode.cityId);

        if (calendarPanelController != null)
            calendarPanelController.RefreshCalendar();

        if (randomEncounterManager != null)
            randomEncounterManager.TryTriggerEncounter(destinationNode);

        selectedConnection = null;

        foreach (var view in connectionViews)
            view.SetSelected(false);

        RefreshAvailableRoutes();

        isMoving = false;
    }

    IEnumerator MoveToPoint(Vector3 targetPosition)
    {
        while (Vector3.Distance(mapPlayerIcon.position, targetPosition) > 0.05f)
        {
            mapPlayerIcon.position = Vector3.MoveTowards(
                mapPlayerIcon.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        mapPlayerIcon.position = targetPosition;
    }

    void PlacePlayerAtCurrentNode()
    {
        if (progressManager == null || mapPlayerIcon == null)
            return;

        MapNodeView nodeView = GetNodeView(progressManager.CurrentNodeId);

        if (nodeView == null)
        {
            Debug.LogWarning("No se encontró MapNodeView para " + progressManager.CurrentNodeId);
            return;
        }

        Transform stopPoint = nodeView.playerStopPoint != null
            ? nodeView.playerStopPoint
            : nodeView.transform;

        mapPlayerIcon.position = stopPoint.position;
    }

    void RefreshAvailableRoutes()
    {
        foreach (var view in connectionViews)
            view.SetSelected(false);

        selectedConnection = null;
        selectedConnectionView = null;
    }

    MapNodeView GetNodeView(string nodeId)
    {
        foreach (var view in nodeViews)
        {
            if (view.nodeId == nodeId)
                return view;
        }

        return null;
    }

    MapConnectionView GetConnectionView(MapConnectionDefinition connection)
    {
        foreach (var view in connectionViews)
        {
            if (view.connection == connection)
                return view;
        }

        return null;
    }
    public void ForceMoveToNode(string nodeId)
    {
        MapNodeView nodeView = GetNodeView(nodeId);

        if (nodeView == null)
        {
            Debug.LogWarning("No se encontró nodo para mover: " + nodeId);
            return;
        }

        Transform stopPoint = nodeView.playerStopPoint != null
            ? nodeView.playerStopPoint
            : nodeView.transform;

        if (mapPlayerIcon != null)
            mapPlayerIcon.position = stopPoint.position;

        MapNodeDefinition node = mapDatabase.GetNodeById(nodeId);

        if (node != null && node.isTown && progressManager != null)
            progressManager.SetCurrentCity(node.cityId);

        if (calendarPanelController != null)
            calendarPanelController.RefreshCalendar();

        RefreshAvailableRoutes();

        Debug.Log($"[Map] Movido forzosamente a nodo: {nodeId}");
    }
}