using UnityEngine;

public class PhysicalShieldController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El objeto vacío o transform base del escudo.")]
    public Transform shieldPivot; 

    [Header("Control del Escudo")]
    public float moveSpeed = 10f;
    [Tooltip("Límites de cuánto puedes mover el escudo por la pantalla (X, Y)")]
    public Vector2 maxShieldMovement = new Vector2(0.5f, 0.4f); 

    private Vector2 currentShieldPos;

    void Update()
    {
        if (shieldPivot == null) return;

        HandleInput();
        ApplyMovement();
    }

    void HandleInput()
    {
        // Stick Izquierdo o Teclas WASD
        float inputX = Input.GetAxis("Horizontal") + Input.GetAxis("LeftStickHorizontal");
        float inputY = Input.GetAxis("Vertical") + Input.GetAxis("LeftStickVertical");

        // Limitamos input para no pasarnos de velocidad diagonal
        Vector2 inputDir = new Vector2(inputX, inputY);
        if (inputDir.magnitude > 1f) inputDir.Normalize();

        Vector2 targetPos = new Vector2(inputDir.x * maxShieldMovement.x, inputDir.y * maxShieldMovement.y);

        // Suavizado del movimiento
        currentShieldPos = Vector2.Lerp(currentShieldPos, targetPos, Time.deltaTime * moveSpeed);
    }

    void ApplyMovement()
    {
        // Mover el escudo en su espacio local
        shieldPivot.localPosition = new Vector3(currentShieldPos.x, currentShieldPos.y, shieldPivot.localPosition.z);
        
        // Efecto visual: inclinar levemente el escudo al moverlo
        float tiltX = -currentShieldPos.y * 20f; 
        float tiltY = currentShieldPos.x * 20f;
        shieldPivot.localRotation = Quaternion.Euler(tiltX, tiltY, 0f);
    }
}
