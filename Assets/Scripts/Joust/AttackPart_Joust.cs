using UnityEngine;

//RESUMEN SCRIPT: Este script controla la parte del ataque de la Justa, cuando la variable attackPartIsOn del JoustManager.cs se vuelve true, se empieza a ejecutar esto

public class AttackPart_Joust : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;
    public Canvas canvas;

    [Header("Shake Settings")]
    public float shakeAmount = 20f;
    public float shakeSpeed = 25f;
    public bool enableShake = true;

    [Header("Manager")]
    public JoustManager joustManager;

    private bool hasClicked = false;
    private bool previousAttackState = false;
    private float shakeTime;

    void Start()
    {
        UpdateCursorState();
    }

    void Update()
    {
        bool attackStarted = joustManager.attackPartIsOn;

        if (attackStarted != previousAttackState)
        {
            UpdateCursorState();
            previousAttackState = attackStarted;

            if (attackStarted)
            {
                hasClicked = false;
                shakeTime = Random.Range(0f, 100f);
            }
        }

        if (!attackStarted) return;

        UpdateCrosshairPosition();
        CheckRaycast();
    }

    void UpdateCursorState()
    {
        bool attackStarted = joustManager.attackPartIsOn;

        crosshair.gameObject.SetActive(attackStarted);

        if (attackStarted)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    void UpdateCrosshairPosition()
    {
        Vector2 mouseScreenPos = Input.mousePosition;

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mouseScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out localPoint
        );

        Vector2 finalPosition = localPoint;

        if (enableShake)
        {
            shakeTime += Time.deltaTime * shakeSpeed;

            float offsetX = Mathf.PerlinNoise(shakeTime, 0f) - 0.5f;
            float offsetY = Mathf.PerlinNoise(0f, shakeTime) - 0.5f;

            Vector2 shakeOffset = new Vector2(offsetX, offsetY) * shakeAmount;
            finalPosition += shakeOffset;
        }

        crosshair.anchoredPosition = finalPosition;
    }

    void CheckRaycast()
    {
        if (hasClicked) return;

        // Convertimos la posición del crosshair a pantalla
        Vector2 crossScreenPos = RectTransformUtility.WorldToScreenPoint(cam, crosshair.position);
        Ray ray = cam.ScreenPointToRay(crossScreenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            bool clickPressed = Input.GetMouseButtonDown(0);
            bool r2Pressed = Input.GetButtonDown("Fire1");

            if (clickPressed || r2Pressed)
            {
                hasClicked = true;

                string hitTag = hit.collider.tag;

                if (hitTag == "Head" || hitTag == "Body" || hitTag == "Shield" || hitTag == "Horse")
                {
                    Debug.Log($"Has dado a {hitTag}");
                }
                else
                {
                    Debug.Log("Has fallado");
                }

                joustManager.EndAttackPhase();
            }
        }
        else
        {
            // Si clicas pero no hay ningún collider en el ray
            if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
            {
                hasClicked = true;
                Debug.Log("Has fallado");
                joustManager.EndAttackPhase();
            }
        }
    }
}