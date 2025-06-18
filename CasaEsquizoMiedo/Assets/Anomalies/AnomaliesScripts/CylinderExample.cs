using UnityEngine;

public class CylinderExample : Anomaly
{
    private float initialY; // Store the initial Y position
    public float amplitude = 1f; // Height of the up and down movement
    public float frequency = 1f; // Speed of the up and down movement

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialY = transform.position.y; // Save the initial Y position
    }

    // Update is called once per frame
    void Update()
    {
        // Calculate the new Y position using a sine wave
        float newY = initialY + Mathf.Sin(Time.time * frequency) * amplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public override void InitAnomaly()
    {
        Destroy(LevelManager.instance.AnomaliesAndModel[modelIdentifier]);
        Debug.Log("CylinderExample anomaly initialized.");
    }

    public override void CaptureAnomaly()
    {
        hasBeenCaptured = true;
        Debug.Log("CylinderExample anomaly captured.");
    }
}

