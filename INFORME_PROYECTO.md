# INFORME DEL PROYECTO SPERMINATOR
*Análisis generado el 2026-05-28*

---

## 1. ESTRUCTURA DEL PROYECTO

### 1.1 Árbol de carpetas Assets

```
Assets/
├── Arm_L.fbx                          ← Modelo brazo izquierdo
├── Arm_R.fbx                          ← Modelo brazo derecho
├── Audio/                             ← VACÍA (sin archivos de audio)
├── Materials/
│   ├── BulletMaterial.mat             ← Material de la bala
│   ├── Mar_paredes_tubo.mat           ← Material paredes del túnel (_Cull: 0)
│   ├── Mat_fondo_tubo.mat             ← Material fondo del túnel (_Cull: 0)
│   ├── OvaryMaterial.mat              ← Material del óvulo
│   ├── TunnelMaterial.mat             ← Material principal del túnel (_Cull: 0)
│   ├── circulo.png                    ← Textura
│   └── paredes.png                    ← Textura
├── Meshy_AI_Twin_White_Pistols_.../   ← Modelo 3D pistola doble con texturas PBR
│   ├── Meshy_AI_Twin_White_Pistols_...fbx
│   ├── .../Materials/                 ← Mat: base, emisión, metálico
│   └── .../Textures/                  ← diffuse, normal, roughness, metallic, emission
├── Models/                            ← VACÍA
├── Oculus/
│   └── OculusProjectConfig.asset
├── Plugins/
│   └── Android/
│       └── AndroidManifest.xml
├── Prefab/
│   ├── PlasmaBullet.prefab            ← Prefab de bala (tag: Bullet)
│   └── Virus.prefab                   ← Prefab de enemigo (tag: Enemy)
├── Resources/
│   ├── ImmersiveDebuggerSettings.asset
│   ├── InputActions.asset             ← VACÍO (sin definiciones)
│   ├── MetaXRAcousticMaterial...asset
│   ├── MetaXRAudioSettings.asset
│   ├── OVROverlayCanvasSettings.asset
│   ├── OVRPlatformToolSettings.asset
│   └── OculusRuntimeSettings.asset
├── Samples/
│   └── XR Interaction Toolkit/3.5.0/
│       ├── Starter Assets/            ← Assets de ejemplo del XR IT
│       │   └── XRI Default Input Actions.inputactions  ← Acciones de entrada usadas por ShootGun
│       └── XR Device Simulator/
├── Scenes/
│   ├── Game.unity                     ← ESCENA PRINCIPAL
│   └── SampleScene.unity
├── Scripts/
│   ├── Enemies/
│   │   ├── EnemyHealth.cs
│   │   ├── VirusAI.cs
│   │   └── VirusSpawner.cs
│   ├── Managers/
│   │   ├── EndScreenUI.cs
│   │   ├── GameManager.cs
│   │   ├── OvaryScript.cs
│   │   └── ScoreUI.cs
│   ├── Player/
│   │   ├── AutoForwardMovement.cs
│   │   ├── AutoMove.cs               ← Existe pero NO se usa en la escena
│   │   └── PlayerHealth.cs
│   └── Shooting/
│       └── ShootGun.cs
└── TextMesh Pro/                      ← Assets de ejemplo de TMP (no relevantes)
```

### 1.2 Archivos por categoría

**Scripts del juego (10 total):**
- Enemies: EnemyHealth.cs, VirusAI.cs, VirusSpawner.cs
- Managers: EndScreenUI.cs, GameManager.cs, OvaryScript.cs, ScoreUI.cs
- Player: AutoForwardMovement.cs, AutoMove.cs (sin usar), PlayerHealth.cs
- Shooting: ShootGun.cs

**Prefabs:** PlasmaBullet.prefab, Virus.prefab

**Escenas:** Game.unity (principal), SampleScene.unity (vacía/default)

**Modelos:** Arm_L.fbx, Arm_R.fbx, Meshy_AI pistola doble

**Materiales:** 5 materiales (.mat) + 2 texturas (.png)

**Audio:** NINGUNO (carpeta vacía)

---

## 2. SCRIPTS EXISTENTES

