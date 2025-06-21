using UnityEngine;

public class VentiladorAnomaly : Anomaly
{

    public GameObject ventilador;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void InitAnomaly()
    {
        Destroy(LevelManager.instance.AnomaliesAndModel[modelIdentifier]);
        Debug.Log("VentiladorAnomaly anomaly initialized.");
    }

    public override void CaptureAnomaly()
    {
        hasBeenCaptured = true;
        Debug.Log("VentiladorAnomaly anomaly captured.");
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ventilador.GetComponent<Rigidbody>().useGravity = true;
        }
    }
}
