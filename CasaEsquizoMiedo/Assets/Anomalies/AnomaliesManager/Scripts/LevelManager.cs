using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    public AnomaliesManager anomaliesManager;

    public int numAnomaliesToSpawn = 3;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel()
    {
        anomaliesManager.SpawnAnomalies(numAnomaliesToSpawn); // Example: spawn 3 anomalies at the start of the level
    }
}