### `EnemyHealth.cs`
- **Ubicación:** Assets/Scripts/Enemies/EnemyHealth.cs
- **Propósito:** Gestiona la vida de cada enemigo (ETS). Recibe daño, muere y suma puntos al GameManager.
- **Clases/métodos principales:**
  - `TakeDamage(float amount)` — reduce vida, llama Die() si llega a 0
  - `Die()` — destruye el GameObject, llama a GameManager.Instance.AddScore()
- **Dependencias:** `GameManager` (singleton)
- **Estado:** ✅ Parece OK. Lógica limpia, sin referencias a XR.

---

### `VirusAI.cs`
- **Ubicación:** Assets/Scripts/Enemies/VirusAI.cs
- **Propósito:** Mueve cada virus hacia el jugador. Al entrar en colisión con balas recibe daño; al tocar al jugador le quita una vida.
- **Clases/métodos principales:**
  - `Start()` — busca al jugador por tag "Player"
  - `Update()` — mira y avanza hacia el jugador con MoveTowards
  - `OnTriggerEnter(Collider other)` — detecta tags "Bullet" y "Player"
- **Dependencias:** `EnemyHealth`, `PlayerHealth`. Busca GameObject con tag "Player".
- **Estado:** ⚠️ PROBLEMA POTENCIAL. El tag "Player" está asignado a "Main Camera" dentro del XR Origin (VR), que es el sistema XR genérico. Con OVR Camera Rig activo simultáneamente, la cámara que está activa en realidad es RightEyeAnchor/LeftEyeAnchor del OVR, no la Main Camera. Dado que la Main Camera existe en la escena y tiene el collider (SphereCollider trigger, radio 0.3), debería funcionar siempre que el XR Origin (VR) también esté activo y moviéndose.

---

### `VirusSpawner.cs`
- **Ubicación:** Assets/Scripts/Enemies/VirusSpawner.cs
- **Propósito:** Genera virus periódicamente delante del jugador, dispersados aleatoriamente dentro del radio del túnel.
- **Clases/métodos principales:**
  - `Update()` — temporizador que cada `spawnInterval` llama a `GenerarVirus()`
  - `GenerarVirus()` — instancia el prefab del virus en posición aleatoria dentro del cilindro
- **Dependencias:** `virusPrefab` (referencia asignada en Inspector — Virus.prefab), usa `transform.position` del objeto padre.
- **Estado:** ⚠️ PROBLEMA CRÍTICO. El VirusSpawner está adjunto al XR Origin (VR) como objeto hijo (dentro de Camera Offset). Esto significa que los virus spawnean relativo a la posición del XR Origin. El AutoForwardMovement mueve el XR Origin, así que el spawner se mueve con él — eso es CORRECTO en principio. Sin embargo, hay dos sistemas VR activos simultáneamente. El jugador "real" (desde el punto de vista del HMD) es el OVR Camera Rig, que está en posición (0,0,0) fija. El XR Origin tiene el AutoForwardMovement pero no es la fuente de la cámara activa. Los virus persiguen al "Player" (Main Camera del XR Origin) pero la vista del jugador viene del OVR Camera Rig — posible desfase espacial.

---

### `GameManager.cs`
- **Ubicación:** Assets/Scripts/Managers/GameManager.cs
- **Propósito:** Singleton que gestiona la puntuación global, número de ETS eliminados, y transición a pantalla final.
- **Clases/métodos principales:**
  - `Awake()` — patrón Singleton con DontDestroyOnLoad
  - `AddScore(int points)` — suma puntos y ETS killed
  - `ResetScore()` — reinicia contadores
  - `GoToEndScreen()` — carga escena "EndScreen"
- **Dependencias:** `SceneManager`. Ninguna dependencia XR.
- **Estado:** ✅ Parece OK. El GameManager en escena está en posición (0, -1.1176, 18.49875) que es exactamente la misma posición que la bala en su prefab — puede ser coincidencia o remanente de configuración. Singleton implementado correctamente.

---

