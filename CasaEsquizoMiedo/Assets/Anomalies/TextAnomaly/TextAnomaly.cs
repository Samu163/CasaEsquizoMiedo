using UnityEngine;

public class TextAnomaly : Anomaly
{
    public GameObject decalGO;
    public GameObject decalGOCompleted;


    public override void CaptureAnomaly()
    {
        if (hasBeenCaptured) return;

        hasBeenCaptured = true;

        decalGO?.SetActive(false);
        decalGOCompleted?.SetActive(true);
        GetComponent<Collider>().enabled = false;
    }
}
