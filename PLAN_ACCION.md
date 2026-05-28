# PLAN DE ACCIÓN — SPERMINATOR
*Basado en el análisis del 2026-05-28*

---

## RESUMEN EJECUTIVO

El proyecto Sperminator está en un estado de transición incompleta. La migración de XR Origin (XR Interaction Toolkit) a OVR Camera Rig (Meta SDK) se realizó parcialmente: se añadió el OVR Camera Rig con los brazos y controllers correctamente configurados, pero el XR Origin NO fue eliminado de la escena. Ambos sistemas coexisten activos simultáneamente. Esto causa que el movimiento automático del jugador, el spawner de virus, la detección del "Player" y la lógica de colisiones sean inoperativos en VR — todo lo que "se mueve" lo hace en el espacio del XR Origin, mientras que la cámara del headset permanece estática en el OVR Camera Rig.

El núcleo del problema es estructural: hay que decidir si se mantiene el OVR Camera Rig como sistema definitivo (recomendado, ya que es lo que Meta exige para Quest) o se revierten los cambios. Si se mantiene OVR, los componentes de gameplay (AutoForwardMovement, VirusSpawner, PlayerHealth) deben migrarse a la jerarquía OVR y el XR Origin debe ser desactivado o eliminado.

Los scripts de C# en sí mismos están bien escritos. No hay errores de sintaxis ni dependencias rotas a API obsoleta. El problema es exclusivamente de configuración de escena: qué objetos tienen qué scripts, y cómo se relacionan entre sí en la jerarquía VR.

---

## CAMBIOS PROPUESTOS

---

### CAMBIO #1: Desactivar (o eliminar) el XR Origin (VR)
- **Archivo(s):** Assets/Scenes/Game.unity — objeto `XR Origin (VR)` (fileID: 1767666333)
- **Qué cambiar:** En el Inspector de Unity, seleccionar el GameObject "XR Origin (VR)" y desmarcarlo (desactivarlo). Opcionalmente eliminarlo si se confirma que no se necesita.
- **Por qué:** Es el origen de todos los conflictos. Con dos sistemas VR activos, el runtime de Meta ignora el XR Origin pero este sigue ejecutando scripts (AutoForwardMovement, VirusSpawner) que mueven objetos en su propio espacio de coordenadas desacoplado del HMD.
- **Riesgo:** 🟡 MEDIO. Al desactivar XR Origin, sus componentes hijo dejan de funcionar — VirusSpawner, AutoForwardMovement y PlayerHealth dejarán de ejecutarse. Esto requiere hacer los cambios #2, #3, #4 en paralelo o inmediatamente después.
- **Dependencias:** Hacer DESPUÉS de los cambios #2, #3, #4.

---

### CAMBIO #2: Mover AutoForwardMovement al OVR Camera Rig
- **Archivo(s):** Assets/Scenes/Game.unity — objeto `[BuildingBlock] Camera Rig` (fileID: 334961907)
- **Qué cambiar:**
  1. En el Inspector, seleccionar el GameObject `[BuildingBlock] Camera Rig`.
  2. Añadir el componente `AutoForwardMovement` (está en Assets/Scripts/Player/AutoForwardMovement.cs).
  3. Configurar: forwardSpeed = 7 (mismo valor que estaba en XR Origin), isMoving = true.
  4. Verificar que el `[BuildingBlock] Camera Rig` tenga su rotación limpia (0,0,0,1) para que `transform.forward` apunte en +Z.
  5. Si el óvulo está en z=-100, ajustar la orientación del Camera Rig o del óvulo para que queden alineados en el mismo eje de avance. (Con el Camera Rig en rotación (0,0,0), forward es +Z, y el óvulo debe estar en z=+100 o cambiar la dirección de movimiento).
