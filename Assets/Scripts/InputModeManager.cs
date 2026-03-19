using UnityEngine;

public class InputModeManager : MonoBehaviour
{
    public static InputMode CurrentInputMode { get; private set; } = InputMode.KeyboardMouse;

    [SerializeField] private InputMode defaultInputMode = InputMode.KeyboardMouse;

    private void Awake()
    {
        CurrentInputMode = defaultInputMode;
        Debug.Log("Input mode inicial: " + CurrentInputMode);
    }

    public void SetInputMode(int index)
    {
        CurrentInputMode = (InputMode)index;
        Debug.Log("Input mode cambiado a: " + CurrentInputMode);
    }

    public static bool IsKeyboardMouse()
    {
        return CurrentInputMode == InputMode.KeyboardMouse;
    }

    public static bool IsGamepad()
    {
        return CurrentInputMode == InputMode.Gamepad;
    }
}