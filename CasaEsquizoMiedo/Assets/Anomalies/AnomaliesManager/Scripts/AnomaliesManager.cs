using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AnomaliesManager : MonoBehaviour
{

    public List<GameObject> allAnomalies = new();

    [SerializeField] private List<GameObject> activeAnomalies = new();

    void Awake()
    {
        foreach (var anomaly in allAnomalies)
        {
            anomaly.SetActive(false);
            if (anomaly.TryGetComponent<Anomaly>(out var anomalyComponent))
            {
                anomalyComponent.hasBeenCaptured = false;
            }
        }
    }

    public void SpawnAnomalies(int numAnomalies)
    {
        if (numAnomalies > allAnomalies.Count)
        {
            return;
        }

        List<GameObject> shuffled = new(allAnomalies);
        Shuffle(shuffled);

        activeAnomalies.Clear();

        var selectedAnomalies = shuffled.Take(numAnomalies);

        foreach (var anomaly in selectedAnomalies)
        {
            anomaly.SetActive(true);
            activeAnomalies.Add(anomaly);
        }

        InitAnomalies();
    }

    public void InitAnomalies()
    {
        foreach (var anomaly in activeAnomalies)
        {
            anomaly.GetComponent<Anomaly>().InitAnomaly();
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
