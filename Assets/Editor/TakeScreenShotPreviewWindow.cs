#pragma once
using UnityEditor;
using UnityEngine;
using System.IO;

public class TakeScreenShotPreviewWindow : EditorWindow
{
    private GameObject _prefab;
    private GameObject _instance;

    private PreviewRenderUtility _preview;

    private Bounds _bounds;
    private Vector3 _modelCenter;

    private Quaternion _baseRotation = Quaternion.Euler(0f, 180f, 0f);

    private Vector2 _targetRotation;
    private Vector2 _currentRotation;
    private Vector2 _rotationVelocity;

    private Vector2 _pan;

    private float _targetZoom;
    private float _currentZoom;
    private float _zoomVelocity;

    private const float MinZoom = 0.01f;
    private float _maxZoom;

    private const int IMAGE_SIZE = 512;

    // =========================
    // MENU
    // =========================

    [MenuItem("Assets/TakeScreenShotPreview", true)]
    private static bool Validate()
    {
        return Selection.activeObject is GameObject;
    }

    [MenuItem("Assets/TakeScreenShotPreview")]
    private static void Open()
    {
        var window = GetWindow<TakeScreenShotPreviewWindow>("Preview Generator");
        window.Init(Selection.activeObject as GameObject);
    }

    // =========================
    // INIT
    // =========================

    private void Init(GameObject prefab)
    {
        _prefab = prefab;

        Cleanup();

        _preview = new PreviewRenderUtility();
        _preview.cameraFieldOfView = 30f;

        _instance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
        _instance.transform.position = Vector3.zero;

        _preview.AddSingleGO(_instance);

        _bounds = CalculateBounds(_instance);
        _modelCenter = _bounds.center;

        _maxZoom = CalculateInitialZoom(_preview.cameraFieldOfView, _bounds);
        _targetZoom = _maxZoom;
        _maxZoom *= 5f;
        _currentZoom = _targetZoom;
    }

    // =========================
    // GUI
    // =========================

    private void OnGUI()
    {
        if (_preview == null || _instance == null)
            return;

        Rect rect = GUILayoutUtility.GetRect(10, 1000, 10, 1000);

        HandleInput(rect);
        DrawPreview(rect);

        GUILayout.Space(10);
        
        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Space)
        {
            SavePreview();
            Event.current.Use();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("SavePreview", GUILayout.Height(40)))
        {
            SavePreview();
        }
    }

    // =========================
    // INPUT
    // =========================

    private void HandleInput(Rect rect)
    {
        Event e = Event.current;

        if (!rect.Contains(e.mousePosition))
            return;

        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            _targetRotation += e.delta;
            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            Camera cam = _preview.camera;

            float distance = _targetZoom;
            float vSize = 2f * distance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float hSize = vSize * (rect.width / rect.height);

            Vector2 panDelta = new Vector2(
                -e.delta.x / rect.width * hSize,
                e.delta.y / rect.height * vSize
            );

            _pan += panDelta;
            e.Use();
        }

        if (e.type == EventType.ScrollWheel)
        {
            _targetZoom += e.delta.y * 0.3f; // 🔥 más agresivo
            _targetZoom = Mathf.Clamp(_targetZoom, MinZoom, _maxZoom);
            e.Use();
        }
    }

    // =========================
    // DRAW
    // =========================

    private void DrawPreview(Rect rect)
    {
        float dt = 1f / 60f;

        _currentRotation = Vector2.SmoothDamp(_currentRotation, _targetRotation, ref _rotationVelocity, 0.25f);
        _currentZoom = Mathf.SmoothDamp(_currentZoom, _targetZoom, ref _zoomVelocity, 0.25f);

        _preview.BeginPreview(rect, GUIStyle.none);

        Camera cam = _preview.camera;

        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(0, 0, 0, 0);
        cam.nearClipPlane = 0.01f;
        cam.farClipPlane = 1000f;

        Quaternion rotation = _baseRotation * Quaternion.Euler(_currentRotation.y, _currentRotation.x, 0f);

        Vector3 panOffset = rotation * new Vector3(_pan.x, _pan.y, 0f);
        Vector3 target = _modelCenter + panOffset;

        Vector3 camOffset = rotation * new Vector3(0, 0, -_currentZoom);

        cam.transform.position = target + camOffset;
        cam.transform.rotation = rotation;

        SetupLighting();

        cam.Render();

        Texture tex = _preview.EndPreview();
        GUI.DrawTexture(rect, tex, ScaleMode.StretchToFill, true);

        if (NeedsRepaint())
            Repaint();
    }

    private bool NeedsRepaint()
    {
        return (_currentRotation - _targetRotation).sqrMagnitude > 0.0001f
            || !Mathf.Approximately(_currentZoom, _targetZoom);
    }

    // =========================
    // SAVE (FIX REAL ALPHA)
    // =========================

    private void SavePreview()
    {
        Camera cam = _preview.camera;

        RenderTexture rt = new RenderTexture(IMAGE_SIZE, IMAGE_SIZE, 24, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        cam.clearFlags = CameraClearFlags.Color;
        cam.backgroundColor = new Color(0, 0, 0, 0);

        GL.Clear(true, true, new Color(0, 0, 0, 0));

        cam.Render();

        Texture2D tex = new Texture2D(IMAGE_SIZE, IMAGE_SIZE, TextureFormat.ARGB32, false);
        tex.ReadPixels(new Rect(0, 0, IMAGE_SIZE, IMAGE_SIZE), 0, 0);
        tex.Apply();

        cam.targetTexture = null;
        RenderTexture.active = prev;
        DestroyImmediate(rt);

        byte[] png = tex.EncodeToPNG();

        string prefabPath = AssetDatabase.GetAssetPath(_prefab);
        string dir = Path.GetDirectoryName(prefabPath);
        string name = Path.GetFileNameWithoutExtension(prefabPath);

        string path = Path.Combine(dir, name + "_preview.png");

        File.WriteAllBytes(path, png);
        AssetDatabase.Refresh();

        TextureImporter importer = (TextureImporter)TextureImporter.GetAtPath(path);

        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;

            importer.SaveAndReimport();
        }

        Debug.Log("Saved preview with REAL transparency: " + path);
    }

    // =========================
    // UTILS
    // =========================

    private Bounds CalculateBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one);

        Bounds b = renderers[0].bounds;

        foreach (var r in renderers)
            b.Encapsulate(r.bounds);

        return b;
    }

    private float CalculateInitialZoom(float fov, Bounds bounds)
    {
        float radius = bounds.extents.magnitude;
        float halfFov = fov * 0.5f * Mathf.Deg2Rad;

        return radius / Mathf.Sin(halfFov);
    }

    private void SetupLighting()
    {
        _preview.lights[0].intensity = 1.4f;
        _preview.lights[0].transform.rotation = Quaternion.Euler(40f, 40f, 0);

        _preview.lights[1].intensity = 1.4f;

        _preview.ambientColor = Color.gray;
    }

    private void OnDisable()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        if (_instance != null)
            DestroyImmediate(_instance);

        if (_preview != null)
        {
            _preview.Cleanup();
            _preview = null;
        }
    }
}