using UnityEngine;

public class AutoForwardMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad a la que el jugador avanza por el túnel.")]
    public float forwardSpeed = 20f;
    
    [Tooltip("Interruptor para pausar o reanudar el avance.")]
    public bool isMoving = true;

    void Update()
    {
        if (isMoving)
        {
            // Mueve el objeto hacia adelante (en su eje Z local) constantemente
            // Time.deltaTime asegura que el movimiento sea fluido sin importar los FPS
            transform.position += transform.forward * forwardSpeed * Time.deltaTime;
        }
    }

    // Función extra por si quieres detener al jugador al llegar a la meta o al morir
    public void StopMovement()
    {
        isMoving = false;
    }

    public void ResumeMovement()
    {
        isMoving = true;
    }
}