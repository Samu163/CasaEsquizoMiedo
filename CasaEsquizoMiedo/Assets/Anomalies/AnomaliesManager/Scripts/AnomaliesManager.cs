using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnomaliesManager : MonoBehaviour
{

    public List<Anomaly> allAnomalies = new();

    [SerializeField] private List<Anomaly> activeAnomalies = new();

    public void SpawnAnomalies(int numAnomalies)
    {
        if (numAnomalies > allAnomalies.Count)
        {
            Debug.LogError("Not enough anomalies to spawn the requested number.");
            return;
        }

        List<Anomaly> shuffled = new(allAnomalies);
        Shuffle(shuffled);

        activeAnomalies.Clear();

        var selectedAnomalies = shuffled.Take(numAnomalies);

        foreach (var anomaly in selectedAnomalies)
        {
            var prefabInstance = Instantiate(anomaly as MonoBehaviour, transform).gameObject;
            Anomaly anomalyInstance = prefabInstance.GetComponent<Anomaly>();
            activeAnomalies.Add(anomalyInstance);
        }

        Debug.Log($"Spawned {activeAnomalies.Count} anomalies.");

        InitAnomalies();
    }

    public void InitAnomalies()
    {
        foreach (var anomaly in activeAnomalies)
        {
            anomaly.InitAnomaly();
        }
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[i], list[rand]) = (list[rand], list[i]);
        }
    }

    public int GetActiveAnomalyCount()
    {
        return activeAnomalies.Count;
    }
}
