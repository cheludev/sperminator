using UnityEngine;

public class OvaryTrigger : MonoBehaviour
{
    public GameObject endScreenCanvas;

    [Header("Audio Settings")]
    [Tooltip("Sonido personalizado para reproducir al llegar al ovario. Si no se asigna, se usará el sonido por defecto.")]
    public AudioClip customSound;

    [Tooltip("Sonido por defecto que se reproduce si customSound es nulo.")]
    public AudioClip defaultSound;

    [Range(0f, 1f)]
    [Tooltip("Volumen de reproducción del sonido.")]
    public float soundVolume = 0.5f;

    private bool hasTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[OvaryTrigger] OnTriggerEnter detectado con objeto: {other.gameObject.name} (Tag: {other.gameObject.tag})");
        if (hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            PlayWinSound();

            Debug.Log("Has llegado al ovario - Game Win!");

            // Detener el movimiento automático del jugador
            AutoForwardMovement autoMove = FindObjectOfType<AutoForwardMovement>();
            if (autoMove != null)
                autoMove.StopMovement();

            // Detener la música de fondo del gameplay para el final
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StopBackgroundMusic();
            }

            // Ocultar la malla del ovario para que no tape el Canvas de la pantalla final
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
                mr.enabled = false;

            // Desactivar colisionadores del ovario para evitar que bloqueen físicamente o repitan eventos
            foreach (Collider c in GetComponents<Collider>())
            {
                c.enabled = false;
            }

            if (endScreenCanvas != null)
            {
                // Posicionar el canvas enfrente de la cámara del jugador (más cerca, a 1.2 unidades, para que no quede detrás de la pared final del túnel)
                Vector3 playerPos = other.transform.position;
                Vector3 playerForward = other.transform.forward;
                
                // Proyectar el vector forward sobre el plano horizontal para mantener el canvas vertical
                playerForward.y = 0f;
                if (playerForward == Vector3.zero)
                {
                    playerForward = other.transform.forward;
                }
                playerForward.Normalize();

                endScreenCanvas.transform.position = playerPos + playerForward * 1.2f;
                
                // Rotar para mirar al jugador (el forward +Z del canvas apunta alejándose del jugador, por lo que su cara frontal -Z le mira de frente)
                endScreenCanvas.transform.rotation = Quaternion.LookRotation(playerForward);

                endScreenCanvas.SetActive(true);
            }
        }
    }

    private void PlayWinSound()
    {
        AudioClip soundToPlay = customSound != null ? customSound : defaultSound;
        if (soundToPlay != null)
        {
            AudioSource.PlayClipAtPoint(soundToPlay, transform.position, soundVolume);
        }
        else
        {
            Debug.LogWarning("[OvaryTrigger] No se ha asignado ningún clip de audio.");
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