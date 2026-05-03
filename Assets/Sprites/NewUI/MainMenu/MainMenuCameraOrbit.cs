using UnityEngine;

public class MainMenuCameraOrbit : MonoBehaviour
{
    [Header("Orbit Targets")]
    [Tooltip("An array of objects to orbit around. The script will randomly pick between them.")]
    public Transform[] targets;
    
    [Tooltip("A fallback point in space if the targets array is empty.")]
    public Vector3 defaultTargetPosition = Vector3.zero;

    [Header("Timing Settings")]
    [Tooltip("Minimum time (in seconds) before switching to a new target.")]
    public float minSwitchTime = 5f;
    [Tooltip("Maximum time (in seconds) before switching to a new target.")]
    public float maxSwitchTime = 12f;

    [Header("Orbit Settings")]
    [Tooltip("How fast the camera orbits in degrees per second.")]
    public float orbitSpeed = 5f;

    [Tooltip("How fast the camera smoothly glides to a new target when switching.")]
    public float transitionSpeed = 2f;

    [Header("Distance & Limits")]
    [Tooltip("How far away the camera should be from the target.")]
    public float orbitDistance = 10f;
    
    [Tooltip("How fast the camera smoothly adjusts to the desired distance.")]
    public float distanceTransitionSpeed = 2f;

    [Tooltip("The lowest absolute Y position the camera is allowed to go. Useful to stop it from clipping through the floor.")]
    public float minYLimit = 1f;

    [Header("Rotation Settings")]
    [Tooltip("If true, the camera will always rotate to look at the center of its orbit.")]
    public bool lookAtTarget = true;

    private Transform currentTarget;
    private Vector3 currentOrbitCenter;
    private float switchTimer;

    private void Start()
    {
        PickRandomTarget();
        currentOrbitCenter = currentTarget != null ? currentTarget.position : defaultTargetPosition;
    }

    private void Update()
    {
        switchTimer -= Time.deltaTime;
        if (switchTimer <= 0f)
        {
            PickRandomTarget();
        }
    }

    private void LateUpdate()
    {
        Vector3 desiredOrbitCenter = currentTarget != null ? currentTarget.position : defaultTargetPosition;

        currentOrbitCenter = Vector3.Lerp(currentOrbitCenter, desiredOrbitCenter, transitionSpeed * Time.deltaTime);

        transform.RotateAround(currentOrbitCenter, Vector3.up, orbitSpeed * Time.deltaTime);

        Vector3 offset = transform.position - currentOrbitCenter;
        float currentDistance = offset.magnitude;
        
        if (currentDistance > 0.001f)
        {
            float newDistance = Mathf.Lerp(currentDistance, orbitDistance, distanceTransitionSpeed * Time.deltaTime);
            
            offset = offset.normalized * newDistance;
            
            float absoluteY = currentOrbitCenter.y + offset.y;
            if (absoluteY < minYLimit)
            {
                float clampedOffsetY = minYLimit - currentOrbitCenter.y;
                
                clampedOffsetY = Mathf.Clamp(clampedOffsetY, -newDistance, newDistance);
                
                float horizontalMagnitude = Mathf.Sqrt(Mathf.Max(0, newDistance * newDistance - clampedOffsetY * clampedOffsetY));
                
                Vector2 horizontalOffset = new Vector2(offset.x, offset.z).normalized * horizontalMagnitude;
                offset = new Vector3(horizontalOffset.x, clampedOffsetY, horizontalOffset.y);
            }

            transform.position = currentOrbitCenter + offset;
        }

        if (lookAtTarget)
        {
            Vector3 direction = (currentOrbitCenter - transform.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, transitionSpeed * 2f * Time.deltaTime);
            }
        }
    }

    private void PickRandomTarget()
    {
        if (targets != null && targets.Length > 0)
        {
            if (targets.Length == 1)
            {
                currentTarget = targets[0];
            }
            else
            {
                Transform newTarget = currentTarget;
                while (newTarget == currentTarget)
                {
                    newTarget = targets[Random.Range(0, targets.Length)];
                }
                currentTarget = newTarget;
            }
        }
        
        switchTimer = Random.Range(minSwitchTime, maxSwitchTime);
    }
}
