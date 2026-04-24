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

    public void PlayImpact(Vector3 hitPoint, Vector3 hitDirection, int roundScore, bool fullRagdoll)
    {
        if (fullRagdoll)
            EnableFullRagdoll();
        else
            EnableUpperBodyRagdoll();

        float forceAmount = Mathf.Clamp(baseForce + roundScore * forcePerPoint, baseForce, maxForce);
        Vector3 finalForce = hitDirection.normalized * forceAmount + Vector3.up * upwardForce;

        Rigidbody closestBody = GetClosestBody(hitPoint);

        if (closestBody != null)
            closestBody.AddForceAtPosition(finalForce, hitPoint, ForceMode.Impulse);

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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false;
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
            cachedTransforms[i].localPosition = startPositions[i];
            cachedTransforms[i].localRotation = startRotations[i];
        }
    }
}