using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click setup for the Enemy Spawner in the open scene (see menu: Tools/Game Setup).
/// Uses SerializedObject so private [SerializeField] fields on EnemySpawner can be assigned from code.
/// </summary>
public static class SetupBasicEnemySpawner
{
    const string MenuPath = "Tools/Game Setup/Setup Basic Enemy Spawner";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        // --- GameObject + component ---
        GameObject go = GameObject.Find("EnemySpawner");
        if (go == null)
        {
            go = new GameObject("EnemySpawner");
            Undo.RegisterCreatedObjectUndo(go, "Create Enemy Spawner");
        }

        EnemySpawner spawner = go.GetComponent<EnemySpawner>();
        if (spawner == null)
        {
            spawner = Undo.AddComponent<EnemySpawner>(go);
        }

        Undo.RecordObject(spawner, "Setup Basic Enemy Spawner");

        SerializedObject so = new SerializedObject(spawner);

        // --- Player (scene) ---
        Transform player = FindPlayerTransform();
        so.FindProperty("player").objectReferenceValue = player;
        if (player == null)
        {
            Debug.LogWarning(
                "[Game Setup] No Player found. Name an object \"Player\", tag it \"Player\", " +
                "or add PlayerMovement — then run this again or assign Player manually on Enemy Spawner.");
        }
        else if (player.GetComponent<PlayerHealth>() == null)
        {
            Undo.AddComponent<PlayerHealth>(player.gameObject);
            EditorUtility.SetDirty(player.gameObject);
        }

        // --- Enemy prefab (project assets) ---
        GameObject enemyPrefab = TryFindEnemyPrefab();
        so.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
        if (enemyPrefab == null)
        {
            EditorUtility.DisplayDialog(
                "Enemy prefab not assigned",
                "Could not find a prefab that has EnemyFollow on the root.\n\n" +
                "Run Tools → Game Setup → Create Placeholder Enemy Prefab (adds trigger hurtbox + EnemyContactDamage), " +
                "or make your own prefab and assign Enemy Prefab on the Enemy Spawner.",
                "OK");
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(spawner);

        EnsureGameOverHandler(player, spawner);

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Adds <see cref="GameOverHandler"/> to the scene and wires health, movement, and spawner references.
    /// </summary>
    static void EnsureGameOverHandler(Transform player, EnemySpawner spawner)
    {
        GameObject handlerGo = GameObject.Find("GameOverHandler");
        if (handlerGo == null)
        {
            handlerGo = new GameObject("GameOverHandler");
            Undo.RegisterCreatedObjectUndo(handlerGo, "Create Game Over Handler");
        }

        GameOverHandler handler = handlerGo.GetComponent<GameOverHandler>();
        if (handler == null)
            handler = Undo.AddComponent<GameOverHandler>(handlerGo);

        Undo.RecordObject(handler, "Wire Game Over Handler");

        SerializedObject ho = new SerializedObject(handler);
        ho.FindProperty("playerHealth").objectReferenceValue =
            player != null ? player.GetComponent<PlayerHealth>() : null;
        ho.FindProperty("playerMovement").objectReferenceValue =
            player != null ? player.GetComponent<PlayerMovement>() : null;
        ho.FindProperty("enemySpawner").objectReferenceValue = spawner;
        ho.ApplyModifiedProperties();

        EditorUtility.SetDirty(handler);
    }

    /// <summary>
    /// Tries tag "Player", then exact name "Player", then any object with PlayerMovement.
    /// </summary>
    static Transform FindPlayerTransform()
    {
        GameObject byTag = null;
        try
        {
            byTag = GameObject.FindGameObjectWithTag("Player");
        }
        catch (UnityException)
        {
            // Tag "Player" is not defined in Tag Manager; skip and try other methods.
        }

        if (byTag != null)
            return byTag.transform;

        GameObject byName = GameObject.Find("Player");
        if (byName != null)
            return byName.transform;

        PlayerMovement movement = Object.FindFirstObjectByType<PlayerMovement>();
        if (movement != null)
            return movement.transform;

        return null;
    }

    /// <summary>
    /// Looks for prefabs whose root has EnemyFollow. If several exist, prefers a name containing "enemy".
    /// </summary>
    static GameObject TryFindEnemyPrefab()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        GameObject firstMatch = null;
        GameObject preferredByName = null;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null || prefab.GetComponent<EnemyFollow>() == null)
                continue;

            if (firstMatch == null)
                firstMatch = prefab;

            if (prefab.name.IndexOf("enemy", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                preferredByName = prefab;
                break;
            }
        }

        return preferredByName != null ? preferredByName : firstMatch;
    }
}
