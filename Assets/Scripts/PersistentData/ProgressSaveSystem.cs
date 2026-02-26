using System.IO;
using UnityEngine;

public static class ProgressSaveSystem
{
    private const string FileName = "progress.json";

    private static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    public static void Save(ProgressSaveData data)
    {
        string path = GetPath();
        string json = JsonUtility.ToJson(data, true);

        string tmp = path + ".tmp";
        File.WriteAllText(tmp, json);

        if (File.Exists(path)) File.Delete(path);
        File.Move(tmp, path);

        Debug.Log($"[Save] Guardado en: {path}\n{json}");
    }

    public static ProgressSaveData Load()
    {
        string path = GetPath();

        if (!File.Exists(path))
        {
            Debug.Log("[Save] No existe progress.json, creando nuevo.");
            return new ProgressSaveData();
        }

        string json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<ProgressSaveData>(json);

        Debug.Log($"[Save] Cargado desde: {path}\n{json}");
        return data;
    }
}