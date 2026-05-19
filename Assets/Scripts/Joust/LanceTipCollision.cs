using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LanceTipCollision : MonoBehaviour
{
    [Header("Referencias")]
    public PhysicalLanceController lanceController;
    
    [Header("Efectos")]
    public ParticleSystem hitParticles;

    void OnTriggerEnter(Collider other)
    {
        // Ignorar nuestro propio escudo o cuerpo
        if (other.CompareTag("Player") ) return;

        string hitTag = other.tag;

        // Comprobar si golpeamos alguna parte válida del enemigo
        if (hitTag == "Head" || hitTag == "Body" || hitTag == "Shield" || hitTag == "Horse")
        {
            if (lanceController != null && lanceController.joustManager != null)
            {
                // Solo procesamos el golpe si estamos activamente en la fase de ataque
                if (!lanceController.joustManager.attackPartIsOn) return;

                int BF = lanceController.GetBF();
                int BL = lanceController.GetBL();
                float chargePercent = lanceController.GetChargePercent();

                // Añadir la puntuación al ScoreManager
                ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddAttackScore(hitTag, BF, BL, chargePercent, 0, 0);
                }

                // Intentar consumir el bonus de tiempo si el jugador atacó en el momento justo
                AttackPart_Joust attackPart = FindFirstObjectByType<AttackPart_Joust>();
                if (attackPart != null)
                {
                    attackPart.ConsumeTimingBonus();
                }

                // Resetea la carga
                lanceController.currentCharge = 0f;

                // Aplicar fuerza física al ragdoll del enemigo
                EnemyRagdollController ragdoll = other.GetComponentInParent<EnemyRagdollController>();
                if (ragdoll == null) 
                    ragdoll = FindFirstObjectByType<EnemyRagdollController>();

                if (ragdoll != null)
                {
                    // Usamos el daño/fuerza calculada para el impacto
                    int forceScore = Mathf.RoundToInt(BF * (1 + chargePercent / 100f));
                    Vector3 hitDirection = lanceController.lancePivot.forward;
                    // Pasamos 'true' para activar el full ragdoll y que salga volando
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    ragdoll.PlayImpact(hitPoint, hitDirection, forceScore, true);
                }

                PlayHitEffect();

                // Terminar la fase de ataque
                lanceController.joustManager.EndAttackPhase();
            }
        }
    }

    void PlayHitEffect()
    {
        if (hitParticles != null)
        {
            hitParticles.transform.position = transform.position;
            hitParticles.Play();
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLanceHit();
        }
    }
}
