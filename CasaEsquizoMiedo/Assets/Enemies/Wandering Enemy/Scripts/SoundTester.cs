using UnityEngine;

public class SoundTester : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private KeyCode softSoundKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode loudSoundKey = KeyCode.Alpha2;
    [SerializeField] private float testSoundRange = 5f;

    private void Update()
    {
        if (Input.GetKeyDown(softSoundKey))
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * testSoundRange;
            randomPos.y = transform.position.y;
            SimpleSoundSystem.EmitSoftSound(randomPos);
        }

        if (Input.GetKeyDown(loudSoundKey))
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * testSoundRange;
            randomPos.y = transform.position.y;
            SimpleSoundSystem.EmitLoudSound(randomPos);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, testSoundRange);
    }
}