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

        // TODO: Añadir efecto de muerte (partículas, sonido, etc.)
        Destroy(gameObject);
    }
}