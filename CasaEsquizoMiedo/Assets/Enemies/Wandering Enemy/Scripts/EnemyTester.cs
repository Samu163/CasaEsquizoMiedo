using UnityEngine;

public class EnemyTester : MonoBehaviour
{
    [Header("Testing Controls")]
    [SerializeField] private EnemyController enemyToTest;
    [SerializeField] private KeyCode stunKey = KeyCode.S;
    [SerializeField] private KeyCode softSoundKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode loudSoundKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode teleportEnemyKey = KeyCode.T;
    
    [Header("Sound Testing")]
    [SerializeField] private float soundRange = 10f;
    [SerializeField] private bool useMousePosition = true;
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        if (enemyToTest == null)
        {
            enemyToTest = FindFirstObjectByType<EnemyController>();
        }
        
        //PrintControls();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void HandleInput()
    {
        // Stun enemy
        if (Input.GetKeyDown(stunKey) && enemyToTest != null)
        {
            enemyToTest.StunEnemy();
            Debug.Log("Enemy stunned!");
        }
        
        // Teleport enemy to this position
        if (Input.GetKeyDown(teleportEnemyKey) && enemyToTest != null)
        {
            enemyToTest.transform.position = transform.position;
            Debug.Log("Enemy teleported!");
        }
        
        // Sound testing
        if (Input.GetKeyDown(softSoundKey))
        {
            Vector3 soundPos = GetSoundPosition();
            SimpleSoundSystem.EmitSoftSound(soundPos);
        }
        
        if (Input.GetKeyDown(loudSoundKey))
        {
            Vector3 soundPos = GetSoundPosition();
            SimpleSoundSystem.EmitLoudSound(soundPos);
        }
    }
    
    private Vector3 GetSoundPosition()
    {
        if (useMousePosition && mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                return hit.point;
            }
        }
        
        Vector3 randomDirection = Random.insideUnitCircle.normalized;
        return transform.position + new Vector3(randomDirection.x, 0, randomDirection.y) * soundRange;
    }
    
    private void PrintControls()
    {
        Debug.Log("=== ENEMY TESTING CONTROLS ===");
        Debug.Log($"[{stunKey}] - Stun Enemy");
        Debug.Log($"[{softSoundKey}] - Emit Soft Sound");
        Debug.Log($"[{loudSoundKey}] - Emit Loud Sound");
        Debug.Log($"[{teleportEnemyKey}] - Teleport Enemy Here");
        Debug.Log("================================");
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, soundRange);
        
        // Draw line to enemy
        if (enemyToTest != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, enemyToTest.transform.position);
        }
    }
}