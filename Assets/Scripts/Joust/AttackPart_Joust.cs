using UnityEngine;

public class AttackPart_Joust : MonoBehaviour
{
    [Header("Manager")]
    public JoustManager joustManager;
    public ScoreManager scoreManager;

    [Header("Cinematics")]
    public JoustCinematicManager cinematicManager;

    private bool previousAttackState = false;
    private bool attackCameraStartedForThisPhase = false;

    void OnEnable()
    {
        previousAttackState = false;
        attackCameraStartedForThisPhase = false;
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
                attackCameraStartedForThisPhase = false;
                TryStartAttackCamera();
            }
            else
            {
                attackCameraStartedForThisPhase = false;
            }
        }

        if (!attackStarted) return;

        if (joustManager.tutorialManager != null && joustManager.tutorialManager.IsTutorialOpen())
            return;

        TryStartAttackCamera();
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