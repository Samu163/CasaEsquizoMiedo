using System.Collections.Generic;
using UnityEngine;

public class InvertedAnomaly : Anomaly
{
    public List<GameObject> possibleObjectsToInvert;

    public override void InitAnomaly()
    {
        possibleObjectsToInvert = new List<GameObject>();
    }

    public override void CaptureAnomaly()
    {
        if (hasBeenCaptured) return;

        hasBeenCaptured = true;

        

        GetComponent<Collider>().enabled = false;
    }
}
