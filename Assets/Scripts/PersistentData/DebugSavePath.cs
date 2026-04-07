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

        Directory.CreateDirectory(folder);

        try
        {
            if (!File.Exists(file))
            {
                string json = "{\n" +
                              "  \"money\": 0,\n" +
                              "  \"equippedHorseId\": \"Farm_Horse\",\n" +
                              "  \"equippedLanceId\": \"Training_Lance\",\n" +
                              "  \"equippedShieldId\": \"Training_Shield\",\n" +
                              "  \"equippedArmorId\": \"Training_Armor\",\n" +
                              "  \"currentCityId\": \"city_valdoren\",\n" +
                              "  \"currentDay\": 1,\n" +
                              "  \"currentMonth\": 1\n" +
                              "}";

                File.WriteAllText(file, json);
                Debug.Log("Wrote default progress.json successfully.");
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