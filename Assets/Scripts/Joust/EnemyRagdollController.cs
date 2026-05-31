using System.Collections;
using UnityEngine;

public class EnemyRagdollController : MonoBehaviour
{
    [Header("Ragdoll")]
    public Animator animator;
    public Rigidbody[] allBodies;

    [Header("Upper Body Roots")]
    public Transform[] upperBodyRoots;

    [Header("Force")]
    public float baseForce = 250f;
    public float forcePerPoint = 25f;
    public float maxForce = 2500f;
    public float upwardForce = 250f;

    [Header("Reset")]
    public float resetDelay = 1.5f;

    private Transform[] cachedTransforms;
    private Vector3[] startPositions;
    private Quaternion[] startRotations;

    void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (allBodies == null || allBodies.Length == 0)
            allBodies = GetComponentsInChildren<Rigidbody>();

        CachePose();
        DisableRagdoll();
        IgnoreCollisions();
    }

    void IgnoreCollisions()
    {
        Collider[] myColliders = GetComponentsInChildren<Collider>();
        
        // Ignorar colisiones internas (entre los propios huesos del ragdoll)
        for (int i = 0; i < myColliders.Length; i++)
        {
            for (int j = i + 1; j < myColliders.Length; j++)
            {
                Physics.IgnoreCollision(myColliders[i], myColliders[j], true);
            }
        }

        // Ignorar colisiones con los caballos
        GameObject[] horses = GameObject.FindGameObjectsWithTag("Horse");
        foreach (GameObject horse in horses)
        {
            Collider[] horseColliders = horse.GetComponentsInChildren<Collider>();
            foreach (Collider hc in horseColliders)
            {
                // NO ignorar los colliders que sean triggers (como la punta de la lanza)
                if (hc.isTrigger) continue;

                foreach (Collider mc in myColliders)
                {
                    Physics.IgnoreCollision(mc, hc, true);
                }
            }
        }
    }

    void CachePose()
    {
        cachedTransforms = GetComponentsInChildren<Transform>();
        startPositions = new Vector3[cachedTransforms.Length];
        startRotations = new Quaternion[cachedTransforms.Length];

        for (int i = 0; i < cachedTransforms.Length; i++)
        {
            startPositions[i] = cachedTransforms[i].localPosition;
            startRotations[i] = cachedTransforms[i].localRotation;
        }
    }

    public void PlayImpact(Vector3 hitPoint, Vector3 hitDirection, int roundScore, bool fullRagdoll, string hitPart = "")
    {
        if (fullRagdoll)
            EnableFullRagdoll();
        else
            EnableUpperBodyRagdoll();

        float forceAmount = Mathf.Clamp(baseForce + roundScore * forcePerPoint, baseForce, maxForce);
        Vector3 finalForce = hitDirection.normalized * forceAmount + Vector3.up * upwardForce;

        Rigidbody closestBody = GetClosestBody(hitPoint);

        if (closestBody != null)
        {
            // Aplicamos la fuerza de impacto lineal principal en el hueso más cercano
            closestBody.AddForceAtPosition(finalForce, hitPoint, ForceMode.Impulse);

            // --- APLICACIÓN DE TORQUE LOCALIZADO Y EXAGERADO ---
            // Calculamos el eje de rotación perpendicular a la dirección del golpe y la vertical
            Vector3 torqueAxis = Vector3.Cross(hitDirection.normalized, Vector3.up).normalized;
            if (torqueAxis.magnitude > 0.1f)
            {
                // El torque base es proporcional a la fuerza
                float torqueAmount = forceAmount * 0.15f; 

                // Si golpeamos la cabeza o el hueso más cercano es la cabeza, aplicamos un fuerte efecto de látigo ("retroceso")
                bool isHead = hitPart == "Head" || closestBody.gameObject.name.ToLower().Contains("head");
                if (isHead)
                {
                    torqueAmount *= 2.5f; // Multiplicador extra para la cabeza
                    
                    // Incrementamos la resistencia en el resto del cuerpo para que la cabeza lidere el movimiento claramente
                    foreach (var rb in allBodies)
                    {
                        if (rb != closestBody)
                        {
                            rb.linearDamping = 2.0f; 
                            rb.angularDamping = 2.0f;
                        }
                    }
                }
                else if (hitPart == "Shield" || closestBody.gameObject.name.ToLower().Contains("shield") || closestBody.gameObject.name.ToLower().Contains("arm"))
                {
                    // Si golpeamos el escudo o brazo, causamos una torsión lateral violenta
                    torqueAmount *= 1.8f;
                    torqueAxis = Vector3.up; 
                }

                closestBody.AddTorque(torqueAxis * torqueAmount, ForceMode.Impulse);
            }
        }

        if (!fullRagdoll)
            StartCoroutine(ResetAfterDelay());
    }

    void EnableFullRagdoll()
    {
        if (animator != null)
            animator.enabled = false;

        foreach (var rb in allBodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    void EnableUpperBodyRagdoll()
    {
        if (animator != null)
            animator.enabled = false;

        foreach (var rb in allBodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (var root in upperBodyRoots)
        {
            if (root == null) continue;

            Rigidbody[] upperBodies = root.GetComponentsInChildren<Rigidbody>();

            foreach (var rb in upperBodies)
            {
                rb.isKinematic = false;
                rb.useGravity = true;
            }
        }
    }

    void DisableRagdoll()
    {
        foreach (var rb in allBodies)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearDamping = 0.05f; // Restablecer damping lineal por defecto
            rb.angularDamping = 0.05f; // Restablecer damping angular por defecto
        }

        if (animator != null)
            animator.enabled = true;
    }

    Rigidbody GetClosestBody(Vector3 hitPoint)
    {
        Rigidbody closest = null;
        float closestDistance = float.MaxValue;

        foreach (var rb in allBodies)
        {
            float distance = Vector3.Distance(rb.worldCenterOfMass, hitPoint);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = rb;
            }
        }

        return closest;
    }

    IEnumerator ResetAfterDelay()
    {
        yield return new WaitForSeconds(resetDelay);
        ResetRagdoll();
    }

    public void ResetRagdoll()
    {
        DisableRagdoll();

        for (int i = 0; i < cachedTransforms.Length; i++)
        {
            // Omitir el transform raíz para evitar que el enemigo completo se teletransporte hacia atrás
            if (cachedTransforms[i] == transform) continue;

            cachedTransforms[i].localPosition = startPositions[i];
            cachedTransforms[i].localRotation = startRotations[i];
        }
    }
}