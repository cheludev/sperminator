using UnityEngine;

public class VirusSpawner : MonoBehaviour
{
    [Header("Configuración del Enemigo")]
    [Tooltip("Arrastra aquí el Prefab del Virus")]
    public GameObject virusPrefab;
    
    [Tooltip("Tiempo en segundos entre la aparición de cada virus")]
    public float spawnInterval = 2f;
    private float timer = 0f;

    [Header("Área de Generación")]
    [Tooltip("Distancia por delante del jugador donde aparecerá el virus")]
    public float distanceAhead = 25f; 
    
    [Tooltip("Radio del tubo (para que no aparezcan fuera de las paredes)")]
    public float tubeRadius = 3f;

    void Update()
    {
        // El temporizador avanza con el tiempo real
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            GenerarVirus();
            timer = 0f; // Reiniciar el temporizador
        }
    }

    void GenerarVirus()
    {
        // 1. Calculamos una posición aleatoria dentro de un círculo (sección transversal del túnel)
        Vector2 randomCircle = Random.insideUnitCircle * tubeRadius;
        
        // 2. Definimos el punto base (la posición del Spawner + X metros hacia adelante)
        Vector3 spawnPos = transform.position + (transform.forward * distanceAhead);
        
        // 3. Aplicamos el desvío aleatorio para que no salgan todos en el centro
        spawnPos += (transform.right * randomCircle.x) + (transform.up * randomCircle.y);

        // 4. Creamos el virus en esa posición
        Instantiate(virusPrefab, spawnPos, Quaternion.identity);
    }
}