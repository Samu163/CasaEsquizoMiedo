using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{

    public static LevelManager instance;

    public AnomaliesManager anomaliesManager;

    [SerializeField] public List<LevelAnomalyInfoPair> anomaliesPerLevelList = new();
    [SerializeField] public List<AnomalyModelPair> anomaliesAndModelList = new();

    [SerializeField] private Dictionary<int, int> anomaliesPerLevelDict;
    private Dictionary<string, GameObject> anomaliesAndModelDict;

    public IReadOnlyDictionary<int, int> AnomaliesPerLevel => anomaliesPerLevelDict;
    public IReadOnlyDictionary<string, GameObject> AnomaliesAndModel => anomaliesAndModelDict;

    public int currentLevelIndex = 0;

    public void Awake()
    {
        instance = this;
        anomaliesPerLevelDict = anomaliesPerLevelList.ToDictionary(p => p.level, p => p.anomalyCount);
        anomaliesAndModelDict = anomaliesAndModelList.ToDictionary(p => p.anomalyId, p => p.modelPrefab);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartLevel();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartLevel()
    {
        anomaliesManager.SpawnAnomalies(AnomaliesPerLevel[currentLevelIndex]); // Example: spawn 3 anomalies at the start of the level
    }

    public void NextLevel()
    {
        currentLevelIndex++;
    }
}


[System.Serializable]
public class LevelAnomalyInfoPair
{
    public int level;
    public int anomalyCount;
}

[System.Serializable]
public class AnomalyModelPair
{
    public string anomalyId;
    public GameObject modelPrefab;
}

