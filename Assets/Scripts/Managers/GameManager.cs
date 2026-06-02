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
    }

    public void GoToEndScreen()
    {
        // Carga la escena de pantalla final (añádela en Build Settings)
        SceneManager.LoadScene("EndScreen");
    }
}
