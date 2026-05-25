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
        // Asegurar que todos los colisionadores en este virote y sus hijos sean triggers al nacer
        Collider[] allCols = GetComponentsInChildren<Collider>();
        foreach (var c in allCols)
        {
            if (c != null)
            {
                c.isTrigger = true;
                c.enabled = true;
            }
        }
    }

    void Start()
    {
        // Destrucción de seguridad si no golpea nada en 10 segundos
        Destroy(gameObject, 10f);
    }



    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // Obtener la ruta de jerarquía completa para facilitar el debug
        string path = other.gameObject.name;
        Transform t = other.transform.parent;
        while (t != null)
        {
            path = t.name + "/" + path;
            t = t.parent;
        }
        Debug.Log($"[CrossbowBolt] Pasó por/Colisionó con: {path} (Tag: {other.gameObject.tag})");

        // Comprobar si golpeamos una diana activa (que exista y no haya sido golpeada/abatida aún)
        ShootingTarget target = other.gameObject.GetComponentInParent<ShootingTarget>();
        bool isDiana = (target != null && !target.isHit);

        // Si no es una diana válida o ya está abatida, la flecha sigue de largo
        if (!isDiana)
        {
            return;
        }

        hasHit = true;

        // Desactivar físicas para clavarse
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Desactivar todos los colisionadores en hijos/raíz para evitar rebotes posteriores o llamadas repetidas
        Collider[] allColliders = GetComponentsInChildren<Collider>();
        foreach (var c in allColliders)
        {
            if (c != null)
            {
                c.enabled = false;
                c.isTrigger = false;
            }
        }

        // Anclarse al objeto golpeado para moverse con él (ej. si la diana cae)
        transform.SetParent(other.transform);

        // Detectar si golpeó un objetivo de disparo
        bool hitRegistered = false;
        
        if (target != null)
        {
            target.OnHit(this);
            hitRegistered = true;
        }

        // Reproducir sonido de impacto
        PlayHitSound(other.gameObject.tag, hitRegistered);

        // Instanciar partículas de impacto en la punta de la flecha
        if (hitParticlePrefab != null)
        {
            GameObject particles = Instantiate(hitParticlePrefab, transform.position, Quaternion.LookRotation(-transform.forward));
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
