# GUÍA DE CAMBIOS DE ESCENA — SPERMINATOR
*Sigue estos pasos en el Unity Editor después de aplicar los cambios de código.*
*Abre la escena: Assets/Scenes/Game.unity*

---

## ⚠️ ANTES DE EMPEZAR

- Haz un git commit o al menos guarda la escena (Ctrl+S) antes de empezar.
- Trabaja con el Play Mode parado.
- Lee cada paso completo antes de ejecutarlo.

---

## PASO 1 — Mover AutoForwardMovement al OVR Camera Rig

**Por qué:** AutoForwardMovement está en "XR Origin (VR)", que ya no controla la cámara. Hay que moverlo al Camera Rig de OVR para que el movimiento sea visible desde el headset.

**Cómo:**

1. En la **Hierarchy**, selecciona `XR Origin (VR)`.
2. En el **Inspector**, localiza el componente `AutoForwardMovement`. Anota los valores: `forwardSpeed = 7`, `isMoving = true`.
3. Haz **clic derecho sobre el componente** → "Remove Component" (lo eliminas de XR Origin).
4. En la **Hierarchy**, selecciona `[BuildingBlock] Camera Rig`.
5. En el **Inspector**, pulsa **Add Component** → busca "AutoForwardMovement" → añádelo.
6. Configura los valores: `forwardSpeed = 7`, `isMoving = true`.

**⚠️ VERIFICACIÓN CRÍTICA — Orientación del túnel:**

El `[BuildingBlock] Camera Rig` debería tener rotación `(0, 0, 0)` (rotación limpia). Con esa rotación, `transform.forward` apunta en **+Z**. Verifica en la escena:
- Si el **túnel (tuberia)** se extiende hacia **+Z** → todo correcto, no hay que cambiar nada.
- Si el **túnel** se extiende hacia **-Z** (situación probable, ya que el óvulo está en Z=-100) → tienes dos opciones:
  - **Opción A (recomendada):** Cambia la rotación Y del `[BuildingBlock] Camera Rig` a `180°`, para que mire hacia -Z.
  - **Opción B:** Mueve el óvulo (Ovary) a posición `(0, 0, 100)` y dale la vuelta al túnel.

---

## PASO 2 — Mover VirusSpawner al OVR Camera Rig

**Por qué:** El VirusSpawner está en la jerarquía del XR Origin. Los virus aparecen relativos a la posición del XR Origin, no a la cámara real del headset.

**Cómo:**

1. En la **Hierarchy**, expande `XR Origin (VR)` y localiza el objeto `VirusSpawner` (está dentro de Camera Offset o directamente en XR Origin).
2. **Arrastra** el objeto `VirusSpawner` en la Hierarchy y **suéltalo directamente sobre `[BuildingBlock] Camera Rig`** para que quede como hijo del Camera Rig.
3. Selecciona `VirusSpawner` y en el Inspector, **resetea su posición local** a `(0, 0, 0)` (botón de engranaje en Transform → Reset Position).
4. Verifica que `distanceAhead` sigue siendo `25` y `tubeRadius` sigue siendo `3`.

---

## PASO 3 — Mover PlayerHealth al CenterEyeAnchor (jerarquía OVR)

**Por qué:** PlayerHealth está en la "Main Camera" del XR Origin. Necesita estar en un objeto del OVR Camera Rig para que el daño se aplique cuando los virus toquen la posición real del jugador.

**Cómo:**

1. En la **Hierarchy**, expande `XR Origin (VR)` → `Camera Offset` → selecciona `Main Camera`.
2. En el Inspector, localiza el componente `PlayerHealth`. Anota el valor: `maxVidas = 3`.
3. Anota también qué objeto tiene asignado en el campo `textoVidas` (haz click en el objeto asignado para ver su nombre).
4. Haz **clic derecho sobre PlayerHealth** → "Remove Component".
5. En la **Hierarchy**, expande `[BuildingBlock] Camera Rig` → `TrackingSpace` → selecciona `CenterEyeAnchor`.
6. En el Inspector de `CenterEyeAnchor`:
   - **Add Component** → busca "PlayerHealth" → añádelo.
   - Configura `maxVidas = 3`.
   - Asigna el mismo objeto en `textoVidas` que tenías antes.

---

## PASO 4 — Cambiar el tag "Player" al objeto correcto

**Por qué:** VirusAI y OvaryTrigger buscan el objeto con tag "Player" para perseguirlo y detectar la victoria. Ahora el "Player" real (el que se mueve con el headset) es el CenterEyeAnchor del OVR Camera Rig.

**Cómo:**

1. En la **Hierarchy**, selecciona `Main Camera` (hijo de XR Origin > Camera Offset).
2. En el **Inspector**, en el dropdown de **Tag** (arriba del todo), cambia de "Player" a "**Untagged**".
3. En la **Hierarchy**, selecciona `CenterEyeAnchor` (hijo de [BuildingBlock] Camera Rig > TrackingSpace).
4. En el **Inspector**, en el dropdown de **Tag**, selecciona "**Player**".

