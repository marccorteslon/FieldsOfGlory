using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WorldMapManager : MonoBehaviour
{
    [Header("Data")]
    public MapDatabase mapDatabase;
    [Tooltip("El ID del nodo donde aparecerá el jugador por defecto si entra a este mapa y su nodo guardado no existe en esta escena.")]
    public string defaultSceneNodeId = "";

    [Header("Refs")]
    public ProgressManager progressManager;
    public CalendarPanelController calendarPanelController;
    public RandomEncounterManager randomEncounterManager;

    [Header("Player")]
    public Transform mapPlayerIcon;
    public float moveSpeed = 300f;

    [Header("Player Animation (Bandera)")]
    public Image playerImage;
    public Sprite[] playerAnimSprites;
    public float animFramesPerSecond = 6f;
    
    private float animTimer;
    private int currentAnimFrame;

    [Header("Input")]
    public string horizontalAxis = "Horizontal";
    public string verticalAxis = "Vertical";
    public KeyCode confirmKey = KeyCode.JoystickButton0;
    public KeyCode keyboardConfirmKey = KeyCode.Return;
    public KeyCode interactKey = KeyCode.X;
    public KeyCode joystickInteractKey = KeyCode.JoystickButton2; // BotÃ³n X en mando Xbox
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

        if (mapPlayerIcon != null)
        {
            if (playerImage == null)
                playerImage = mapPlayerIcon.GetComponent<Image>();

            if (playerImage == null)
                playerImage = mapPlayerIcon.gameObject.AddComponent<Image>();
        }

        if (playerAnimSprites == null || playerAnimSprites.Length == 0)
        {
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>("Sprites/BANDERA");
            if (loadedSprites != null && loadedSprites.Length > 0)
                playerAnimSprites = loadedSprites;
        }
    }

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        PlacePlayerAtCurrentNode();
        RefreshAvailableRoutes();
    }

    void Update()
    {
        UpdatePlayerAnimation();

        if (isMoving || PauseMenuController.IsPaused)
            return;

        HandleCityInteractionInput();
        HandleDirectionInput();
        HandleConfirmInput();
    }

        private bool axisInUse = false;

    void UpdatePlayerAnimation()
    {
        if (playerImage == null || playerAnimSprites == null || playerAnimSprites.Length == 0)
            return;

        animTimer += Time.deltaTime;
        if (animTimer >= 1f / animFramesPerSecond)
        {
            animTimer = 0f;
            currentAnimFrame = (currentAnimFrame + 1) % playerAnimSprites.Length;
            playerImage.sprite = playerAnimSprites[currentAnimFrame];
        }
    }

    void HandleCityInteractionInput()
    {
        if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(joystickInteractKey))
        {
            if (mapDatabase == null || progressManager == null) return;

            MapNodeDefinition currentNode = mapDatabase.GetNodeById(progressManager.CurrentNodeId);
            if (currentNode != null && currentNode.isTown)
            {
                MapNodeView nodeView = GetNodeView(progressManager.CurrentNodeId);
                if (nodeView != null)
                {
                    TownNode townNode = nodeView.GetComponent<TownNode>();
                    if (townNode != null)
                    {
                        townNode.EnterTown();
                        Debug.Log("[Interaction] Abriendo panel de ciudad: " + currentNode.cityId);
                    }
                    else
                    {
                        Debug.LogWarning("[Interaction] El nodo " + progressManager.CurrentNodeId + " es una ciudad pero no tiene componente TownNode.");
                    }
                }
            }
        }
    }

    void HandleDirectionInput()
    {
        MapDirection? direction = null;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) { direction = MapDirection.Up; Debug.Log("[Input] W/UpArrow pressed"); }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) { direction = MapDirection.Down; Debug.Log("[Input] S/DownArrow pressed"); }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) { direction = MapDirection.Left; Debug.Log("[Input] A/LeftArrow pressed"); }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) { direction = MapDirection.Right; Debug.Log("[Input] D/RightArrow pressed"); }

