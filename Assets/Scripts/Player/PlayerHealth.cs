using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Estadísticas")]
    public int vidas = 3;

    // Referencia opcional al script de movimiento para detenerlo al morir
    private AutoForwardMovement movimiento;

    void Start()
    {
        // Busca el script de movimiento en el XR Origin (padre de la cámara)
        movimiento = GetComponentInParent<AutoForwardMovement>();
    }

    public void RecibirDano()
    {
        vidas--;
        Debug.Log("¡Impacto del virus! Vidas restantes: " + vidas);

        // Feedback visual burdo: Pintar la pantalla de rojo en la consola
        Debug.LogWarning("🔴 PANTALLAZO ROJO (Marcador visual burdo) 🔴");

        if (vidas <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.LogError("💀 ¡GAME OVER! Te quedaste sin vidas. 💀");
        
        // Detenemos el avance del jugador
        if (movimiento != null)
        {
            movimiento.StopMovement();
        }

        // Aquí podrías recargar la escena más adelante
    }
}