using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Dibuja speed lines directamente sobre el render target de la cámara usando GL.
/// No crea ningún objeto en la escena — es un efecto puramente de cámara.
/// Compatible con URP mediante RenderPipelineManager.endCameraRendering.
/// </summary>
public class HorseSpeedLinesEffect : MonoBehaviour
{
    [Header("Cámara")]
    [Tooltip("Cámara objetivo. Se busca la cámara principal si está vacío.")]
    public Camera targetCamera;

    [Header("Líneas")]
    [Tooltip("Número máximo de líneas (Verde). Amarillo usa la mitad.")]
    public int maxLines = 24;
    [Tooltip("Radio interior — qué tan cerca del centro empieza cada línea (0..1 del ancho de pantalla).")]
    [Range(0f, 0.49f)] public float innerRadius = 0.12f;
    [Tooltip("Radio exterior — qué tan lejos del centro llega cada línea (0..1 del ancho de pantalla).")]
    [Range(0f, 0.49f)] public float outerRadius = 0.46f;
    [Tooltip("Grosor de cada línea (en coordenadas de pantalla 0..1).")]
    [Range(0.001f, 0.05f)] public float lineWidth = 0.012f;

    [Header("Verde (Perfect)")]
    public Color greenColor = new Color(0.45f, 1f, 0.45f, 1f);

    [Header("Amarillo (Good)")]
    public Color yellowColor = new Color(1f, 0.85f, 0.15f, 1f);

    [Header("Animación")]
    [Tooltip("Tiempo en segundos que tardan en desvanecerse.")]
    public float fadeDuration = 0.3f;

    // ---------------------------------------------------------------
    // Privados
    // ---------------------------------------------------------------

    // Cada línea en coordenadas normalizadas (0..1) de pantalla
    private struct LineSegment
    {
        public Vector2 start, end;
        public bool active;
    }

    private LineSegment[] lineData;
    private Material      glMaterial;
    private Color         activeColor;
    private float         alpha;
    private bool          isPlaying;
    private Coroutine     fadeRoutine;

    // ---------------------------------------------------------------
    // Ciclo de vida
    // ---------------------------------------------------------------

    void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        lineData = new LineSegment[maxLines];
        BuildMaterial();
    }

    void OnEnable()
    {
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        alpha     = 0f;
        isPlaying = false;
    }

    void OnDestroy()
    {
        if (glMaterial != null)
            Destroy(glMaterial);
    }

    // ---------------------------------------------------------------
    // Material GL (sin textura, solo color + alpha)
    // ---------------------------------------------------------------

    void BuildMaterial()
    {
        // Hidden/Internal-Colored soporta GL inmediato y funciona en URP/Built-in
        Shader shader = Shader.Find("Hidden/Internal-Colored")
                     ?? Shader.Find("Sprites/Default");

        glMaterial = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        glMaterial.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull",     (int)CullMode.Off);
        glMaterial.SetInt("_ZWrite",   0);
        glMaterial.SetInt("_ZTest",    (int)CompareFunction.Always);
    }

    // ---------------------------------------------------------------
    // API pública
    // ---------------------------------------------------------------

    /// <summary>Lanza el efecto de speed lines para la zona dada ("Verde" o "Amarillo").</summary>
    public void PlayBurst(string zone)
    {
        int   count;
        Color color;

        switch (zone)
        {
            case "Verde":
                count = maxLines;
                color = greenColor;
                break;
            case "Amarillo":
                count = maxLines / 2;
                color = yellowColor;
                break;
            default:
                return; // Rojo: sin efecto
        }

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(BurstRoutine(count, color));
    }

    // ---------------------------------------------------------------
    // Corrutina de animación
    // ---------------------------------------------------------------

    IEnumerator BurstRoutine(int count, Color color)
    {
        LayoutLines(count);
        activeColor = color;
        alpha       = 1f;
        isPlaying   = true;

        for (float t = 0f; t < fadeDuration; t += Time.deltaTime)
        {
            alpha = 1f - (t / fadeDuration);
            yield return null;
        }

        alpha     = 0f;
        isPlaying = false;
        fadeRoutine = null;
    }

    // ---------------------------------------------------------------
    // Posición de las líneas (coordenadas normalizadas 0..1)
    // ---------------------------------------------------------------

    void LayoutLines(int count)
    {
        // Recrear el array si maxLines cambió desde Awake
        if (lineData == null || lineData.Length < maxLines)
            lineData = new LineSegment[maxLines];

        var center = new Vector2(0.5f, 0.5f);

        for (int i = 0; i < maxLines; i++)
        {
            if (i >= count)
            {
                lineData[i].active = false;
                continue;
            }

            float angle = (360f / count) * i + Random.Range(-8f, 8f);
            float rad   = angle * Mathf.Deg2Rad;

            // Dirección radial — se corrige el aspect ratio para que sean
            // realmente radiales en pantalla (no ovaladas)
            float aspect = targetCamera != null
                ? (float)targetCamera.pixelHeight / targetCamera.pixelWidth
                : 1f;

            var dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad) * aspect).normalized;

            float inner = innerRadius * Random.Range(0.85f, 1.15f);
            float outer = outerRadius * Random.Range(0.85f, 1.15f);

            lineData[i].start  = center + dir * inner;
            lineData[i].end    = center + dir * outer;
            lineData[i].active = true;
        }
    }

    // ---------------------------------------------------------------
    // Dibujado GL — solo se ejecuta para la targetCamera
    // ---------------------------------------------------------------

    void OnEndCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam != targetCamera || !isPlaying || alpha <= 0f || glMaterial == null)
            return;

        DrawLines();
    }

    void DrawLines()
    {
        GL.PushMatrix();
        GL.LoadOrtho();

        glMaterial.SetPass(0);
        GL.Begin(GL.QUADS);

        for (int i = 0; i < maxLines; i++)
        {
            if (!lineData[i].active) continue;

            Vector2 s = lineData[i].start;
            Vector2 e = lineData[i].end;

            // Dirección y perpendicular para dar grosor
            Vector2 dir  = (e - s).normalized;
            Vector2 perp = new Vector2(-dir.y, dir.x) * lineWidth * 0.5f;

            Color colorBase = new Color(activeColor.r, activeColor.g, activeColor.b, alpha);
            Color colorTip  = new Color(activeColor.r, activeColor.g, activeColor.b, 0f);

            // Quad: base ancha (opaca) → punta estrecha (transparente)
            // Base izquierda
            GL.Color(colorBase);
            GL.Vertex3(s.x - perp.x, s.y - perp.y, 0f);
            // Base derecha
            GL.Color(colorBase);
            GL.Vertex3(s.x + perp.x, s.y + perp.y, 0f);
            // Punta derecha (más estrecha)
            GL.Color(colorTip);
            GL.Vertex3(e.x + perp.x * 0.15f, e.y + perp.y * 0.15f, 0f);
            // Punta izquierda
            GL.Color(colorTip);
            GL.Vertex3(e.x - perp.x * 0.15f, e.y - perp.y * 0.15f, 0f);
        }

        GL.End();
        GL.PopMatrix();
    }
}
