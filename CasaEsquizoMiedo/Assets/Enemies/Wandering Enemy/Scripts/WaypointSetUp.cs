using UnityEngine;

// Herramienta simple para configurar waypoints automáticamente
public class WaypointSetup : MonoBehaviour
{
    [Header("Waypoint Configuration")]
    [SerializeField] private EnemyController targetEnemy;
    [SerializeField] private int numberOfWaypoints = 4;
    [SerializeField] private float waypointRadius = 8f;
    [SerializeField] private bool useCircularPattern = true;
    
    [Header("Manual Waypoints")]
    [SerializeField] private Transform[] manualWaypoints;
    
    [Header("Visualization")]
    [SerializeField] private bool showWaypointConnections = true;
    [SerializeField] private Color waypointColor = Color.green;
    [SerializeField] private Color connectionColor = Color.yellow;
    
    private Transform[] generatedWaypoints;
    
    [ContextMenu("Generate Circular Waypoints")]
    public void GenerateCircularWaypoints()
    {
        ClearGeneratedWaypoints();
        
        generatedWaypoints = new Transform[numberOfWaypoints];
        
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            float angle = (360f / numberOfWaypoints) * i;
            Vector3 position = transform.position + 
                new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * waypointRadius,
                    0,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * waypointRadius
                );
                
            // Create waypoint GameObject
            GameObject waypoint = new GameObject($"Waypoint_{i}");
            waypoint.transform.position = position;
            waypoint.transform.parent = this.transform;
            waypoint.tag = "Waypoint";
            
            // Add visual indicator
            waypoint.AddComponent<WaypointVisual>();
            
            generatedWaypoints[i] = waypoint.transform;
        }
        
        AssignWaypointsToEnemy(generatedWaypoints);
        
        Debug.Log($"Generated {numberOfWaypoints} waypoints in circular pattern");
    }
    
    [ContextMenu("Generate Random Waypoints")]
    public void GenerateRandomWaypoints()
    {
        ClearGeneratedWaypoints();
        
        generatedWaypoints = new Transform[numberOfWaypoints];
        
        for (int i = 0; i < numberOfWaypoints; i++)
        {
            Vector3 randomDirection = Random.insideUnitCircle.normalized;
            float randomDistance = Random.Range(waypointRadius * 0.5f, waypointRadius);
            
            Vector3 position = transform.position + 
                new Vector3(randomDirection.x * randomDistance, 0, randomDirection.y * randomDistance);
                
            GameObject waypoint = new GameObject($"Waypoint_{i}");
            waypoint.transform.position = position;
            waypoint.transform.parent = this.transform;
            waypoint.tag = "Waypoint";
            
            waypoint.AddComponent<WaypointVisual>();
            
            generatedWaypoints[i] = waypoint.transform;
        }
        
        AssignWaypointsToEnemy(generatedWaypoints);
        
        Debug.Log($"Generated {numberOfWaypoints} waypoints in random pattern");
    }
    
    [ContextMenu("Use Manual Waypoints")]
    public void UseManualWaypoints()
    {
        if (manualWaypoints.Length > 0)
        {
            AssignWaypointsToEnemy(manualWaypoints);
            Debug.Log($"Assigned {manualWaypoints.Length} manual waypoints to enemy");
        }
        else
        {
            Debug.LogWarning("No manual waypoints assigned!");
        }
    }
    
    [ContextMenu("Clear Generated Waypoints")]
    public void ClearGeneratedWaypoints()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
            {
                Destroy(transform.GetChild(i).gameObject);
            }
            else
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
        
        generatedWaypoints = null;
    }
    
    private void AssignWaypointsToEnemy(Transform[] waypoints)
    {
        if (targetEnemy != null)
        {
            targetEnemy.SetPatrolWaypoints(waypoints);
        }
        else
        {
            Debug.LogWarning("No target enemy assigned! Drag an EnemyController to the Target Enemy field.");
        }
    }
    
    [ContextMenu("Auto-Find Enemy")]
    public void AutoFindEnemy()
    {
        if (targetEnemy == null)
        {
            targetEnemy = FindFirstObjectByType<EnemyController>();
            if (targetEnemy != null)
            {
                Debug.Log($"Auto-found enemy: {targetEnemy.name}");
            }
            else
            {
                Debug.LogWarning("No EnemyController found in scene!");
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw radius circle
        Gizmos.color = waypointColor;
        Gizmos.DrawWireSphere(transform.position, waypointRadius);
        
        // Draw generated waypoints
        if (generatedWaypoints != null)
        {
            DrawWaypointConnections(generatedWaypoints);
        }
        
        // Draw manual waypoints
        if (manualWaypoints != null && manualWaypoints.Length > 0)
        {
            DrawWaypointConnections(manualWaypoints);
        }
    }
    
    private void DrawWaypointConnections(Transform[] waypoints)
    {
        if (!showWaypointConnections || waypoints == null) return;
        
        Gizmos.color = waypointColor;
        
        // Draw waypoint spheres
        foreach (var waypoint in waypoints)
        {
            if (waypoint != null)
            {
                Gizmos.DrawWireSphere(waypoint.position, 0.5f);
            }
        }
        
        // Draw connections
        Gizmos.color = connectionColor;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                int nextIndex = (i + 1) % waypoints.Length;
                if (waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                }
            }
        }
    }
    
    private void OnValidate()
    {
        // Clamp values
        numberOfWaypoints = Mathf.Max(2, numberOfWaypoints);
        waypointRadius = Mathf.Max(1f, waypointRadius);
    }
}