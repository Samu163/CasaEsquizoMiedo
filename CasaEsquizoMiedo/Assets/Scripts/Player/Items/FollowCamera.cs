using UnityEngine;

public abstract class FollowCamera : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform cameraTransform;
    public Vector3 positionOffset = new(0f, -0.2f, 0.5f);
    public Vector3 rotationOffset = Vector3.zero;

    [SerializeField] private float positionSmoothTime = 0.05f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    protected Vector3 positionVelocity;

    protected virtual void LateFollowUpdate()
    {
        if (!cameraTransform) return;

        Vector3 targetPos = cameraTransform.TransformPoint(positionOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref positionVelocity, positionSmoothTime);

        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);
    }
}
