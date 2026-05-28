using UnityEngine;

public class OvaryTrigger : MonoBehaviour
{
    public GameObject endScreenCanvas;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Has llegado al ovario - Game Win!");

            // Detener el movimiento automático del jugador
            AutoForwardMovement autoMove = FindObjectOfType<AutoForwardMovement>();
            if (autoMove != null)
                autoMove.StopMovement();

            if (endScreenCanvas != null)
                endScreenCanvas.SetActive(true);
        }
    }
}