#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class JoustUIEditorGenerator : EditorWindow
{
    [MenuItem("Tools/Fields of Glory/1. Generate Joust Cards UI")]
    public static void GenerateCardsUI()
    {
        GenerateCardsUIInternal(true);
    }

    public static void GenerateCardsUIInternal(bool showDialog)
    {
        // 1. Intentar localizar o crear el EffectManager en la escena
        EffectManager effectManager = FindFirstObjectByType<EffectManager>();
        GameObject effectManagerObj;
        if (effectManager == null)
        {
            effectManagerObj = new GameObject("EffectManager", typeof(EffectManager));
            effectManager = effectManagerObj.GetComponent<EffectManager>();
            Undo.RegisterCreatedObjectUndo(effectManagerObj, "Create EffectManager");
            Debug.Log("[UI Generator] Se ha creado un nuevo GameObject 'EffectManager' en la escena.");
        }
        else
        {
            effectManagerObj = effectManager.gameObject;
        }

        // 2. Buscar específicamente el Canvas 'JoustCanvas' por nombre para ser 100% explícitos
        GameObject canvasObj = GameObject.Find("JoustCanvas");
        Canvas canvasComp = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
        if (canvasObj == null || canvasComp == null)
        {
            canvasObj = new GameObject("JoustCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasComp = canvasObj.GetComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
            Debug.Log("[UI Generator] Se ha creado un nuevo Canvas de alta resolución 'JoustCanvas'.");
        }

        // Asegurarnos de que el Canvas tenga un GraphicRaycaster para recibir clics
        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
        {
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(raycaster, "Add GraphicRaycaster to JoustCanvas");
            Debug.Log("[UI Generator] Se ha añadido un componente GraphicRaycaster al Canvas.");
        }

        // Forzar orden de renderizado prioritario (Sorting Order 999) para recibir clics por encima de otros Canvas
        if (canvasComp != null)
        {
            canvasComp.sortingOrder = 999;
        }

        // Asegurar que exista un EventSystem en la escena (crucial para procesar clics de UI)
        EnsureEventSystemExists();

        // 3. Crear el Panel Principal (choicePanel) debajo del Canvas
        GameObject choicePanelObj = new GameObject("JoustChoicePanel", typeof(RectTransform), typeof(Image));
        choicePanelObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform panelRect = choicePanelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero; // Estirado completo

        // Estética Premium: Cristal esmerilado oscuro (Glassmorphic dark overlay)
        Image panelImg = choicePanelObj.GetComponent<Image>();
        panelImg.color = new Color(0.04f, 0.04f, 0.06f, 0.93f); 

        Undo.RegisterCreatedObjectUndo(choicePanelObj, "Create Choice Panel");

        // 4. Obtener la fuente TMP por defecto del proyecto
        TMP_FontAsset fontAsset = GetDefaultFontAsset();

        // 5. Crear el Título Principal del Panel
        GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(choicePanelObj.transform, false);
        
        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = "SELECCIONA TU DESAFÍO";
        titleTxt.fontSize = 45;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f); // Dorado/Ámbar Premium
        titleTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) titleTxt.font = fontAsset;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -100f); // 100px por debajo del borde superior
        titleRect.sizeDelta = new Vector2(800f, 80f);

        Undo.RegisterCreatedObjectUndo(titleObj, "Create UI Title");

        // 6. Crear el Contenedor de las Tarjetas
        GameObject cardsContainer = new GameObject("CardsContainer", typeof(RectTransform));
        cardsContainer.transform.SetParent(choicePanelObj.transform, false);
        
        RectTransform containerRect = cardsContainer.GetComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.5f, 0.5f);
        containerRect.anchorMax = new Vector2(0.5f, 0.5f);
        containerRect.anchoredPosition = new Vector2(0f, 20f); // Ligeramente centrado hacia arriba
        containerRect.sizeDelta = new Vector2(900f, 380f);

        Undo.RegisterCreatedObjectUndo(cardsContainer, "Create Cards Container");

        // 7. Crear las 3 Tarjetas (Botones) mediante matemáticas de espaciado
        float cardWidth = 240f;
        float cardHeight = 360f;
        float spacing = 290f; // Espacio entre centros

        effectManager.choiceButtons = new EffectManager.EffectChoiceButton[3];

        for (int i = 0; i < 3; i++)
        {
            int index = i;
            float posX = (i - 1) * spacing; // Centrado en X: -290, 0, 290

            // Borde exterior procedural (Simula un borde elegante de 2px de grosor)
            GameObject borderObj = new GameObject($"CardBorder_{i}", typeof(RectTransform), typeof(Image));
            borderObj.transform.SetParent(cardsContainer.transform, false);
            
            RectTransform borderRect = borderObj.GetComponent<RectTransform>();
            borderRect.sizeDelta = new Vector2(cardWidth + 4f, cardHeight + 4f);
            borderRect.anchoredPosition = new Vector2(posX, 0f);
            
            Image borderImg = borderObj.GetComponent<Image>();
            borderImg.color = new Color(0.28f, 0.28f, 0.32f, 1f); // Plateado oscuro metálico

            // Tarjeta Botón Interna
            GameObject cardBtnObj = new GameObject($"CardButton_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            cardBtnObj.transform.SetParent(borderObj.transform, false);
            
            RectTransform btnRect = cardBtnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = Vector2.zero;
            btnRect.anchorMax = Vector2.one;
            btnRect.sizeDelta = Vector2.zero; // Rellenar borde

            Image btnImg = cardBtnObj.GetComponent<Image>();
            btnImg.color = new Color(0.09f, 0.09f, 0.12f, 1f); // Negro obsidiana elegante

            Button btnComp = cardBtnObj.GetComponent<Button>();

            // Crear el Texto del Reto (Parte superior)
            GameObject negTextObj = new GameObject("ChallengeText", typeof(RectTransform), typeof(TextMeshProUGUI));
            negTextObj.transform.SetParent(cardBtnObj.transform, false);
            
            TextMeshProUGUI negTxt = negTextObj.GetComponent<TextMeshProUGUI>();
            negTxt.text = "NIEBLA ESPESA";
            negTxt.fontSize = 18;
            negTxt.fontStyle = FontStyles.Bold;
            negTxt.color = new Color(0.95f, 0.3f, 0.3f, 1f); // Coral/Rojo de reto
            negTxt.alignment = TextAlignmentOptions.Center;
            if (fontAsset != null) negTxt.font = fontAsset;

            RectTransform negRect = negTextObj.GetComponent<RectTransform>();
            negRect.anchorMin = new Vector2(0f, 0.55f);
            negRect.anchorMax = new Vector2(1f, 0.95f);
            negRect.offsetMin = new Vector2(10f, 10f);
            negRect.offsetMax = new Vector2(-10f, -10f);

            // Crear una línea divisoria procedural en medio
            GameObject dividerObj = new GameObject("Divider", typeof(RectTransform), typeof(Image));
            dividerObj.transform.SetParent(cardBtnObj.transform, false);
            
            RectTransform divRect = dividerObj.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.15f, 0.5f);
            divRect.anchorMax = new Vector2(0.85f, 0.5f);
            divRect.sizeDelta = new Vector2(0f, 2f); // 2px de grosor
            divRect.anchoredPosition = Vector2.zero;

            Image divImg = dividerObj.GetComponent<Image>();
            divImg.color = new Color(0.2f, 0.2f, 0.25f, 0.5f);

            // Crear el Texto del Premio (Parte inferior)
            GameObject posTextObj = new GameObject("RewardText", typeof(RectTransform), typeof(TextMeshProUGUI));
            posTextObj.transform.SetParent(cardBtnObj.transform, false);
            
            TextMeshProUGUI posTxt = posTextObj.GetComponent<TextMeshProUGUI>();
            posTxt.text = "+150 ORO";
            posTxt.fontSize = 18;
            posTxt.fontStyle = FontStyles.Bold;
            posTxt.color = new Color(0.3f, 0.85f, 0.5f, 1f); // Esmeralda/Verde premio
            posTxt.alignment = TextAlignmentOptions.Center;
            if (fontAsset != null) posTxt.font = fontAsset;

            RectTransform posRect = posTextObj.GetComponent<RectTransform>();
            posRect.anchorMin = new Vector2(0f, 0.05f);
            posRect.anchorMax = new Vector2(1f, 0.45f);
            posRect.offsetMin = new Vector2(10f, 10f);
            posRect.offsetMax = new Vector2(-10f, -10f);

            // Registrar creación en el Undo
            Undo.RegisterCreatedObjectUndo(borderObj, "Create Card Button Structure");

            // Rellenar las referencias de la clase del botón en el EffectManager
            EffectManager.EffectChoiceButton choiceButtonData = new EffectManager.EffectChoiceButton();
            choiceButtonData.button = btnComp;
            choiceButtonData.negativeText = negTxt;
            choiceButtonData.positiveText = posTxt;

            effectManager.choiceButtons[index] = choiceButtonData;
        }

        // 8. Crear el Botón "Sin Modificador" (Skip Button)
        GameObject skipBorderObj = new GameObject("SkipButtonBorder", typeof(RectTransform), typeof(Image));
        skipBorderObj.transform.SetParent(choicePanelObj.transform, false);
        
        RectTransform skipBorderRect = skipBorderObj.GetComponent<RectTransform>();
        skipBorderRect.anchorMin = new Vector2(0.5f, 0.5f);
        skipBorderRect.anchorMax = new Vector2(0.5f, 0.5f);
        skipBorderRect.anchoredPosition = new Vector2(0f, -240f); // Por debajo de las cartas
        skipBorderRect.sizeDelta = new Vector2(302f, 52f);

        Image skipBorderImg = skipBorderObj.GetComponent<Image>();
        skipBorderImg.color = new Color(0.35f, 0.35f, 0.4f, 0.7f); // Borde plateado elegante

        GameObject skipBtnObj = new GameObject("NoModifierButton", typeof(RectTransform), typeof(Image), typeof(Button));
        skipBtnObj.transform.SetParent(skipBorderObj.transform, false);
        
        RectTransform skipBtnRect = skipBtnObj.GetComponent<RectTransform>();
        skipBtnRect.anchorMin = Vector2.zero;
        skipBtnRect.anchorMax = Vector2.one;
        skipBtnRect.sizeDelta = Vector2.zero;

        Image skipBtnImg = skipBtnObj.GetComponent<Image>();
        skipBtnImg.color = new Color(0.12f, 0.12f, 0.15f, 1f);

        Button skipButtonComp = skipBtnObj.GetComponent<Button>();

        // Texto del botón de saltar
        GameObject skipTextObj = new GameObject("SkipText", typeof(RectTransform), typeof(TextMeshProUGUI));
        skipTextObj.transform.SetParent(skipBtnObj.transform, false);
        
        TextMeshProUGUI skipTxt = skipTextObj.GetComponent<TextMeshProUGUI>();
        skipTxt.text = "JUGAR SEGURO (Ronda Estándar)";
        skipTxt.fontSize = 14;
        skipTxt.fontStyle = FontStyles.Bold;
        skipTxt.color = new Color(0.7f, 0.7f, 0.75f, 1f);
        skipTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) skipTxt.font = fontAsset;

        RectTransform skipTextRect = skipTextObj.GetComponent<RectTransform>();
        skipTextRect.anchorMin = Vector2.zero;
        skipTextRect.anchorMax = Vector2.one;
        skipTextRect.sizeDelta = Vector2.zero;

        Undo.RegisterCreatedObjectUndo(skipBorderObj, "Create Skip Button Structure");

        // 9. Crear el Texto de Estado Flotante en Pantalla (effectText) fuera del Choice Panel
        GameObject statusTextObj = new GameObject("JoustStatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
        statusTextObj.transform.SetParent(canvasObj.transform, false);
        
        TextMeshProUGUI statusTxt = statusTextObj.GetComponent<TextMeshProUGUI>();
        statusTxt.text = "";
        statusTxt.fontSize = 32;
        statusTxt.fontStyle = FontStyles.Bold;
        statusTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) statusTxt.font = fontAsset;

        RectTransform statusRect = statusTextObj.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.5f, 1f);
        statusRect.anchorMax = new Vector2(0.5f, 1f);
        statusRect.anchoredPosition = new Vector2(0f, -120f); // Debajo del título o cabecera
        statusRect.sizeDelta = new Vector2(1000f, 120f);

        statusTextObj.SetActive(false); // Empieza desactivado por defecto

        Undo.RegisterCreatedObjectUndo(statusTextObj, "Create Status Text");

        // 10. Vincular las referencias estructurales al componente EffectManager
        effectManager.choicePanel = choicePanelObj;
        effectManager.noModifierButton = skipButtonComp;
        effectManager.effectText = statusTxt;

        // Marcar objetos y escena como modificados
        EditorUtility.SetDirty(effectManagerObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Mensaje de feedback exitoso
        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "UI Generada con Éxito",
                "¡El Canvas y todo el panel premium de selección de cartas de Desafío/Recompensa han sido generados permanentemente en la escena y vinculados con éxito al EffectManager!",
                "Excelente"
            );
        }
    }

    [MenuItem("Tools/Fields of Glory/2. Generate Joust Stats Panel")]
    public static void GenerateStatsPanelUI()
    {
        GenerateStatsPanelUIInternal(true);
    }

    public static void GenerateStatsPanelUIInternal(bool showDialog)
    {
        // 1. Intentar localizar el WinManager en la escena
        WinManager winManager = FindFirstObjectByType<WinManager>();
        if (winManager == null)
        {
            EditorUtility.DisplayDialog("Falta WinManager", "No se encontró un componente WinManager en la escena activa. Asegúrate de estar en la escena de la Justa antes de crear el panel.", "Entendido");
            return;
        }

        // 2. Buscar específicamente el Canvas 'JoustCanvas' por nombre
        GameObject canvasObj = GameObject.Find("JoustCanvas");
        Canvas canvasComp = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
        if (canvasObj == null || canvasComp == null)
        {
            canvasObj = new GameObject("JoustCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasComp = canvasObj.GetComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        // Asegurarnos de que el Canvas tenga un GraphicRaycaster para recibir clics
        if (canvasObj.GetComponent<GraphicRaycaster>() == null)
        {
            GraphicRaycaster raycaster = canvasObj.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(raycaster, "Add GraphicRaycaster to JoustCanvas");
        }

        // Forzar orden de renderizado prioritario (Sorting Order 999) para recibir clics por encima de otros Canvas
        if (canvasComp != null)
        {
            canvasComp.sortingOrder = 999;
        }

        // Asegurar que exista un EventSystem en la escena (crucial para procesar clics de UI)
        EnsureEventSystemExists();

        TMP_FontAsset fontAsset = GetDefaultFontAsset();

        // 3. Crear el borde exterior del Panel de Estadísticas (Elegante marco procedural plateado)
        GameObject panelBorderObj = new GameObject("StatsPanelBorder", typeof(RectTransform), typeof(Image));
        panelBorderObj.transform.SetParent(canvasObj.transform, false);
        
        RectTransform borderRect = panelBorderObj.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(0f, 10f);
        borderRect.sizeDelta = new Vector2(1204f, 754f); // 4px extra para el borde

        Image borderImg = panelBorderObj.GetComponent<Image>();
        borderImg.color = new Color(0.25f, 0.25f, 0.28f, 1f); // Marco metálico

        // 4. Panel Interno del Panel de Estadísticas
        GameObject panelObj = new GameObject("JoustStatsPanel", typeof(RectTransform), typeof(Image), typeof(JoustStatsPanelController));
        panelObj.transform.SetParent(panelBorderObj.transform, false);
        
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero; // Rellenar borde

        Image panelImg = panelObj.GetComponent<Image>();
        panelImg.color = new Color(0.06f, 0.06f, 0.08f, 0.98f); // Fondo obsidian premium

        JoustStatsPanelController controller = panelObj.GetComponent<JoustStatsPanelController>();

        Undo.RegisterCreatedObjectUndo(panelBorderObj, "Create Stats Panel Structure");

        // 5. Encabezado del Panel
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

        // Subtítulo de Resultado Dinámico (resultTitleText)
        GameObject resultObj = new GameObject("ResultText", typeof(RectTransform), typeof(TextMeshProUGUI));
        resultObj.transform.SetParent(panelObj.transform, false);
        
        TextMeshProUGUI resultTxt = resultObj.GetComponent<TextMeshProUGUI>();
        resultTxt.text = "¡VICTORIA EN LA JUSTA!";
        resultTxt.fontSize = 42;
        resultTxt.fontStyle = FontStyles.Bold | FontStyles.Italic;
        resultTxt.color = new Color(0.28f, 0.88f, 0.52f, 1f); // Verde por defecto
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

        // 6. Contenedor de las 3 Columnas
        GameObject colsContainer = new GameObject("ColumnsContainer", typeof(RectTransform));
        colsContainer.transform.SetParent(panelObj.transform, false);
        
        RectTransform colsRect = colsContainer.GetComponent<RectTransform>();
        colsRect.anchorMin = new Vector2(0.5f, 0.5f);
        colsRect.anchorMax = new Vector2(0.5f, 0.5f);
        colsRect.anchoredPosition = new Vector2(0f, -30f); // Centrado con offset vertical
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
        // Sección 1: Puntuación por fases
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

        // Sección 2: Recompensas conseguidas
        controller.rewardsGoldText = CreateRowText(col3.transform, "Oro Ganado:", "+150 Monedas", -80f, fontAsset, true, false);
        controller.rewardsItemText = CreateRowText(col3.transform, "Objeto Obtenido:", "Ninguno", -135f, fontAsset, false, false, false, true);

        // 7. Crear el Botón "Finalizar Torneo" en la parte inferior
        GameObject btnBorderObj = new GameObject("FinishBtnBorder", typeof(RectTransform), typeof(Image));
        btnBorderObj.transform.SetParent(panelObj.transform, false);
        
        RectTransform btnBorderRect = btnBorderObj.GetComponent<RectTransform>();
        btnBorderRect.anchorMin = new Vector2(0.5f, 0f);
        btnBorderRect.anchorMax = new Vector2(0.5f, 0f);
        btnBorderRect.anchoredPosition = new Vector2(0f, 60f); // 60px por encima de la base del panel
        btnBorderRect.sizeDelta = new Vector2(322f, 56f);

        Image btnBorderImg = btnBorderObj.GetComponent<Image>();
        btnBorderImg.color = new Color(1.0f, 0.72f, 0.18f, 0.8f); // Borde dorado metálico

        GameObject finishBtnObj = new GameObject("FinishTournamentButton", typeof(RectTransform), typeof(Image), typeof(Button));
        finishBtnObj.transform.SetParent(btnBorderObj.transform, false);
        
        RectTransform finishBtnRect = finishBtnObj.GetComponent<RectTransform>();
        finishBtnRect.anchorMin = Vector2.zero;
        finishBtnRect.anchorMax = Vector2.one;
        finishBtnRect.sizeDelta = Vector2.zero;

        Image finishBtnImg = finishBtnObj.GetComponent<Image>();
        finishBtnImg.color = new Color(0.09f, 0.09f, 0.12f, 1f); // Obsidian

        Button finishButtonComp = finishBtnObj.GetComponent<Button>();

        // Texto del botón
        GameObject btnTextObj = new GameObject("FinishBtnText", typeof(RectTransform), typeof(TextMeshProUGUI));
        btnTextObj.transform.SetParent(finishBtnObj.transform, false);
        
        TextMeshProUGUI btnTxt = btnTextObj.GetComponent<TextMeshProUGUI>();
        btnTxt.text = "FINALIZAR TORNEO";
        btnTxt.fontSize = 16;
        btnTxt.fontStyle = FontStyles.Bold;
        btnTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f); // Texto Dorado
        btnTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) btnTxt.font = fontAsset;

        RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        controller.finishButton = finishButtonComp;

        Undo.RegisterCreatedObjectUndo(btnBorderObj, "Create Finish Button Structure");

        // 8. Ocultar el panel de estadísticas por defecto (para que empiece desactivado)
        panelBorderObj.SetActive(false);

        // 9. Vincular el controlador de estadísticas al WinManager
        winManager.statsPanelController = controller;
        controller.panelObject = panelBorderObj;
        controller.nextSceneName = "World";
        winManager.nextSceneName = "World";

        // Marcar objetos y escena como modificados
        EditorUtility.SetDirty(panelObj);
        EditorUtility.SetDirty(winManager.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Mensaje de éxito
        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "Panel de Estadísticas Generado",
                "¡El panel premium de estadísticas con su marco metálico, desglose de 3 columnas (equipo, estadísticas y puntuaciones/recompensas) y botón de finalizar torneo ha sido creado e integrado con éxito al WinManager de la justa!",
                "Excelente"
            );
        }
    }

    private static GameObject CreateColumn(Transform parent, float posX, string colTitle, TMP_FontAsset fontAsset)
    {
        // Contenedor de Columna (Borde / Marco procedural)
        GameObject colBorder = new GameObject($"ColBorder_{colTitle}", typeof(RectTransform), typeof(Image));
        colBorder.transform.SetParent(parent, false);
        
        RectTransform borderRect = colBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 0.5f);
        borderRect.anchorMax = new Vector2(0.5f, 0.5f);
        borderRect.anchoredPosition = new Vector2(posX, 0f);
        borderRect.sizeDelta = new Vector2(332f, 382f);
        colBorder.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f, 0.5f); // Marco sutil

        // Columna Interna
        GameObject colObj = new GameObject($"Column_{colTitle}", typeof(RectTransform), typeof(Image));
        colObj.transform.SetParent(colBorder.transform, false);
        
        RectTransform colRect = colObj.GetComponent<RectTransform>();
        colRect.anchorMin = Vector2.zero;
        colRect.anchorMax = Vector2.one;
        colRect.anchoredPosition = Vector2.zero; // Evitar desfase por compensación de Unity
        colRect.sizeDelta = new Vector2(-4f, -4f); // Espesor de 2px de borde

        Image colImg = colObj.GetComponent<Image>();
        colImg.color = new Color(0.08f, 0.08f, 0.1f, 0.85f); // Panel oscuro

        // Título de la columna
        GameObject titleObj = new GameObject("ColTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(colObj.transform, false);
        
        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = colTitle;
        titleTxt.fontSize = 16;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f); // Dorado
        titleTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) titleTxt.font = fontAsset;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0f, -25f);
        titleRect.sizeDelta = new Vector2(300f, 30f);

        return colObj;
    }

    private static TMP_Text CreateRowText(Transform parent, string label, string defaultValue, float posY, TMP_FontAsset fontAsset, bool rightAlignedValue = false, bool isScoreStyle = false, bool highlightTotal = false, bool multilineValue = false)
    {
        // 1. GameObject de la Etiqueta (Lado Izquierdo o Centrado)
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
            // Alineación a la izquierda fijando el pivot en (0, 0.5) para evitar desbordamientos
            labelRect.pivot = new Vector2(0f, 0.5f);
            labelRect.anchorMin = new Vector2(0f, 0.5f);
            labelRect.anchorMax = new Vector2(0f, 0.5f);
            labelRect.anchoredPosition = new Vector2(15f, posY); // Margen de 15px
            labelRect.sizeDelta = new Vector2(210f, 25f); // 210px de ancho para el texto descriptivo
        }
        else
        {
            labelRect.pivot = new Vector2(0.5f, 0.5f);
            labelRect.anchorMin = new Vector2(0.5f, 0.5f);
            labelRect.anchorMax = new Vector2(0.5f, 0.5f);
            labelRect.anchoredPosition = new Vector2(0f, posY + 15f);
            labelRect.sizeDelta = new Vector2(300f, 25f);
        }

        // 2. GameObject del Valor (Lado Derecho o Centrado Abajo)
        GameObject valObj = new GameObject($"RowValue_{label}", typeof(RectTransform), typeof(TextMeshProUGUI));
        valObj.transform.SetParent(parent, false);

        TextMeshProUGUI valTxt = valObj.GetComponent<TextMeshProUGUI>();
        valTxt.text = defaultValue;
        valTxt.fontSize = highlightTotal ? 15 : 13;
        valTxt.fontStyle = FontStyles.Bold;
        
        // Colores temáticos
        if (isScoreStyle)
        {
            valTxt.color = defaultValue.Contains("-") ? new Color(0.95f, 0.35f, 0.35f, 1f) : new Color(0.4f, 0.8f, 1.0f, 1f);
        }
        else if (highlightTotal)
        {
            valTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f);
        }
        else if (label.Contains("Oro") || label.Contains("Objeto"))
        {
            valTxt.color = new Color(0.35f, 0.88f, 0.55f, 1f); // Verde éxito
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
            // Alineación a la derecha fijando el pivot en (1, 0.5) para evitar desbordamientos
            valRect.pivot = new Vector2(1f, 0.5f);
            valRect.anchorMin = new Vector2(1f, 0.5f);
            valRect.anchorMax = new Vector2(1f, 0.5f);
            valRect.anchoredPosition = new Vector2(-15f, posY); // Margen de 15px de la derecha
            valRect.sizeDelta = new Vector2(80f, 25f); // 80px para números/puntos
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

    [MenuItem("Tools/Fields of Glory/3. Generate Best of 3 Rounds HUD")]
    public static void GenerateRoundsHUDUI()
    {
        GenerateRoundsHUDUIInternal(true);
    }

    public static void GenerateRoundsHUDUIInternal(bool showDialog)
    {
        // 1. Locate WinManager
        WinManager winManager = FindFirstObjectByType<WinManager>();
        if (winManager == null)
        {
            EditorUtility.DisplayDialog("Falta WinManager", "No se encontró un componente WinManager en la escena activa. Asegúrate de estar en la escena de la Justa antes de crear el HUD de Rondas.", "Entendido");
            return;
        }

        // 2. Find or create JoustCanvas
        GameObject canvasObj = GameObject.Find("JoustCanvas");
        Canvas canvasComp = canvasObj != null ? canvasObj.GetComponent<Canvas>() : null;
        if (canvasObj == null || canvasComp == null)
        {
            canvasObj = new GameObject("JoustCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasComp = canvasObj.GetComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            
            CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Canvas");
        }

        TMP_FontAsset fontAsset = GetDefaultFontAsset();

        // 3. Check if HUD_RoundsPanel already exists, delete if so to rebuild fresh
        GameObject oldHUD = GameObject.Find("HUD_RoundsPanelBorder");
        if (oldHUD != null)
        {
            Undo.DestroyObjectImmediate(oldHUD);
        }

        // 4. Create premium border for the Rounds HUD (Marco dorado premium)
        GameObject roundsBorder = new GameObject("HUD_RoundsPanelBorder", typeof(RectTransform), typeof(Image));
        roundsBorder.transform.SetParent(canvasObj.transform, false);
        
        RectTransform borderRect = roundsBorder.GetComponent<RectTransform>();
        borderRect.anchorMin = new Vector2(0.5f, 1f);
        borderRect.anchorMax = new Vector2(0.5f, 1f);
        borderRect.anchoredPosition = new Vector2(0f, -60f); // 60px below top
        borderRect.sizeDelta = new Vector2(704f, 74f); // 2px border on all sides

        Image borderImg = roundsBorder.GetComponent<Image>();
        borderImg.color = new Color(1.0f, 0.72f, 0.18f, 0.65f); // Borde dorado premium semitransparente

        // 5. Create main panel container
        GameObject roundsPanel = new GameObject("HUD_RoundsPanel", typeof(RectTransform), typeof(Image));
        roundsPanel.transform.SetParent(roundsBorder.transform, false);

        RectTransform panelRect = roundsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = new Vector2(-4f, -4f); // 2px margin inside border

        Image panelImg = roundsPanel.GetComponent<Image>();
        panelImg.color = new Color(0.06f, 0.06f, 0.08f, 0.90f); // Obsidian translúcido premium

        Undo.RegisterCreatedObjectUndo(roundsBorder, "Create Rounds HUD Structure");

        // 6. Central text for the Round title (HUD_RoundTitleText)
        GameObject titleObj = new GameObject("HUD_RoundTitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
        titleObj.transform.SetParent(roundsPanel.transform, false);

        TextMeshProUGUI titleTxt = titleObj.GetComponent<TextMeshProUGUI>();
        titleTxt.text = "RONDA 1";
        titleTxt.fontSize = 22;
        titleTxt.fontStyle = FontStyles.Bold;
        titleTxt.color = new Color(1.0f, 0.72f, 0.18f, 1f); // Dorado premium
        titleTxt.alignment = TextAlignmentOptions.Center;
        if (fontAsset != null) titleTxt.font = fontAsset;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = Vector2.zero;
        titleRect.sizeDelta = new Vector2(300f, 50f);

        // 7. Get standard Knob sprite for premium circle slots
        Sprite knobSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        // Helper to spawn elegant win slots
        Image CreateWinCircle(Transform parent, float posX, string name)
        {
            // Borde del círculo metálico
            GameObject circBorder = new GameObject($"{name}_Border", typeof(RectTransform), typeof(Image));
            circBorder.transform.SetParent(parent, false);

            RectTransform cBorderRect = circBorder.GetComponent<RectTransform>();
            cBorderRect.anchorMin = new Vector2(0.5f, 0.5f);
            cBorderRect.anchorMax = new Vector2(0.5f, 0.5f);
            cBorderRect.anchoredPosition = new Vector2(posX, 0f);
            cBorderRect.sizeDelta = new Vector2(28f, 28f);

            Image cbImg = circBorder.GetComponent<Image>();
            cbImg.sprite = knobSprite;
            cbImg.color = new Color(0.28f, 0.28f, 0.32f, 1f); // Gris metálico

            // Círculo interno (color activo/inactivo se cambia en tiempo de ejecución)
            GameObject circInner = new GameObject($"{name}_Inner", typeof(RectTransform), typeof(Image));
            circInner.transform.SetParent(circBorder.transform, false);

            RectTransform cInnerRect = circInner.GetComponent<RectTransform>();
            cInnerRect.anchorMin = Vector2.zero;
            cInnerRect.anchorMax = Vector2.one;
            cInnerRect.sizeDelta = new Vector2(-4f, -4f); // 2px margin

            Image ciImg = circInner.GetComponent<Image>();
            ciImg.sprite = knobSprite;
            ciImg.color = winManager.indicatorInactiveColor;

            return ciImg;
        }

        // 8. Spawn Player Win Indicators
        Image playerImg0 = CreateWinCircle(roundsPanel.transform, -220f, "PlayerIndicator_0");
        Image playerImg1 = CreateWinCircle(roundsPanel.transform, -170f, "PlayerIndicator_1");

        // 9. Spawn Enemy Win Indicators
        Image enemyImg0 = CreateWinCircle(roundsPanel.transform, 170f, "EnemyIndicator_0");
        Image enemyImg1 = CreateWinCircle(roundsPanel.transform, 220f, "EnemyIndicator_1");

        // 10. Spawn Player Label Text
        GameObject pLabelObj = new GameObject("PlayerLabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        pLabelObj.transform.SetParent(roundsPanel.transform, false);

        TextMeshProUGUI pLabelTxt = pLabelObj.GetComponent<TextMeshProUGUI>();
        pLabelTxt.text = "JUGADOR";
        pLabelTxt.fontSize = 13;
        pLabelTxt.fontStyle = FontStyles.Bold;
        pLabelTxt.color = new Color(0.75f, 0.75f, 0.8f, 1f);
        pLabelTxt.alignment = TextAlignmentOptions.Right;
        if (fontAsset != null) pLabelTxt.font = fontAsset;

        RectTransform pLabelRect = pLabelObj.GetComponent<RectTransform>();
        pLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        pLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        pLabelRect.anchoredPosition = new Vector2(-290f, 0f);
        pLabelRect.sizeDelta = new Vector2(100f, 30f);

        // 11. Spawn Enemy Label Text
        GameObject eLabelObj = new GameObject("EnemyLabelText", typeof(RectTransform), typeof(TextMeshProUGUI));
        eLabelObj.transform.SetParent(roundsPanel.transform, false);

        TextMeshProUGUI eLabelTxt = eLabelObj.GetComponent<TextMeshProUGUI>();
        eLabelTxt.text = "RIVAL";
        eLabelTxt.fontSize = 13;
        eLabelTxt.fontStyle = FontStyles.Bold;
        eLabelTxt.color = new Color(0.75f, 0.75f, 0.8f, 1f);
        eLabelTxt.alignment = TextAlignmentOptions.Left;
        if (fontAsset != null) eLabelTxt.font = fontAsset;

        RectTransform eLabelRect = eLabelObj.GetComponent<RectTransform>();
        eLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        eLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        eLabelRect.anchoredPosition = new Vector2(290f, 0f);
        eLabelRect.sizeDelta = new Vector2(100f, 30f);

        // 12. Programmatically Bind to WinManager
        winManager.hudRoundTitleText = titleTxt;
        winManager.playerWinIndicators = new Image[] { playerImg0, playerImg1 };
        winManager.enemyWinIndicators = new Image[] { enemyImg0, enemyImg1 };

        // Save progress, update HUD initial state, and mark dirty
        winManager.UpdateBestOf3UI();
        EditorUtility.SetDirty(winManager);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Display Dialog success
        if (showDialog)
        {
            EditorUtility.DisplayDialog(
                "HUD de Rondas Generado con Éxito",
                "¡El panel premium de Rondas ('Best of 3') en la parte superior con círculos dorados/esmeralda y rojo rubí procedurales ha sido creado y vinculado perfectamente al WinManager!",
                "Excelente"
            );
        }
    }

    [MenuItem("Tools/Fields of Glory/4. Regenerate All Joust UIs")]
    public static void RegenerateAllUIs()
    {
        // 1. Regenerar Tarjetas (sin popup individual)
        GenerateCardsUIInternal(false);
        
        // 2. Regenerar Panel de Estadísticas (sin popup individual)
        GenerateStatsPanelUIInternal(false);
        
        // 3. Regenerar HUD de Rondas (sin popup individual)
        GenerateRoundsHUDUIInternal(false);
        
        EditorUtility.DisplayDialog(
            "Regeneración Completa",
            "¡Todas las interfaces premium de la Justa (Tarjetas, Panel de Estadísticas y HUD de Rondas) han sido recreadas y vinculadas perfectamente con éxito!",
            "Excelente"
        );
    }

    private static void EnsureEventSystemExists()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystemObj, "Create EventSystem");
            Debug.Log("[UI Generator] Se ha creado un nuevo 'EventSystem' en la escena porque no existía ninguno.");
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
#endif