---

## PASO 5 — Añadir Rigidbody kinematic y Collider al objeto "Player" OVR

**Por qué:** Sin Rigidbody, Unity no ejecuta `OnTriggerEnter` entre dos objetos estáticos. El óvulo nunca detectará al jugador y la condición de victoria nunca se activará. También necesita un SphereCollider trigger para que los virus puedan golpearlo.

**Cómo:**

1. En la **Hierarchy**, selecciona `CenterEyeAnchor` (el nuevo "Player").
2. En el Inspector, **Add Component** → busca "**Rigidbody**" → añádelo.
3. Configura el Rigidbody:
   - `Is Kinematic` = ✅ **true**
   - `Use Gravity` = ❌ **false**
4. En el Inspector, **Add Component** → busca "**Sphere Collider**" → añádelo.
5. Configura el SphereCollider:
   - `Is Trigger` = ✅ **true**
   - `Radius` = `0.3`
   - `Center` = `(0, 0, 0)`

---

## PASO 6 — Configurar el nuevo ShootGun.cs en Arm_L y Arm_R

**Por qué:** El script ShootGun fue reescrito — ya no usa InputActionReference. Tienes que actualizar la configuración en el Inspector.

**Cómo:**

**Para Arm_L:**
1. En la Hierarchy, localiza `Arm_L` (en la jerarquía del OVR Camera Rig, dentro de LeftControllerAnchor).
2. Selecciona `Arm_L` → Inspector → componente `ShootGun`.
3. Verifica que el campo `Is Left Hand` esté **marcado ✅** (esto lo hace usar el trigger del controller izquierdo).
4. Verifica que `Bullet Prefab` y `Fire Point` (Muzzle_L) siguen asignados.
5. Si ves un campo `Trigger Action` (del script antiguo) con un aviso de "Missing", ya no existe en el nuevo script — ignóralo, Unity lo limpiará solo.

**Para Arm_R:**
1. Localiza `Arm_R` (dentro de RightControllerAnchor).
2. Selecciona `Arm_R` → Inspector → componente `ShootGun`.
3. Verifica que `Is Left Hand` esté **desmarcado ❌** (mano derecha).
4. Verifica que `Bullet Prefab` y `Fire Point` (Muzzle_R) siguen asignados.

---

## PASO 7 — Desactivar XR Origin (VR) — ⚠️ HACERLO ÚLTIMO

**Por qué:** El XR Origin (VR) ya no tiene ningún componente de gameplay activo (los hemos migrado todos a la jerarquía OVR). Dejarlo activo solo genera conflictos de tracking y cámara.

**Cómo:**

1. Primero, verifica que todos los pasos anteriores están hechos.
2. En la **Hierarchy**, selecciona `XR Origin (VR)`.
3. En el **Inspector**, **desactiva el checkbox** junto al nombre del GameObject (arriba del todo, a la izquierda del nombre).
   - El objeto quedará en gris en la Hierarchy — sigue existiendo pero no se ejecuta.
   - **NO lo borres** todavía, por si hay que revertir algo.
4. Guarda la escena (Ctrl+S).

---

## VERIFICACIÓN FINAL

Después de todos los pasos, el estado de la escena debería ser:

| Check | Estado esperado |
|-------|----------------|
| `[BuildingBlock] Camera Rig` tiene `AutoForwardMovement` | ✅ |
| `VirusSpawner` es hijo del `Camera Rig` | ✅ |
| `CenterEyeAnchor` tiene tag "Player" | ✅ |
| `CenterEyeAnchor` tiene `PlayerHealth` | ✅ |
| `CenterEyeAnchor` tiene `Rigidbody` (Is Kinematic = true) | ✅ |
| `CenterEyeAnchor` tiene `SphereCollider` (Is Trigger = true) | ✅ |
| `Main Camera` (XR Origin) tiene tag "Untagged" | ✅ |
| `Arm_L` ShootGun → `Is Left Hand` = true | ✅ |
| `Arm_R` ShootGun → `Is Left Hand` = false | ✅ |
| `XR Origin (VR)` está desactivado | ✅ |

---

## SI ALGO VA MAL

- Si el juego no arranca: reactiva el `XR Origin (VR)` y revisa qué paso faltó.
- Si los virus no persiguen al jugador: verifica que `CenterEyeAnchor` tiene tag "Player".
- Si el disparo no funciona: en el Inspector de Arm_L/R, verifica que el componente ShootGun no tiene errores en rojo. Compila la solución desde Unity (Ctrl+R o Edit > Preferences > External Tools > Regenerate .csproj).
- Si el movimiento va en dirección contraria: cambia la rotación Y del `[BuildingBlock] Camera Rig` de 0° a 180° (o viceversa).

