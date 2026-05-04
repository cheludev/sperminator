using UnityEngine;
using UnityEngine.SceneManagement;

public class OvaryTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Has llegado al ovario - Game Win!");
            SceneManager.LoadScene("GameOver");
        }
    }
}