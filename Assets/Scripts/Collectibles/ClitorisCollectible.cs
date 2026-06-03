using UnityEngine;

public class ClitorisCollectible : MonoBehaviour
{
    [Header("Settings")]
    public float doubleShotDuration = 30f;

    [Header("Audio Settings")]
    [Tooltip("Sonido personalizado para reproducir al dispararle a la bola. Si no se asigna, se usará el sonido por defecto.")]
    public AudioClip customSound;

    [Tooltip("Sonido por defecto que se reproduce si customSound es nulo.")]
    public AudioClip defaultSound;

    [Range(0f, 1f)]
    [Tooltip("Volumen de reproducción del sonido.")]
    public float soundVolume = 0.5f;

    private bool isCollected = false;

    public void Collect()
    {
        if (isCollected) return;
        isCollected = true;

        PlayHitSound();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActivateDoubleShot(doubleShotDuration);
        }
        else
        {
            Debug.LogWarning("[ClitorisCollectible] GameManager.Instance is null!");
        }

        // Destruir el objeto clítoris recolectado
        Destroy(gameObject);
    }

    private void PlayHitSound()
    {
        AudioClip soundToPlay = customSound != null ? customSound : defaultSound;
        if (soundToPlay != null)
        {
            AudioSource.PlayClipAtPoint(soundToPlay, transform.position, soundVolume);
        }
        else
        {
            Debug.LogWarning("[ClitorisCollectible] No se ha asignado ningún clip de audio.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            Collect();
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        LoadDefaultSound();
    }

    private void OnValidate()
    {
        if (defaultSound == null)
        {
            LoadDefaultSound();
        }
    }

    private void LoadDefaultSound()
    {
        string[] paths = {
            "Assets/Audio/237928__foolboymedia__messy-splat-3a.wav",
            "Assets/Samples/XR Interaction Toolkit/3.5.0/Starter Assets/DemoAssets/Audio/Button Pop.wav"
        };

        foreach (var path in paths)
        {
            defaultSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (defaultSound != null)
            {
                break;
            }
        }
    }
#endif
}
