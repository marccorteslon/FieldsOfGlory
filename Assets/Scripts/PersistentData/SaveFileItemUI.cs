using System.IO;
using TMPro;
using UnityEngine;

public class SaveFileItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text saveNameText;
    private string filePath;
    private SaveFilesListUI listUI;

    public void Setup(string newFilePath, SaveFilesListUI newListUI)
    {
        filePath = newFilePath;
        listUI = newListUI;

        if (saveNameText != null)
            saveNameText.text = Path.GetFileNameWithoutExtension(filePath);
    }

    public void LoadSave()
    {
        bool ok = ProgressSaveSystem.LoadSaveFileAsCurrent(filePath);

        if (ok)
        {
            Debug.Log("[Save] Partida cargada como progress.json.");
        }
    }

    public void DeleteSave()
    {
        bool ok = ProgressSaveSystem.DeleteSaveFile(filePath);

        if (ok && listUI != null)
        {
            listUI.RefreshList();
        }
    }
}