### `OvaryScript.cs` (clase `OvaryTrigger`)
- **Ubicación:** Assets/Scripts/Managers/OvaryScript.cs
- **Propósito:** Detecta cuando el jugador llega al óvulo y activa la pantalla final.
- **Clases/métodos principales:**
  - `OnTriggerEnter(Collider other)` — comprueba tag "Player", activa endScreenCanvas
- **Dependencias:** `endScreenCanvas` (referencia asignada en Inspector — apunta a "EndScreen" canvas).
- **Estado:** ⚠️ PROBLEMA. El Ovary tiene un SphereCollider trigger (además de uno normal). El trigger compara con tag "Player". El "Player" es la Main Camera del XR Origin (VR), que tiene un SphereCollider trigger propio. Sin embargo, la Main Camera no tiene Rigidbody, y el Ovary tampoco. Para que OnTriggerEnter funcione, al menos uno de los dos objetos involucrados debe tener Rigidbody. Falta verificar si Physics.queriesHitTriggers está habilitado. ADICIONALMENTE, el Ovary está en posición (0, 0, -100) con escala (3,3,3) y el XR Origin en (0, 0, 30) moviéndose en dirección forward. Si el forward del XR Origin apunta en -Z (tiene rotación m_LocalRotation: x: -1, y: 0, z: 0 = 180° en X = mirando hacia abajo), hay un bug de orientación. Necesita revisión.

---

### `ScoreUI.cs`
- **Ubicación:** Assets/Scripts/Managers/ScoreUI.cs
- **Propósito:** Actualiza el texto de puntuación en el HUD en cada frame.
- **Clases/métodos principales:**
  - `Update()` — lee GameManager.Instance.Score y ETSKilled, actualiza TextMeshProUGUI
- **Dependencias:** `GameManager`, `TextMeshProUGUI` (TMP).
- **Estado:** ✅ Parece OK. Las referencias `scoreText` y `etsKilledText` están asignadas en la escena (fileIDs 1908658712 y 1810974912).

---

### `EndScreenUI.cs`
- **Ubicación:** Assets/Scripts/Managers/EndScreenUI.cs
- **Propósito:** Muestra la pantalla de fin de juego con puntuación, ETS eliminados y rango.
- **Clases/métodos principales:**
  - `Start()` — lee datos del GameManager y rellena los textos
  - `GetRank(int score)` — calcula rango S/A/B/C/D
  - `PlayAgain()` / `QuitGame()` — botones de reinicio y salida
- **Dependencias:** `GameManager`, `TextMeshProUGUI`, `SceneManager`. Intenta cargar escena "MainGame" al reiniciar.
- **Estado:** ⚠️ PROBLEMA MENOR. El nombre de escena al reiniciar es "MainGame" (`SceneManager.LoadScene("MainGame")`), pero la escena principal se llama "Game" (Assets/Scenes/Game.unity). Si el jugador intenta "Jugar de Nuevo", fallará con "Scene 'MainGame' couldn't be loaded". Tampoco hay escena "EndScreen" separada: el canvas EndScreen está en la misma escena Game.unity como objeto desactivado (no carga escena, sino activa el objeto). El GameManager.GoToEndScreen() carga escena "EndScreen" (que no existe), pero OvaryTrigger llama directamente a activar el canvas — esto funciona.

---

### `AutoForwardMovement.cs`
- **Ubicación:** Assets/Scripts/Player/AutoForwardMovement.cs
- **Propósito:** Mueve el objeto hacia adelante automáticamente en su eje Z local.
- **Clases/métodos principales:**
  - `Update()` — mueve transform.position en transform.forward
  - `StopMovement()` / `ResumeMovement()` — control de pausa
- **Dependencias:** Ninguna.
- **Estado:** ⚠️ PROBLEMA CRÍTICO. Está adjunto al XR Origin (VR) (velocidad: 7, isMoving: 1). El XR Origin tiene rotación m_LocalRotation: {x: -1, y: 0, z: 0, w: 0}, que equivale a una rotación de 180° en el eje X. Esto invierte el forward del objeto, por lo que `transform.forward` apunta en dirección -Z en lugar de +Z, pero el óvulo está en posición (0, 0, -100). Con ese forward invertido, el jugador realmente SÍ avanzaría hacia el óvulo. Sin embargo, con DOS sistemas VR activos (XR Origin + OVR Camera Rig), el movimiento de XR Origin no afecta la posición de la cámara del HMD (que viene del OVR Camera Rig en posición (0,0,0)). Conclusión: desde la perspectiva del headset, el jugador parece estático — la cámara no se mueve.

