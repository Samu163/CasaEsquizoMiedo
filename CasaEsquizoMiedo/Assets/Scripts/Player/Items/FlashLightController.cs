using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    public Light flashlight;
    public KeyCode toggleKey = KeyCode.Mouse0;
    public KeyCode fixKey = KeyCode.E;

    public float onDuration = 10f;
    public float blinkDuration = 2f;

    public Transform cameraTransform;
    public Transform flashlightLightObject;
    public Vector3 positionOffset = new Vector3(0f, -0.2f, 0.5f);
    public Vector3 initialRotationOffset = new Vector3(0f, -0.2f, 0.5f);
    public Vector3 flashRotationOffset = new Vector3(0f, -0.2f, 0.5f);

    [SerializeField] private float positionSmoothTime = 0.05f;
    [SerializeField] private float rotationSmoothSpeed = 10f;

    private bool isOn = false;
    private float onTimer = 0f;
    private bool isBlinking = false;
    private bool isDisabled = false;

    private int requiredPresses = 10;
    private int pressCounter = 0;
    private float pressResetTime = 3f;
    private float lastPressTime = -10f;

    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            HandleKeyPress();
        }

        FollowCameraWithOffset();
        ApplyCameraRotationToLight();

        if (isOn && !isBlinking && !isDisabled)
        {
            onTimer += Time.deltaTime;
            if (onTimer >= onDuration)
            {
                StartCoroutine(BlinkAndShutdown());
            }
        }
    }

    void HandleKeyPress()
    {
        if (isDisabled)
        {
            float timeSinceLast = Time.time - lastPressTime;
            if (timeSinceLast > pressResetTime)
                pressCounter = 0;

            pressCounter++;
            lastPressTime = Time.time;

            if (pressCounter >= requiredPresses)
            {
                pressCounter = 0;
                isDisabled = false;
                ToggleFlashlight(true);
            }
        }
        else
        {
            ToggleFlashlight(!isOn);
        }
    }

    void ToggleFlashlight(bool turnOn)
    {
        isOn = turnOn;
        flashlight.enabled = isOn;

        if (isOn)
        {
            onTimer = 0f;
        }
    }

    void FollowCameraWithOffset()
    {
        if (cameraTransform != null)
        {
            Vector3 targetPosition = cameraTransform.position + cameraTransform.TransformDirection(positionOffset);
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, positionSmoothTime);

            Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(initialRotationOffset.x, initialRotationOffset.y, initialRotationOffset.z);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    void ApplyCameraRotationToLight()
    {
        if (flashlightLightObject != null && cameraTransform != null)
        {
            Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(flashRotationOffset.x, flashRotationOffset.y, flashRotationOffset.z);
            flashlightLightObject.rotation = Quaternion.Slerp(flashlightLightObject.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
        }
    }

    System.Collections.IEnumerator BlinkAndShutdown()
    {
        isBlinking = true;

        float blinkTime = 0f;
        float blinkRate = 0.2f;

        while (blinkTime < blinkDuration)
        {
            flashlight.enabled = !flashlight.enabled;
            yield return new WaitForSeconds(blinkRate);
            blinkTime += blinkRate;
        }

        flashlight.enabled = false;
        isOn = false;
        isBlinking = false;
        isDisabled = true;
    }
}