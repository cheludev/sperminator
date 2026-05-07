using UnityEngine;

public class VirusAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;         // El jugador a perseguir
    private EnemyHealth healthSystem; // El script de tu compañero

    [Header("Movimiento")]
    public float speed = 3f;
    public float stoppingDistance = 1.5f;

    void Start()
    {
        // Buscamos el script de salud que está en este mismo objeto
        healthSystem = GetComponent<EnemyHealth>();
        
        // Si no has asignado al jugador manualmente, buscamos la cámara principal
        if (player == null && Camera.main != null)
        {
            player = Camera.main.transform;
        }
    }

    void Update()
    {
        if (player != null)
        {
            MoverHaciaJugador();
        }
    }

    void MoverHaciaJugador()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > stoppingDistance)
        {
            // Rotar suavemente hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);

            // Avanzar
            transform.position += direction * speed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Si nos toca algo con el tag "Bullet"
        if (other.CompareTag("Bullet"))
        {
            // ¡Aquí ocurre la magia de la colaboración!
            // Llamamos a la función de tu compañero
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(10f); 
            }
            
            // Destruimos la bala
            Destroy(other.gameObject);
        }
    }
}