using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton que gestiona la puntuación global.
/// Ponlo en un GameObject vacío llamado "GameManager" en la escena.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int Score { get; private set; } = 0;
    public int ETSKilled { get; private set; } = 0;

    public float DoubleShotTimeRemaining { get; private set; } = 0f;
    public bool IsDoubleShotActive => DoubleShotTimeRemaining > 0f;

    [Header("Audio Settings")]
    [Tooltip("Música de fondo (BGM) para el gameplay.")]
    public AudioClip backgroundMusic;

    [Tooltip("Música de fondo por defecto.")]
    [SerializeField] private AudioClip defaultBackgroundMusic;

    private AudioSource bgmAudioSource;

    void Awake()
    {
        // Singleton: solo existe uno y no se destruye al cambiar de escena
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        PlayBackgroundMusic();
    }

    void Update()
    {
        if (DoubleShotTimeRemaining > 0f)
        {
            DoubleShotTimeRemaining -= Time.deltaTime;
            if (DoubleShotTimeRemaining <= 0f)
            {
                DoubleShotTimeRemaining = 0f;
                Debug.Log("[GameManager] Disparo doble desactivado.");
            }
        }
    }

    public void ActivateDoubleShot(float duration)
    {
        DoubleShotTimeRemaining = Mathf.Max(DoubleShotTimeRemaining, duration);
        Debug.Log($"[GameManager] ¡Disparo doble activado! Tiempo restante: {DoubleShotTimeRemaining}s");
    }

    public void AddScore(int points)
    {
        Score += points;
        ETSKilled++;
        Debug.Log($"Puntuación: {Score} | ETS eliminadas: {ETSKilled}");
    }

    public void ResetScore()
    {
        Score = 0;
        ETSKilled = 0;
        PlayBackgroundMusic();
    }

    public void GoToEndScreen()
    {
        // Carga la escena de pantalla final (añádela en Build Settings)
        SceneManager.LoadScene("EndScreen");
    }

    public void PlayBackgroundMusic()
    {
        AudioClip clipToPlay = backgroundMusic != null ? backgroundMusic : defaultBackgroundMusic;
        if (clipToPlay != null)
        {
            bgmAudioSource = GetComponent<AudioSource>();
            if (bgmAudioSource == null)
            {
                bgmAudioSource = gameObject.AddComponent<AudioSource>();
            }

            if (bgmAudioSource.clip != clipToPlay || !bgmAudioSource.isPlaying)
            {
                bgmAudioSource.clip = clipToPlay;
                bgmAudioSource.loop = true;
                bgmAudioSource.volume = 0.3f;
                bgmAudioSource.spatialBlend = 0f; // Sonido 2D
                bgmAudioSource.playOnAwake = false;
                bgmAudioSource.Play();
            }
        }
        else
        {
            Debug.LogWarning("[GameManager] No se ha asignado música de fondo.");
        }
    }

    public void StopBackgroundMusic()
    {
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        LoadDefaultBGM();
    }

    private void OnValidate()
    {
        if (defaultBackgroundMusic == null)
        {
            LoadDefaultBGM();
        }
    }

    private void LoadDefaultBGM()
    {
        string[] paths = {
            "Assets/Audio/237928__foolboymedia__messy-splat-3a.wav",
            "Assets/Samples/XR Interaction Toolkit/3.5.0/Starter Assets/DemoAssets/Audio/Button Pop.wav"
        };

        foreach (var path in paths)
        {
            defaultBackgroundMusic = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (defaultBackgroundMusic != null)
            {
                break;
            }
        }
    }
#endif
}
