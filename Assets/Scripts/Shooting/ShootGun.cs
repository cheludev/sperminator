using UnityEngine;

/// <summary>
/// Gestiona el disparo desde un brazo (Arm_L o Arm_R).
/// Usa OVRInput directamente — compatible con Meta SDK / OVR Camera Rig.
///
/// HOW TO USE:
/// 1. Añade este script a Arm_L y a Arm_R en el Inspector.
/// 2. En Arm_L: marca la casilla "Is Left Hand" en el Inspector.
///    En Arm_R: déjala desmarcada (mano derecha por defecto).
/// 3. Asigna "Bullet Prefab" (PlasmaBullet) y "Fire Point" (Muzzle_L / Muzzle_R).
/// 4. Ajusta "Bullet Speed", "Damage" y "Fire Rate" según gustos.
/// </summary>
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
    [Tooltip("Marca esta casilla en el Inspector para el brazo IZQUIERDO (Arm_L). " +
             "Desmarca para el brazo DERECHO (Arm_R).")]
    public bool isLeftHand = false;

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

        // Selecciona el controller según qué mano es este brazo
        activeController = isLeftHand ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
    }

    void Update()
    {
        // Detecta el trigger del controller correspondiente con OVRInput
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, activeController) 
    || Input.GetMouseButtonDown(0))
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }

        // Animación de retroceso (recoil)
        if (isRecoiling)
        {
            recoilTimer += Time.deltaTime * recoilSpeed;

            if (recoilTimer < 1f)
            {
                // Fase 1: mover hacia atrás
                transform.localPosition = Vector3.Lerp(
                    originalPosition,
                    originalPosition - transform.forward * recoilAmount,
                    recoilTimer);
            }
            else if (recoilTimer < 2f)
            {
                // Fase 2: volver a la posición original
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
        // Iniciar animación de retroceso
        isRecoiling = true;
        recoilTimer = 0f;

        Vector3 origin    = firePoint != null ? firePoint.position : transform.position;
        Vector3 direction = firePoint != null ? firePoint.forward  : transform.forward;

        // Raycast: daño instantáneo al impactar (funciona aunque la bala no llegue visualmente)
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            Debug.Log($"[ShootGun] Impacto en: {hit.collider.gameObject.name}");

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

        // Bala física visible (solo si hay prefab asignado)
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

        // Rayo de debug visible en la ventana Scene de Unity
        Debug.DrawRay(origin, direction * range, Color.red, 0.5f);
    }
}
