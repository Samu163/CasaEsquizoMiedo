using UnityEngine;

public class InteractiveDoorManager : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactionRange = 3f;
    public float maxOpenAngle = 90f;

    private Camera playerCamera;
    private bool isInteracting = false;
    private Transform currentDoor = null;
    private Vector3 initialMousePosition;
    private float doorStartAngle;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryStartInteraction();
        }

        if (Input.GetMouseButton(0) && isInteracting && currentDoor != null)
        {
            RotateDoorWithMouse();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopInteraction();
        }
    }

    void TryStartInteraction()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            if (hit.collider.CompareTag("Door"))
            {
                currentDoor = hit.collider.transform;
                doorStartAngle = currentDoor.localEulerAngles.y;
                initialMousePosition = Input.mousePosition;
                isInteracting = true;
                print("Interaction started with door: " + currentDoor.name);
            }
        }
    }

    void RotateDoorWithMouse()
    {
        Vector3 mouseDelta = Input.mousePosition - initialMousePosition;
        float mouseMovement = mouseDelta.x * 0.2f;

        float newAngle = Mathf.Clamp(doorStartAngle + mouseMovement, doorStartAngle, doorStartAngle + maxOpenAngle);
        currentDoor.localEulerAngles = new Vector3(currentDoor.localEulerAngles.x, newAngle, currentDoor.localEulerAngles.z);
    }

    void StopInteraction()
    {
        isInteracting = false;
        currentDoor = null;
    }
}