#if ENABLE_INPUT_SYSTEM
        if (direction == null && UnityEngine.InputSystem.Gamepad.current != null)
        {
            var dpad = UnityEngine.InputSystem.Gamepad.current.dpad;
            if (dpad.up.wasPressedThisFrame) { direction = MapDirection.Up; Debug.Log("[Input] Gamepad Dpad UP pressed"); }
            else if (dpad.down.wasPressedThisFrame) { direction = MapDirection.Down; Debug.Log("[Input] Gamepad Dpad DOWN pressed"); }
            else if (dpad.left.wasPressedThisFrame) { direction = MapDirection.Left; Debug.Log("[Input] Gamepad Dpad LEFT pressed"); }
            else if (dpad.right.wasPressedThisFrame) { direction = MapDirection.Right; Debug.Log("[Input] Gamepad Dpad RIGHT pressed"); }
        }
#endif

        if (direction == null)
        {
            float h = Input.GetAxisRaw(horizontalAxis);
            float v = Input.GetAxisRaw(verticalAxis);

            Vector2 input = new Vector2(h, v);

            if (input.magnitude >= inputDeadzone)
            {
                if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                    direction = input.x > 0 ? MapDirection.Right : MapDirection.Left;
                else
                    direction = input.y > 0 ? MapDirection.Up : MapDirection.Down;
                
                // Reducimos el spam de log del AxisRaw limitÃƒÂ¡ndolo a cuando se acaba de pulsar
                if (!axisInUse) Debug.Log($"[Input] AxisRaw triggered with direction: {direction}");
                axisInUse = true;
            }
            else
            {
                axisInUse = false;
            }
        }

        if (direction != null)
        {
            Debug.Log($"[Input] Trying to select connection for direction: {direction}");
            SelectConnectionByDirection(direction.Value);
        }
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
            Debug.Log($"Ruta seleccionada: {destinationNode.displayName} | DÃƒÂ­as: {destinationNode.travelDaysCost} | Peligro: {destinationNode.dangerIndex}");
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
        if (!destinationNode.isCrossroad)
            progressManager.AdvanceDays(destinationNode.travelDaysCost);

        if (destinationNode.isTown)
            progressManager.SetCurrentCity(destinationNode.cityId);

        if (calendarPanelController != null)
            calendarPanelController.RefreshCalendar();

        if (randomEncounterManager != null && !destinationNode.isCrossroad)
            randomEncounterManager.TryTriggerEncounter(destinationNode);

        selectedConnection = null;

        foreach (var view in connectionViews)
            view.SetSelected(false);

        RefreshAvailableRoutes();

        isMoving = false;
    }

    IEnumerator MoveToPoint(Vector3 targetPosition)
    {
        float timeout = 5f;
        while (Vector3.Distance(mapPlayerIcon.position, targetPosition) > 1f && timeout > 0f)
        {
            timeout -= Time.deltaTime;
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
            Debug.LogWarning("No se encontró MapNodeView para " + progressManager.CurrentNodeId + " en esta escena.");
            
            if (!string.IsNullOrEmpty(defaultSceneNodeId))
            {
                Debug.Log($"Intentando usar el nodo por defecto de la escena: {defaultSceneNodeId}");
                nodeView = GetNodeView(defaultSceneNodeId);
                
                if (nodeView != null)
                {
                    progressManager.SetCurrentNode(defaultSceneNodeId);
                    
                    if (mapDatabase != null)
                    {
                        MapNodeDefinition def = mapDatabase.GetNodeById(defaultSceneNodeId);
                        if (def != null && def.isTown)
                            progressManager.SetCurrentCity(def.cityId);
                    }
                }
            }

            if (nodeView == null)
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
            Debug.LogWarning("No se encontrÃƒÂ³ nodo para mover: " + nodeId);
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









