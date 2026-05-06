using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// HOW TO USE:
/// 1. Drag this script into Assets/Scripts/Shooting
/// 2. Add this script to both Arm_L and Arm_R
/// 3. In the Inspector, assign the Trigger Action:
///    - For Arm_L: XRI LeftHand > Activate (or trigger)
///    - For Arm_R: XRI RightHand > Activate (or trigger)
/// 4. Set bulletSpeed and damage as you like
/// </summary>
public class ShootGun : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float bulletSpeed = 50f;
    public float damage = 10f;
    public float fireRate = 0.3f;
    public float range = 100f;

    [Header("Bullet (optional)")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Input")]
    public InputActionReference triggerAction;

    [Header("Recoil Animation")]
    public float recoilAmount = 0.05f;
    public float recoilSpeed = 10f;

    private float nextFireTime = 0f;
    private Vector3 originalPosition;
    private bool isRecoiling = false;
    private float recoilTimer = 0f;

    void Start()
    {
        originalPosition = transform.localPosition;

        if (triggerAction != null)
        {
            triggerAction.action.Enable();
        }
    }

    void Update()
    {
        // Check trigger input
        if (triggerAction != null && triggerAction.action.WasPressedThisFrame())
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        // Recoil animation
        if (isRecoiling)
        {
            recoilTimer += Time.deltaTime * recoilSpeed;
            if (recoilTimer < 1f)
            {
                // Move back
                transform.localPosition = Vector3.Lerp(originalPosition,
                    originalPosition - transform.forward * recoilAmount, recoilTimer);
            }
            else if (recoilTimer < 2f)
            {
                // Return to original
                transform.localPosition = Vector3.Lerp(
                    originalPosition - transform.forward * recoilAmount,
                    originalPosition, recoilTimer - 1f);
            }
            else
            {
                transform.localPosition = originalPosition;
                isRecoiling = false;
                recoilTimer = 0f;
            }
        }
    }

    void Shoot()
    {
        // Start recoil
        isRecoiling = true;
        recoilTimer = 0f;

        // Determine fire origin
        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = firePoint != null ? firePoint.forward : transform.forward;

        // Option A: Raycast (simple, no bullet visible)
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);

            // Check if we hit an enemy
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Option B: Spawn bullet (if prefab assigned)
        if (bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, origin, Quaternion.LookRotation(direction));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = direction * bulletSpeed;
            }
            Destroy(bullet, 3f);
        }

        // Draw debug ray in Scene view
        Debug.DrawRay(origin, direction * range, Color.red, 0.5f);
    }
}
