using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState
{
    Patrol,
    Investigate,
    Chase,
    Stunned,
    Search,
    Alert,
    Stalking,
    PostStunRetreat
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    [Header("DRAW GIZMOS")]
    [SerializeField] private bool drawGizmos = true;

    [Header("Detection Settings")]
    [Tooltip("Maximum distance the enemy can see the player")]
    [SerializeField] private float visionRange = 15f;
    [Tooltip("Field of view angle in degrees")]
    [SerializeField] private float visionAngle = 60f;
    [Tooltip("Distance at which the enemy can hear sounds")]
    [SerializeField] private float hearingRange = 10f;
    [Tooltip("Layer mask for obstacles that block vision")]
    [SerializeField] private LayerMask obstacleMask = 1;
    [Tooltip("Layer mask for the player")]
    [SerializeField] private LayerMask playerMask = 1;
    
    [Header("Progressive Detection")]
    [Tooltip("Detection meter value needed to start chasing")]
    [SerializeField] private float detectionThreshold = 100f;
    [Tooltip("Base rate at which detection increases per second")]
    [SerializeField] private float baseDetectionRate = 20f;
    [Tooltip("Rate at which detection decreases per second when not detecting")]
    [SerializeField] private float detectionDecayRate = 15f;
    [Tooltip("Distance at which player is detected regardless of vision")]
    [SerializeField] private float proximityRadius = 3f;
    [Tooltip("Detection multiplier when player is in front of enemy")]
    [SerializeField] private float frontDetectionMultiplier = 3f;
    
    [Header("Movement Settings")]
    [Tooltip("Speed during normal patrol")]
    [SerializeField] private float patrolSpeed = 2f;
    [Tooltip("Speed when chasing the player")]
    [SerializeField] private float chaseSpeed = 5f;
    [Tooltip("Speed when investigating a point of interest")]
    [SerializeField] private float investigateSpeed = 3f;
    [Tooltip("Speed when urgently investigating (loud sounds)")]
    [SerializeField] private float urgentInvestigateSpeed = 4.5f;
    [Tooltip("Speed when actively searching for the player")]
    [SerializeField] private float searchSpeed = 3.5f;
    [Tooltip("Speed when in alert mode")]
    [SerializeField] private float alertSpeed = 2.5f;
    [Tooltip("Speed when stalking the player")]
    [SerializeField] private float stalkingSpeed = 1.5f;
    [Tooltip("Speed when retreating after being stunned")]
    [SerializeField] private float retreatSpeed = 6f;
    
    [Header("Behavior Timings")]
    [Tooltip("Minimum time to wait at each waypoint")]
    [SerializeField] private float minWaitTimeAtWaypoint = 2f;
    [Tooltip("Maximum time to wait at each waypoint")]
    [SerializeField] private float maxWaitTimeAtWaypoint = 5f;
    [Tooltip("Time spent investigating a position")]
    [SerializeField] private float investigateTime = 8f;
    [Tooltip("Duration of stun effect")]
    [SerializeField] private float stunDuration = 3f;
    [Tooltip("Time before giving up chase and starting search")]
    [SerializeField] private float losePlayerTime = 5f;
    [Tooltip("Duration of active search mode")]
    [SerializeField] private float searchDuration = 15f;
    [Tooltip("Duration of alert mode")]
    [SerializeField] private float alertDuration = 10f;
    [Tooltip("Duration of stalking mode")]
    [SerializeField] private float stalkingDuration = 8f;
    [Tooltip("Time to wait at waypoint after retreating from stun")]
    [SerializeField] private float postStunWaitTime = 4f;
    
    [Header("Search Behavior")]
    [Tooltip("Radius around investigation point to search")]
    [SerializeField] private float searchRadius = 8f;
    [Tooltip("Number of search points to investigate")]
    [SerializeField] private int numberOfSearchPoints = 4;
    [Tooltip("Time to spend at each search point")]
    [SerializeField] private float timePerSearchPoint = 3f;
    
    [Header("Stalking Behavior")]
    [Tooltip("Distance to maintain when stalking player")]
    [SerializeField] private float stalkingDistance = 7f;
    [Tooltip("How often to update stalking position (seconds)")]
    [SerializeField] private float stalkingUpdateInterval = 2f;
    
    [Header("Waypoints")]
    [Tooltip("Array of patrol waypoints")]
    [SerializeField] private Transform[] patrolWaypoints;
    
    private NavMeshAgent agent;
    
    public EnemyState currentState { get; private set; }
    
    private Transform player;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 investigatePosition;
    private float lastPlayerSightTime;
    private bool isUrgentInvestigation = false;
    
    private float detectionMeter = 0f;
    private bool isDetectingPlayer = false;
    
    private int currentWaypointIndex = 0;
    private bool waitingAtWaypoint = false;
    private float currentWaitTime = 0f;
    private bool isPatrollingForward = true;
    
    private Vector3[] searchPoints;
    private int currentSearchPointIndex = 0;
    private bool searchingAroundPoint = false;
    
    private float lastStalkingUpdate = 0f;
    private Vector3 stalkingPosition;
    
    private float stateTimer = 0f;
    
    public System.Action<EnemyState> OnStateChanged;
    public System.Action<float> OnDetectionChanged;
    
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
        
        agent.speed = patrolSpeed;
        agent.stoppingDistance = 0.5f;
    }
    
    private void Update()
    {
        UpdateDetection();
        UpdateDetectionMeter();
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
                agent.isStopped = false;
                break;
            case EnemyState.Investigate:
                agent.speed = isUrgentInvestigation ? urgentInvestigateSpeed : investigateSpeed;
                agent.isStopped = false;
                break;
            case EnemyState.Chase:
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                break;
            case EnemyState.Stunned:
                agent.isStopped = true;
                break;
            case EnemyState.Search:
                agent.speed = searchSpeed;
                agent.isStopped = false;
                StartSearchBehavior();
                break;
            case EnemyState.Alert:
                agent.speed = alertSpeed;
                agent.isStopped = false;
                StartAlertBehavior();
                break;
            case EnemyState.Stalking:
                agent.speed = stalkingSpeed;
                agent.isStopped = false;
                break;
            case EnemyState.PostStunRetreat:
                agent.speed = retreatSpeed;
                agent.isStopped = false;
                RetreatToNearestWaypoint();
                break;
        }
        
        Debug.Log($"Enemy state changed to: {currentState}");
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
            case EnemyState.Search:
                UpdateSearchState();
                break;
            case EnemyState.Alert:
                UpdateAlertState();
                break;
            case EnemyState.Stalking:
                UpdateStalkingState();
                break;
            case EnemyState.PostStunRetreat:
                UpdatePostStunRetreatState();
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
                StartCoroutine(WaitAtWaypointRandomly());
            }
        }
    }
    
    private void UpdateInvestigateState()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            if (!searchingAroundPoint)
            {
                searchingAroundPoint = true;
                StartCoroutine(SearchAroundPoint(investigatePosition));
            }
        }
    }
    
    private void UpdateChaseState()
    {
        if (player != null && (CanSeePlayer() || CanDetectPlayerInProximity()))
        {
            lastKnownPlayerPosition = player.position;
            lastPlayerSightTime = Time.time;
            agent.SetDestination(player.position);
        }
        else
        {
            if (Time.time - lastPlayerSightTime > losePlayerTime)
            {
                if (Random.value < 0.6f)
                {
                    SetState(EnemyState.Stalking);
                }
                else
                {
                    SetState(EnemyState.Search);
                }
            }
        }
    }
    
    private void UpdateStunnedState()
    {
        if (stateTimer >= stunDuration)
        {
            SetState(EnemyState.PostStunRetreat);
        }
    }
    
    private void UpdateSearchState()
    {
        if (stateTimer >= searchDuration)
        {
            SetState(EnemyState.Patrol);
            MoveToNearestWaypointInDirection();
            return;
        }
        
        if (searchPoints != null && currentSearchPointIndex < searchPoints.Length)
        {
            if (!agent.pathPending && agent.remainingDistance < 1f)
            {
                if (stateTimer - (currentSearchPointIndex * timePerSearchPoint) >= timePerSearchPoint)
                {
                    currentSearchPointIndex++;
                    if (currentSearchPointIndex < searchPoints.Length)
                    {
                        agent.SetDestination(searchPoints[currentSearchPointIndex]);
                    }
                }
            }
        }
    }
    
    private void UpdateAlertState()
    {
        if (stateTimer >= alertDuration)
        {
            SetState(EnemyState.Patrol);
            MoveToNearestWaypointInDirection();
        }
    }
    
    private void UpdateStalkingState()
    {
        if (stateTimer >= stalkingDuration)
        {
            SetState(EnemyState.Search);
            return;
        }
        
        if (player != null)
        {
            if (Time.time - lastStalkingUpdate >= stalkingUpdateInterval)
            {
                UpdateStalkingPosition();
                lastStalkingUpdate = Time.time;
            }
            
            if (CanSeePlayer())
            {
                SetState(EnemyState.Chase);
            }
        }
    }
    
    private void UpdatePostStunRetreatState()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
        {
            StartCoroutine(WaitAfterRetreat());
        }
    }
    
    #endregion
    
    #region Detection System
    
    private void UpdateDetection()
    {
        if (player == null || currentState == EnemyState.Stunned || currentState == EnemyState.PostStunRetreat) return;
        
        bool canSeePlayer = CanSeePlayer();
        bool canDetectInProximity = CanDetectPlayerInProximity();
        
        isDetectingPlayer = canSeePlayer || canDetectInProximity;
        
        if (detectionMeter >= detectionThreshold && currentState != EnemyState.Chase)
        {
            SetState(EnemyState.Chase);
        }
    }
    
    private void UpdateDetectionMeter()
    {
        if (currentState == EnemyState.Stunned || currentState == EnemyState.PostStunRetreat) return;
        
        if (isDetectingPlayer)
        {
            float detectionRate = CalculateDetectionRate();
            detectionMeter = Mathf.Min(detectionMeter + detectionRate * Time.deltaTime, detectionThreshold);
        }
        else
        {
            detectionMeter = Mathf.Max(detectionMeter - detectionDecayRate * Time.deltaTime, 0f);
        }
        
        OnDetectionChanged?.Invoke(detectionMeter / detectionThreshold);
    }
    
    private float CalculateDetectionRate()
    {
        if (player == null) return 0f;
        
        float distance = Vector3.Distance(transform.position, player.position);
        float distanceMultiplier = Mathf.Lerp(3f, 1f, distance / visionRange);
        
        bool canSeePlayer = CanSeePlayer();
        bool isInProximity = CanDetectPlayerInProximity();
        
        if (canSeePlayer)
        {
            return baseDetectionRate * distanceMultiplier;
        }
        else if (isInProximity)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            
            if (angleToPlayer <= 90f)
            {
                return baseDetectionRate * frontDetectionMultiplier * distanceMultiplier;
            }
            else
            {
                return baseDetectionRate * 0.5f * distanceMultiplier;
            }
        }
        
        return 0f;
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
    
    private bool CanDetectPlayerInProximity()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= proximityRadius;
    }
    
    private void HandleSoundEmitted(Vector3 soundPosition, bool isLoudSound)
    {
        if (currentState == EnemyState.Stunned || currentState == EnemyState.PostStunRetreat) return;
        
        float distanceToSound = Vector3.Distance(transform.position, soundPosition);
        
        if (distanceToSound <= hearingRange)
        {
            investigatePosition = soundPosition;
            isUrgentInvestigation = isLoudSound;
            
            if (currentState != EnemyState.Chase)
            {
                SetState(EnemyState.Alert);
                agent.SetDestination(investigatePosition);
            }
        }
    }
    
    #endregion
    
    #region Advanced Behaviors
    
    private void StartSearchBehavior()
    {
        searchPoints = GenerateSearchPoints(lastKnownPlayerPosition);
        currentSearchPointIndex = 0;
        if (searchPoints.Length > 0)
        {
            agent.SetDestination(searchPoints[0]);
        }
    }
    
    private Vector3[] GenerateSearchPoints(Vector3 centerPoint)
    {
        Vector3[] points = new Vector3[numberOfSearchPoints];
        
        for (int i = 0; i < numberOfSearchPoints; i++)
        {
            float angle = (360f / numberOfSearchPoints) * i;
            Vector3 direction = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad));
            Vector3 searchPoint = centerPoint + direction * searchRadius;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(searchPoint, out hit, searchRadius, NavMesh.AllAreas))
            {
                points[i] = hit.position;
            }
            else
            {
                points[i] = centerPoint;
            }
        }
        
        return points;
    }
    
    private void StartAlertBehavior()
    {
        searchPoints = GenerateSearchPoints(investigatePosition);
        currentSearchPointIndex = 0;
        if (searchPoints.Length > 0)
        {
            agent.SetDestination(searchPoints[0]);
        }
    }
    
    private void UpdateStalkingPosition()
    {
        if (player == null) return;
        
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;
        Vector3 targetPosition = player.position + directionFromPlayer * stalkingDistance;
        
        Vector3 randomOffset = new Vector3(
            Random.Range(-2f, 2f),
            0,
            Random.Range(-2f, 2f)
        );
        targetPosition += randomOffset;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 5f, NavMesh.AllAreas))
        {
            stalkingPosition = hit.position;
            agent.SetDestination(stalkingPosition);
        }
    }
    
    private IEnumerator SearchAroundPoint(Vector3 centerPoint)
    {
        Vector3[] localSearchPoints = GenerateSearchPoints(centerPoint);
        
        for (int i = 0; i < localSearchPoints.Length; i++)
        {
            agent.SetDestination(localSearchPoints[i]);
            
            while (agent.pathPending || agent.remainingDistance > 1f)
            {
                yield return null;
            }
            
            yield return new WaitForSeconds(timePerSearchPoint * 0.5f);
        }
        
        searchingAroundPoint = false;
        isUrgentInvestigation = false;
        SetState(EnemyState.Patrol);
        MoveToNearestWaypointInDirection();
    }
    
    private void RetreatToNearestWaypoint()
    {
        if (patrolWaypoints.Length == 0) return;
        
        int nearestWaypointIndex = GetNearestWaypointInDirection();
        currentWaypointIndex = nearestWaypointIndex;
        agent.SetDestination(patrolWaypoints[nearestWaypointIndex].position);
    }
    
    private IEnumerator WaitAfterRetreat()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(postStunWaitTime);
        agent.isStopped = false;
        SetState(EnemyState.Patrol);
    }
    
    #endregion
    
    #region Movement
    
    private void MoveToNextWaypoint()
    {
        if (patrolWaypoints.Length == 0) return;
        
        if (isPatrollingForward)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= patrolWaypoints.Length)
            {
                currentWaypointIndex = patrolWaypoints.Length - 1;
                isPatrollingForward = false;
            }
        }
        else
        {
            currentWaypointIndex--;
            if (currentWaypointIndex < 0)
            {
                currentWaypointIndex = 0;
                isPatrollingForward = true;
            }
        }
        
        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
    }
    
    private void MoveToNearestWaypointInDirection()
    {
        if (patrolWaypoints.Length == 0) return;
        
        currentWaypointIndex = GetNearestWaypointInDirection();
        agent.SetDestination(patrolWaypoints[currentWaypointIndex].position);
    }
    
    private int GetNearestWaypointInDirection()
    {
        if (patrolWaypoints.Length == 0) return 0;
        if (patrolWaypoints.Length == 1) return 0;
        
        Vector3 forward = transform.forward;
        int bestWaypoint = 0;
        float bestScore = float.MinValue;
        
        for (int i = 0; i < patrolWaypoints.Length; i++)
        {
            if (patrolWaypoints[i] == null) continue;
            
            Vector3 directionToWaypoint = (patrolWaypoints[i].position - transform.position).normalized;
            float dotProduct = Vector3.Dot(forward, directionToWaypoint);
            float distance = Vector3.Distance(transform.position, patrolWaypoints[i].position);
            
            float score = dotProduct - (distance * 0.1f);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestWaypoint = i;
            }
        }
        
        DeterminePatrolDirection(bestWaypoint);
        return bestWaypoint;
    }
    
    private void DeterminePatrolDirection(int targetWaypointIndex)
    {
        if (patrolWaypoints.Length <= 2)
        {
            isPatrollingForward = true;
            return;
        }
        
        int nextForwardIndex = (targetWaypointIndex + 1) % patrolWaypoints.Length;
        int nextBackwardIndex = (targetWaypointIndex - 1 + patrolWaypoints.Length) % patrolWaypoints.Length;
        
        if (patrolWaypoints[nextForwardIndex] == null || patrolWaypoints[nextBackwardIndex] == null)
        {
            isPatrollingForward = true;
            return;
        }
        
        Vector3 forward = transform.forward;
        Vector3 directionToForward = (patrolWaypoints[nextForwardIndex].position - patrolWaypoints[targetWaypointIndex].position).normalized;
        Vector3 directionToBackward = (patrolWaypoints[nextBackwardIndex].position - patrolWaypoints[targetWaypointIndex].position).normalized;
        
        float forwardDot = Vector3.Dot(forward, directionToForward);
        float backwardDot = Vector3.Dot(forward, directionToBackward);
        
        isPatrollingForward = forwardDot >= backwardDot;
    }
    
    private IEnumerator WaitAtWaypointRandomly()
    {
        waitingAtWaypoint = true;
        currentWaitTime = Random.Range(minWaitTimeAtWaypoint, maxWaitTimeAtWaypoint);
        yield return new WaitForSeconds(currentWaitTime);
        waitingAtWaypoint = false;
        MoveToNextWaypoint();
    }
    
    #endregion
    
    #region Public Methods
    
    public void StunEnemy()
    {
        SetState(EnemyState.Stunned);
        detectionMeter = 0f;
    }
    
    public void SetPatrolWaypoints(Transform[] waypoints)
    {
        patrolWaypoints = waypoints;
        currentWaypointIndex = 0;
        isPatrollingForward = true;
        
        if (waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[0].position);
        }
    }
    
    public float GetDetectionProgress()
    {
        return detectionMeter / detectionThreshold;
    }
    
    public void InvestigatePosition(Vector3 position, bool isUrgent = false)
    {
        if (currentState == EnemyState.Stunned || currentState == EnemyState.PostStunRetreat) return;
        
        investigatePosition = position;
        isUrgentInvestigation = isUrgent;
        searchingAroundPoint = false;
        
        if (currentState != EnemyState.Chase)
        {
            SetState(EnemyState.Alert);
        }
    }
    
    #endregion
    
    #region Debug Visualization
    
    private void OnDrawGizmos()
    {
        if (!drawGizmos)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Vector3 leftBoundary = Quaternion.Euler(0, -visionAngle / 2, 0) * transform.forward * visionRange;
        Vector3 rightBoundary = Quaternion.Euler(0, visionAngle / 2, 0) * transform.forward * visionRange;
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, hearingRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, proximityRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, stalkingDistance);
        
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
        
        Gizmos.color = GetStateColor();
        Gizmos.DrawWireCube(transform.position + Vector3.up * 2.5f, Vector3.one * 0.5f);
        
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Vector3 barPosition = transform.position + Vector3.up * 3f;
            float barWidth = 2f;
            float barHeight = 0.2f;
            float fillAmount = detectionMeter / detectionThreshold;
            
            Gizmos.DrawWireCube(barPosition, new Vector3(barWidth, barHeight, 0.1f));
            Gizmos.color = Color.Lerp(Color.green, Color.red, fillAmount);
            Gizmos.DrawCube(barPosition - Vector3.right * (barWidth * 0.5f * (1f - fillAmount)), 
                           new Vector3(barWidth * fillAmount, barHeight, 0.1f));
        }
        
        if (Application.isPlaying && searchPoints != null)
        {
            Gizmos.color = Color.orange;
            foreach (Vector3 point in searchPoints)
            {
                Gizmos.DrawWireSphere(point, 0.3f);
            }
        }
    }
    
    private Color GetStateColor()
    {
        switch (currentState)
        {
            case EnemyState.Patrol: return Color.green;
            case EnemyState.Investigate: return isUrgentInvestigation ? Color.red : Color.yellow;
            case EnemyState.Chase: return Color.red;
            case EnemyState.Stunned: return Color.gray;
            case EnemyState.Search: return Color.orange;
            case EnemyState.Alert: return Color.cyan;
            case EnemyState.Stalking: return Color.magenta;
            case EnemyState.PostStunRetreat: return Color.black;
            default: return Color.white;
        }
    }
    
    #endregion
}