---

### `AutoMove.cs`
- **Ubicación:** Assets/Scripts/Player/AutoMove.cs
- **Propósito:** Mueve el player hacia un target (Ovary). Script alternativo, más completo que AutoForwardMovement.
- **Clases/métodos principales:**
  - `Update()` — calcula dirección al target y avanza; se detiene a `stopDistance`
- **Dependencias:** `target` Transform (el óvulo, no asignado en escena).
- **Estado:** ❌ NO ESTÁ EN USO. El script existe pero no está asignado a ningún objeto en la escena.

---

### `PlayerHealth.cs`
- **Ubicación:** Assets/Scripts/Player/PlayerHealth.cs
- **Propósito:** Gestiona las vidas del jugador y actualiza el HUD.
- **Clases/métodos principales:**
  - `Start()` — obtiene AutoForwardMovement del padre
  - `RecibirDano()` — resta vida, actualiza UI, llama Morir() si llega a 0
  - `Morir()` — detiene movimiento, muestra GAME OVER
  - `ActualizarInterfaz()` — actualiza texto, pone rojo si queda 1 vida
- **Dependencias:** `TextMeshPro` (no TextMeshProUGUI — usa el 3D TextMesh), `AutoForwardMovement` (del padre).
- **Estado:** ⚠️ POSIBLE PROBLEMA. Está en el Main Camera del XR Origin (VR). El `GetComponentInParent<AutoForwardMovement>()` buscará en Camera Offset y luego en XR Origin (VR), donde sí existe — esto debería funcionar. `textoVidas` referencia el fileID 1297445745, que es un TextMeshPro 3D. Necesita verificar que esté correctamente posicionado en el mundo VR.

---

### `ShootGun.cs`
- **Ubicación:** Assets/Scripts/Shooting/ShootGun.cs
- **Propósito:** Gestiona el disparo desde cada brazo (Arm_L y Arm_R). Usa InputSystem para detectar el trigger del controller. Hace Raycast + dispara bala física. Anima retroceso.
- **Clases/métodos principales:**
  - `Start()` — guarda posición original, habilita la InputAction
  - `Update()` — detecta `WasPressedThisFrame()` en el trigger, llama Shoot()
  - `Shoot()` — Raycast hacia enemigos + instancia bala + animación de recoil
- **Dependencias:** `InputActionReference` (triggerAction, asignado en escena), `EnemyHealth`, `bulletPrefab` (PlasmaBullet, asignado en escena), `firePoint` (Transform).
- **Estado:** ⚠️ PROBLEMA CRÍTICO POTENCIAL. El script usa `UnityEngine.InputSystem` (New Input System) con `InputActionReference`. Las acciones asignadas provienen del archivo "XRI Default Input Actions.inputactions" (parte del XR Interaction Toolkit). Arm_R usa el action con fileID 83097790271614945 y Arm_L usa -5982496924579745919. Estos son sub-assets del inputactions file. El problema: con OVR Camera Rig como sistema de tracking ACTIVO, los inputs pueden no llegar a estas acciones XRI porque OVR usa su propio sistema (OVRInput). Esto depende de si el XR Input System está correctamente configurado para recibir inputs del Meta SDK. Es necesario verificar en ejecución si los triggers del Quest disparan estas acciones. ADICIONALMENTE, los brazos están en la jerarquía del OVR Camera Rig (TrackingSpace > RightHandAnchor > RightControllerAnchor > Arm_R), pero los inputs son del XR Interaction Toolkit — posible incompatibilidad.

---

## 3. ESCENA ACTUAL (Game.unity)

### Jerarquía principal detectada

La escena tiene DOS sistemas VR activos simultáneamente:

