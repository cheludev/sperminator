using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public class SetupClitoris : MonoBehaviour
{
    [MenuItem("Tools/Sperminator/Setup Clitoris Power-up")]
    public static void RunSetup()
    {
        // 1. Asegurar la escena correcta abierta
        string scenePath = "Assets/Scenes/Game.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);
        if (!scene.IsValid())
        {
            Debug.LogError($"No se pudo abrir la escena en {scenePath}");
            return;
        }

        // 2. Crear el Prefab del Clítoris a partir de bola.blend si no existe
        string prefabPath = "Assets/Prefab/ClitorisPrefab.prefab";
        GameObject clitorisPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        
        if (clitorisPrefab == null)
        {
            GameObject bolaModel = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/bola.blend");
            if (bolaModel == null)
            {
                Debug.LogError("No se encontró el modelo Assets/bola.blend");
                return;
            }

            // Instanciar temporalmente para configurar
            GameObject tempGo = (GameObject)PrefabUtility.InstantiatePrefab(bolaModel);
            tempGo.name = "ClitorisPrefab";

            // Eliminar BoxCollider si existe
            BoxCollider boxCol = tempGo.GetComponent<BoxCollider>();
            if (boxCol != null) DestroyImmediate(boxCol, true);

            // Añadir SphereCollider con Trigger
            SphereCollider sphereCol = tempGo.GetComponent<SphereCollider>();
            if (sphereCol == null) sphereCol = tempGo.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;

            // Añadir script collectible
            ClitorisCollectible collectible = tempGo.GetComponent<ClitorisCollectible>();
            if (collectible == null) collectible = tempGo.AddComponent<ClitorisCollectible>();

            // Guardar prefab
            clitorisPrefab = PrefabUtility.SaveAsPrefabAsset(tempGo, prefabPath);
            DestroyImmediate(tempGo);
            Debug.Log($"Prefab del Clítoris creado en {prefabPath}");
        }

        // 3. Modificar la bola existente en escena para que actúe como coleccionable (no repelente de balas)
        GameObject bolaInScene = GameObject.Find("bola");
        if (bolaInScene != null)
        {
            // Eliminar BoxCollider
            BoxCollider boxCol = bolaInScene.GetComponent<BoxCollider>();
            if (boxCol != null) DestroyImmediate(boxCol, true);

            // Añadir SphereCollider con Trigger
            SphereCollider sphereCol = bolaInScene.GetComponent<SphereCollider>();
            if (sphereCol == null) sphereCol = bolaInScene.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;

            // Añadir ClitorisCollectible
            ClitorisCollectible collectible = bolaInScene.GetComponent<ClitorisCollectible>();
            if (collectible == null) collectible = bolaInScene.AddComponent<ClitorisCollectible>();

            Debug.Log("Objeto 'bola' estático en escena configurado como ClitorisCollectible.");
        }
        else
        {
            Debug.LogWarning("No se encontró el objeto 'bola' estático en la escena.");
        }

        // 4. Configurar el Spawner
        VirusSpawner virusSpawner = Object.FindObjectOfType<VirusSpawner>();
        ClitorisSpawner existingSpawner = Object.FindObjectOfType<ClitorisSpawner>();
        if (existingSpawner == null)
        {
            GameObject spawnerGo = new GameObject("ClitorisSpawner");
            if (virusSpawner != null)
            {
                spawnerGo.transform.SetParent(virusSpawner.transform.parent);
                spawnerGo.transform.localPosition = virusSpawner.transform.localPosition;
                spawnerGo.transform.localRotation = virusSpawner.transform.localRotation;
            }
            ClitorisSpawner spawner = spawnerGo.AddComponent<ClitorisSpawner>();
            spawner.clitorisPrefab = clitorisPrefab;
            
            // Configuración razonable de Spawner de clítoris
            spawner.spawnInterval = 15f;
            spawner.distanceAhead = 35f;
            spawner.tubeRadius = 3f;

            Debug.Log("ClitorisSpawner creado y configurado en la escena.");
        }
        else
        {
            existingSpawner.clitorisPrefab = clitorisPrefab;
            Debug.Log("ClitorisSpawner ya existía en la escena, prefab actualizado.");
        }

        // 5. Configurar el HUD de Double Shot
        PlayerHealth playerHealth = Object.FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            if (playerHealth.textoDoubleShot == null)
            {
                if (playerHealth.textoVidas != null)
                {
                    // Duplicamos el texto de vidas para heredar formato
                    GameObject doubleShotGo = Object.Instantiate(playerHealth.textoVidas.gameObject, playerHealth.textoVidas.transform.parent);
                    doubleShotGo.name = "HUD_DoubleShot";
                    
                    // Mover ligeramente hacia abajo
                    doubleShotGo.transform.localPosition = playerHealth.textoVidas.transform.localPosition + Vector3.down * 0.25f;
                    
                    TextMeshPro doubleShotText = doubleShotGo.GetComponent<TextMeshPro>();
                    doubleShotText.text = "";
                    
                    playerHealth.textoDoubleShot = doubleShotText;
                    Debug.Log("Texto HUD de Double Shot creado a partir del HUD de Vidas.");
                }
                else
                {
                    Debug.LogWarning("PlayerHealth.textoVidas está vacío. No se pudo duplicar el HUD para el Double Shot.");
                }
            }
            else
            {
                Debug.Log("HUD de Double Shot ya configurado en PlayerHealth.");
            }
        }
        else
        {
            Debug.LogError("No se encontró PlayerHealth en la escena.");
        }

        // Guardar cambios en la escena y assets
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Configuración completada y escena guardada correctamente.");
    }
}
