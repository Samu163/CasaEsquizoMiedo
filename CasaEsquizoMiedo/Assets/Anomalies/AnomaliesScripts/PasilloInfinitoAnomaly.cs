using UnityEngine;

public class PasilloInfinitoAnomaly : Anomaly
{

    public GameObject finalWall;
    [SerializeField] private Vector3 initialWorldPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initialWorldPosition = finalWall.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (hasBeenCaptured)
        {
            Vector3 localForward = finalWall.transform.TransformDirection(Vector3.forward);
            float distanceMoved = Vector3.Dot(finalWall.transform.position - initialWorldPosition, localForward);

            if (distanceMoved < 15f)
            {
                finalWall.transform.Translate(Vector3.forward * Time.deltaTime * 3f, Space.Self);
            }

        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            CaptureAnomaly();
        }
    }

    public override void InitAnomaly()
    {
        Destroy(LevelManager.instance.AnomaliesAndModel[modelIdentifier]);
        Debug.Log("Pasillo Infinito Anomaly has started.");
    }

    public override void CaptureAnomaly()
    {
        hasBeenCaptured = true;
        Debug.Log("Pasillo Infinito Anomaly has ended.");
    }
}
