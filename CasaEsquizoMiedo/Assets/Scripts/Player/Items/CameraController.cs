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
            TakeShot();
            isZoomed = false;
            StartCoroutine(FadeZoomTransition(Color.white));
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

    IEnumerator FadeZoomTransition(Color fadeColor)
    {
        fadeColor.a = 0f;
        fadePanel.color = fadeColor;
        fadePanel.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        cameraModel.SetActive(!isZoomed);
        volume.SetActive(isZoomed);

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
        Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, captureRange, anomalyMask))
        {
            Anomaly anomaly = hit.collider.GetComponentInParent<Anomaly>();
            if (anomaly && !anomaly.hasBeenCaptured)
            {
                anomaly.CaptureAnomaly();
            }
        }
    }
}