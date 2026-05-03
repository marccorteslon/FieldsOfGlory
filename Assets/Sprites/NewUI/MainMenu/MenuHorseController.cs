using UnityEngine;
using System.Collections.Generic;

public class MenuHorseController : MonoBehaviour
{
    public enum HorseState { Idle, IdleBreak, Moving }
    
    [Header("Components")]
    [Tooltip("Animator to control the horse animations.")]
    public Animator animator;
    [Tooltip("Particle system for the dust/smoke when galloping.")]
    public ParticleSystem gallopSmoke;
    [Tooltip("BoxCollider defining the area where the horse can move. Set it to 'Is Trigger'.")]
    public BoxCollider roamArea;

    [Header("Settings")]
    [Tooltip("Check this if the horse is a 2D Sprite moving on X/Y. Leave unchecked for 3D X/Z movement.")]
    public bool is2D = false;
    public float trotSpeed = 1.5f;
    public float gallopSpeed = 4f;
    public float rotationSpeed = 5f;
    
    [Header("Timers")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    public float minMoveTime = 3f;
    public float maxMoveTime = 6f;

    [Header("Idle Break Settings")]
    [Tooltip("Minimum amount of seconds the horse will spend eating.")]
    public float minEatTime = 3f;
    [Tooltip("Maximum amount of seconds the horse will spend eating.")]
    public float maxEatTime = 8f;
    [Tooltip("Exactly how long your Stomp animation takes to finish in seconds (since it probably doesn't loop).")]
    public float stompAnimLength = 3f;
    
    [Header("Avoidance Settings")]
    [Tooltip("How close another horse can get before this one steers away.")]
    public float avoidanceRadius = 3f;
    [Tooltip("How strongly the horse pushes away from others.")]
    public float avoidanceStrength = 2f;
    
    public static List<MenuHorseController> allHorses = new List<MenuHorseController>();

    private HorseState currentState = HorseState.Idle;
    private float stateTimer;
    private Vector3 targetPosition;
    private bool isGalloping = false;

    private static readonly int SpeedParam = Animator.StringToHash("Speed");
    private static readonly int IdleBreakTrigger = Animator.StringToHash("IdleBreak");
    private static readonly int StopBreakTrigger = Animator.StringToHash("StopBreak");
    private static readonly int IdleBreakTypeParam = Animator.StringToHash("IdleBreakType");

    private void OnEnable()
    {
        if (!allHorses.Contains(this)) allHorses.Add(this);
    }

    private void OnDisable()
    {
        if (allHorses.Contains(this)) allHorses.Remove(this);
    }

    private void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (gallopSmoke != null) gallopSmoke.Stop();
        
        SetIdleState();
    }

    private void Update()
    {
        stateTimer -= Time.deltaTime;

        switch (currentState)
        {
            case HorseState.Idle:
            case HorseState.IdleBreak:
                if (stateTimer <= 0)
                {
                    ChooseNextAction();
                }
                break;

            case HorseState.Moving:
                MoveTowardsTarget();
                
                float distance = is2D ? 
                    Vector2.Distance(transform.position, targetPosition) : 
                    Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), new Vector3(targetPosition.x, 0, targetPosition.z));
                
                if (distance < 0.1f || stateTimer <= 0)
                {
                    SetIdleState();
                }
                break;
        }
    }

    private void ChooseNextAction()
    {
        if (currentState == HorseState.IdleBreak)
        {
            SetIdleState();
            return;
        }

        int rand = Random.Range(0, 100);
        
        if (rand < 40) SetMovingState();
        else if (rand < 70) SetIdleBreakState();
        else SetIdleState();
    }

    private void SetIdleState()
    {
        if (currentState == HorseState.IdleBreak && animator != null)
        {
            animator.SetTrigger(StopBreakTrigger);
        }

        currentState = HorseState.Idle;
        stateTimer = Random.Range(minIdleTime, maxIdleTime);
        isGalloping = false;
        
        if (animator != null) animator.SetFloat(SpeedParam, 0f);
        if (gallopSmoke != null) gallopSmoke.Stop();
    }

    private void SetIdleBreakState()
    {
        currentState = HorseState.IdleBreak;
        
        if (animator != null)
        {
            animator.SetFloat(SpeedParam, 0f); 
            int breakType = Random.Range(0, 2);
            animator.SetInteger(IdleBreakTypeParam, breakType);
            animator.SetTrigger(IdleBreakTrigger);
            
            stateTimer = (breakType == 0) ? Random.Range(minEatTime, maxEatTime) : stompAnimLength;
        }
        else
        {
            stateTimer = 3f;
        }
        
        isGalloping = false;
        if (gallopSmoke != null) gallopSmoke.Stop();
    }

    private void SetMovingState()
    {
        if (currentState == HorseState.IdleBreak && animator != null)
        {
            animator.SetTrigger(StopBreakTrigger);
        }

        currentState = HorseState.Moving;
        
        if (roamArea != null)
        {
            Bounds bounds = roamArea.bounds;
            if (is2D)
            {
                targetPosition = new Vector3(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y), transform.position.z);
            }
            else
            {
                targetPosition = new Vector3(Random.Range(bounds.min.x, bounds.max.x), transform.position.y, Random.Range(bounds.min.z, bounds.max.z));
            }
        }
        else
        {
            Vector2 randCircle = Random.insideUnitCircle * 5f;
            targetPosition = is2D ? transform.position + new Vector3(randCircle.x, randCircle.y, 0) : transform.position + new Vector3(randCircle.x, 0, randCircle.y);
        }

        isGalloping = Random.value > 0.5f;
        
        if (animator != null)
        {
            animator.SetFloat(SpeedParam, isGalloping ? 2f : 1f);
        }
        
        if (isGalloping && gallopSmoke != null) gallopSmoke.Play();
        else if (gallopSmoke != null) gallopSmoke.Stop();

        stateTimer = Random.Range(minMoveTime, maxMoveTime);
    }

    private void MoveTowardsTarget()
    {
        float speed = isGalloping ? gallopSpeed : trotSpeed;
        
        Vector3 targetDirection = targetPosition - transform.position;
        if (!is2D) targetDirection.y = 0;
        
        Vector3 avoidanceVector = Vector3.zero;
        foreach (var otherHorse in allHorses)
        {
            if (otherHorse == this || otherHorse == null) continue;
            
            float dist = Vector3.Distance(transform.position, otherHorse.transform.position);
            if (dist < avoidanceRadius && dist > 0.001f)
            {
                Vector3 away = transform.position - otherHorse.transform.position;
                if (!is2D) away.y = 0;
                
                avoidanceVector += away.normalized * (1f - (dist / avoidanceRadius)) * avoidanceStrength;
            }
        }

        Vector3 finalDirection = (targetDirection.normalized + avoidanceVector).normalized;

        transform.position += finalDirection * speed * Time.deltaTime;

        if (is2D)
        {
            if (Mathf.Abs(finalDirection.x) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(finalDirection.x);
                transform.localScale = scale;
            }
        }
        else
        {
            if (finalDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(finalDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
    }
}