**SISTEMA 1: OVR Camera Rig (Meta SDK) — ACTIVO**
```
[BuildingBlock] Camera Rig             ← OVRCameraRig + OVRManager
└── TrackingSpace
    ├── LeftEyeAnchor                  ← Cámara izquierda (deshabilitada por defecto)
    ├── RightEyeAnchor                 ← Cámara con AudioListener, UniversalAdditionalCameraData
    ├── CenterEyeAnchor                ← (posición central HMD)
    ├── LeftHandAnchor
    │   └── LeftControllerAnchor
    │       └── [BuildingBlock] Controller Tracking Left
    │           └── OculusTouchForQuest* controllers (múltiples modelos)
    └── RightHandAnchor
        └── RightControllerAnchor
            ├── [BuildingBlock] Controller Tracking Right
            │   └── OculusTouchForQuest* controllers (múltiples modelos)
            └── Arm_R (prefab FBX) ← ShootGun.cs, Muzzle_R (firePoint)
[BuildingBlock] Controller Tracking Left
└── LeftControllerAnchor
    └── Arm_L (prefab FBX) ← ShootGun.cs, Muzzle_L (firePoint)
```

**SISTEMA 2: XR Origin (VR) — ACTIVO**
```
XR Origin (VR)                         ← XROrigin + XRInputModalityManager + AutoForwardMovement
├── Camera Offset
│   ├── Main Camera                    ← Tag: "Player", PlayerHealth, SphereCollider(trigger), Camera
│   ├── right                          ← XRRayInteractor + LineRenderer
│   └── left                           ← XRRayInteractor + LineRenderer
└── VirusSpawner                       ← VirusSpawner.cs (spawnea virus relativo a XR Origin)
```

**Otros objetos raíz:**
```
GameManager                            ← GameManager.cs (Singleton)
Ovary                                  ← SphereCollider x2 (uno trigger), OvaryTrigger.cs, Material
EventSystem                            ← EventSystem + XRUIInputModule
Directional Light
tuberia (prefab)                       ← Túnel cilíndrico con materiales _Cull:0
[Canvas HUD]                           ← ScoreUI, textos Score/ETS/Vidas
[Canvas EndScreen]                     ← EndScreenUI (desactivado al inicio)
```

### Objetos importantes y sus scripts

| Objeto | Script(s) | Estado |
|--------|-----------|--------|
| `[BuildingBlock] Camera Rig` | OVRCameraRig, OVRManager | Sistema VR activo #1 |
| `XR Origin (VR)` | XROrigin, XRInputModalityManager, **AutoForwardMovement** | Sistema VR activo #2 |
| `Main Camera` (hijo de XR Origin) | PlayerHealth (vidas:3), XRTrackedPoseDriver, Camera | Tag: Player |
| `VirusSpawner` (hijo de XR Origin) | VirusSpawner (spawnInterval:2, distanceAhead:25) | Depende del movimiento XR Origin |
| `Arm_R` (hijo de RightControllerAnchor) | **ShootGun** (triggerAction: XRI Right Activate, bulletPrefab: PlasmaBullet) | En jerarquía OVR |
| `Arm_L` (hijo de LeftControllerAnchor) | **ShootGun** (triggerAction: XRI Left Activate, bulletPrefab: PlasmaBullet) | En jerarquía OVR |
| `Ovary` | OvaryTrigger (endScreenCanvas asignado) | Posición (0,0,-100) |
| `GameManager` | GameManager (singleton) | Activo |
| `ScoreUI` | ScoreUI (referencias asignadas) | Canvas World Space |

---

## 4. DEPENDENCIAS Y PACKAGES

### manifest.json — Packages principales

| Package | Versión | Notas |
|---------|---------|-------|
| `com.meta.xr.sdk.all` | 201.0.0 | Meta XR SDK completo (OVR, OpenXR Meta) |
| `com.unity.render-pipelines.universal` | 14.0.12 | URP — Unity 2022 compatible |
| `com.unity.xr.interaction.toolkit` | 3.5.0 | XR Interaction Toolkit |
| `com.unity.xr.management` | 4.6.0 | XR Management |
| `com.unity.xr.openxr` | 1.14.3 | OpenXR |
| `com.unity.textmeshpro` | 3.0.9 | TextMeshPro |
| `com.unity.visualscripting` | 1.9.4 | Visual Scripting |
| `com.unity.timeline` | 1.7.7 | Timeline |

