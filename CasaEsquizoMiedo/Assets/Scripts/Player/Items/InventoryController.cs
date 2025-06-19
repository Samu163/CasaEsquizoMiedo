using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private GameObject[] equipableObjects; 
    public FlashlightController flashLight; 
    public CameraController cameraItem; 
    private int currentIndex = 0;

    [SerializeField] private float switchCooldown = 0.5f;
    private float lastSwitchTime = -10f;

    private void Awake()
    {
        equipableObjects = new GameObject[2];
        equipableObjects[0] = flashLight.gameObject; 
        equipableObjects[1] = cameraItem.gameObject; 
    }

    void Start()
    {
        UpdateEquippedObject();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if ((Mathf.Abs(scroll) > 0.05f && Time.time - lastSwitchTime >= switchCooldown) && !cameraItem.IsZoomed)
        {
            lastSwitchTime = Time.time;
            currentIndex = (currentIndex + (scroll > 0 ? 1 : -1) + equipableObjects.Length) % equipableObjects.Length;
            UpdateEquippedObject();
        }
    }

    void UpdateEquippedObject()
    {
        for (int i = 0; i < equipableObjects.Length; i++)
        {
            if (equipableObjects[i])
                equipableObjects[i].SetActive(i == currentIndex);
            if (equipableObjects[i] == flashLight.gameObject && flashLight.IsBlinking)
                flashLight.ShutDown();
        }
    }
}