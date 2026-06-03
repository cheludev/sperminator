using UnityEngine;

public class OvaryTrigger : MonoBehaviour
{
    public GameObject endScreenCanvas;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[OvaryTrigger] OnTriggerEnter detectado con objeto: {other.gameObject.name} (Tag: {other.gameObject.tag})");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Has llegado al ovario - Game Win!");

            // Detener el movimiento automático del jugador
            AutoForwardMovement autoMove = FindObjectOfType<AutoForwardMovement>();
            if (autoMove != null)
                autoMove.StopMovement();

            // Ocultar la malla del ovario para que no tape el Canvas de la pantalla final
            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr != null)
                mr.enabled = false;

            // Desactivar colisionadores del ovario para evitar que bloqueen físicamente o repitan eventos
            foreach (Collider c in GetComponents<Collider>())
            {
                c.enabled = false;
            }

            if (endScreenCanvas != null)
            {
                // Posicionar el canvas enfrente de la cámara del jugador (a 3 unidades de distancia)
                Vector3 playerPos = other.transform.position;
                Vector3 playerForward = other.transform.forward;
                
                // Proyectar el vector forward sobre el plano horizontal para mantener el canvas vertical
                playerForward.y = 0f;
                if (playerForward == Vector3.zero)
                {
                    playerForward = other.transform.forward;
                }
                playerForward.Normalize();

                endScreenCanvas.transform.position = playerPos + playerForward * 3f;
                
                // Rotar para mirar al jugador (el forward +Z del canvas apunta alejándose del jugador, por lo que su cara frontal -Z le mira de frente)
                endScreenCanvas.transform.rotation = Quaternion.LookRotation(playerForward);

                endScreenCanvas.SetActive(true);
            }
        }
    }
}