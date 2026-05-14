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
        if (other.CompareTag("Player") || other.CompareTag("PlayerShield")) return;

        // Comprobar si golpeamos al escudo enemigo
        if (other.CompareTag("EnemyShield"))
        {
            Debug.Log("🛡️ ¡Golpe bloqueado por el escudo enemigo!");
            // Aquí podríamos reproducir un sonido metálico o chispas
            PlayHitEffect();
            return;
        }

        // Comprobar si golpeamos el cuerpo/ragdoll del enemigo
        if (other.CompareTag("Enemy"))
        {
            float damage = 0f;
            if (lanceController != null)
            {
                damage = lanceController.GetImpactDamage();
            }

            Debug.Log($"⚔️ ¡Impacto físico al enemigo! Daño/Puntos: {damage}");
            
            // Aquí llamaríamos al ScoreManager o EnemyRagdollController para aplicar el golpe
            // Ejemplo: FindObjectOfType<ScoreManager>().AddPhysicalScore(damage);
            
            PlayHitEffect();
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
