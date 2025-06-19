// File: FlashlightController.cs
using UnityEngine;

public class FlashlightController : FollowCamera
{
    public Light flashlight;
    public KeyCode toggleKey = KeyCode.Mouse0;
    public KeyCode rechargeKey = KeyCode.Mouse1;

    public float onDuration = 10f;
    public float blinkDuration = 2f;
    public float rechargeHoldTime = 3f;

    public Transform flashlightLightObject;
    public Vector3 flashRotationOffset = new Vector3(0f, -0.2f, 0.5f);
    public Animator flashlightAnimator;

    private bool isOn = false;
    private float onTimer = 0f;
    private bool isBlinking = false;
    private bool needsRecharge = false;
    private bool wasOnBeforeDisable = false;

    private float rechargeTimer = 0f;
    private bool isRecharging = false;
    private bool shouldLowerAfterRecharge = false;

    public bool IsBlinking => isBlinking;

    void OnEnable()
    {
        ToggleFlashlight(!isBlinking && wasOnBeforeDisable);
    }

    void OnDisable()
    {
        wasOnBeforeDisable = isOn && !isBlinking;
        ToggleFlashlight(false);
        StopRecharge();
    }

    void Update()
    {
        HandleInput();

        if (isOn && !isBlinking)
        {
            onTimer += Time.deltaTime;
            if (onTimer >= onDuration)
                StartCoroutine(BlinkAndShutdown());
        }

        ApplyCameraRotationToLight();
    }

    void LateUpdate()
    {
        LateFollowUpdate();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(toggleKey) && !isBlinking && !needsRecharge)
        {
            ToggleFlashlight(!isOn);
        }

        if (needsRecharge)
        {
            if (Input.GetKeyDown(rechargeKey)) BeginRecharge();

            if (Input.GetKey(rechargeKey))
            {
                rechargeTimer += Time.deltaTime;
                if (rechargeTimer >= rechargeHoldTime)
                {
                    RechargeFlashlight();
                }
            }
            else if (Input.GetKeyUp(rechargeKey))
            {
                shouldLowerAfterRecharge = true;
                StopRecharge();
            }
        }
    }

    void BeginRecharge()
    {
        if (isRecharging) return;

        rechargeTimer = 0f;
        isRecharging = true;
        shouldLowerAfterRecharge = false;

        flashlightAnimator.ResetTrigger("flashLightDown");
        flashlightAnimator.SetTrigger("flashLightUp");
    }

    void StopRecharge()
    {
        if (!isRecharging) return;

        rechargeTimer = 0f;
        isRecharging = false;

        if (shouldLowerAfterRecharge || !isOn)
        {
            flashlightAnimator.ResetTrigger("flashLightUp");
            flashlightAnimator.SetTrigger("flashLightDown");
        }
    }

    void RechargeFlashlight()
    {
        needsRecharge = false;
        isRecharging = false;
        ToggleFlashlight(true);

        flashlightAnimator.ResetTrigger("flashLightUp");
        flashlightAnimator.SetTrigger("flashLightDown");
    }

    void ToggleFlashlight(bool turnOn)
    {
        isOn = turnOn;
        flashlight.enabled = isOn;
        if (isOn) onTimer = 0f;
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
        needsRecharge = true;
    }

    public void ShutDown()
    {
        flashlight.enabled = false;
        isOn = false;
        needsRecharge = true;
        isBlinking = false;
        wasOnBeforeDisable = false;
        StopRecharge();
    }

    void ApplyCameraRotationToLight()
    {
        if (!flashlightLightObject || !cameraTransform) return;

        Quaternion targetRotation = cameraTransform.rotation * Quaternion.Euler(flashRotationOffset);
        flashlightLightObject.rotation = Quaternion.Slerp(flashlightLightObject.rotation, targetRotation, Time.deltaTime * 10f);
    }
}
