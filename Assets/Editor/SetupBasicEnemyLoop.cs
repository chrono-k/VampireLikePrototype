using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click flow: ensure a Player exists, then enemy prefab, then wire Enemy Spawner in the open scene.
/// Delegates to <see cref="CreateBasicPlayer"/>, <see cref="CreatePlaceholderEnemyPrefab"/>, and <see cref="SetupBasicEnemySpawner"/>.
/// Menu: Tools/Game Setup/Setup Basic Enemy Loop
/// </summary>
public static class SetupBasicEnemyLoop
{
    const string MenuPath = "Tools/Game Setup/Setup Basic Enemy Loop";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        // 1) Scene: Player so the spawner can assign a transform (and enemies have something to chase).
        CreateBasicPlayer.Run();

        // 2) Asset: enemy prefab (and placeholder PNG) — needed before spawner references it.
        CreatePlaceholderEnemyPrefab.Run();

        // 3) Scene: EnemySpawner + EnemySpawner component + player + prefab references.
        SetupBasicEnemySpawner.Run();

        // 4) Player: GameOverHandler + Inspector refs (needs spawner + health/movement from steps 1–3).
        WireGameOverHandlerOnPlayer();

        Debug.Log(
            "[Game Setup] Basic enemy loop is ready: Player at origin, enemy prefab at Assets/Prefabs/Enemy.prefab, " +
            "Enemy Spawner updated, GameOverHandler wired. Press Play to test.");
    }

    /// <summary>
    /// Ensures <see cref="GameOverHandler"/> exists on the Player and fills only empty serialized references.
    /// Runs after the spawner exists so <c>enemySpawner</c> can be resolved in the scene.
    /// </summary>
    static void WireGameOverHandlerOnPlayer()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            Debug.LogWarning(
                "[Game Setup] No GameObject named \"Player\" — skipping GameOverHandler wiring. " +
                "Run Create Basic Player or name your player object \"Player\".");
            return;
        }

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (health == null || movement == null)
        {
            Debug.LogWarning(
                "[Game Setup] Player is missing PlayerHealth or PlayerMovement — GameOverHandler references may stay empty.");
        }

        GameOverHandler handler = player.GetComponent<GameOverHandler>();
        if (handler == null)
            handler = Undo.AddComponent<GameOverHandler>(player);

        Undo.RecordObject(handler, "Wire GameOver Handler");

        EnemySpawner spawner = FindEnemySpawnerInScene();

        SerializedObject so = new SerializedObject(handler);
        AssignReferenceIfNull(so, "playerHealth", health);
        AssignReferenceIfNull(so, "playerMovement", movement);
        AssignReferenceIfNull(so, "enemySpawner", spawner);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(handler);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    /// <summary>
    /// Prefers the object named EnemySpawner (matches our setup tool), then any EnemySpawner in the scene.
    /// </summary>
    static EnemySpawner FindEnemySpawnerInScene()
    {
        GameObject spawnerObject = GameObject.Find("EnemySpawner");
        if (spawnerObject != null)
        {
            EnemySpawner onNamed = spawnerObject.GetComponent<EnemySpawner>();
            if (onNamed != null)
                return onNamed;
        }

        return Object.FindAnyObjectByType<EnemySpawner>();
    }

    /// <summary>
    /// Only writes when the slot is empty — keeps any hand-assigned references the user already set.
    /// </summary>
    static void AssignReferenceIfNull(SerializedObject so, string propertyName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null)
            return;

        if (prop.objectReferenceValue == null && value != null)
            prop.objectReferenceValue = value;
    }
}
