using UnityEngine;

public class VirusAI : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    private EnemyHealth healthSystem;

    [Header("Movimiento")]
    public float speed = 12f; 

    // 1. Creamos una variable para saber si el virus ya está en proceso de morir
    private bool yaEstaMuerto = false;

    void Start()
    {
        healthSystem = GetComponent<EnemyHealth>();
        if (player == null)
        {
            GameObject jugadorEncontrado = GameObject.FindGameObjectWithTag("Player");
            if (jugadorEncontrado != null) player = jugadorEncontrado.transform;
        }
    }

    void Update()
    {
        // 2. Si ya está muerto, dejamos de movernos hacia el jugador
        if (yaEstaMuerto) return;

        if (player != null)
        {
            transform.LookAt(player);
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Si este virus ya está marcado para destruirse, ignoramos cualquier otra colisión
        if (yaEstaMuerto) return;

        if (other.CompareTag("Bullet"))
        {
            Destroy(other.gameObject); // Destruye la bala

            if (healthSystem != null) 
            {
                healthSystem.TakeDamage(10f); 
                
                // Comprobamos la salud usando 'currentHealth' con la 'c' minúscula
                if (healthSystem.currentHealth <= 0) 
                {
                    // Apagamos sus colisiones en el acto para que sea inofensivo mientras se borra
                    yaEstaMuerto = true; 
                    GetComponent<Collider>().enabled = false; 
                }
            }
        }

        if (other.CompareTag("Player"))
        {
            PlayerHealth saludJugador = other.GetComponent<PlayerHealth>();
            if (saludJugador != null) saludJugador.RecibirDano();
            
            yaEstaMuerto = true; // Evita que este mismo virus te vuelva a hacer daño en el mismo frame
            Destroy(gameObject);
        }
    }
}