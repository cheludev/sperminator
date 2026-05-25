using UnityEngine;

public class VirusAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private EnemyHealth healthSystem;

    [Header("Movimiento")]
    [Tooltip("ESTA VELOCIDAD DEBE SER MAYOR A LA DEL JUGADOR")]
    public float speed = 20f; 

   void Start()
    {
        healthSystem = GetComponent<EnemyHealth>();
        
        // Buscamos al jugador por el tag que le pusimos para las colisiones
        if (player == null)
        {
            GameObject jugadorEncontrado = GameObject.FindGameObjectWithTag("Player");
            if (jugadorEncontrado != null)
            {
                player = jugadorEncontrado.transform;
            }
            else
            {
                Debug.LogError("¡No se ha encontrado ningún objeto con el tag 'Player'!");
            }
        }
    }

    void Update()
    {
        if (player != null)
        {
            // 1. Miramos a la cámara
            transform.LookAt(player);

            // 2. Volamos directos hacia ella. 
            // Como ya es Kinematic, nada va a frenar este movimiento.
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            if (healthSystem != null) healthSystem.TakeDamage(10f); 
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth saludJugador = other.GetComponent<PlayerHealth>();
            if (saludJugador != null) saludJugador.RecibirDano();
            Destroy(gameObject);
        }
    }
}