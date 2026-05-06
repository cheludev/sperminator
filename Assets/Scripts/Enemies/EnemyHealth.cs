using UnityEngine;

/// <summary>
/// Add this script to any enemy (ETS) in the scene.
/// When shot, it loses health and dies when health reaches 0.
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
        Debug.Log(gameObject.name + " hit! Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " destroyed! +" + scoreValue + " points");
        // TODO: Add score to GameManager
        // TODO: Add explosion/death effect
        Destroy(gameObject);
    }
}