### Plugins detectados en Assets/

- `Assets/Samples/XR Interaction Toolkit/3.5.0/` — Starter Assets instalados (contiene "XRI Default Input Actions.inputactions" usado por ShootGun)
- `Assets/Oculus/` — Configuración del proyecto Oculus
- `Assets/Plugins/Android/` — AndroidManifest.xml configurado para Meta Quest 2/3/Pro

### Configuración de proyecto VR

- **XRSettings.asset:** VR Device Disabled: False (VR habilitado)
- **OculusProjectConfig:** Target devices: Quest series, focusAware: true
- **AndroidManifest:** categoria `com.oculus.intent.category.VR`, supportedDevices: quest|quest2|questpro|quest3|quest3s
- **ProjectSettings:** m_StereoRenderingPath: 2 (Instancing)

---

## 5. PROBLEMAS DETECTADOS

### Problema #1: DOS SISTEMAS VR COEXISTIENDO EN LA ESCENA
- **Archivo afectado:** Assets/Scenes/Game.unity
- **Causa:** Tanto `[BuildingBlock] Camera Rig` (OVRCameraRig) como `XR Origin (VR)` (XR Interaction Toolkit) están activos simultáneamente. En tiempo de ejecución, dos sistemas de tracking intentarán controlar la cámara/posición del jugador. El Meta SDK (OVR) tomará control del HMD, pero el XR Origin seguirá existiendo con su propia cámara y lógica.
- **Consecuencia concreta:** La vista que el jugador ve desde el headset proviene del OVR Camera Rig (RightEyeAnchor). El AutoForwardMovement está en XR Origin, por lo que el movimiento automático mueve el XR Origin pero NO la cámara OVR — el jugador parece estar estático en el HMD.
- **Gravedad:** 🔴 CRÍTICO

---

### Problema #2: AUTO-MOVIMIENTO DEL JUGADOR NO AFECTA LA VISTA VR
- **Archivo afectado:** Assets/Scripts/Player/AutoForwardMovement.cs (en XR Origin (VR))
- **Causa:** AutoForwardMovement está en el XR Origin (VR). El OVR Camera Rig en posición (0,0,0) no se mueve — su posición la controla exclusivamente el OVR tracking. Para que el jugador avance en VR, el script debería estar en el `[BuildingBlock] Camera Rig` o en un objeto padre del OVR Camera Rig.
- **Gravedad:** 🔴 CRÍTICO

---

### Problema #3: VIRUS SPAWNER RELATIVO AL SISTEMA EQUIVOCADO
- **Archivo afectado:** Assets/Scenes/Game.unity (VirusSpawner hijo de XR Origin)
- **Causa:** El VirusSpawner está dentro de la jerarquía del XR Origin (VR). Si XR Origin se mueve pero el OVR Camera Rig no, los virus aparecerán en posiciones que no tienen relación espacial con lo que el jugador ve en el headset.
- **Gravedad:** 🔴 CRÍTICO

---

### Problema #4: SHOOT GUN USA XR INTERACTION TOOLKIT INPUTS EN JERARQUÍA OVR
- **Archivo afectado:** Assets/Scripts/Shooting/ShootGun.cs
- **Causa:** El script ShootGun está en los brazos (Arm_R/Arm_L) que son hijos del OVR Camera Rig. Pero usa `InputActionReference` del XR Interaction Toolkit (XRI Default Input Actions, acciones "XRI Left/Right Interaction/Activate"). Al haber migrado a OVR, es posible que los inputs XRI no estén mapeados correctamente y los triggers del Quest no disparen estas acciones. Depende de si Meta SDK publica sus inputs al XR Input System. En la práctica puede funcionar si OpenXR está configurado como backend, pero es frágil y puede fallar.
- **Gravedad:** 🟡 MODERADO (puede funcionar vía OpenXR, pero necesita verificación)

---