- **Por qué:** El OVR Camera Rig es el objeto raíz que representa la posición física del jugador en el mundo. Mover este objeto es mover lo que el jugador ve en el headset.
- **Riesgo:** 🔴 ALTO. Mover el OVR Camera Rig cambia la posición de toda la jerarquía VR (brazos, controllers, cámaras). Hay que asegurarse de que el óvulo y el túnel estén alineados con la dirección de movimiento.
- **Dependencias:** Debe hacerse ANTES de desactivar XR Origin (#1). Verificar posición del óvulo y orientación del túnel.

**NOTA IMPORTANTE sobre la orientación del túnel:** El XR Origin tenía rotación 180° en X (probablemente para que el túnel que iba en -Z pareciera ir hacia adelante desde la perspectiva de esa cámara). Con el OVR Camera Rig en rotación neutral, si el túnel va hacia -Z, el Camera Rig debe mirar hacia -Z (rotación 180° en Y) o el túnel debe ser reorientado. Revisar en editor.

---

### CAMBIO #3: Mover VirusSpawner al OVR Camera Rig
- **Archivo(s):** Assets/Scenes/Game.unity
- **Qué cambiar:**
  1. En el Inspector, arrastrar el GameObject `VirusSpawner` (actualmente hijo de XR Origin > Camera Offset) y soltarlo como hijo directo de `[BuildingBlock] Camera Rig` (o como hijo del `TrackingSpace`).
  2. Resetear la posición local del VirusSpawner a (0, 0, 0).
  3. Verificar que los parámetros `distanceAhead: 25` y `tubeRadius: 3` sigan siendo correctos con la nueva escala.
- **Por qué:** El spawner usa `transform.position + transform.forward * distanceAhead` — necesita estar en el mismo sistema de coordenadas que la cámara activa para que los virus aparezcan donde el jugador los ve.
- **Riesgo:** 🟡 MEDIO. Cambio de jerarquía en escena, pero el script no necesita modificación.
- **Dependencias:** Debe hacerse junto con o después del cambio #2.

---

### CAMBIO #4: Mover PlayerHealth al objeto correcto en la jerarquía OVR
- **Archivo(s):** Assets/Scenes/Game.unity
- **Qué cambiar:**
  1. Actualmente `PlayerHealth` está en `Main Camera` (hijo de XR Origin). Necesita estar en un objeto de la jerarquía OVR.
  2. Opción A (recomendada): Añadir `PlayerHealth` al `CenterEyeAnchor` o al propio `[BuildingBlock] Camera Rig`. Este objeto también necesita:
     - Tag: "Player"
     - SphereCollider (IsTrigger: true, radio 0.3) para que los virus puedan golpear al jugador
     - Rigidbody (IsKinematic: true) — VER cambio #6
  3. Asignar la referencia `textoVidas` al mismo TextMeshPro 3D que ya existía.
- **Por qué:** PlayerHealth detecta daño cuando un virus llega al objeto con tag "Player". Si el objeto "Player" no sigue al HMD, el jugador nunca recibirá daño aunque los virus pasen por su cara.
- **Riesgo:** 🟡 MEDIO. Necesita reorganización en el Inspector.
- **Dependencias:** Debe hacerse junto con el cambio #5.

---

### CAMBIO #5: Cambiar el tag "Player" al objeto correcto del OVR Camera Rig
- **Archivo(s):** Assets/Scenes/Game.unity
- **Qué cambiar:**
  1. Seleccionar `Main Camera` (en XR Origin) y cambiar su tag de "Player" a "Untagged".
  2. Seleccionar el objeto al que se añadió PlayerHealth en el cambio #4 (CenterEyeAnchor o Camera Rig) y asignarle el tag "Player".
- **Por qué:** VirusAI.cs busca el objeto con tag "Player" para perseguirlo y para aplicar daño. OvaryTrigger también comprueba tag "Player". Deben apuntar al objeto que se mueve con el OVR Camera Rig.
- **Riesgo:** 🟢 BAJO. Solo cambio de tag en el Inspector.
- **Dependencias:** Cambio #4 debe hacerse antes.

---

### CAMBIO #6: Añadir Rigidbody kinematic al objeto "Player" para activar los triggers
- **Archivo(s):** Assets/Scenes/Game.unity — el objeto con tag "Player" (después del cambio #5)
- **Qué cambiar:**
  1. Seleccionar el objeto "Player" (CenterEyeAnchor o Camera Rig).
  2. Añadir componente `Rigidbody`.
  3. Configurar: Is Kinematic = true, Use Gravity = false.
  4. Verificar que el SphereCollider (IsTrigger: true) también esté en este objeto.
- **Por qué:** En Unity, para que `OnTriggerEnter` funcione entre dos objetos estáticos (sin Rigidbody), al menos uno debe tener Rigidbody (aunque sea kinematic). Sin esto, el óvulo nunca detectará al jugador y la condición de victoria no se activará. El mismo principio aplica para la detección virus-jugador.
- **Riesgo:** 🟢 BAJO. Añadir un Rigidbody kinematic no cambia el comportamiento visual ni de posición.
- **Dependencias:** Cambio #4 y #5 deben estar hechos.

---

### CAMBIO #7: Corregir el nombre de escena en EndScreenUI.PlayAgain()
- **Archivo(s):** Assets/Scripts/Managers/EndScreenUI.cs — línea `SceneManager.LoadScene("MainGame")`
- **Qué cambiar:**
  ```csharp
  // ANTES:
  SceneManager.LoadScene("MainGame");
  
  // DESPUÉS:
  SceneManager.LoadScene("Game");
  ```
- **Por qué:** La escena principal se llama "Game" (Assets/Scenes/Game.unity). Si se llama con "MainGame", Unity lanzará un error en tiempo de ejecución al intentar reiniciar.
- **Riesgo:** 🟢 BAJO. Solo modificación de string en código.
- **Dependencias:** Asegurarse de que la escena "Game" esté añadida en File > Build Settings.

---

### CAMBIO #8: Verificar y corregir los inputs de ShootGun con OVR
- **Archivo(s):** Assets/Scripts/Shooting/ShootGun.cs, Assets/Scenes/Game.unity
- **Qué cambiar:**
  Hay DOS opciones:

  **Opción A — Mantener XRI Input Actions (recomendada si OpenXR está activo):**
  - Abrir en Unity el XR Management (Project Settings > XR Plug-in Management).
  - Verificar que "Oculus" o "Meta XR" esté habilitado como provider.
  - Verificar que los bindings del inputactions file incluyan los paths del Quest controller para Activate (trigger).
  - En Assets/Samples/XR Interaction Toolkit/3.5.0/Starter Assets/XRI Default Input Actions.inputactions, los bindings de "Activate" en XRI Left/Right Interaction deberían incluir `<XRController>{LeftHand}/trigger` y `<XRController>{RightHand}/trigger`.
  - Si están correctamente mapeados, ShootGun funcionará sin cambio de código.

  **Opción B — Reescribir ShootGun para usar OVRInput directamente:**
  ```csharp
  // En ShootGun.cs, reemplazar:
  // if (triggerAction != null && triggerAction.action.WasPressedThisFrame())
  
  // Añadir campo:
  public OVRInput.Button ovrTriggerButton = OVRInput.Button.PrimaryIndexTrigger;
  
  // Y en Update():
  if (OVRInput.GetDown(ovrTriggerButton))
  {
      if (Time.time >= nextFireTime)
      {
          Shoot();
          nextFireTime = Time.time + fireRate;
      }
  }
  ```
  - Para Arm_R: usar `OVRInput.Button.SecondaryIndexTrigger` (mano derecha)
  - Para Arm_L: usar `OVRInput.Button.PrimaryIndexTrigger` (mano izquierda)
  
  **RECOMENDACIÓN:** Probar primero la Opción A (sin cambio de código). Si los triggers no responden en el dispositivo, usar Opción B.
- **Riesgo:** 🟡 MEDIO. Opción B requiere cambio de código y eliminar la dependencia de InputActionReference.
- **Dependencias:** Ninguno previo necesario.

---

### CAMBIO #9: Eliminar (o limpiar) AutoMove.cs innecesario
- **Archivo(s):** Assets/Scripts/Player/AutoMove.cs
- **Qué cambiar:**
  Eliminar el archivo AutoMove.cs ya que no se usa en la escena y crea confusión con AutoForwardMovement.cs. Alternativamente, si se prefiere la lógica de "detenerse al llegar al target" que tiene AutoMove, reemplazar AutoForwardMovement por AutoMove.
- **Por qué:** Código muerto. No causa errores pero ensucia el proyecto.
- **Riesgo:** 🟢 BAJO.
- **Dependencias:** Ninguno.

---

### CAMBIO #10: Añadir sonido de disparo al ShootGun
- **Archivo(s):** Assets/Scripts/Shooting/ShootGun.cs, necesita un archivo .wav/.ogg de disparo
- **Qué cambiar:**
  1. Añadir un archivo de audio de disparo en Assets/Audio/.
  2. En ShootGun.cs, añadir campo:
     ```csharp
     [Header("Audio")]
     public AudioClip shootSound;
     private AudioSource audioSource;
     ```
  3. En `Start()`: `audioSource = GetComponent<AudioSource>();`
  4. En `Shoot()`: `if (audioSource && shootSound) audioSource.PlayOneShot(shootSound);`
  5. Añadir componente `AudioSource` a los objetos Arm_R y Arm_L en la escena.
- **Riesgo:** 🟢 BAJO.
- **Dependencias:** Tener un archivo de audio disponible.

---

## ORDEN DE IMPLEMENTACIÓN RECOMENDADO

El orden es importante para no romper referencias mientras se trabaja:

1. **CAMBIO #7** — Corregir nombre de escena en EndScreenUI.cs (cambio de código seguro, independiente)
2. **CAMBIO #8** — Verificar inputs de ShootGun (probar en dispositivo antes de cambiar código)
3. **CAMBIO #2** — Mover AutoForwardMovement al OVR Camera Rig (primero mover el componente, verificar orientación del túnel y óvulo)
4. **CAMBIO #3** — Mover VirusSpawner al OVR Camera Rig (hacerlo justo después del #2 mientras se trabaja en la jerarquía)
5. **CAMBIO #4** — Mover PlayerHealth al objeto OVR correcto (CenterEyeAnchor o Camera Rig)
6. **CAMBIO #5** — Cambiar tag "Player" al objeto OVR correcto
7. **CAMBIO #6** — Añadir Rigidbody kinematic al objeto "Player" OVR
8. **CAMBIO #1** — Desactivar el XR Origin (VR) — ÚLTIMO porque hasta este punto toda la lógica del juego dependía de él
9. **CAMBIO #9** — Limpiar AutoMove.cs (cosmético, puede hacerse en cualquier momento)
10. **CAMBIO #10** — Añadir sonidos (mejora, hacer cuando lo anterior funcione)

---

## CAMBIOS QUE REQUIEREN REVISIÓN MANUAL EN UNITY EDITOR

Los siguientes cambios NO se pueden hacer editando solo archivos de código — requieren abrir Unity Editor y modificar la escena:

1. **Desactivar XR Origin (VR):** Seleccionar el GameObject en la jerarquía y desmarcar la casilla de activo (o Delete si se quiere eliminar).

2. **Añadir AutoForwardMovement al Camera Rig:** Arrastrar el script desde Project > Scripts/Player al Inspector del '[BuildingBlock] Camera Rig'.

3. **Mover VirusSpawner de jerarquía:** Arrastrar el objeto VirusSpawner en la ventana Hierarchy y soltarlo como hijo del '[BuildingBlock] Camera Rig'.

4. **Cambiar tag "Player":** En el Inspector de los objetos correspondientes, cambiar el dropdown de Tag.

5. **Añadir PlayerHealth al objeto OVR:** Arrastrar el script al objeto correcto y reasignar la referencia `textoVidas` en el Inspector.

6. **Añadir Rigidbody al Player OVR:** En el Inspector, Add Component > Rigidbody, configurar Is Kinematic = true.

7. **Verificar orientación del túnel y óvulo:** Con el OVR Camera Rig en posición/rotación neutral, comprobar que el túnel y el óvulo están en la dirección correcta para el movimiento (puede requerir mover el Ovary de z=-100 a z=+100 o rotar el Camera Rig).

8. **Verificar XR Plug-in Management:** Project Settings > XR Plug-in Management > Android — asegurarse de que "Oculus" esté habilitado como provider (no OpenXR puro) para que OVRInput funcione.

9. **Añadir AudioSource a Arm_R y Arm_L:** Si se implementa el cambio #10, añadir el componente AudioSource en el Inspector de cada brazo y asignar el AudioClip en el campo del script.

10. **Verificar Build Settings:** File > Build Settings, asegurarse de que la escena "Game" está en la lista de escenas a compilar. Si se añade una escena "EndScreen" separada, añadirla también.

