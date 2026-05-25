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

        Debug.Log("🚀 Iniciando configuración inteligente de la escena de Disparo...");

        // 2. Buscar JoustManager existente en la escena
        JoustManager joustManager = FindFirstObjectByType<JoustManager>();
        Transform playerTransform = null;
        Animator horseAnimator = null;
        Camera mainCamera = null;

        if (joustManager != null)
        {
            playerTransform = joustManager.player;
            horseAnimator = joustManager.playerHorseAnimator;
            mainCamera = joustManager.mainCamera;
            Debug.Log("✓ Encontrado JoustManager. Guardadas referencias de Player y Cámara.");
        }

        // Si no se encuentran referencias base, intentar buscarlas
        if (playerTransform == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Player");
            if (pObj != null) playerTransform = pObj.transform;
        }

        if (playerTransform != null)
        {
            horseAnimator = playerTransform.GetComponentInChildren<Animator>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        // Asegurar iluminación (Directional Light soleada con sombras suaves si no hay ninguna)
        Light existingDirLight = null;
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            if (l.type == LightType.Directional)
            {
                existingDirLight = l;
                break;
            }
        }

        if (existingDirLight == null)
        {
            GameObject lightObj = new GameObject("Directional Light", typeof(Light));
            Light light = lightObj.GetComponent<Light>();
            light.type = LightType.Directional;
            light.shadows = LightShadows.Soft;
            light.intensity = 1.3f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            Undo.RegisterCreatedObjectUndo(lightObj, "Crear Iluminación");
            Debug.Log("✓ Iluminación solar de soporte creada.");
        }
        else
        {
            // Ajustar sombras suaves en la luz existente para mejorar la estética
            Undo.RegisterCompleteObjectUndo(existingDirLight.gameObject, "Ajustar Iluminación");
            existingDirLight.shadows = LightShadows.Soft;
            if (existingDirLight.intensity < 1.0f) existingDirLight.intensity = 1.2f;
            Debug.Log("✓ Iluminación solar existente ajustada para máxima calidad visual.");
        }

        // 3. Crear o buscar el nuevo GameManager de Disparo
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

        // Asignar referencias básicas
        gameplayManager.player = playerTransform;
        gameplayManager.playerHorseAnimator = horseAnimator;
        gameplayManager.crossbowController = crossbowController;
        gameplayManager.progressManager = FindFirstObjectByType<ProgressManager>();

        // 4. LIMPIEZA PROFUNDA: Eliminar elementos redundantes de justa
        Debug.Log("🧹 Realizando limpieza de elementos redundantes de justa...");

        // A. Destruir el oponente (Enemy) y sus referencias
        if (joustManager != null && joustManager.enemy != null)
        {
            GameObject enemyObj = joustManager.enemy.gameObject;
            Undo.DestroyObjectImmediate(enemyObj);
        }
        else
        {
            GameObject enemyTagObj = GameObject.Find("Enemy") ?? GameObject.Find("EnemyRider") ?? GameObject.Find("Opponent");
            if (enemyTagObj != null)
            {
                Undo.DestroyObjectImmediate(enemyTagObj);
            }
        }

        // B. Destruir armas de justa del Player
        if (playerTransform != null)
        {
            var allChildren = new List<Transform>();
            GetChildrenRecursive(playerTransform, allChildren);

            foreach (var child in allChildren)
            {
                if (child == null) continue;
                string childName = child.name.ToLower();
                bool isJoustEquipment = childName.Contains("lance") || childName.Contains("shield") || 
                                       childName.Contains("lanza") || childName.Contains("escudo");
                                       
                bool hasJoustComponent = child.GetComponent<PhysicalLanceController>() != null || 
                                         child.GetComponent<PhysicalShieldController>() != null ||
                                         child.GetComponent<LanceTipCollision>() != null;

                if (isJoustEquipment || hasJoustComponent)
                {
                    Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
        }

        // C. Destruir puntos de cámara obsoletos
        if (joustManager != null)
        {
            if (joustManager.attackCameraPoint != null) Undo.DestroyObjectImmediate(joustManager.attackCameraPoint.gameObject);
            if (joustManager.defenseCameraPoint != null) Undo.DestroyObjectImmediate(joustManager.defenseCameraPoint.gameObject);
            if (joustManager.horseCameraPoint != null) Undo.DestroyObjectImmediate(joustManager.horseCameraPoint.gameObject);
            if (joustManager.cinematicManager != null) Undo.DestroyObjectImmediate(joustManager.cinematicManager.gameObject);
        }

        string[] obsoleteObjectNames = { "JoustCinematicManager", "JoustTutorialManager", "WinManager", "EnemyWaypoints", "PlayerPreJoustWaypoints" };
        foreach (var obsName in obsoleteObjectNames)
        {
            GameObject obsObj = GameObject.Find(obsName);
            if (obsObj != null)
            {
                Undo.DestroyObjectImmediate(obsObj);
            }
        }

        // D. Destruir UIs obsoletas de justa
        string[] obsoleteUiNames = { "SliderArea", "ChargeSlider", "HitMarker", "JoustHUD", "ShieldHUD", "TimingBar", "JoustCanvas" };
        foreach (var uiName in obsoleteUiNames)
        {
            GameObject uiObj = GameObject.Find(uiName);
            if (uiObj != null)
            {
                Undo.DestroyObjectImmediate(uiObj);
            }
        }

        // 5. Configurar la cámara de Cinemachine en Primera Persona
        if (playerTransform != null)
        {
            if (mainCamera != null)
            {
                Undo.RegisterCompleteObjectUndo(mainCamera.gameObject, "Configurar Main Camera");
                var brain = mainCamera.GetComponent<CinemachineBrain>();
                if (brain == null)
                {
                    brain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                }
                else
                {
                    brain.enabled = true;
                }

                MonoBehaviour[] cameraScripts = mainCamera.GetComponents<MonoBehaviour>();
                foreach (var script in cameraScripts)
                {
                    if (script != null && script.GetType().Name != "CinemachineBrain" && script.GetType().Name != "Camera" && script.GetType().Name != "AudioListener")
                    {
                        Undo.DestroyObjectImmediate(script);
                    }
                }
            }

            CinemachineCamera[] oldVirtualCams = Resources.FindObjectsOfTypeAll<CinemachineCamera>();
            foreach (var cam in oldVirtualCams)
            {
                if (cam != null && cam.gameObject.scene.name != null && cam.gameObject.name != "FP_CinemachineCamera")
                {
                    Undo.DestroyObjectImmediate(cam.gameObject);
                }
            }

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

            fpCamObj.transform.SetParent(playerTransform);
            fpCamObj.transform.localPosition = new Vector3(0f, 1.8f, 0.4f);
            fpCamObj.transform.localRotation = Quaternion.identity;

            fpVirtualCamera.Follow = null;
            fpVirtualCamera.LookAt = null;
            fpVirtualCamera.Priority = 999;

            Transform attachPoint = fpCamObj.transform.Find("CrossbowAttachPoint");
            if (attachPoint == null)
            {
                GameObject attachObj = new GameObject("CrossbowAttachPoint");
                attachObj.transform.SetParent(fpCamObj.transform, false);
                attachPoint = attachObj.transform;
                Undo.RegisterCreatedObjectUndo(attachObj, "Crear CrossbowAttachPoint");
            }
            
            attachPoint.localPosition = new Vector3(-0.2406f, 0.2399f, -0.4831f);
            attachPoint.localRotation = Quaternion.Euler(17.502f, -110.179f, 0f);
            attachPoint.localScale = new Vector3(0.3f, 0.3f, 0.3f);

            crossbowController.fpVirtualCamera = fpVirtualCamera;
            crossbowController.firstPersonCamera = mainCamera;
            crossbowController.playerRoot = playerTransform;
            crossbowController.crossbowAttachPoint = attachPoint;

            crossbowController.crossbowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Medieval Weapons (Melee + Ranged)/Prefabs/Crossbow.prefab");
            crossbowController.boltPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Low Poly Medieval Weapons (Melee + Ranged)/Prefabs/Armor_Piercing_Arrow.prefab");
            
            crossbowController.shootSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Shoot.wav") ?? AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Bow.wav");
            crossbowController.reloadSound = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/Reload.wav");
        }

        // 6. Recrear el HUD Canvas y el Panel de Estadísticas Final (Stats Panel) de forma procedural
        GameObject hudCanvas = CreateHUDCanvas(gameplayManager, crossbowController);
        EnsureEventSystemExists();
        CreateStatsPanelUI(hudCanvas, gameplayManager);

        // 7. Remover scripts obsoletos de justa en el manager
        if (joustManager != null)
        {
            Undo.DestroyObjectImmediate(joustManager);
        }

        // 8. Crear objetivos de prueba (dianas) a lo largo de la pista si no existen
        CreateDefaultTargets();

        // 9. Completar
        EditorUtility.SetDirty(managerObj);
        if (playerTransform != null) EditorUtility.SetDirty(playerTransform.gameObject);
        
        Undo.CollapseUndoOperations(undoGroup);
        
        EditorUtility.DisplayDialog(
            "¡Escena Configurada!", 
            "La escena 'Disparo' se ha configurado con total éxito de forma no destructiva.\n\n" +
            "Se mantuvieron tus elementos del mapa y decoración, y se configuró la iluminación soleada, la cámara Cinemachine FPS, el GameManager, la ballesta, la interfaz HUD, el panel final y 6 dianas interactivas.\n" +
            "¡Guarda la escena y pulsa PLAY para disfrutar del juego!", 
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

            // Crear contenedor de colisión y rotación con escala limpia (1, 1, 1) para evitar deformación de las flechas
            GameObject visualBoard = new GameObject("Board");
            visualBoard.transform.SetParent(targetObj.transform, false);
            visualBoard.transform.localPosition = Vector3.zero;
            visualBoard.transform.localScale = Vector3.one; // Escala limpia (1, 1, 1)!
            
            // Añadir el colisionador con la escala exacta directamente en el BoxCollider
            BoxCollider boxCol = visualBoard.AddComponent<BoxCollider>();
            boxCol.size = new Vector3(1.2f, 1.2f, 0.15f);
            boxCol.isTrigger = true;

            // Crear el renderizador visual como hijo con escala
            GameObject visualMesh = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visualMesh.name = "BoardMesh";
            visualMesh.transform.SetParent(visualBoard.transform, false);
            visualMesh.transform.localPosition = Vector3.zero;
            visualMesh.transform.localScale = new Vector3(1.2f, 1.2f, 0.15f);
            DestroyImmediate(visualMesh.GetComponent<BoxCollider>()); // Quitar el colisionador del hijo
            
            // Añadir el script de diana interactiva
            ShootingTarget st = targetObj.AddComponent<ShootingTarget>();
            st.targetType = types[i];
            st.animatedVisual = visualBoard.transform;
            
            // Configurar puntos según tipo
            if (st.targetType == ShootingTarget.TargetType.Golden)
            {
                st.scorePoints = 35;
                visualMesh.GetComponent<Renderer>().sharedMaterial.color = Color.yellow; // Pintarla dorada
            }
            else if (st.targetType == ShootingTarget.TargetType.Moving)
            {
                st.scorePoints = 25;
                visualMesh.GetComponent<Renderer>().sharedMaterial.color = Color.cyan;
            }
            else
            {
                st.scorePoints = 15;
                visualMesh.GetComponent<Renderer>().sharedMaterial.color = Color.red; // Pintarla roja
            }

            // Cambiar tag a Untagged
            visualBoard.tag = "Untagged";
            
            // Aseguramos que la diana esté inclinada (oculta) al inicio
            visualBoard.transform.localRotation = Quaternion.Euler(st.hiddenLocalRotation);
        }

        Debug.Log("✓ Dianas de prueba optimizadas y distribuidas a lo largo de la pista.");
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

    private static GameObject CreateHUDCanvas(DisparoGameplayManager gameplayManager, CrossbowController crossbowController)
    {
        // Crear Canvas Principal
        GameObject canvasObj = new GameObject("HUDCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        Undo.RegisterCreatedObjectUndo(canvasObj, "Crear Canvas");

        // Crear una retícula/mira en el centro de la pantalla
        GameObject crosshairObj = new GameObject("Crosshair", typeof(Image));
        crosshairObj.transform.SetParent(canvasObj.transform, false);
        Image crosshairImage = crosshairObj.GetComponent<Image>();
        crosshairImage.color = Color.white;
        RectTransform crosshairRt = crosshairObj.GetComponent<RectTransform>();
        crosshairRt.sizeDelta = new Vector2(25f, 25f);
        crosshairRt.anchoredPosition = Vector2.zero;
        crossbowController.crosshairImage = crosshairImage;
        crossbowController.crosshairNormalColor = Color.white;
        crossbowController.crosshairHitColor = Color.green;
        Undo.RegisterCreatedObjectUndo(crosshairObj, "Crear Crosshair UI");

        // Panel superior para albergar los textos de HUD
        GameObject textPanel = new GameObject("TextPanel", typeof(RectTransform));
        textPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform panelRt = textPanel.GetComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0f, 1f);
        panelRt.anchorMax = new Vector2(0f, 1f);
        panelRt.pivot = new Vector2(0f, 1f);
        panelRt.anchoredPosition = new Vector2(50f, -50f);
        panelRt.sizeDelta = new Vector2(600f, 300f);

        // Texto de puntuación (ScoreText)
        GameObject scoreObj = new GameObject("ScoreText", typeof(TextMeshProUGUI));
        scoreObj.transform.SetParent(textPanel.transform, false);
        TextMeshProUGUI scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
        scoreText.fontSize = 42;
        scoreText.color = Color.white;
        scoreText.text = "Puntos: 0 / Objetivo: 80";
        scoreText.alignment = TextAlignmentOptions.Left;
        gameplayManager.scoreText = scoreText;
        RectTransform scoreRt = scoreObj.GetComponent<RectTransform>();
        scoreRt.anchorMin = new Vector2(0f, 1f);
        scoreRt.anchorMax = new Vector2(1f, 1f);
        scoreRt.pivot = new Vector2(0f, 1f);
        scoreRt.anchoredPosition = Vector2.zero;
        scoreRt.sizeDelta = new Vector2(0f, 60f);

        // Texto de Munición (AmmoText)
        GameObject ammoObj = new GameObject("AmmoText", typeof(TextMeshProUGUI));
        ammoObj.transform.SetParent(textPanel.transform, false);
        TextMeshProUGUI ammoText = ammoObj.GetComponent<TextMeshProUGUI>();
        ammoText.fontSize = 36;
        ammoText.color = new Color(0.9f, 0.9f, 0.9f);
        ammoText.text = "Virotes: 20";
        ammoText.alignment = TextAlignmentOptions.Left;
        gameplayManager.ammoText = ammoText;
        RectTransform ammoRt = ammoObj.GetComponent<RectTransform>();
        ammoRt.anchorMin = new Vector2(0f, 1f);
        ammoRt.anchorMax = new Vector2(1f, 1f);
        ammoRt.pivot = new Vector2(0f, 1f);
        ammoRt.anchoredPosition = new Vector2(0f, -65f);
        ammoRt.sizeDelta = new Vector2(0f, 50f);

        // Texto de cuenta atrás en el centro (CountdownText)
        GameObject countObj = new GameObject("CountdownText", typeof(TextMeshProUGUI));
        countObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI countdownText = countObj.GetComponent<TextMeshProUGUI>();
        countdownText.fontSize = 120;
        countdownText.color = Color.yellow;
        countdownText.text = "3";
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.gameObject.SetActive(false);
        gameplayManager.countdownText = countdownText;
        RectTransform countRt = countObj.GetComponent<RectTransform>();
        countRt.sizeDelta = new Vector2(400f, 200f);
        countRt.anchoredPosition = Vector2.zero;

        // Texto de Controles HUD (ControlsText)
        GameObject controlObj = new GameObject("ControlsText", typeof(TextMeshProUGUI));
        controlObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI controlsText = controlObj.GetComponent<TextMeshProUGUI>();
        controlsText.fontSize = 24;
        controlsText.color = new Color(0.8f, 0.8f, 0.8f, 0.9f);
        controlsText.text = "APUNTAR: Ratón / Joystick Der\nDISPARAR: Click Izq / R2\nSPRINT: Shift Izq / Botón A";
        controlsText.alignment = TextAlignmentOptions.Center;
        gameplayManager.controlsText = controlsText;
        RectTransform controlRt = controlObj.GetComponent<RectTransform>();
        controlRt.anchorMin = new Vector2(0.5f, 0f);
        controlRt.anchorMax = new Vector2(0.5f, 0f);
        controlRt.pivot = new Vector2(0.5f, 0f);
        controlRt.anchoredPosition = new Vector2(0f, 120f);
        controlRt.sizeDelta = new Vector2(800f, 100f);

        // Barra de Progreso (ProgressBar - Slider)
        GameObject sliderObj = new GameObject("ProgressBar", typeof(Slider));
        sliderObj.transform.SetParent(canvasObj.transform, false);
        Slider slider = sliderObj.GetComponent<Slider>();
        gameplayManager.progressBar = slider;
        
        RectTransform sliderRt = sliderObj.GetComponent<RectTransform>();
        sliderRt.anchorMin = new Vector2(0.5f, 0f);
        sliderRt.anchorMax = new Vector2(0.5f, 0f);
        sliderRt.pivot = new Vector2(0.5f, 0f);
        sliderRt.anchoredPosition = new Vector2(0f, 50f);
        sliderRt.sizeDelta = new Vector2(600f, 30f);

        // Estilo visual rápido para el slider de progreso (Fondo y Relleno)
        GameObject background = new GameObject("Background", typeof(Image));
        background.transform.SetParent(sliderObj.transform, false);
        background.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.2f, 0.7f);
        RectTransform bgRt = background.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRt = fillArea.GetComponent<RectTransform>();
        faRt.anchorMin = Vector2.zero;
        faRt.anchorMax = Vector2.one;
        faRt.sizeDelta = Vector2.zero;

        GameObject fill = new GameObject("Fill", typeof(Image));
        fill.transform.SetParent(fillArea.transform, false);
        fill.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.4f, 0.9f);
        RectTransform fillRt = fill.GetComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.sizeDelta = Vector2.zero;

        slider.targetGraphic = fill.GetComponent<Image>();
        slider.fillRect = fillRt;

        return canvasObj;
    }

    private static void CreateStatsPanelUI(GameObject canvasObj, DisparoGameplayManager gameplayManager)
    {
        TMP_FontAsset fontAsset = GetDefaultFontAsset();

        // 1. Crear el borde exterior del Panel de Estadísticas (Elegante marco procedural plateado)
        GameObject panelBorderObj = new GameObject("StatsPanelBorder", typeof(RectTransform), typeof(Image));
        panelBorderObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform borderRect = panelBorderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(0f, 10f);
        borderRect.sizeDelta = new Vector2(1204f, 754f);

        Image borderImg = panelBorderObj.GetComponent<Image>();
        borderImg.color = new Color(0.25f, 0.25f, 0.28f, 1f); // Marco metálico

        // 2. Panel Interno del Panel de Estadísticas con el controlador
        GameObject panelObj = new GameObject("JoustStatsPanel", typeof(RectTransform), typeof(Image), typeof(JoustStatsPanelController));
        panelObj.transform.SetParent(panelBorderObj.transform, false);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.06f, 0.06f, 0.08f, 0.98f); // Fondo obsidian

        JoustStatsPanelController controller = panelObj.GetComponent<JoustStatsPanelController>();
        Undo.RegisterCreatedObjectUndo(panelBorderObj, "Create Stats Panel Structure");

        // 3. Encabezado del Panel
        GameObject headerObj = new GameObject("HeaderTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        headerObj.transform.SetParent(panelObj.transform, false);
        
        TextMeshProUGUI headerTxt = headerObj.GetComponent<TextMeshProUGUI>();
        headerTxt.text = "ESTADÍSTICAS DEL COMBATE";
        headerTxt.fontSize = 24;
        headerTxt.fontStyle = FontStyles.Bold;
        headerTxt.color = new Color(0.7f, 0.7f, 0.75f, 1f);
        headerTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) headerTxt.font = fontAsset;

        RectTransform headerRect = headerObj.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0.5f, 1f);
        headerRect.anchorMax = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0f, -50f);
        headerRect.sizeDelta = new Vector2(800f, 40f);

        // Subtítulo de Resultado Dinámico
        GameObject resultObj = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        resultObj.transform.SetParent(panelObj.transform, false);
        
        TextMeshProUGUI resultTxt = resultObj.GetComponent<TextMeshProUGUI>();
        resultTxt.text = "¡VICTORIA EN LA JUSTA!";
        resultTxt.fontSize = 42;
        resultTxt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        resultTxt.color = new Color(0.28f, 0.88f, 0.52f, 1f);
        resultTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) resultTxt.font = fontAsset;

        RectTransform resultRect = resultObj.GetComponent<RectTransform>();
        resultRect.anchorMin = new Vector2(0.5f, 1f);
        resultRect.anchorMax = new Vector2(0.5f, 1f);
        resultRect.anchoredPosition = new Vector2(0f, -100f);
        resultRect.sizeDelta = new Vector2(1000f, 60f);

        controller.resultTitleText = resultTxt;

        Undo.RegisterCreatedObjectUndo(headerObj, "Create Header Title Text");
        Undo.RegisterCreatedObjectUndo(resultObj, "Create Result Text");

        // 4. Contenedor de las 3 Columnas
        GameObject colsContainer = new GameObject("ColumnsContainer", typeof(RectTransform));
        colsContainer.transform.SetParent(panelObj.transform, false);
        
        RectTransform colsRect = colsContainer.GetComponent<RectTransform>();
        colsRect.anchorMin = new Vector2(0.5f, 0.5f);
        colsRect.anchorMax = new Vector2(0.5f, 0.5f);
        colsRect.anchoredPosition = new Vector2(0f, -30f);
        colsRect.sizeDelta = new Vector2(1100f, 420f);
        Undo.RegisterCreatedObjectUndo(colsContainer, "Create Columns Container");

        // --- COLUMNA 1 (IZQUIERDA): TU EQUIPAMIENTO ---
        GameObject col1 = CreateColumn(colsContainer.transform, -360f, "TU EQUIPAMIENTO", fontAsset);
        controller.horseEquippedText = CreateRowText(col1.transform, "Caballo:", "Guerra", 100f, fontAsset);
        controller.lanceEquippedText = CreateRowText(col1.transform, "Lanza:", "Pine Lance", 30f, fontAsset);
        controller.shieldEquippedText = CreateRowText(col1.transform, "Escudo:", "Training Shield", -40f, fontAsset);
        controller.armorEquippedText = CreateRowText(col1.transform, "Armadura:", "Training Armor", -110f, fontAsset);

        // --- COLUMNA 2 (MEDIO): TUS ESTADÍSTICAS ---
        GameObject col2 = CreateColumn(colsContainer.transform, 0f, "ESTADÍSTICAS FINALES", fontAsset);
        controller.statBFText = CreateRowText(col2.transform, "Fuerza de Impacto (BF):", "4", 110f, fontAsset, true);
        controller.statBLText = CreateRowText(col2.transform, "Precisión de Lanza (BL):", "3", 60f, fontAsset, true);
        controller.statMText = CreateRowText(col2.transform, "Maniobrabilidad (M):", "2", 10f, fontAsset, true);
        controller.statBBText = CreateRowText(col2.transform, "Defensa Escudo (BB):", "2", -40f, fontAsset, true);
        controller.statMVText = CreateRowText(col2.transform, "Velocidad Caballo (MV):", "12", -90f, fontAsset, true);

        // --- COLUMNA 3 (DERECHA): PUNTOS Y PREMIOS ---
        GameObject col3 = CreateColumn(colsContainer.transform, 360f, "RESULTADOS", fontAsset);
        controller.horseScoreText = CreateRowText(col3.transform, "Fase Caballo:", "+15 Ptos", 120f, fontAsset, true, true);
        controller.attackScoreText = CreateRowText(col3.transform, "Fase Ataque:", "+45 Ptos", 80f, fontAsset, true, true);
        controller.defenseScoreText = CreateRowText(col3.transform, "Fase Defensa:", "-4 Ptos", 40f, fontAsset, true, true);
        
        // Línea divisoria en los puntos
        GameObject divPoints = new GameObject("DivPoints", typeof(RectTransform), typeof(Image));
        divPoints.transform.SetParent(col3.transform, false);
        RectTransform divPointsRect = divPoints.GetComponent<RectTransform>();
        divPointsRect.anchoredPosition = new Vector2(0f, 15f);
        divPointsRect.sizeDelta = new Vector2(280f, 2f);
        divPoints.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.3f, 0.8f);

        controller.totalScoreText = CreateRowText(col3.transform, "PUNTUACIÓN TOTAL:", "56 Ptos", -10f, fontAsset, true, true, true);

        // Línea divisoria de sección
        GameObject divSection = new GameObject("DivSection", typeof(RectTransform), typeof(Image));
        divSection.transform.SetParent(col3.transform, false);
        RectTransform divSecRect = divSection.GetComponent<RectTransform>();
        divSecRect.anchoredPosition = new Vector2(0f, -40f);
        divSecRect.sizeDelta = new Vector2(290f, 2f);
        divSection.GetComponent<Image>().color = new Color(0.35f, 0.35f, 0.4f, 0.4f);

        controller.rewardsGoldText = CreateRowText(col3.transform, "Oro Ganado:", "+150 Monedas", -80f, fontAsset, true, false);
        controller.rewardsItemText = CreateRowText(col3.transform, "Objeto Obtenido:", "Ninguno", -135f, fontAsset, false, false, false, false, true);

        // 5. Botón de Finalizar
        GameObject btnBorderObj = new GameObject("FinishBtnBorder", typeof(RectTransform), typeof(Image));
        btnBorderObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform btnBorderRect = btnBorderObj.GetComponent<RectTransform>();
        btnBorderRect.anchorMin = new Vector2(0.5f, 0f);
        btnBorderRect.anchorMax = new Vector2(0.5f, 0f);
        btnBorderRect.anchoredPosition = new Vector2(0f, 60f);
        btnBorderRect.sizeDelta = new Vector2(322f, 56f);

        Image btnBorderImg = btnBorderObj.GetComponent<Image>();
        btnBorderImg.color = new Color(1.0f, 0.72f, 0.18f, 0.8f); // Borde dorado

        GameObject finishBtnObj = new GameObject("FinishTournamentButton", typeof(RectTransform), typeof(Image), typeof(Button));
        finishBtnObj.transform.SetParent(btnBorderObj.transform, false);
        
        RectTransform finishBtnRect = finishBtnObj.GetComponent<RectTransform>();
        finishBtnRect.anchorMin = Vector2.zero;
        finishBtnRect.anchorMax = Vector2.one;
        finishBtnRect.sizeDelta = Vector2.zero;

        Image finishBtnImg = finishBtnObj.GetComponent<Image>();
        finishBtnImg.color = new Color(0.09f, 0.09f, 0.12f, 1f);

        Button finishButtonComp = finishBtnObj.GetComponent<Button>();

        GameObject btnTextObj = new GameObject("FinishBtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(finishBtnObj.transform, false);
        
        TextMeshProUGUI btnTxt = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnTxt.text = "REGRESAR AL MAPA";
        btnTxt.fontSize = 16;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f);
        btnTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) btnTxt.font = fontAsset;

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        controller.finishButton = finishButtonComp;
        Undo.RegisterCreatedObjectUndo(btnBorderObj, "Create Finish Button Structure");

        // Ocultar panel por defecto
        panelBorderObj.SetActive(false);

        // Vincular al GameManager
        gameplayManager.statsPanelController = controller;
        controller.panelObject = panelBorderObj;
        controller.nextSceneName = "World";
    }

    private static GameObject CreateColumn(Transform parent, float posX, string colTitle, TMP_FontAsset fontAsset)
    {
        GameObject colBorder = new GameObject($"ColBorder_{colTitle}", typeof(RectTransform), typeof(Image));
        colBorder.transform.SetParent(parent, false);
        
        RectTransform borderRect = colBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(posX, 0f);
        borderRect.sizeDelta = new Vector2(332f, 382f);
        colBorder.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f, 0.5f);

        GameObject colObj = new GameObject($"Column_{colTitle}", typeof(RectTransform), typeof(Image));
        colObj.transform.SetParent(colBorder.transform, false);
        
        RectTransform colRect = colObj.GetComponent<RectTransform>();
        colRect.anchorMin = Vector2.zero;
        colRect.anchorMax = Vector2.one;
        colRect.anchoredPosition = Vector2.zero;
        colRect.sizeDelta = new Vector2(-4f, -4f);

        Image colImg = colObj.GetComponent<Image>();
        colImg.color = new Color(0.08f, 0.08f, 0.1f, 0.85f);

        GameObject titleObj = new GameObject("ColTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(colObj.transform, false);
        
        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = colTitle;
        titleTxt.fontSize = 16;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f);
        titleTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) titleTxt.font = fontAsset;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -25f);
        titleRect.sizeDelta = new Vector2(300f, 30f);

        return colObj;
    }

    private static TMP_Text CreateRowText(Transform parent, string label, string defaultValue, float posY, TMP_FontAsset fontAsset, bool rightAlignedValue = false, bool isScoreStyle = false, bool highlightTotal = false, bool multilineValue = false, bool isRewardItem = false)
    {
        GameObject labelObj = new GameObject($"RowLabel_{label}", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelObj.transform.SetParent(parent, false);

        TextMeshProUGUI labelTxt = labelObj.GetComponent<TextMeshProUGUI>();
        labelTxt.text = label;
        labelTxt.fontSize = highlightTotal ? 15 : 13;
        labelTxt.fontStyle = (highlightTotal || isScoreStyle) ? FontStyles.Bold : FontStyles.Normal;
        labelTxt.color = highlightTotal ? new Color(1.0f, 0.72f, 0.18f, 1f) : new Color(0.75f, 0.75f, 0.8f, 1f);
        labelTxt.alignment = rightAlignedValue ? TextAlignmentOptions.Left : TextAlignmentOptions.Center;
        if (fontAsset != null) labelTxt.font = fontAsset;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        
        if (rightAlignedValue)
        {
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(15f, posY);
            labelRect.sizeDelta = new Vector2(210f, 25f);
        }
        else
        {
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, posY + 15f);
            labelRect.sizeDelta = new Vector2(300f, 25f);
        }

        GameObject valObj = new GameObject($"RowValue_{label}", typeof(RectTransform), typeof(TextMeshProUGUI));
        valObj.transform.SetParent(parent, false);

        TextMeshProUGUI valTxt = valObj.GetComponent<TextMeshProUGUI>();
        valTxt.text = defaultValue;
        valTxt.fontSize = highlightTotal ? 15 : 13;
        valTxt.fontStyle = FontStyles.Bold;
        
        if (isScoreStyle)
        {
            valTxt.color = defaultValue.Contains("-") ? new Color(0.95f, 0.35f, 0.35f, 1f) : new Color(0.4f, 0.8f, 1.0f, 1f);
        }
        else if (highlightTotal)
        {
            valTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f);
        }
        else if (label.Contains("Oro") || isRewardItem)
        {
            valTxt.color = new Color(0.35f, 0.88f, 0.55f, 1f);
        }
        else
        {
            valTxt.color = Color.white;
        }

        valTxt.alignment = rightAlignedValue ? TextAlignmentOptions.Right : TextAlignmentOptions.Center;
        if (fontAsset != null) valTxt.font = fontAsset;

        RectTransform valRect = valObj.GetComponent<RectTransform>();

        if (rightAlignedValue)
        {
            valRect.pivot = new Vector2(1f, 0.5f);
            valRect.anchorMin = new Vector2(1f, 0.5f);
            valRect.anchorMax = new Vector2(1f, 0.5f);
            valRect.anchoredPosition = new Vector2(-15f, posY);
            valRect.sizeDelta = new Vector2(80f, 25f);
        }
        else
        {
            valRect.pivot = new Vector2(0.5f, 0.5f);
            valRect.anchorMin = new Vector2(0.5f, 0.5f);
            valRect.anchorMax = new Vector2(0.5f, 0.5f);
            valRect.anchoredPosition = new Vector2(0f, multilineValue ? (posY - 10f) : posY);
            valRect.sizeDelta = new Vector2(300f, multilineValue ? 45f : 25f);
            
            if (multilineValue)
            {
                valTxt.textWrappingMode = TextWrappingModes.Normal;
                valTxt.fontSize = 12;
            }
        }

        return valTxt;
    }

    private static void EnsureEventSystemExists()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
            Debug.Log("✓ EventSystem creado.");
        }
    }

    private static TMP_FontAsset GetDefaultFontAsset()
    {
        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset");
        if (guids != null && guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
        }
        return null;
    }
}