### Problema #5: TAG "PLAYER" EN MAIN CAMERA DEL XR ORIGIN (SISTEMA INCORRECTO)
- **Archivo afectado:** Assets/Scenes/Game.unity
- **Causa:** El tag "Player" está en la "Main Camera" del XR Origin. VirusAI busca un objeto con tag "Player" para perseguirlo. Como el OVR Camera Rig es quien realmente representa la posición del jugador en VR, los virus perseguirán la Main Camera del XR Origin — que puede estar en una posición diferente a la cámara OVR.
- **Gravedad:** 🟡 MODERADO

---

### Problema #6: COLISIÓN OVARY-PLAYER SIN RIGIDBODY
- **Archivo afectado:** Assets/Scripts/Managers/OvaryScript.cs, Assets/Scenes/Game.unity
- **Causa:** `OnTriggerEnter` requiere que al menos uno de los dos objetos tenga Rigidbody. El Ovary tiene dos SphereColliders (uno trigger) pero no tiene Rigidbody. La "Main Camera" del XR Origin (el "Player") tiene un SphereCollider trigger pero tampoco tiene Rigidbody explícito. Aunque el XRTrackedPoseDriver no añade Rigidbody. En Unity, los triggers estáticos (sin Rigidbody) SÍ disparan OnTriggerEnter con objetos kinematic — pero ambos objetos estáticos NO se detectan entre sí. Falta Rigidbody en uno de ellos.
- **Gravedad:** 🔴 CRÍTICO (la condición de victoria no funcionará)

---

### Problema #7: NOMBRE DE ESCENA INCORRECTO EN PLAYAGAIN
- **Archivo afectado:** Assets/Scripts/Managers/EndScreenUI.cs (línea `SceneManager.LoadScene("MainGame")`)
- **Causa:** El método PlayAgain() intenta cargar la escena "MainGame" pero la escena principal se llama "Game".
- **Gravedad:** 🟡 MODERADO (crasheará si el usuario intenta reiniciar)

---

### Problema #8: AUTOMOVE.CS EXISTE PERO NO SE USA
- **Archivo afectado:** Assets/Scripts/Player/AutoMove.cs
- **Causa:** Hay dos scripts de movimiento: AutoForwardMovement (en uso) y AutoMove (sin usar). AutoMove tiene más funcionalidad (se detiene al llegar al target). Genera confusión y potencial desperdicio de mantenimiento.
- **Gravedad:** 🟢 MENOR

---

### Problema #9: CARPETA AUDIO VACÍA — SIN SONIDOS
- **Archivo afectado:** Assets/Audio/
- **Causa:** No hay ningún archivo de audio. El script ShootGun no reproduce sonidos al disparar y no hay música de fondo.
- **Gravedad:** 🟢 MENOR (no rompe nada, pero la experiencia VR carece de feedback sonoro)

---

### Problema #10: TAG "PLAYER" NO DEFINIDO EN TAGMANAGER
- **Archivo afectado:** ProjectSettings/TagManager.asset
- **Causa:** Los tags definidos son: Virus, Orb, Enemy, Bullet. El tag "Player" que se usa en VirusAI y OvaryTrigger para comparar con `CompareTag("Player")` NO está en la lista de tags definidos del proyecto. Sin embargo, "Player" es un tag reservado de Unity por defecto — Unity lo incluye siempre aunque no aparezca en el TagManager. No debería ser un error crítico, pero confirma que el setup se hizo sin prestar atención a los tags.
- **Gravedad:** 🟢 MENOR (Unity incluye "Player" por defecto)

---

### Problema #11: XR ORIGIN TIENE ROTACIÓN INVERTIDA (180° EN X)
- **Archivo afectado:** Assets/Scenes/Game.unity (XR Origin transform)
- **Causa:** El XR Origin tiene `m_LocalRotation: {x: -1, y: 0, z: 0, w: 0}` = 180° de rotación en X. Esto invierte el eje Y y Z del objeto. El AutoForwardMovement usa `transform.forward` que con esta rotación apunta en dirección -Z. Como el óvulo está en Z=-100, el movimiento sería correcto matemáticamente, pero es una configuración poco ortodoxa que puede causar problemas si se agrega lógica adicional.
- **Gravedad:** 🟡 MODERADO

---

