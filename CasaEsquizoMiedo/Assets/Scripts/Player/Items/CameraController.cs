using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : FollowCamera
{
    public Transform zoomTarget;
    public Image fadePanel;
    public GameObject cameraModel;
    public GameObject volume;
    public Animator cameraAnimator;
    public float fadeDuration = 0.2f;

    [SerializeField] private float zoomSmoothTime = 0.15f;
    [SerializeField] private float captureCooldown = 5f;
    [SerializeField] private float captureRange = 100f;
    [SerializeField] private LayerMask anomalyMask;

    private Vector3 zoomVelocity;
    private bool isZoomed;
    private bool isCooldown;
    private bool isShakePlaying;

    public bool IsZoomed => isZoomed;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (isShakePlaying || isCooldown)
            {
                if (!isShakePlaying)
                    StartCoroutine(PlayShakeAndBlockZoom());
                return;
            }

            isZoomed = !isZoomed;
            StartCoroutine(FadeZoomTransition(Color.black));
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && isZoomed && !isShakePlaying)
        {
            isZoomed = false;
            fadeDuration = 0.1f;
            StartCoroutine(FadeZoomTransition(Color.white, true));
            fadeDuration = 0.2f;
            StartCoroutine(ZoomCooldown());
        }
    }

    IEnumerator PlayShakeAndBlockZoom()
    {
        isShakePlaying = true;
        cameraAnimator.SetTrigger("Shake");
        yield return new WaitForSeconds(0.5f);
        isShakePlaying = false;
    }

    void LateUpdate()
    {
        if (isZoomed && zoomTarget)
        {
            transform.position = Vector3.SmoothDamp(transform.position, zoomTarget.position, ref zoomVelocity, zoomSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, zoomTarget.rotation, Time.deltaTime * 10f);
        }
        else
        {
            LateFollowUpdate();
        }
    }

    IEnumerator FadeZoomTransition(Color fadeColor, bool cameraBackShot = false)
    {
        fadeColor.a = 0f;
        fadePanel.color = fadeColor;
        fadePanel.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        if (cameraBackShot) TakeShot();
        cameraModel.SetActive(!isZoomed);
        volume.SetActive(isZoomed);

        if (cameraBackShot) fadeDuration = 1f;
        fadePanel.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);
    }

    IEnumerator ZoomCooldown()
    {
        isCooldown = true;
        yield return new WaitForSeconds(captureCooldown);
        isCooldown = false;
    }

    public void TakeShot()
    {
        Vector3 rayOrigin = cameraTransform.position + cameraTransform.forward * 2f;
        Ray ray = new Ray(rayOrigin, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, captureRange, anomalyMask))
        {
            if (hit.collider.TryGetComponent<Anomaly>(out Anomaly anomaly))
            {
                if (!anomaly.hasBeenCaptured)
                {
                    anomaly.CaptureAnomaly();
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        //takeshot ray
        if (cameraTransform != null)
        {
            Vector3 rayOrigin = cameraTransform.position + cameraTransform.forward * 1.2f;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + cameraTransform.forward * captureRange);
            Gizmos.DrawWireSphere(rayOrigin + cameraTransform.forward * captureRange, 0.5f);
        }
    }
}