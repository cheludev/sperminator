using UnityEngine;

/// <summary>
/// Posiciona un brazo (Arm_L o Arm_R) combinando el movimiento del PlayerRoot
/// con el tracking físico del controller de Meta Quest.
///
/// Úsalo cuando los brazos son hijos directos del PlayerRoot (NO del OVR Camera Rig).
/// Así el brazo avanza con la cámara Y rota con el controller físico.
///
/// HOW TO USE:
/// 1. Saca Arm_L y Arm_R de la jerarquía del Camera Rig — ponlos como hijos de PlayerRoot.
/// 2. Añade este script a cada brazo.
/// 3. En Arm_L: asigna PlayerRoot en "Player Root" y pon Controller = LTouch.
///    En Arm_R: asigna PlayerRoot en "Player Root" y pon Controller = RTouch.
/// </summary>
public class OVRControllerFollower : MonoBehaviour
{
    [Tooltip("Arrastra aquí el objeto PlayerRoot de la Hierarchy.")]
    public Transform playerRoot;

    [Tooltip("LTouch para el brazo IZQUIERDO (Arm_L). RTouch para el DERECHO (Arm_R).")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    void Update()
    {
        if (playerRoot == null) return;

        // Posición del controller en el espacio de tracking (relativa al suelo físico)
        Vector3 localPos = OVRInput.GetLocalControllerPosition(controller);

        // Rotación del controller en el espacio de tracking
        Quaternion localRot = OVRInput.GetLocalControllerRotation(controller);

        // Trasladar esa posición local al espacio del PlayerRoot
        // → así el brazo avanza con el jugador Y sigue al controller físico
        transform.position = playerRoot.TransformPoint(localPos);
        transform.rotation = playerRoot.rotation * localRot;
    }
}
