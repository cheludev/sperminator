using UnityEngine;

/// <summary>
/// Añade este script a cada enemigo (ETS) de la escena.
/// Cuando lo matan, suma puntos al GameManager.
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    public float currentHealth;
    public int scoreValue = 100;

    [Header("Audio")]
    public AudioClip deathSound;
    private static AudioClip cachedDefaultDeathSound;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " hit! Vida: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Suma puntos al GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.AddScore(scoreValue);

        Debug.Log(gameObject.name + " eliminada! +" + scoreValue + " puntos");

        // Reproducir sonido de muerte
        AudioClip clipToPlay = deathSound;
        if (clipToPlay == null)
        {
            if (cachedDefaultDeathSound == null)
            {
                cachedDefaultDeathSound = CreateDefaultDeathSound();
            }
            clipToPlay = cachedDefaultDeathSound;
        }

        if (clipToPlay != null)
        {
            AudioSource.PlayClipAtPoint(clipToPlay, transform.position);
        }

        // TODO: Añadir efecto de muerte (partículas, etc.)
        Destroy(gameObject);
    }

    private AudioClip CreateDefaultDeathSound()
    {
        int frequency = 44100;
        float duration = 0.15f;
        int samplesCount = Mathf.RoundToInt(frequency * duration);
        float[] data = new float[samplesCount];

        for (int i = 0; i < samplesCount; i++)
        {
            float t = (float)i / frequency;
            // Frecuencia descendente (sweep) para un sonido tipo "zap/pop" retro
            float freq = Mathf.Lerp(600f, 150f, t / duration);
            // Atenuación exponencial
            float envelope = Mathf.Exp(-5f * (t / duration));
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope;
        }

        AudioClip clip = AudioClip.Create("ProceduralDeathBeep", samplesCount, 1, frequency, false);
        clip.SetData(data, 0);
        return clip;
    }
}