using UnityEngine;

public class SaveFilesListUI : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject saveFileItemPrefab;

    void OnEnable()
    {
        RefreshList();
    }

    public void RefreshList()
    {
        if (contentParent == null || saveFileItemPrefab == null)
        {
            Debug.LogWarning("[Save] Falta asignar contentParent o saveFileItemPrefab.");
            return;
        }

        for (int i = contentParent.childCount - 1; i >= 0; i--)
        {
            Destroy(contentParent.GetChild(i).gameObject);
        }

        string[] files = ProgressSaveSystem.GetSavedFiles();

        for (int i = 0; i < files.Length; i++)
        {
            GameObject item = Instantiate(saveFileItemPrefab, contentParent);
            SaveFileItemUI itemUI = item.GetComponent<SaveFileItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(files[i], this);
            }
            else
            {
                Debug.LogWarning("[Save] El prefab no tiene SaveFileItemUI.");
            }
        }
    }
}