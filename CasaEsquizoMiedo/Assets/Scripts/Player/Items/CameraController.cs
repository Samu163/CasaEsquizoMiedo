using DG.Tweening;
using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform zoomTarget;
    public Image fadePanel;
    public GameObject cameraModel;
    public GameObject volume;
    public float fadeDuration = 0.2f;

    public Vector3 positionOffset = new(0f, -0.2f, 0.5f);
    public Vector3 rotationOffset = Vector3.zero;

    [Header("Smooth Settings")]
    [SerializeField] private float followSmoothTime = 0.05f;
    [SerializeField] private float zoomSmoothTime = 0.15f;        
    [SerializeField] private float rotationSmoothSpeed = 10f;

    private Vector3 followVelocity = Vector3.zero;
    private Vector3 zoomVelocity = Vector3.zero;
    private bool isZoomed = false;
    private bool isFading;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeZoomTransition());
            isZoomed = !isZoomed;
        }
    }

    void LateUpdate()
    {
        if (!cameraTransform) return;

        if (!isZoomed)
        {
            Vector3 targetPos = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, followSmoothTime);

            Quaternion targetRot = cameraTransform.rotation * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);
        }
        else
        {
            if (!zoomTarget) return;

            Vector3 targetPos = zoomTarget.position;
            Quaternion targetRot = zoomTarget.rotation;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref zoomVelocity, zoomSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    System.Collections.IEnumerator FadeZoomTransition()
    {
        isFading = true;

        fadePanel.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);
        cameraModel.SetActive(!isZoomed);
        volume.SetActive(isZoomed);

        fadePanel.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        isFading = false;
    }
}
