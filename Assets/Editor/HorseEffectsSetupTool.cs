#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Info tool. El post-processing y las speed lines ahora se auto-crean en runtime.
/// Este menú solo sirve de referencia.
/// </summary>
public class HorseEffectsSetupTool : EditorWindow
{
    [MenuItem("Fields of Glory/Setup Horse Effects")]
    public static void ShowWindow()
    {
        var w = GetWindow<HorseEffectsSetupTool>("Horse Effects Setup");
        w.minSize = new Vector2(380, 160);
    }

    void OnGUI()
    {
        GUILayout.Space(10);

        var title = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        GUILayout.Label("⚔  Horse Phase Effects", title);
        GUILayout.Space(6);

        EditorGUILayout.HelpBox(
            "Todo se crea automáticamente en runtime:\n\n" +
            "  •  Volume global (Motion Blur + Vignette)\n" +
            "  •  Speed Lines (GL screen-space)\n\n" +
            "Ajusta los parámetros en el Inspector de HorsePart_Joust:\n" +
            "  - Max Blur Intensity\n" +
            "  - Max Vignette Intensity\n" +
            "  - Max Fov Excess\n" +
            "  - Speed Line Count / Radios / Width / Colores",
            MessageType.Info);

        GUILayout.Space(8);

        EditorGUILayout.HelpBox(
            "Requisito: La cámara principal debe tener\n" +
            "el checkbox 'Post Processing' activado.",
            MessageType.Warning);
    }
}
#endif
