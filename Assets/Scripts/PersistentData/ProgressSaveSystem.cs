using System.IO;
using UnityEngine;

public static class ProgressSaveSystem
{
    private const string FileName = "progress.json";
    private const string SavesFolderName = "SaveGames";

    private static string GetPath()
    {
        return Path.Combine(Application.persistentDataPath, FileName);
    }

    public static string GetSavesFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, SavesFolderName);
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

    public static bool CopyCurrentSaveToFolder(string saveFileName)
    {
        string sourcePath = GetPath();

        if (!File.Exists(sourcePath))
        {
            Debug.LogWarning("[Save] No existe el archivo principal para copiar.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(saveFileName))
        {
            Debug.LogWarning("[Save] El nombre de la partida está vacío.");
            return false;
        }

        foreach (char c in Path.GetInvalidFileNameChars())
        {
            saveFileName = saveFileName.Replace(c.ToString(), "");
        }

        saveFileName = saveFileName.Trim();

        if (string.IsNullOrWhiteSpace(saveFileName))
        {
            Debug.LogWarning("[Save] El nombre de la partida no es válido.");
            return false;
        }

        string savesFolder = GetSavesFolderPath();
        Directory.CreateDirectory(savesFolder);

        string destinationPath = Path.Combine(savesFolder, saveFileName + ".json");
        File.Copy(sourcePath, destinationPath, true);

        Debug.Log($"[Save] Copia creada en: {destinationPath}");
        return true;
    }

    public static string[] GetSavedFiles()
    {
        string savesFolder = GetSavesFolderPath();

        if (!Directory.Exists(savesFolder))
            Directory.CreateDirectory(savesFolder);

        return Directory.GetFiles(savesFolder, "*.json");
    }

    public static bool LoadSaveFileAsCurrent(string sourceFilePath)
    {
        if (string.IsNullOrWhiteSpace(sourceFilePath))
        {
            Debug.LogWarning("[Save] Ruta de archivo vacía.");
            return false;
        }

        if (!File.Exists(sourceFilePath))
        {
            Debug.LogWarning("[Save] El archivo a cargar no existe: " + sourceFilePath);
            return false;
        }

        string destinationPath = GetPath();
        File.Copy(sourceFilePath, destinationPath, true);

        Debug.Log($"[Save] Archivo cargado como progress.json: {sourceFilePath}");
        return true;
    }

    public static bool DeleteSaveFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            Debug.LogWarning("[Save] Ruta de borrado vacía.");
            return false;
        }

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("[Save] El archivo a borrar no existe: " + filePath);
            return false;
        }

        File.Delete(filePath);
        Debug.Log($"[Save] Archivo borrado: {filePath}");
        return true;
    }
}