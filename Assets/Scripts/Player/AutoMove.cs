using UnityEngine;

/// <summary>
/// Moves the player automatically forward through the tunnel.
/// Add this to XR Origin (VR).
/// 
/// The player moves constantly towards the target (Ovary).
/// Speed can be adjusted in the Inspector.
/// </summary>
public class AutoMove : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 3f;
    public Transform target; // Drag the Ovary here

    [Header("Optional")]
    public bool stopAtTarget = true;
    public float stopDistance = 2f;

    void Update()
    {
        if (target == null) return;

        float distance = Vector3.Distance(transform.position, target.position);

        if (stopAtTarget && distance <= stopDistance)
        {
            // Player reached the ovary
            return;
        }

        // Move towards target
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
    }
}
