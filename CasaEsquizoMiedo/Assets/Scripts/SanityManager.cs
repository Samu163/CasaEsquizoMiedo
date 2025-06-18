using UnityEngine;
using TMPro;

public class SanityManager : MonoBehaviour
{
    [Header("Sanity Configuration")]
    public int maxSanity = 100;
    public float timeToDecreaseSanity = 5f;
    public TextMeshProUGUI sanityText;

    private float timer = 0f;

    void Start()
    {
        UpdateSanityUI();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= timeToDecreaseSanity)
        {
            if (maxSanity > 0)
            {
                maxSanity--;
                UpdateSanityUI();
            }

            timer = 0f;
        }
    }

    private void UpdateSanityUI()
    {
        sanityText.text = $"Sanity: {maxSanity}";
    }
}
