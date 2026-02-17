using UnityEngine;

//RESUMEN SCRIPT: Este script controla la parte del ataque de la Justa, cuando la variable attackPartIsOn del JoustManager.cs se vuelve true, se empieza a ejecutar esto

public class AttackPart_Joust : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;
    public Canvas canvas;

    [Header("Control")]
    public float mouseSensitivity = 800f;
    public float stickSensitivity = 800f;

    [Header("Manager")]
    public JoustManager joustManager; // Referencia al manager

    private Vector2 currentPosition;
    private bool hasClicked = false;
    private bool previousAttackState = false;

    void Start()
    {
        currentPosition = Vector2.zero;
        UpdateCursorState();
    }

    void Update()
    {
        // Leer directamente del manager
        bool attackStarted = joustManager.attackPartIsOn;

        if (attackStarted != previousAttackState)
        {
            UpdateCursorState();
            previousAttackState = attackStarted;

            if (attackStarted)
            {
                currentPosition = Vector2.zero;
                hasClicked = false;
            }
        }

        if (!attackStarted) return;

        MoveCrosshair();
        CheckRaycast();
    }

    void UpdateCursorState()
    {
        bool attackStarted = joustManager.attackPartIsOn;

        // Activar/desactivar la retícula
        crosshair.gameObject.SetActive(attackStarted);

        if (attackStarted)
        {
            // Ocultar cursor del sistema
            Cursor.visible = false;

            // Bloquearlo al centro (más limpio que Confined)
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // Mostrar cursor cuando no estamos atacando
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }


    void MoveCrosshair()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float stickX = Input.GetAxis("RightStickHorizontal");
        float stickY = Input.GetAxis("RightStickVertical");

        Vector2 input = new Vector2(mouseX, mouseY) * mouseSensitivity +
                        new Vector2(stickX, stickY) * stickSensitivity;

        currentPosition += input * Time.deltaTime;
        ClampToCanvas();
        crosshair.anchoredPosition = currentPosition;
    }

    void ClampToCanvas()
    {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        float halfWidth = canvasRect.rect.width / 2f;
        float halfHeight = canvasRect.rect.height / 2f;

        currentPosition.x = Mathf.Clamp(currentPosition.x, -halfWidth, halfWidth);
        currentPosition.y = Mathf.Clamp(currentPosition.y, -halfHeight, halfHeight);
    }

    void CheckRaycast()
    {
        if (hasClicked) return;

        Ray ray = cam.ScreenPointToRay(crosshair.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool clickPressed = Input.GetMouseButtonDown(0);
            bool r2Pressed = Input.GetButtonDown("Fire1");

            if (clickPressed || r2Pressed)
            {
                hasClicked = true;
                Debug.Log("Has atacado. Próxima fase: defensa.");

                // Notificar al manager para pasar a la siguiente fase
                joustManager.EndAttackPhase();
            }
        }
    }
}
