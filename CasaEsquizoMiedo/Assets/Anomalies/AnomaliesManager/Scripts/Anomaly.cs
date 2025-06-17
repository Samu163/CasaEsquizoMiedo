using UnityEngine;

public abstract class Anomaly : MonoBehaviour
{

    public bool hasBeenCaptured = false;

    public virtual void InitAnomaly() { }

    public virtual void CaptureAnomaly() { }
}
