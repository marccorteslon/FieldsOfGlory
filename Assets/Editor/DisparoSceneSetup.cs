using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;

public class DisparoSceneSetup : EditorWindow
{
    [MenuItem("Fields of Glory/Setup Scene Disparo")]
    public static void ShowWindow()
    {
        GetWindow<DisparoSceneSetup>("Setup Escena Disparo");
    }

    private void OnGUI()
    {
        GUILayout.Label("Configuración Automática de la Escena Disparo", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "Este script configurará automáticamente la escena activa 'Disparo' (que es una copia de Justa), " +
            "removiendo los scripts obsoletos de justa, configurando la cámara en primera persona, " +
            "creando el GameManager con la ballesta y colocando dianas interactivas de prueba a lo largo de la pista.", 
            MessageType.Info
        );

        GUILayout.Space(15);

        if (GUILayout.Button("¡Configurar Escena Disparo Ahora!", GUILayout.Height(40)))
        {
            SetupScene();
        }
    }

    private static void SetupScene()
    {
        // 1. Verificar nombre de escena
        var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (activeScene.name != "Disparo")
        {
            bool proceed = EditorUtility.DisplayDialog(
                "Escena Incorrecta", 
                $"La escena activa es '{activeScene.name}', no 'Disparo'. ¿Seguro que quieres configurar esta escena?", 
                "Sí, configurar", 
                "Cancelar"
            );
            if (!proceed) return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Setup Escena Disparo");
        int undoGroup = Undo.GetCurrentGroup();

        Debug.Log("🚀 Iniciando configuración de la escena de Disparo...");

        // 2. Buscar JoustManager existente en la escena
        JoustManager joustManager = FindFirstObjectByType<JoustManager>();
        Transform playerTransform = null;
        Animator horseAnimator = null;
        Camera mainCamera = null;
        JoustStatsPanelController statsPanel = null;

        if (joustManager != null)
        {
            playerTransform = joustManager.player;
            horseAnimator = joustManager.playerHorseAnimator;
            mainCamera = joustManager.mainCamera;
            statsPanel = joustManager.winManager != null ? joustManager.winManager.statsPanelController : null;
            
            Debug.Log("✓ Encontrado JoustManager. Guardadas referencias de Player, Cámara y Panel de Stats.");
        }

        // Si no se encuentran referencias base, intentar buscarlas por tag/nombre
        if (playerTransform == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (pObj != null) playerTransform = pObj.transform;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (statsPanel == null)
        {
            statsPanel = FindFirstObjectByType<JoustStatsPanelController>();
        }

        // 3. Crear el nuevo GameManager de Disparo
        GameObject managerObj = GameObject.Find("DisparoGameplayManager");
        if (managerObj == null)
        {
            managerObj = new GameObject("DisparoGameplayManager");
            Undo.RegisterCreatedObjectUndo(managerObj, "Crear DisparoGameplayManager");
        }

        var gameplayManager = managerObj.GetComponent<DisparoGameplayManager>();
        if (gameplayManager == null)
        {
            gameplayManager = managerObj.AddComponent<DisparoGameplayManager>();
            Undo.RegisterCompleteObjectUndo(managerObj, "Añadir DisparoGameplayManager");
        }

        var crossbowController = managerObj.GetComponent<CrossbowController>();
        if (crossbowController == null)
        {
            crossbowController = managerObj.AddComponent<CrossbowController>();
            Undo.RegisterCompleteObjectUndo(managerObj, "Añadir CrossbowController");
        }

        // Asignar referencias en el GameManager
        gameplayManager.player = playerTransform;
        gameplayManager.playerHorseAnimator = horseAnimator;
        gameplayManager.statsPanelController = statsPanel;
        gameplayManager.crossbowController = crossbowController;

        // Auto-detectar ProgressManager en la escena
        gameplayManager.progressManager = FindFirstObjectByType<ProgressManager>();

        // 4. LIMPIEZA PROFUNDA: Eliminar todo lo redundante que no sea del formato Disparo
        Debug.Log("🧹 Realizando limpieza profunda de elementos obsoletos...");

        // A. Destruir el oponente (Enemy) y sus referencias por completo
        if (joustManager != null && joustManager.enemy != null)
        {
            GameObject enemyObj = joustManager.enemy.gameObject;
            Debug.Log($"✓ Eliminando oponente de justa: {enemyObj.name}");
            Undo.DestroyObjectImmediate(enemyObj);
        }
        else
        {
            // Evitar buscar por tag "Enemy" ya que puede no estar definido en las configuraciones del proyecto
            GameObject enemyTagObj = GameObject.Find("Enemy") ?? GameObject.Find("EnemyRider") ?? GameObject.Find("Opponent");
            if (enemyTagObj != null)
            {
                Debug.Log($"✓ Eliminando oponente encontrado por búsqueda de nombre: {enemyTagObj.name}");
                Undo.DestroyObjectImmediate(enemyTagObj);
            }
        }

        // B. Destruir armas de justa del Player (Lanza y Escudo visuales en sus manos)
        if (playerTransform != null)
        {
            var allChildren = new List<Transform>();
            GetChildrenRecursive(playerTransform, allChildren);

            foreach (var child in allChildren)
            {
                if (child == null) continue;
                string childName = child.name.ToLower();
                
                // Si el nombre contiene palabras clave de justa o tiene scripts de justa asignados
                bool isJoustEquipment = childName.Contains("lance") || childName.Contains("shield") || 
                                       childName.Contains("lanza") || childName.Contains("escudo");
                                       
                bool hasJoustComponent = child.GetComponent<PhysicalLanceController>() != null || 
                                         child.GetComponent<PhysicalShieldController>() != null ||
                                         child.GetComponent<LanceTipCollision>() != null;

                if (isJoustEquipment || hasJoustComponent)
                {
                    Debug.Log($"✓ Eliminando equipamiento de justa del jugador: {child.gameObject.name}");
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        // C. Destruir puntos de cámara obsoletos y gestores cinemáticos
        if (joustManager != null)
        {
            if (joustManager.attackCameraPoint != null) Undo.DestroyObjectImmediate(joustManager.attackCameraPoint.gameObject);
            if (joustManager.defenseCameraPoint != null) Undo.DestroyObjectImmediate(joustManager.defenseCameraPoint.gameObject);
            if (joustManager.horseCameraPoint != null) Undo.DestroyObjectImmediate(joustManager.horseCameraPoint.gameObject);
            
            if (joustManager.cinematicManager != null) Undo.DestroyObjectImmediate(joustManager.cinematicManager.gameObject);
        }

        // Destruir por nombre otros elementos obsoletos si quedasen sueltos
        string[] obsoleteObjectNames = { "JoustCinematicManager", "JoustTutorialManager", "WinManager", "EnemyWaypoints", "PlayerPreJoustWaypoints" };
        foreach (var obsName in obsoleteObjectNames)
        {
            GameObject obsObj = GameObject.Find(obsName);
            if (obsObj != null)
            {
                Debug.Log($"✓ Eliminando objeto obsoleto: {obsName}");
                Undo.DestroyObjectImmediate(obsObj);
            }
        }

        // D. Destruir elementos UI obsoletos de la justa (timing slider, sliders de carga, marcadores de golpe, etc.)
        // Buscamos componentes específicos de Justa en la escena antes de borrarlos para saber qué UI tenían asignadas
        var horsePart = FindFirstObjectByType<HorsePart_Joust>();
        if (horsePart != null)
        {
            if (horsePart.sliderArea != null) Undo.DestroyObjectImmediate(horsePart.sliderArea.gameObject);
            if (horsePart.movingIndicatorPrefab != null && horsePart.movingIndicatorPrefab.gameObject.scene.name != null) 
                Undo.DestroyObjectImmediate(horsePart.movingIndicatorPrefab.gameObject);
        }

        var lanceController = FindFirstObjectByType<PhysicalLanceController>();
        if (lanceController != null)
        {
            if (lanceController.chargeSlider != null) Undo.DestroyObjectImmediate(lanceController.chargeSlider.gameObject);
            if (lanceController.hitMarker != null) Undo.DestroyObjectImmediate(lanceController.hitMarker.gameObject);
        }

        // Buscar UIs huérfanas comunes por nombre
        string[] obsoleteUiNames = { "SliderArea", "ChargeSlider", "HitMarker", "JoustHUD", "ShieldHUD", "TimingBar" };
        foreach (var uiName in obsoleteUiNames)
        {
            GameObject uiObj = GameObject.Find(uiName);
            if (uiObj != null)
            {
                Debug.Log($"✓ Eliminando UI obsoleta: {uiName}");
                Undo.DestroyObjectImmediate(uiObj);
            }
        }

        // 5. Configurar la cámara de Cinemachine en Primera Persona
        if (playerTransform != null)
        {
            // A. Buscar y destruir cualquier cámara virtual antigua para evitar interferencias
            CinemachineCamera[] oldVirtualCams = FindObjectsByType<CinemachineCamera>(FindObjectsSortMode.None);
            foreach (var cam in oldVirtualCams)
            {
                if (cam != null && cam.gameObject.name != "FP_CinemachineCamera")
                {
                    Debug.Log($"✓ Eliminando cámara virtual obsoleta: {cam.gameObject.name}");
                    Undo.DestroyObjectImmediate(cam.gameObject);
                }
            }

            // B. Crear o buscar la cámara virtual de Cinemachine para Primera Persona
            GameObject fpCamObj = GameObject.Find("FP_CinemachineCamera");
            if (fpCamObj == null)
            {
                fpCamObj = new GameObject("FP_CinemachineCamera");
                Undo.RegisterCreatedObjectUndo(fpCamObj, "Crear FP_CinemachineCamera");
            }

            CinemachineCamera fpVirtualCamera = fpCamObj.GetComponent<CinemachineCamera>();
            if (fpVirtualCamera == null)
            {
                fpVirtualCamera = fpCamObj.AddComponent<CinemachineCamera>();
                Undo.RegisterCompleteObjectUndo(fpCamObj, "Añadir CinemachineCamera");
            }

            // C. Acoplar la cámara virtual como hija del jugador (cabeza del jinete)
            fpCamObj.transform.SetParent(playerTransform);
            fpCamObj.transform.localPosition = new Vector3(0f, 1.8f, 0.4f);
            fpCamObj.transform.localRotation = Quaternion.identity;

            // D. Configuración de Cinemachine
            fpVirtualCamera.Follow = null;  // No necesita Follow ya que es hija directa
            fpVirtualCamera.LookAt = null;  // Tampoco LookAt porque la rotación la maneja el CrossbowController
            fpVirtualCamera.Priority = 99;  // Máxima prioridad para que tome el control inmediato

            // E. Crear el punto de acople de la ballesta como hijo de la cámara virtual (para que rote con ella)
            Transform attachPoint = fpCamObj.transform.Find("CrossbowAttachPoint");
            if (attachPoint == null)
            {
                GameObject attachObj = new GameObject("CrossbowAttachPoint");
                attachObj.transform.SetParent(fpCamObj.transform, false);
                attachPoint = attachObj.transform;
                Undo.RegisterCreatedObjectUndo(attachObj, "Crear CrossbowAttachPoint");
            }

            // Vincular al controlador
            crossbowController.fpVirtualCamera = fpVirtualCamera;
            crossbowController.firstPersonCamera = mainCamera;
            crossbowController.crossbowAttachPoint = attachPoint;

            Debug.Log("✓ Cinemachine Virtual Camera (FP_CinemachineCamera) configurada y vinculada con éxito.");
        }

        // 6. Cargar pre-existencias del HUD para conectarlas
        // Buscamos componentes UI existentes en el Canvas de la escena
        TextMeshProUGUI[] tmps = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        foreach (var t in tmps)
        {
            string nameLower = t.gameObject.name.ToLower();
            if (nameLower.Contains("score") || nameLower.Contains("puntos"))
            {
                gameplayManager.scoreText = t;
            }
            else if (nameLower.Contains("timer") || nameLower.Contains("cuenta") || nameLower.Contains("counter"))
            {
                gameplayManager.countdownText = t;
            }
            else if (nameLower.Contains("controls") || nameLower.Contains("control"))
            {
                gameplayManager.controlsText = t;
            }
        }

        // Crear dinámicamente un texto para la munición si no se asignó automáticamente
        if (gameplayManager.ammoText == null && gameplayManager.scoreText != null)
        {
            GameObject ammoUIObj = Instantiate(gameplayManager.scoreText.gameObject, gameplayManager.scoreText.transform.parent);
            ammoUIObj.name = "AmmoText";
            RectTransform rt = ammoUIObj.GetComponent<RectTransform>();
            rt.anchoredPosition += new Vector2(0f, -40f); // Posicionar debajo del texto de puntuación
            
            gameplayManager.ammoText = ammoUIObj.GetComponent<TextMeshProUGUI>();
            gameplayManager.ammoText.text = "Virotes: 20";
            
            Undo.RegisterCreatedObjectUndo(ammoUIObj, "Crear Texto Munición UI");
        }

        // 7. Remover scripts obsoletos de justa en la escena
        if (joustManager != null)
        {
            Undo.DestroyObjectImmediate(joustManager);
            Debug.Log("✓ Eliminados scripts redundantes de la Justa original.");
        }

        // 8. Crear objetivos de prueba (dianas) a lo largo de la pista para que la escena sea jugable
        CreateDefaultTargets();


        // 9. Completar
        EditorUtility.SetDirty(managerObj);
        if (playerTransform != null) EditorUtility.SetDirty(playerTransform.gameObject);
        
        Undo.CollapseUndoOperations(undoGroup);
        
        EditorUtility.DisplayDialog(
            "¡Escena Configurada!", 
            "La escena 'Disparo' se ha configurado de forma totalmente automática.\n\n" +
            "Se han creado el GameManager, la cámara FPS y 6 dianas interactivas a lo largo de la pista.\n" +
            "¡Guarda la escena y pulsa PLAY para probar tu ballesta!", 
            "Genial"
        );
    }

    private static void CreateDefaultTargets()
    {
        // Buscar si ya existen objetivos en la escena para no duplicar
        ShootingTarget[] existingTargets = FindObjectsByType<ShootingTarget>(FindObjectsSortMode.None);
        if (existingTargets.Length > 0)
        {
            Debug.Log($"⚠ Ya existen {existingTargets.Length} dianas en la escena. Omitiendo creación de dianas de prueba.");
            return;
        }

        GameObject targetsParent = GameObject.Find("ShootingTargets");
        if (targetsParent == null)
        {
            targetsParent = new GameObject("ShootingTargets");
            Undo.RegisterCreatedObjectUndo(targetsParent, "Crear Contenedor de Dianas");
        }

        // Colocar 6 dianas a lo largo de la pista (Z de 30 a 180) a ambos lados (X = -4 y X = 4)
        float[] distancesZ = { 30f, 60f, 90f, 120f, 150f, 180f };
        float[] positionsX = { -4f, 4f, -5f, 5f, -4f, 4f };
        ShootingTarget.TargetType[] types = {
            ShootingTarget.TargetType.Standard,
            ShootingTarget.TargetType.Standard,
            ShootingTarget.TargetType.Golden, // Especial dorada en Z=90
            ShootingTarget.TargetType.Standard,
            ShootingTarget.TargetType.Moving, // Móvil en Z=150
            ShootingTarget.TargetType.Standard
        };

        for (int i = 0; i < distancesZ.Length; i++)
        {
            // Crear una diana básica usando primitivas 3D de Unity
            GameObject targetObj = new GameObject($"Target_Test_{i + 1}");
            targetObj.transform.SetParent(targetsParent.transform);
            targetObj.transform.position = new Vector3(positionsX[i], 1.2f, distancesZ[i]);
            
            Undo.RegisterCreatedObjectUndo(targetObj, $"Crear Diana {i + 1}");

            // Crear el cuerpo visual (un cilindro como poste y un cubo aplanado como diana)
            GameObject visualPost = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visualPost.name = "Post";
            visualPost.transform.SetParent(targetObj.transform, false);
            visualPost.transform.localPosition = new Vector3(0f, -0.6f, 0f);
            visualPost.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
            DestroyImmediate(visualPost.GetComponent<Collider>()); // No necesita colisionador

            GameObject visualBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualBoard.name = "Board";
            visualBoard.transform.SetParent(targetObj.transform, false);
            visualBoard.transform.localPosition = Vector3.zero;
            visualBoard.transform.localScale = new Vector3(1.2f, 1.2f, 0.15f);
            
            // Añadir el script de diana interactiva
            ShootingTarget st = targetObj.AddComponent<ShootingTarget>();
            st.targetType = types[i];
            st.animatedVisual = visualBoard.transform;
            
            // Configurar puntos según tipo
            if (st.targetType == ShootingTarget.TargetType.Golden)
            {
                st.scorePoints = 35;
                visualBoard.GetComponent<Renderer>().sharedMaterial.color = Color.yellow; // Pintarla dorada
            }
            else if (st.targetType == ShootingTarget.TargetType.Moving)
            {
                st.scorePoints = 25;
                visualBoard.GetComponent<Renderer>().sharedMaterial.color = Color.cyan;
            }
            else
            {
                st.scorePoints = 15;
                visualBoard.GetComponent<Renderer>().sharedMaterial.color = Color.red; // Pintarla roja
            }

            // Cambiar tag del colisionador de la diana a Board para que el virote sepa dónde impacta
            visualBoard.tag = "Untagged";
            
            // El virote se clava en el colisionador del Board
            // Aseguramos que la diana esté inclinada (oculta) al inicio
            visualBoard.transform.localRotation = Quaternion.Euler(st.hiddenLocalRotation);
        }

        Debug.Log("✓ Dianas de prueba creadas y distribuidas a lo largo de la pista.");
    }

    private static void GetChildrenRecursive(Transform parent, List<Transform> list)
    {
        foreach (Transform child in parent)
        {
            if (child == null) continue;
            list.Add(child);
            GetChildrenRecursive(child, list);
        }
    }
}

