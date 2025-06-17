using UnityEngine;

public class WaypointVisual : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color waypointColor = Color.green;
    [SerializeField] private float waypointSize = 0.5f;
    [SerializeField] private bool showIndex = true;
    
    private void OnDrawGizmos()
    {
        Gizmos.color = waypointColor;
        Gizmos.DrawWireSphere(transform.position, waypointSize);
        
        if (showIndex)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position, Vector3.up * 2f);
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, waypointSize);
    }
}