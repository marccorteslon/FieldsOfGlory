using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class DisparoGameplayManager : MonoBehaviour
{
    public static DisparoGameplayManager Instance;

    [Header("Referencias del Jugador")]
    public Transform player;
    public Animator playerHorseAnimator;
    public string horseSpeedParameter = "Speed";
    public float preJoustRunAnimSpeed = 1f;
    public float preJoustIdleAnimSpeed = 0f;

    [Header("Configuración del Recorrido")]
    public float startZ = 0f;
    public float endZ = 200f; // Longitud de la pista
    public float baseGallopSpeed = 12f;
    public float sprintSpeed = 18f;
    public float decelerationSpeed = 4f;

    [Header("Sistema de Puntuación")]
    public int requiredScore = 80;
    [HideInInspector] public int totalScore = 0;
    private int targetsHitCount = 0;

    [Header("Referencias UI HUD")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI countdownText;
    public TextMeshProUGUI controlsText;
    public Slider progressBar;

    [Header("Referencias UI Final")]
    public JoustStatsPanelController statsPanelController;

    [Header("Managers y Componentes")]
    public ProgressManager progressManager;
    public CrossbowController crossbowController;

    [Header("Mapas y Entornos")]
    public List<CityMapMapping> cityMaps = new();
    public GameObject defaultMap;

    [Header("Estado de Partida")]
    public bool isGameplayActive = false;
    public bool isGameEnded = false;

    private float currentSpeed = 0f;
    private Vector3 initialPlayerPosition;
    private Quaternion initialPlayerRotation;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Guardar posiciones iniciales para posibles reinicios
        if (player != null)
        {
            initialPlayerPosition = player.position;
            initialPlayerRotation = player.rotation;
        }

        if (progressManager == null)
            progressManager = FindFirstObjectByType<ProgressManager>();

        if (crossbowController == null)
            crossbowController = FindFirstObjectByType<CrossbowController>();

        if (statsPanelController == null)
            statsPanelController = FindFirstObjectByType<JoustStatsPanelController>();

        // Activar el mapa correspondiente a la ciudad
        if (progressManager != null)
        {
            string activeCityId = progressManager.CurrentCityId;
            if (!string.IsNullOrEmpty(activeCityId))
            {
                bool mapFound = false;
                foreach (var mapping in cityMaps)
                {
                    if (mapping.mapGameObject != null)
                    {
                        bool isCurrentCityMap = string.Equals(mapping.cityId, activeCityId, System.StringComparison.OrdinalIgnoreCase);
                        mapping.mapGameObject.SetActive(isCurrentCityMap);
                        if (isCurrentCityMap)
                        {
                            mapFound = true;
                        }
                    }
                }
                
                if (defaultMap != null)
                {
                    defaultMap.SetActive(!mapFound);
                }
            }
            else
            {
                if (defaultMap != null) defaultMap.SetActive(true);
                foreach (var mapping in cityMaps)
                {
                    if (mapping.mapGameObject != null) mapping.mapGameObject.SetActive(false);
                }
            }
        }

        // Desactivar panel final si estuviera activo
        if (statsPanelController != null && statsPanelController.panelObject != null)
            statsPanelController.panelObject.SetActive(false);

        // Iniciar flujo del juego
        StartCoroutine(GameplaySequence());
    }

    IEnumerator GameplaySequence()
    {
        // Retrasar 1 frame para permitir que TextCore de Unity 6 inicialice fuentes de forma segura en el Main Thread
        yield return null;

        // 1. Preparar HUD y estados
        isGameplayActive = false;
        isGameEnded = false;
        totalScore = 0;
        targetsHitCount = 0;
        currentSpeed = 0f;

        if (progressBar != null)
        {
            progressBar.minValue = startZ;
            progressBar.maxValue = endZ;
            progressBar.value = startZ;
        }

        UpdateScoreUI();
        if (crossbowController != null)
        {
            crossbowController.ResetAmmo();
            UpdateAmmoUI(crossbowController.remainingBolts);
        }

        if (playerHorseAnimator != null)
            playerHorseAnimator.SetFloat(horseSpeedParameter, preJoustIdleAnimSpeed);

        // 2. Mostrar Controles en pantalla
        if (controlsText != null)
        {
            controlsText.text = "APUNTAR: Ratón / Mando Stick Derecho\n" +
                                "DISPARAR: Click Izquierdo / R2\n" +
                                "SPRINT: Mantener Shift Izquierdo / Botón A";
        }

        // 3. Cuenta atrás inicial
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null) countdownText.text = i.ToString();
            yield return new WaitForSeconds(1f);
        }

        if (countdownText != null)
        {
            countdownText.text = "¡FUEGO!";
            yield return new WaitForSeconds(1f);
            countdownText.gameObject.SetActive(false);
        }

        // 4. Iniciar Gameplay
        isGameplayActive = true;
        
        if (playerHorseAnimator != null)
            playerHorseAnimator.SetFloat(horseSpeedParameter, preJoustRunAnimSpeed);
    }

    void Update()
    {
        if (!isGameplayActive)
        {
            if (isGameEnded)
            {
                // Frenado progresivo al terminar la pista
                currentSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * decelerationSpeed);
                MovePlayer(currentSpeed);

                // Reducir la velocidad de la animación progresivamente hacia 0
                if (playerHorseAnimator != null)
                {
                    float currentAnim = playerHorseAnimator.GetFloat(horseSpeedParameter);
                    float blendedAnim = Mathf.Lerp(currentAnim, preJoustIdleAnimSpeed, Time.deltaTime * decelerationSpeed);
                    playerHorseAnimator.SetFloat(horseSpeedParameter, blendedAnim);
                }
            }
            return;
        }

        // Determinar velocidad según sprint
        bool holdingSprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.JoystickButton0);
        float targetSpeed = holdingSprint ? sprintSpeed : baseGallopSpeed;

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 3f);
        MovePlayer(currentSpeed);

        // Actualizar animación del caballo dinámicamente: 2f para galope en sprint, 1f para trote normal
        if (playerHorseAnimator != null)
        {
            float targetAnimVal = holdingSprint ? 2f : 1f;
            float currentAnimVal = playerHorseAnimator.GetFloat(horseSpeedParameter);
            float blendedAnimVal = Mathf.Lerp(currentAnimVal, targetAnimVal, Time.deltaTime * 5f);
            playerHorseAnimator.SetFloat(horseSpeedParameter, blendedAnimVal);
        }

        // Actualizar barra de progreso
        if (player != null && progressBar != null)
        {
            progressBar.value = Mathf.Clamp(player.position.z, startZ, endZ);

            // Llegar al final de la pista
            if (player.position.z >= endZ)
            {
                EndGameplay();
            }
        }
    }

    void MovePlayer(float speed)
    {
        if (player != null)
        {
            player.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    public void AddScore(int points, ShootingTarget.TargetType targetType)
    {
        if (!isGameplayActive) return;

        totalScore += points;
        targetsHitCount++;
        UpdateScoreUI();

        // Modificar color de la retícula temporalmente para dar feedback positivo
        if (crossbowController != null && crossbowController.crosshairImage != null)
        {
            StartCoroutine(FlashCrosshairColor());
        }
    }

    IEnumerator FlashCrosshairColor()
    {
        if (crossbowController != null && crossbowController.crosshairImage != null)
        {
            crossbowController.crosshairImage.color = crossbowController.crosshairHitColor;
            yield return new WaitForSeconds(0.15f);
            crossbowController.crosshairImage.color = crossbowController.crosshairNormalColor;
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Puntos: {totalScore} / Objetivo: {requiredScore}";
        }
    }

    public void UpdateAmmoUI(int ammo)
    {
        if (ammoText != null)
        {
            ammoText.text = $"Virotes: {ammo}";
        }
    }

    void EndGameplay()
    {
        isGameplayActive = false;
        isGameEnded = true;

        if (playerHorseAnimator != null)
            playerHorseAnimator.SetFloat(horseSpeedParameter, preJoustIdleAnimSpeed);

        // Liberar cursor
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Evaluar resultado
        bool won = totalScore >= requiredScore;

        int goldEarned = 0;
        if (won && progressManager != null)
        {
            // Recompensa calculada sobre la puntuación del jugador
            goldEarned = progressManager.CalculateReward(totalScore, 1);
            progressManager.AddMoney(goldEarned);
            Debug.Log($"[Disparo] ¡Ronda ganada! Sumadas {goldEarned} monedas.");
        }

        // Configurar u obtener un ScoreManager local para poblar el panel final
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            scoreManager = gameObject.AddComponent<ScoreManager>();
        }
        scoreManager.ResetScore();
        scoreManager.horsePhaseScore = targetsHitCount;
        scoreManager.attackPhaseScore = totalScore;
        scoreManager.defensePhaseScore = 0;
        scoreManager.totalScore = totalScore;

        // Personalizar etiquetas de la interfaz de resultados para el modo disparo
        if (statsPanelController != null)
        {
            var labels = statsPanelController.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var labelText in labels)
            {
                if (labelText.text == "Fase Caballo:") labelText.text = "Dianas Acertadas:";
                if (labelText.text == "Fase Ataque:") labelText.text = "Puntos de Dianas:";
                if (labelText.text == "Fase Defensa:") labelText.text = "Penalizaciones:";
            }
        }

        // Mostrar Panel de Estadísticas final
        if (statsPanelController != null)
        {
            statsPanelController.PopulateAndShow(won, goldEarned, "");
            
            // Re-vincular el botón del panel final para permitir reiniciar la escena o salir al mapa
            if (statsPanelController.finishButton != null)
            {
                statsPanelController.finishButton.onClick.RemoveAllListeners();
                if (won)
                {
                    statsPanelController.finishButton.onClick.AddListener(() => SceneManager.LoadScene(statsPanelController.nextSceneName));
                }
                else
                {
                    // Si pierde, el botón actúa como reintento de la escena de disparo
                    statsPanelController.finishButton.onClick.AddListener(ResetGame);
                }
            }
        }
    }

    // Reiniciar juego completo (para reintentar tras derrota)
    public void ResetGame()
    {
        // Ocultar panel final
        if (statsPanelController != null && statsPanelController.panelObject != null)
            statsPanelController.panelObject.SetActive(false);

        // Resetear posición de jugador
        if (player != null)
        {
            player.position = initialPlayerPosition;
            player.rotation = initialPlayerRotation;
        }

        // Resetear objetivos
        ShootingTarget[] targets = FindObjectsByType<ShootingTarget>(FindObjectsSortMode.None);
        foreach (var t in targets)
        {
            t.ResetTarget();
        }

        // Reiniciar munición de ballesta
        if (crossbowController != null)
        {
            crossbowController.ResetAmmo();
        }

        // Re-bloquear cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Comenzar de nuevo
        StartCoroutine(GameplaySequence());
    }
}
