using UnityEngine;
using TMPro; // <--- ¡IMPORTANTE! Añadimos la librería de TextMeshPro

public class PlayerHealth : MonoBehaviour
{
    [Header("Estadísticas")]
    public int vidas = 3;

    [Header("Interfaz Visual")]
    public TextMeshPro textoVidas; // Arrastra aquí el objeto HUD_Vidas

    private AutoForwardMovement movimiento;

    void Start()
    {
        movimiento = GetComponentInParent<AutoForwardMovement>();
        
        // Al empezar, actualizamos el texto por primera vez
        ActualizarInterfaz();
    }

    public void RecibirDano()
    {
        if (vidas <= 0) return; // Evita que baje de 0 si ya moriste

        vidas--;
        ActualizarInterfaz();

        if (vidas <= 0)
        {
            Morir();
        }
    }

    void ActualizarInterfaz()
    {
        if (textoVidas != null)
        {
            textoVidas.text = "Vidas: " + vidas;
            
            // Un toque extra: Si le queda 1 vida, poner el texto en rojo
            if (vidas == 1) 
                textoVidas.color = Color.red;
            else 
                textoVidas.color = Color.white;
        }
    }

    void Morir()
    {
        if (textoVidas != null) textoVidas.text = "GAME OVER";
        
        if (movimiento != null)
        {
            movimiento.StopMovement();
        }
        Debug.LogError("💀 GAME OVER");
    }
}