## 6. CONFIGURACIÓN VR

### Sistema actual

La escena tiene **dos sistemas VR coexistiendo**:

| Sistema | Estado | Propósito en escena |
|---------|--------|---------------------|
| **OVR Camera Rig** ([BuildingBlock] Camera Rig) | ACTIVO | Tracking del HMD + controllers Quest. Contiene los brazos con ShootGun. |
| **XR Origin (VR)** | ACTIVO | AutoForwardMovement, VirusSpawner, Main Camera (Player tag), Ray Interactors |

### El OVR Camera Rig contiene:
- `OVRManager` (guid: 7e933e81d3c20c74ea6fdc708a67e3a5) con configuración completa del HMD
- `OVRCameraRig` (guid: df9f338034892c44ebb62d97894772f1)
- `TrackingSpace` → LeftEyeAnchor, RightEyeAnchor, LeftHandAnchor, RightHandAnchor
- Los controladores físicos de Quest (múltiples modelos: Touch, Quest2, Quest3, Pro)
- **Arm_R** y **Arm_L** como hijos de los controladores respectivos ← Correcto para tracking
- **RightEyeAnchor** tiene AudioListener y cámara activa

### El XR Origin contiene:
- `XROrigin` component
- `XRInputModalityManager`
- `AutoForwardMovement` (velocidad 7, activo)
- `Camera Offset` con Main Camera (tag Player, PlayerHealth), ray interactors izquierdo/derecho
- `VirusSpawner`

### Conflictos detectados:
1. Dos sistemas de rendering/tracking activos → posible doble renderizado, posible conflicto de tracking
2. El movimiento del jugador (AutoForwardMovement) está en el sistema "muerto" (XR Origin)
3. Los inputs de disparo (XRI) pueden no mapearse al OVR controller
4. La posición del "jugador" para gameplay (VirusAI, OvaryTrigger) no coincide con la posición física en VR

---

## 7. CHECKLIST DE FUNCIONALIDADES

| Funcionalidad | Estado | Notas |
|---------------|--------|-------|
| Brazos separados (Arm_L y Arm_R) como hijos de los controllers VR | ✅ Implementado | En jerarquía OVR: RightControllerAnchor/LeftControllerAnchor |
| Brazos siguen el movimiento de los controllers | ✅ Implementado | Son hijos directos de los anchors del controller |
| Script de disparo funcional con triggers de Quest | ⚠️ Parcial | ShootGun usa XRI Input Actions — puede fallar con OVR |
| Animación de retroceso (recoil) al disparar | ✅ Implementado | En ShootGun.cs con Lerp |
| Movimiento automático del jugador (AutoMove) | ❌ Roto | AutoForwardMovement en XR Origin — no mueve la cámara OVR |
| Detección colisión bala → virus (EnemyHealth) | ✅ Implementado | Raycast en ShootGun + OnTriggerEnter en VirusAI para bala física |
| Sistema de puntuación | ✅ Implementado | GameManager + ScoreUI con referencias asignadas |
| Condición de victoria al llegar al óvulo | ❌ Roto | OvaryTrigger sin Rigidbody → OnTriggerEnter no funciona |
| Texturas en brazos/pistolas | ⚠️ Parcial | Material asignado en prefab de brazo (guid: 1dc5c09c...), pero textura de pistola en sub-carpeta separada |
| Material del túnel visible por dentro (Render Face: Both) | ✅ Implementado | _Cull: 0 en TunnelMaterial, Mar_paredes_tubo, Mat_fondo_tubo |
| Material del óvulo | ✅ Implementado | OvaryMaterial asignado |
| Virus (ETSs) spawneando dentro del túnel | ⚠️ Parcial | VirusSpawner activo pero relativo al XR Origin (posición incorrecta en VR) |
| VirusAI (movimiento de virus) | ✅ Implementado | Persigue al "Player" (Main Camera del XR Origin) |
| Sonidos de disparo | ❌ Falta | Carpeta Audio vacía, ShootGun no tiene AudioSource |
| UI de puntuación en VR | ✅ Implementado | Canvas World Space con ScoreUI, posición z:-5 frente a la cámara |

