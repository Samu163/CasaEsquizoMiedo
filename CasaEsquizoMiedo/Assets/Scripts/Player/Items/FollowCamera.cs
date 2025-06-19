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
    private Vector3 lastParentScale = Vector3.one;
    private Vector3 lastAppliedScale = Vector3.one;

    protected virtual void LateFollowUpdate()
    {
        if (!cameraTransform) return;

        Vector3 targetPos = cameraTransform.TransformPoint(positionOffset);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref positionVelocity, positionSmoothTime);

        Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);

        Vector3 parentScale = transform.parent ? transform.parent.localScale : Vector3.one;
        if (parentScale != lastParentScale)
        {
            lastParentScale = parentScale;

            if (parentScale.y < 1f)
            {
                lastAppliedScale = new Vector3(1, 1 / parentScale.y, 1);
            }
            else
            {
                lastAppliedScale = Vector3.one;
            }

            transform.localScale = lastAppliedScale;
        }
    }
}
