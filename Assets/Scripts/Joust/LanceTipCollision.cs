using UnityEngine;

[RequireComponent(typeof(Collider))]
public class LanceTipCollision : MonoBehaviour
{
    [Header("Referencias")]
    public PhysicalLanceController lanceController;
    
    [Header("Efectos")]
    public ParticleSystem hitParticles;

    void Awake()
    {
        if (lanceController == null)
        {
            // Busca el controlador en este objeto o en los padres (por si está en un modelo 3D instanciado)
            lanceController = GetComponentInParent<PhysicalLanceController>();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DEBUG JOUST] LanceTipCollision OnTriggerEnter con '{other.gameObject.name}', tag: '{other.tag}'");

        // Ignorar nuestro propio escudo o cuerpo
        if (other.CompareTag("Player") ) return;

        string hitTag = other.tag;

        // Comprobar si golpeamos alguna parte válida del enemigo
        if (hitTag == "Head" || hitTag == "Body" || hitTag == "Shield" || hitTag == "Horse")
        {
            Debug.Log($"[DEBUG JOUST] ¡¡IMPACTO VÁLIDO REGISTRADO en LanceTipCollision!! Tag: {hitTag}, objeto: '{other.gameObject.name}'");
            if (lanceController != null && lanceController.joustManager != null)
            {
                // Solo procesamos el golpe si estamos activamente en la fase de ataque
                if (!lanceController.joustManager.attackPartIsOn)
                {
                    Debug.Log($"[DEBUG JOUST] Colisión ignorada porque attackPartIsOn es FALSE.");
                    return;
                }

                int BF = lanceController.GetBF();
                int BL = lanceController.GetBL();
                float chargePercent = lanceController.GetChargePercent();

                // Añadir la puntuación al ScoreManager
                ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddAttackScore(hitTag, BF, BL, chargePercent, 0, 0);
                }

                // Resetea la carga
                lanceController.currentCharge = 0f;

                // Si la defensa sigue activa en el momento de la colisión física, el jugador no la completó a tiempo.
                // Forzamos su fin como fallida.
                JoustManager joustManager = lanceController.joustManager;
                if (joustManager != null)
                {
                    if (joustManager.defensePartIsOn && joustManager.defensePart != null)
                    {
                        joustManager.defensePart.ForceEndDefense(false);
                    }
                }

                // Guardamos los datos del impacto en el WinManager, que decidirá de inmediato
                // en este mismo frame a quién aplicar el ragdoll (según si ganamos la justa o perdimos).
                WinManager winManager = FindFirstObjectByType<WinManager>();
                if (winManager != null)
                {
                    int forceScore = Mathf.RoundToInt(BF * (1 + chargePercent / 100f));
                    Vector3 hitDirection = lanceController.lancePivot.forward;
                    Vector3 hitPoint = other.ClosestPoint(transform.position);
                    winManager.CacheEnemyImpact(hitPoint, hitDirection, forceScore, hitTag);
                }

                PlayHitEffect();

                // Terminar la fase de ataque (esto consolidará y evaluará el fin de la justa en este frame)
                if (joustManager != null)
                {
                    joustManager.EndAttackPhase();
                }
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
