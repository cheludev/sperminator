using UnityEngine;

public class ClitorisCollectible : MonoBehaviour
{
    [Header("Settings")]
    public float doubleShotDuration = 30f;

    public void Collect()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject);
            Collect();
        }
    }
}
