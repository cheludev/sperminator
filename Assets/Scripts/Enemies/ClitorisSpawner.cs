using UnityEngine;

public class ClitorisSpawner : MonoBehaviour
{
    [Header("Configuración del Clítoris")]
    [Tooltip("Arrastra aquí el Prefab del Clítoris")]
    public GameObject clitorisPrefab;
    
    [Tooltip("Tiempo en segundos entre la aparición de cada clítoris")]
    public float spawnInterval = 15f;
    private float timer = 0f;

    [Header("Área de Generación")]
    [Tooltip("Distancia por delante del jugador donde aparecerá el clítoris")]
    public float distanceAhead = 35f; 
    
    [Tooltip("Radio del tubo (para que no aparezcan fuera de las paredes)")]
    public float tubeRadius = 3f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            GenerarClitoris();
            timer = 0f;
        }
    }

    void GenerarClitoris()
    {
        if (clitorisPrefab == null) return;

        // 1. Calculamos una posición aleatoria dentro de un círculo (sección transversal del túnel)
        Vector2 randomCircle = Random.insideUnitCircle * tubeRadius;
        
        // 2. Definimos el punto base (la posición del Spawner + X metros hacia adelante)
        Vector3 spawnPos = transform.position + (transform.forward * distanceAhead);
        
        // 3. Aplicamos el desvío aleatorio para que no salgan todos en el centro
        spawnPos += (transform.right * randomCircle.x) + (transform.up * randomCircle.y);

        // 4. Creamos el clítoris en esa posición
        Instantiate(clitorisPrefab, spawnPos, Quaternion.identity);
    }
}
