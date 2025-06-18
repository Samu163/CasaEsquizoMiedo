using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public GameObject[] equipableObjects; 
    private int currentIndex = 0;

    [SerializeField] private float switchCooldown = 0.5f;
    private float lastSwitchTime = -10f;

    void Start()
    {
        UpdateEquippedObject();
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.05f && Time.time - lastSwitchTime >= switchCooldown)
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
        }
    }
}