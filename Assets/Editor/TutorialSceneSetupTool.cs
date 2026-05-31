using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Herramienta de Editor para preparar la escena NewTutorial.
/// Menú: Tools → Tutorial
/// </summary>
public static class TutorialSceneSetupTool
{
    // ---------------------------------------------------------------
    // 1. LIMPIAR ESCENA — Elimina lo que sobra de la copia de Justa
    // ---------------------------------------------------------------
    [MenuItem("Tools/Tutorial/1. Limpiar escena NewTutorial")]
    static void CleanupTutorialScene()
    {
        var jm = Object.FindFirstObjectByType<JoustManager>();
        if (jm == null)
        {
            Debug.LogError("[Tutorial Setup] No se encontró JoustManager en la escena actual. Abre la escena NewTutorial primero.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(jm, "Tutorial Scene Cleanup");

        int deleted = 0;

        // --- Eliminar el enemigo actual (caballero) ---
        if (jm.enemy != null)
        {
            GameObject enemyGO = jm.enemy.gameObject;
            // Comprobar que NO es el mismo GameObject que el JoustManager
            if (enemyGO != jm.gameObject)
            {
                string enemyName = enemyGO.name;
                Undo.DestroyObjectImmediate(enemyGO);
                jm.enemy = null;
                Debug.Log($"[Tutorial Setup] Eliminado enemigo: '{enemyName}'");
                deleted++;
            }
            else
            {
                Debug.LogWarning("[Tutorial Setup] El enemigo está en el mismo GameObject que el JoustManager. Se desasigna la referencia sin eliminar el objeto.");
                jm.enemy = null;
            }
        }

        // --- Eliminar EffectManager (cartas) ---
        // IMPORTANTE: Solo eliminamos el COMPONENTE si comparte GameObject con algo importante.
        // Si tiene su propio GameObject, lo eliminamos entero.
        if (jm.effectManager != null)
        {
            GameObject effectGO = jm.effectManager.gameObject;
            if (effectGO == jm.gameObject)
            {
                // Está en el MISMO GameObject que el JoustManager — solo eliminamos el componente
                string effectName = jm.effectManager.GetType().Name;
                Undo.DestroyObjectImmediate(jm.effectManager);
                jm.effectManager = null;
                Debug.Log($"[Tutorial Setup] Eliminado componente {effectName} (compartía GameObject con JoustManager).");
                deleted++;
            }
            else
            {
                // Tiene su propio GameObject — verificar que no tiene otros managers
                bool hasOtherManagers = effectGO.GetComponent<JoustManager>() != null
                    || effectGO.GetComponent<WinManager>() != null
                    || effectGO.GetComponent<ScoreManager>() != null;

                if (hasOtherManagers)
                {
                    Undo.DestroyObjectImmediate(jm.effectManager);
                    jm.effectManager = null;
                    Debug.Log($"[Tutorial Setup] Eliminado componente EffectManager (su GameObject tiene otros managers).");
                }
                else
                {
                    string effectName = effectGO.name;
                    Undo.DestroyObjectImmediate(effectGO);
                    jm.effectManager = null;
                    Debug.Log($"[Tutorial Setup] Eliminado GameObject EffectManager: '{effectName}'");
                }
                deleted++;
            }
        }

        // --- Eliminar mapas de ciudad (dejar solo defaultMap) ---
        foreach (var mapping in jm.cityMaps)
        {
            if (mapping.mapGameObject != null && mapping.mapGameObject != jm.gameObject)
            {
                string mapName = mapping.mapGameObject.name;
                Undo.DestroyObjectImmediate(mapping.mapGameObject);
                Debug.Log($"[Tutorial Setup] Eliminado mapa de ciudad: '{mapName}'");
                deleted++;
            }
        }
        jm.cityMaps.Clear();

        // --- Desactivar indicadores Best-of-3 del HUD (en vez de eliminar) ---
        var wm = jm.winManager;
        if (wm != null)
        {
            Undo.RegisterCompleteObjectUndo(wm, "Tutorial Scene Cleanup - WinManager");

            if (wm.hudRoundTitleText != null)
            {
                wm.hudRoundTitleText.gameObject.SetActive(false);
                Debug.Log("[Tutorial Setup] Desactivado HUD Round Title.");
            }

            if (wm.playerWinIndicators != null)
            {
                foreach (var indicator in wm.playerWinIndicators)
                {
                    if (indicator != null)
                        indicator.gameObject.SetActive(false);
                }
                Debug.Log("[Tutorial Setup] Desactivados indicadores de victoria del Player.");
            }

            if (wm.enemyWinIndicators != null)
            {
                foreach (var indicator in wm.enemyWinIndicators)
                {
                    if (indicator != null)
                        indicator.gameObject.SetActive(false);
                }
                Debug.Log("[Tutorial Setup] Desactivados indicadores de victoria del Enemigo.");
            }

            EditorUtility.SetDirty(wm);
        }

        // --- Configurar flags del JoustManager ---
        jm.isTutorialMode = true;
        jm.usePreJoustIntro = false;
        jm.useEffectChoiceButtons = false;

        // --- Configurar JoustTutorialManager si existe ---
        if (jm.tutorialManager != null)
        {
            Undo.RegisterCompleteObjectUndo(jm.tutorialManager, "Tutorial Scene Cleanup - TutorialManager");
            jm.tutorialManager.isTutorialScene = true;
            EditorUtility.SetDirty(jm.tutorialManager);
        }
        else
        {
            // Buscar en la escena
            var tm = Object.FindFirstObjectByType<JoustTutorialManager>();
            if (tm != null)
            {
                Undo.RegisterCompleteObjectUndo(tm, "Tutorial Scene Cleanup - TutorialManager");
                tm.isTutorialScene = true;
                jm.tutorialManager = tm;
                EditorUtility.SetDirty(tm);
            }
            else
            {
                Debug.LogWarning("[Tutorial Setup] No se encontró JoustTutorialManager. Añádelo manualmente al JoustManager o a otro GameObject.");
            }
        }

        EditorUtility.SetDirty(jm);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[Tutorial Setup] ¡Limpieza completada! {deleted} objetos procesados.");
        Debug.Log("  → isTutorialMode = true ✓");
        Debug.Log("  → usePreJoustIntro = false ✓");
        Debug.Log("  → useEffectChoiceButtons = false ✓");
        Debug.Log("  → Ahora arrastra tu asset Dummy a la escena y asígnalo en JoustManager → Enemy.");
        Debug.Log("  → Luego usa 'Tools → Tutorial → 2. Añadir colliders al Dummy' con el Dummy seleccionado.");
    }

    // ---------------------------------------------------------------
    // 2. AÑADIR COLLIDERS AL DUMMY — Crea la estructura de hitboxes
    // ---------------------------------------------------------------
    [MenuItem("Tools/Tutorial/2. Añadir colliders al Dummy (selecciónalo primero)")]
    static void AddDummyColliders()
    {
        GameObject selected = Selection.activeGameObject;
        if (selected == null)
        {
            Debug.LogError("[Tutorial Setup] Selecciona el GameObject del Dummy en la jerarquía antes de usar esta opción.");
            return;
        }

        Undo.RegisterCompleteObjectUndo(selected, "Add Dummy Colliders");

        // Estructura de hitboxes: hijos con Box Collider (trigger) + tag
        CreateHitbox(selected.transform, "Head",   new Vector3(0f, 2.4f, 0f),  new Vector3(0.4f, 0.4f, 0.4f));
        CreateHitbox(selected.transform, "Body",   new Vector3(0f, 1.6f, 0f),  new Vector3(0.7f, 0.8f, 0.5f));
        CreateHitbox(selected.transform, "Shield", new Vector3(-0.4f, 1.6f, 0.2f), new Vector3(0.3f, 0.6f, 0.1f));
        CreateHitbox(selected.transform, "Horse",  new Vector3(0f, 0.8f, 0f),  new Vector3(0.6f, 0.8f, 1.2f));

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        Debug.Log($"[Tutorial Setup] ¡Hitboxes añadidos a '{selected.name}'! Ajusta las posiciones y tamaños en el Inspector si es necesario.");
        Debug.Log("  → Asegúrate de que los tags Head, Body, Shield y Horse existen en Project Settings → Tags.");
    }

    static void CreateHitbox(Transform parent, string tagName, Vector3 localPos, Vector3 size)
    {
        string goName = $"Hitbox_{tagName}";

        // Comprobar si ya existe
        Transform existing = parent.Find(goName);
        if (existing != null)
        {
            Debug.LogWarning($"[Tutorial Setup] Ya existe '{goName}' en '{parent.name}'. Se omite.");
            return;
        }

        GameObject hitbox = new GameObject(goName);
        Undo.RegisterCreatedObjectUndo(hitbox, "Create Hitbox");
        hitbox.transform.SetParent(parent, false);
        hitbox.transform.localPosition = localPos;
        hitbox.transform.localRotation = Quaternion.identity;
        hitbox.transform.localScale = Vector3.one;

        // Tag
        hitbox.tag = tagName;

        // Box Collider como trigger
        BoxCollider col = hitbox.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = size;

        // Rigidbody kinematic para que funcione OnTriggerEnter
        Rigidbody rb = hitbox.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Debug.Log($"  Hitbox creado: '{goName}' → Tag: {tagName}, Pos: {localPos}, Size: {size}");
    }

    // ---------------------------------------------------------------
    // 3. VERIFICAR TAGS — Comprueba que los tags necesarios existen
    // ---------------------------------------------------------------
    [MenuItem("Tools/Tutorial/3. Verificar tags necesarios")]
    static void VerifyTags()
    {
        string[] requiredTags = { "Head", "Body", "Shield", "Horse", "Player", "Enemy" };
        bool allOk = true;

        foreach (string tag in requiredTags)
        {
            try
            {
                GameObject.FindGameObjectWithTag(tag);
                Debug.Log($"  ✓ Tag '{tag}' existe.");
            }
            catch (UnityException)
            {
                Debug.LogError($"  ✗ Tag '{tag}' NO existe. Añádelo en Edit → Project Settings → Tags and Layers.");
                allOk = false;
            }
        }

        if (allOk)
            Debug.Log("[Tutorial Setup] ¡Todos los tags necesarios están configurados!");
        else
            Debug.LogWarning("[Tutorial Setup] Faltan tags. Añádelos antes de continuar.");
    }
}
