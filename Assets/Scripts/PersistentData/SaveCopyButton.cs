using UnityEngine;
using TMPro;

public class SaveCopyButton : MonoBehaviour
{
    [SerializeField] private TMP_InputField saveNameInput;

    public void CopyCurrentSave()
    {
        if (saveNameInput == null)
        {
            Debug.LogWarning("[Save] No hay TMP_InputField asignado.");
            return;
        }

        bool ok = ProgressSaveSystem.CopyCurrentSaveToFolder(saveNameInput.text);

        if (ok)
            Debug.Log("[Save] Partida copiada correctamente.");
    }
}