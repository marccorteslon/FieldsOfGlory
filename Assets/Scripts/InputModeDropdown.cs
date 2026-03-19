using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputModeDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private InputModeManager inputModeManager;

    private void Awake()
    {
        if (dropdown == null)
            dropdown = GetComponent<TMP_Dropdown>();
    }

    private void Start()
    {
        if (dropdown == null)
        {
            Debug.LogError("No hay TMP_Dropdown asignado en InputModeDropdown.");
            return;
        }

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "Keyboard + Mouse", "Gamepad" });

        dropdown.onValueChanged.RemoveAllListeners();

        if (inputModeManager != null)
        {
            dropdown.value = (int)InputModeManager.CurrentInputMode;
            dropdown.onValueChanged.AddListener(inputModeManager.SetInputMode);
        }
        else
        {
            Debug.LogError("No hay InputModeManager asignado en InputModeDropdown.");
        }
    }
}