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
        // 1. Si nos pega una bala
        if (other.CompareTag("Bullet"))
        {
            if (healthSystem != null)
            {
                healthSystem.TakeDamage(10f); 
            }
            Destroy(other.gameObject);
        }

        // 2. Si chocamos con el jugador (¡NUEVO!)
        if (other.CompareTag("Player"))
        {
            // Buscamos el script de vida en el jugador
            PlayerHealth saludJugador = other.GetComponent<PlayerHealth>();
            
            if (saludJugador != null)
            {
                saludJugador.RecibirDano();
            }

            // Destruimos el virus (Si no lo destruyes, chocará 60 veces por segundo y te matará al instante)
            Destroy(gameObject);
        }
    }
}