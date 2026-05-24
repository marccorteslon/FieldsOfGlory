using UnityEngine;

public class CrossbowBolt : MonoBehaviour
{
    [Header("Configuración Física")]
    public float lifeTimeAfterHit = 5f;
    
    [Header("Efectos y Sonidos")]
    public GameObject hitParticlePrefab;
    public AudioClip woodHitSound;
    public AudioClip metalHitSound;
    public float soundVolume = 0.8f;

    private Rigidbody rb;
    private Collider col;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void Start()
    {
        // Destrucción de seguridad si no golpea nada en 10 segundos
        Destroy(gameObject, 10f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        // Desactivar físicas para clavarse
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        // Anclarse al objeto golpeado para moverse con él (ej. si la diana cae)
        transform.SetParent(collision.transform);

        // Detectar si golpeó un objetivo de disparo
        ShootingTarget target = collision.gameObject.GetComponentInParent<ShootingTarget>();
        bool hitRegistered = false;
        
        if (target != null)
        {
            target.OnHit(this);
            hitRegistered = true;
        }

        // Reproducir sonido de impacto
        PlayHitSound(collision.gameObject.tag, hitRegistered);

        // Instanciar partículas de impacto en el punto de contacto
        if (hitParticlePrefab != null && collision.contactCount > 0)
        {
            ContactPoint contact = collision.contacts[0];
            GameObject particles = Instantiate(hitParticlePrefab, contact.point, Quaternion.LookRotation(contact.normal));
            Destroy(particles, 2f);
        }

        // Programar destrucción del virote clavado
        Destroy(gameObject, lifeTimeAfterHit);
    }

    private void PlayHitSound(string objectTag, bool hitRegistered)
    {
        AudioClip clipToPlay = null;

        if (hitRegistered)
        {
            // Si es un objetivo registrado, elegimos sonido según su tipo
            clipToPlay = (objectTag == "GoldenTarget") ? metalHitSound : woodHitSound;
        }
        else
        {
            // Fallback para otros objetos
            clipToPlay = woodHitSound;
        }

        if (clipToPlay != null)
        {
            // Creamos un audio source temporal para reproducir el sonido 3D en el punto de impacto
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position, soundVolume);
        }
    }
}
