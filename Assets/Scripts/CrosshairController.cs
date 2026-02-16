using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;

    [Header("Control")]
    public float mouseSensitivity = 800f;
    public float stickSensitivity = 800f;

    [Header("State")]
    public bool attackStarted = false;

    private Vector2 currentPosition;
    private bool isHovering;

    void Start()
    {
        currentPosition = Vector2.zero;
        crosshair.gameObject.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined; // Evitar que salga de la ventana
    }

    void Update()
    {
        if (!attackStarted)
        {
            crosshair.gameObject.SetActive(false);
            return;
        }

        crosshair.gameObject.SetActive(true);

        MoveCrosshair();
        CheckRaycast();
    }

    void MoveCrosshair()
    {
        // Input ratón
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Input mando (Right Stick)
        float stickX = Input.GetAxis("RightStickHorizontal");
        float stickY = Input.GetAxis("RightStickVertical");

        Vector2 input = Vector2.zero;

        input.x += mouseX * mouseSensitivity;
        input.y += mouseY * mouseSensitivity;

        input.x += stickX * stickSensitivity;
        input.y += stickY * stickSensitivity;

        currentPosition += input * Time.deltaTime;

        ClampToScreen();

        crosshair.anchoredPosition = currentPosition;
    }

    void ClampToScreen()
    {
        float halfWidth = Screen.width / 2f;
        float halfHeight = Screen.height / 2f;

        currentPosition.x = Mathf.Clamp(currentPosition.x, -halfWidth, halfWidth);
        currentPosition.y = Mathf.Clamp(currentPosition.y, -halfHeight, halfHeight);
    }


    void CheckRaycast()
    {
        Vector3 screenPosition =
            RectTransformUtility.WorldToScreenPoint(null, crosshair.position);

        Ray ray = cam.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool clickPressed = Input.GetMouseButtonDown(0);
            bool r2Pressed = Input.GetButtonDown("Fire1");

            if (clickPressed || r2Pressed)
            {
                if (hit.collider.CompareTag("Objective1"))
                {
                    Debug.Log("Has clicado el Objective 1");
                }
                else if (hit.collider.CompareTag("Objective2"))
                {
                    Debug.Log("Has clicado el Objective 2");
                }
            }
        }
    }

}
