using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState
{
    Patrol,
    Investigate,
    Chase,
    Stunned
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class EnemyController : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private float visionRange = 15f;
    [SerializeField] private float visionAngle = 60f;
    [SerializeField] private float hearingRange = 10f;
    [SerializeField] private LayerMask obstacleMask = 1;
    [SerializeField] private LayerMask playerMask = 1;
    
    [Header("Movement Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float chaseSpeed = 5f;
    [SerializeField] private float investigateSpeed = 3f; //sonidos normales
    [SerializeField] private float urgentInvestigateSpeed = 4.5f; //sonidos fuertes
    
    [Header("Behavior Settings")]
    [SerializeField] private float waitTimeAtWaypoint = 3f;
    [SerializeField] private float investigateTime = 8f;
    [SerializeField] private float stunDuration = 3f;
    [SerializeField] private float losePlayerTime = 5f;
    
    [Header("Waypoints")]
    [SerializeField] private Transform[] patrolWaypoints;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] patrolSounds;
    [SerializeField] private AudioClip[] investigateSounds;
    [SerializeField] private AudioClip[] chaseSounds;
    [SerializeField] private AudioClip stunSound;
    
    //components
    private NavMeshAgent agent;
    private AudioSource audioSource;
    
    //states
    public EnemyState currentState { get; private set; }
    
    //detection
    private Transform player;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 investigatePosition;
    private float lastPlayerSightTime;
    private bool isUrgentInvestigation = false; //sonidos fuertes
    
    //patrol
    private int currentWaypointIndex = 0;
    private bool waitingAtWaypoint = false;
    
    //timer
    private float stateTimer = 0f;
    
    //events para debug
    public System.Action<EnemyState> OnStateChanged;
    
    private void Start()
    {
        InitializeComponents();
        SetState(EnemyState.Patrol);
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
            
        SimpleSoundSystem.OnSoundEmitted += HandleSoundEmitted;
    }
    
    private void OnDestroy()
    {
        SimpleSoundSystem.OnSoundEmitted -= HandleSoundEmitted;
    }
    
    private void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.5f;
    }
    
    private void Update()
    {
        if (currentState == EnemyState.Stunned) return;
        
        UpdateDetection();
        UpdateState();
        
        stateTimer += Time.deltaTime;
    }
    
    #region State Management
    
    private void SetState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        currentState = newState;
        stateTimer = 0f;
        
        OnStateChanged?.Invoke(currentState);
        
        switch (currentState)
        {
            case EnemyState.Patrol:
                agent.speed = patrolSpeed;
                PlayRandomSound(patrolSounds);
                break;
            case EnemyState.Investigate:
                agent.speed = isUrgentInvestigation ? urgentInvestigateSpeed : investigateSpeed;
                PlayRandomSound(investigateSounds);
                break;
            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                PlayRandomSound(chaseSounds);
                break;
            case EnemyState.Stunned:
                agent.isStopped = true;
                PlaySound(stunSound);
                break;
        }
        
        Debug.Log($"current state: {currentState}");
    }
    
    private void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrolState();
                break;
            case EnemyState.Investigate:
                UpdateInvestigateState();
                break;
            case EnemyState.Chase:
                UpdateChaseState();
                break;
            case EnemyState.Stunned:
                UpdateStunnedState();
                break;
        }
    }
    
    #endregion
    
    #region State Behaviors
    
    private void UpdatePatrolState()
    {
        if (patrolWaypoints.Length == 0) return;
        
        if (!waitingAtWaypoint)
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                StartCoroutine(WaitAtWaypoint());
            }
        }
    }
    
    private void UpdateInvestigateState()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            if (stateTimer >= investigateTime)
            {
                SetState(EnemyState.Patrol);
                MoveToNextWaypoint();
                isUrgentInvestigation = false;
            }
        }
    }
    
    private void UpdateChaseState()
    {
        if (player != null && CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            lastPlayerSightTime = Time.time;
            agent.SetDestination(player.position);
        }
        else
        {
            if (Time.time - lastPlayerSightTime > losePlayerTime)
            {
                SetState(EnemyState.Patrol);
                MoveToNextWaypoint();
            }
        }
    }
    
    private void UpdateStunnedState()
    {
        if (stateTimer >= stunDuration)
        {
            agent.isStopped = false;
            SetState(EnemyState.Patrol);
            MoveToNextWaypoint();
        }
    }
    
    #endregion
    
    #region Detection System
    
    private void UpdateDetection()
    {
        if (player == null || currentState == EnemyState.Stunned) return;
        
        bool canSeePlayer = CanSeePlayer();
        
        if (canSeePlayer && currentState != EnemyState.Chase)
        {
            SetState(EnemyState.Chase);
        }
    }
    
    private bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer > visionRange) return false;
        
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > visionAngle / 2) return false;
        
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, distanceToPlayer, obstacleMask))
        {
            return false;
        }
        
        return true;
    }
    
    private void HandleSoundEmitted(Vector3 soundPosition, bool isLoudSound)
    {
        if (currentState == EnemyState.Stunned) return;
        
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        
        if (distanceToSound <= hearingRange)
        {
            investigatePosition = soundPosition;
            isUrgentInvestigation = isLoudSound;
            
            if (currentState != EnemyState.Chase)
            {
                SetState(EnemyState.Investigate);
                agent.SetDestination(investigatePosition);
            }
        }
    }
    
    #endregion
    
    #region Movement
    
    private void MoveToNextWaypoint()
    {
        if (patrolWaypoints.Length == 0) return;
        
        currentWaypointIndex = (currentWaypointIndex + 1) % patrolWaypoints.Length;
        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
    }
    
    private IEnumerator WaitAtWaypoint()
    {
        waitingAtWaypoint = true;
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        waitingAtWaypoint = false;
        MoveToNextWaypoint();
    }
    
    #endregion
    
    #region Audio System
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    private void PlayRandomSound(AudioClip[] clips)
    {
        if (clips.Length > 0)
        {
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            PlaySound(randomClip);
        }
    }
    
    #endregion
    
    #region Public Methods
    
    public void StunEnemy()
    {
        SetState(EnemyState.Stunned);
    }
    
    public void SetPatrolWaypoints(Transform[] waypoints)
    {
        patrolWaypoints = waypoints;
        currentWaypointIndex = 0;
        
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }
    
    #endregion
    
    #region Debug Visualization
    
    private void OnDrawGizmosSelected()
    {
        // Vision cone
        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        // Hearing range
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        
        // Waypoints
        if (patrolWaypoints != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < patrolWaypoints.Length; i++)
            {
                if (patrolWaypoints[i] != null)
                {
                    Gizmos.DrawWireSphere(patrolWaypoints[i].position, 0.5f);
                    
                    if (i < patrolWaypoints.Length - 1 && patrolWaypoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[i + 1].position);
                    }
                    else if (i == patrolWaypoints.Length - 1 && patrolWaypoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolWaypoints[i].position, patrolWaypoints[0].position);
                    }
                }
            }
        }
        
        // Current state indicator
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * 0.5f);
    }
    
    private Color GetStateColor()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: return Color.green;
            case EnemyState.Investigate: return isUrgentInvestigation ? Color.red : Color.yellow;
            case EnemyState.Chase: return Color.red;
            case EnemyState.Stunned: return Color.gray;
            default: return Color.white;
        }
    }
    
    #endregion
}