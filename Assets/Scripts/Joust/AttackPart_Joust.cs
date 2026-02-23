using UnityEngine;
using UnityEngine.UI;

//RESUMEN SCRIPT: Este script controla la parte del ataque de la Justa, cuando la variable attackPartIsOn del JoustManager.cs se vuelve true, se empieza a ejecutar esto

public class AttackPart_Joust : MonoBehaviour
{
    [Header("References")]
    public RectTransform crosshair;
    public Camera cam;
    public Canvas canvas;
    public Slider powerSlider;

    [Header("Settings")]
    public float maxChargeTime = 2f;
    public float baseShakeAmount = 200f;
    public float shakeSpeed = 25f;
    public bool enableShake = true;

    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Lanza Stats")]
    public int BF = 4; // Bonificador fuerza
    public int BL = 2; // Bonificador localización

    private bool previousAttackState = false;
    private bool isCharging = false;
    private float chargeTimer = 0f;
    private float currentShakeAmount;
    private float shakeTime;

    void Start()
    {
        powerSlider.gameObject.SetActive(false);
        powerSlider.minValue = 0;
        powerSlider.maxValue = 100;
        crosshair.gameObject.SetActive(false);
    }

    void Update()
    {
        bool attackStarted = joustManager.attackPartIsOn;
        if (attackStarted != previousAttackState)
        {
            crosshair.gameObject.SetActive(attackStarted);
            previousAttackState = attackStarted;

            if (attackStarted)
            {
                ResetCharge();
                shakeTime = Random.Range(0f, 100f);
            }
        }

        if (!attackStarted) return;

        UpdateCrosshair();
        HandleChargeInput();
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
            powerSlider.value = percent * 100f;
            currentShakeAmount = baseShakeAmount + (baseShakeAmount * percent);
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
        currentShakeAmount = baseShakeAmount;
    }

    void UpdateCrosshair()
    {
        Vector2 mouseScreenPos = Input.mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            mouseScreenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cam,
            out Vector2 localPoint
        );

        Vector2 finalPosition = localPoint;

        if (enableShake)
        {
            shakeTime += Time.deltaTime * shakeSpeed;
            float offsetX = Mathf.PerlinNoise(shakeTime, 0f) - 0.5f;
            float offsetY = Mathf.PerlinNoise(0f, shakeTime) - 0.5f;
            finalPosition += new Vector2(offsetX, offsetY) * currentShakeAmount;
        }

        crosshair.anchoredPosition = finalPosition;
    }

    void PerformAttack()
    {
        Ray ray = cam.ScreenPointToRay(crosshair.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
        {
            float chargePercent = Mathf.Clamp01(chargeTimer / maxChargeTime) * 100f;
            scoreManager.AddAttackScore(hit.collider.tag, BF, BL, chargePercent, 0, 0);
            Debug.Log($"Golpe con tag {hit.collider.tag} aplicado.");
        }
        else
        {
            Debug.Log("Ataque fallido.");
        }

        joustManager.EndAttackPhase();
    }
}