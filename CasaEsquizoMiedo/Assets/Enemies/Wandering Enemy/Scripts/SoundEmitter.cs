using UnityEngine;

public class SoundEmitter : MonoBehaviour
{
    [Header("Sound Settings")]
    [SerializeField] private bool isLoudSound = false;
    [SerializeField] private bool emitOnStart = false;
    [SerializeField] private bool emitOnCollision = false;
    [SerializeField] private float minCollisionForce = 2f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip soundClip;
    
    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        if (emitOnStart)
            EmitSound();
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (emitOnCollision && collision.relativeVelocity.magnitude > minCollisionForce)
        {
            EmitSound();
        }
    }
    
    [ContextMenu("Emit Sound")]
    public void EmitSound()
    {
        if (isLoudSound)
        {
            SimpleSoundSystem.EmitLoudSound(transform.position);
        }
        else
        {
            SimpleSoundSystem.EmitSoftSound(transform.position);
        }
        
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
    }
    
    //public methods to emit sounds
    public void EmitSoftSound()
    {
        SimpleSoundSystem.EmitSoftSound(transform.position);
        PlayAudio();
    }
    
    public void EmitLoudSound()
    {
        SimpleSoundSystem.EmitLoudSound(transform.position);
        PlayAudio();
    }
    
    private void PlayAudio()
    {
        if (audioSource != null && soundClip != null)
        {
            audioSource.PlayOneShot(soundClip);
        }
    }
}