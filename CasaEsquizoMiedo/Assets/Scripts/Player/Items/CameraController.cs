// File: CameraController.cs
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : FollowCamera
{
    public Transform zoomTarget;
    public Image fadePanel;
    public GameObject cameraModel;
    public GameObject volume;
    public float fadeDuration = 0.2f;

    [SerializeField] private float zoomSmoothTime = 0.15f;

    private Vector3 zoomVelocity = Vector3.zero;
    private bool isZoomed = false;
    private bool isFading = false;

    public bool IsZoomed => isZoomed;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            StartCoroutine(FadeZoomTransition(Color.black));
            isZoomed = !isZoomed;
        }

        if (Input.GetKeyDown(KeyCode.Mouse0) && isZoomed)
        {
            TakeShot();
            StartCoroutine(FadeZoomTransition(Color.white));
            isZoomed = !isZoomed;
        }
    }

    void LateUpdate()
    {
        if (isZoomed)
        {
            if (!zoomTarget) return;

            Vector3 targetPos = zoomTarget.position;
            Quaternion targetRot = zoomTarget.rotation;

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref zoomVelocity, zoomSmoothTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
        else
        {
            LateFollowUpdate();
        }
    }

    System.Collections.IEnumerator FadeZoomTransition(Color fadeColor)
    {
        isFading = true;
        fadeColor.a = 0f;
        fadePanel.color = fadeColor;
        fadePanel.DOFade(1f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        cameraModel.SetActive(!isZoomed);
        volume.SetActive(isZoomed);

        fadePanel.DOFade(0f, fadeDuration);
        yield return new WaitForSeconds(fadeDuration);

        isFading = false;
    }

    public void TakeShot()
    {
        // Implementar lógica de captura con raycast
    }
}
