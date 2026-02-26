using UnityEngine;
using System.IO;

public class DebugSavePath : MonoBehaviour
{
    void Start()
    {
        string folder = Application.persistentDataPath;
        string file = Path.Combine(folder, "progress.json");

        Debug.Log("===== SAVE PATH CHECK =====");
        Debug.Log("Folder: " + folder);
        Debug.Log("File: " + file);

        Debug.Log("Folder exists? " + Directory.Exists(folder));
        Debug.Log("File exists? " + File.Exists(file));

        // Asegura que la carpeta existe
        Directory.CreateDirectory(folder);
        Debug.Log("After CreateDirectory - Folder exists? " + Directory.Exists(folder));

        // Fuerza un write de prueba (para comprobar permisos y que aparece)
        try
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, "{\n  \"money\": 0\n}");
                Debug.Log("Wrote test progress.json successfully.");
            }
            else
            {
                Debug.Log("progress.json ya existía, no lo sobrescribo.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("ERROR writing progress.json: " + e);
        }

        Debug.Log("===========================");
    }
}