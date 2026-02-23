using UnityEngine;
using UnityEngine.UI;

//RESUMEN SCRIPT: Este script controla la parte del ataque de la Justa, cuando la variable attackPartIsOn del JoustManager.cs se vuelve true, se empieza a ejecutar esto

public class AttackPart_Joust : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;
    public Canvas canvas;

    [Header("UI Power")]
    public Slider powerSlider;          // Slider 0-100
    public float maxChargeTime = 2f;    // Tiempo para llegar a 100

    [Header("Shake Settings")]
    public float baseShakeAmount = 200f;
    public float shakeSpeed = 25f;
    public bool enableShake = true;

    [Header("Manager")]
    public JoustManager joustManager;

    private bool previousAttackState = false;
    private float shakeTime;

    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float currentShakeAmount;

    void Start()
    {
        UpdateCursorState();
        powerSlider.gameObject.SetActive(false);
        powerSlider.minValue = 0;
        powerSlider.maxValue = 100;
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
                ResetCharge();
                shakeTime = Random.Range(0f, 100f);
            }
        }

        if (!attackStarted) return;

        UpdateCrosshairPosition();
        HandleChargeInput();
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

    void HandleChargeInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetButtonDown("Fire1"))
        {
            isCharging = true;
            chargeTimer = 0f;
            powerSlider.gameObject.SetActive(true);
        }

        if (isCharging)
        {
            chargeTimer += Time.deltaTime;

            float percent = Mathf.Clamp01(chargeTimer / maxChargeTime);
            float sliderValue = percent * 100f;

            powerSlider.value = sliderValue;

            currentShakeAmount = baseShakeAmount +
                (baseShakeAmount * (sliderValue / 100f));
        }

        if ((Input.GetMouseButtonUp(0) || Input.GetButtonUp("Fire1")) && isCharging)
        {
            isCharging = false;
            powerSlider.gameObject.SetActive(false);

            PerformAttack();
        }
    }

    void ResetCharge()
    {
        isCharging = false;
        chargeTimer = 0f;
        powerSlider.value = 0;
        powerSlider.gameObject.SetActive(false);
        currentShakeAmount = baseShakeAmount;
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

            Vector2 shakeOffset = new Vector2(offsetX, offsetY) * currentShakeAmount;
            finalPosition += shakeOffset;
        }

        crosshair.anchoredPosition = finalPosition;
    }

    void PerformAttack()
    {
        Vector3 screenPoint = crosshair.position;
        Ray ray = cam.ScreenPointToRay(screenPoint);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            Debug.Log($"Impacto con fuerza: {currentShakeAmount} en {hit.collider.name}");
        }
        else
        {
            Debug.Log($"Ataque fallido con fuerza: {currentShakeAmount}");
        }

        joustManager.EndAttackPhase();
    }
}