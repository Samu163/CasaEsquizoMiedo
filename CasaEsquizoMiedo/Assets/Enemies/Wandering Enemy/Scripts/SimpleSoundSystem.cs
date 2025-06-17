using UnityEngine;

//sistema sonidos para que el enemigo reaccione
public static class SimpleSoundSystem
{
    //evento que escucha el enemigo
    public static System.Action<Vector3, bool> OnSoundEmitted;
    
    /// <summary>
    /// Emite un sonido suave - el enemigo va hacia la posición a velocidad normal
    /// </summary>
    /// <param name="position">Posición donde se emitió el sonido</param>
    public static void EmitSoftSound(Vector3 position)
    {
        OnSoundEmitted?.Invoke(position, false);
        //Debug.Log($"Soft sound emitted at {position}");
    }
    
    /// <summary>
    /// Emite un sonido fuerte - el enemigo va hacia la posición rapidamente
    /// </summary>
    /// <param name="position">Posición donde se emitió el sonido</param>
    public static void EmitLoudSound(Vector3 position)
    {
        OnSoundEmitted?.Invoke(position, true);
        
        //Debug.Log($"Loud sound emitted at {position}");
    }
}