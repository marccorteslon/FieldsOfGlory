using UnityEngine;
using UnityEngine.InputSystem; // Si usas el nuevo Input System

public class GamepadCursor : MonoBehaviour
{
    public RectTransform cursor;
    public float speed = 1000f;

    private Vector2 position;

    void Start()
    {
        position = cursor.anchoredPosition;
        Cursor.visible = false; // Oculta el cursor real
    }

    void Update()
    {
        Vector2 input = Gamepad.current.rightStick.ReadValue();

        position += input * speed * Time.deltaTime;

        // Limitar dentro de la pantalla
        position.x = Mathf.Clamp(position.x, 0, Screen.width);
        position.y = Mathf.Clamp(position.y, 0, Screen.height);

        cursor.anchoredPosition = position;
    }
}