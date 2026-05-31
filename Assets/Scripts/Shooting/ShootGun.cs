using UnityEngine;

public class ShootGun : MonoBehaviour
{
    [Header("Shooting Settings")]
    public float bulletSpeed = 50f;
    public float damage = 10f;
    public float fireRate = 0.3f;
    public float range = 100f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Controller")]
    [Tooltip("Marca para Arm_L, desmarca para Arm_R")]
    public bool isLeftHand = false;

    [Header("Desktop Testing")]
    [Tooltip("Tecla para disparar este brazo en el editor sin Quest")]
    public KeyCode desktopShootKey = KeyCode.Mouse0;

    [Header("Recoil Animation")]
    public float recoilAmount = 0.05f;
    public float recoilSpeed = 10f;

    private float nextFireTime = 0f;
    private Vector3 originalPosition;
    private bool isRecoiling = false;
    private float recoilTimer = 0f;
    private OVRInput.Controller activeController;

    void Start()
    {
        originalPosition = transform.localPosition;
        activeController = isLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
    }

    void Update()
    {
        bool vrTrigger = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, activeController);
        bool desktopTrigger = Input.GetKeyDown(desktopShootKey);

        if (vrTrigger || desktopTrigger)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        if (isRecoiling)
        {
            recoilTimer += Time.deltaTime * recoilSpeed;
            if (recoilTimer < 1f)
            {
                transform.localPosition = Vector3.Lerp(
                    originalPosition,
                    originalPosition - transform.forward * recoilAmount,
                    recoilTimer);
            }
            else if (recoilTimer < 2f)
            {
                transform.localPosition = Vector3.Lerp(
                    originalPosition - transform.forward * recoilAmount,
                    originalPosition,
                    recoilTimer - 1f);
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
        isRecoiling = true;
        recoilTimer = 0f;

        Vector3 origin = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction;

        // Con Quest: la bala va donde apunta la mano
        if (OVRInput.IsControllerConnected(activeController))
        {
            direction = firePoint != null ? firePoint.forward : transform.forward;
        }
        else
        {
            // Sin Quest: la bala va donde mira la camara
            Camera cam = Camera.main;
            if (cam == null) cam = FindObjectOfType<Camera>();
            Vector3 aimPoint = cam.transform.position + cam.transform.forward * range;
            direction = (aimPoint - origin).normalized;
        }

        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            Debug.Log("[ShootGun] Impacto en: " + hit.collider.gameObject.name);
            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

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

        Debug.DrawRay(origin, direction * range, Color.red, 0.5f);
    }
}