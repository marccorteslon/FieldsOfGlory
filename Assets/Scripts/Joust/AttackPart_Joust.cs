using UnityEngine;

public class AttackPart_Joust : MonoBehaviour
{
    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Cinematics")]
    public JoustCinematicManager cinematicManager;

    [Header("Timing Bonus")]
    public bool enableTimingBonus = true;
    public float timingCountdown = 1.2f;
    public float timingWindowDuration = 0.25f;
    public int timingBonusPoints = 5;
    public ParticleSystem timingWindowParticles;
    public ParticleSystem timingSuccessParticles;

    private float timingTimer;
    private float timingWindowTimer;
    private bool timingWindowOpen;
    private bool timingWindowConsumed;

    private bool previousAttackState = false;
    private bool attackCameraStartedForThisPhase = false;

    void Start()
    {
        SetParticlesActive(timingWindowParticles, false);
        SetParticlesActive(timingSuccessParticles, false);
    }

    void Update()
    {
        if (joustManager == null) return;

        bool attackStarted = joustManager.attackPartIsOn;

        if (attackStarted != previousAttackState)
        {
            previousAttackState = attackStarted;

            if (attackStarted)
            {
                StartTimingBonusTimer();

                attackCameraStartedForThisPhase = false;
                TryStartAttackCamera();
            }
            else
            {
                attackCameraStartedForThisPhase = false;
                CloseTimingWindow();
            }
        }

        if (!attackStarted) return;

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        TryStartAttackCamera();
        UpdateTimingBonusTimer();
    }

    void TryStartAttackCamera()
    {
        if (attackCameraStartedForThisPhase) return;

        if (joustManager != null && joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        attackCameraStartedForThisPhase = true;

        if (cinematicManager != null)
            cinematicManager.StartAttackPhaseCamera();
    }

    void StartTimingBonusTimer()
    {
        timingTimer = timingCountdown;
        timingWindowTimer = timingWindowDuration;
        timingWindowOpen = false;
        timingWindowConsumed = false;

        SetParticlesActive(timingWindowParticles, false);
        SetParticlesActive(timingSuccessParticles, false);
    }

    void UpdateTimingBonusTimer()
    {
        if (!enableTimingBonus || timingWindowConsumed)
            return;

        if (!timingWindowOpen)
        {
            timingTimer -= Time.deltaTime;

            if (timingTimer <= 0f)
                OpenTimingWindow();
        }
        else
        {
            timingWindowTimer -= Time.deltaTime;

            if (timingWindowTimer <= 0f)
                CloseTimingWindow();
        }
    }

    void OpenTimingWindow()
    {
        timingWindowOpen = true;
        timingWindowTimer = timingWindowDuration;
        PlayParticles(timingWindowParticles);
    }

    void CloseTimingWindow()
    {
        timingWindowOpen = false;
        SetParticlesActive(timingWindowParticles, false);
    }

    // Este método lo puede llamar el PhysicalLanceController o LanceTipCollision si impactan con éxito
    public bool ConsumeTimingBonus()
    {
        if (!enableTimingBonus || timingWindowConsumed)
            return false;

        bool success = timingWindowOpen;

        timingWindowConsumed = true;
        timingWindowOpen = false;

        SetParticlesActive(timingWindowParticles, false);

        if (success)
        {
            PlayParticles(timingSuccessParticles);
            if (scoreManager != null)
            {
                scoreManager.totalScore += timingBonusPoints;
                Debug.Log($"[Attack Timing] Bonus conseguido: +{timingBonusPoints}");
            }
        }

        return success;
    }

    void PlayParticles(ParticleSystem particles)
    {
        if (particles == null) return;

        particles.gameObject.SetActive(true);
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particles.Play();
    }

    void SetParticlesActive(ParticleSystem particles, bool active)
    {
        if (particles == null) return;

        if (!active)
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        particles.gameObject.SetActive(active);
    }

    public void ForceAttack()
    {
        // Se llama cuando se acaba el tiempo de la fase de ataque sin que el jugador haya impactado.
        // Como ahora funciona por físicas, si el tiempo se acaba, es simplemente un fallo.
        // No disparamos ningún raycast mágico al centro de la pantalla.
        
        if (cinematicManager != null)
            cinematicManager.OnAttackInputReleased();

        if (joustManager != null)
            joustManager.EndAttackPhase();
    }
}