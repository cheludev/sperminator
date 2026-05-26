using UnityEngine;

public class OvaryTrigger : MonoBehaviour
{
    public GameObject endScreenCanvas;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Has llegado al ovario - Game Win!");
            
            if (endScreenCanvas != null)
                endScreenCanvas.SetActive(true);
        }
    }
}