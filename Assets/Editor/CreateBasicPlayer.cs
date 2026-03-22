using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Creates or updates a minimal Player in the active scene for top-down prototyping.
/// Safe to run repeatedly: fills in missing components and refreshes key settings without duplicating the Player.
/// Menu: Tools/Game Setup/Create Basic Player
/// </summary>
public static class CreateBasicPlayer
{
    const string MenuPath = "Tools/Game Setup/Create Basic Player";

    const string PlaceholderTexturePath = "Assets/Prefabs/PlayerPlaceholder.png";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null)
        {
            player = new GameObject("Player");
            Undo.RegisterCreatedObjectUndo(player, "Create Basic Player");
        }

        Undo.RecordObject(player.transform, "Create Basic Player");
        player.transform.position = Vector3.zero;

        // Helps EnemySpawner and other code find the player without manual assignment.
        Undo.RecordObject(player, "Create Basic Player");
        try
        {
            player.tag = "Player";
        }
        catch (UnityException)
        {
            Debug.LogWarning(
                "[Game Setup] Tag \"Player\" is not defined in Tag Manager. " +
                "Add it under Edit → Project Settings → Tags and Layers, or leave the default.");
        }

        // --- Placeholder art (blue square; separate PNG from the red enemy placeholder) ---
        Sprite sprite = GameSetupPlaceholderTexture.GetOrCreateSquareSprite(
            PlaceholderTexturePath,
            new Color(0.2f, 0.55f, 0.95f));
        if (sprite == null)
        {
            Debug.LogWarning(
                "[Game Setup] Could not load/create player placeholder sprite. " +
                "Assign a Sprite on the Player's Sprite Renderer when ready.");
        }

        SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = Undo.AddComponent<SpriteRenderer>(player);
        else
            Undo.RecordObject(sr, "Create Basic Player");

        sr.sprite = sprite;
        sr.color = Color.white;

        // --- Physics: top-down ---
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = Undo.AddComponent<Rigidbody2D>(player);
        else
            Undo.RecordObject(rb, "Create Basic Player");

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // --- Solid collider for physics (enemies often use trigger hurtboxes) ---
        Collider2D col = player.GetComponent<Collider2D>();
        if (col == null)
            col = Undo.AddComponent<BoxCollider2D>(player);
        else
            Undo.RecordObject(col, "Create Basic Player");

        col.isTrigger = false;
        if (col is BoxCollider2D box)
            box.size = new Vector2(0.9f, 0.9f);
        else if (col is CircleCollider2D circle)
            circle.radius = 0.45f;

        if (player.GetComponent<PlayerMovement>() == null)
            Undo.AddComponent<PlayerMovement>(player);

        if (player.GetComponent<PlayerHealth>() == null)
            Undo.AddComponent<PlayerHealth>(player);

        WireCameraFollowTarget(player.transform);

        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        Debug.Log("[Game Setup] Basic Player ready at (0,0,0) with movement + health. Save the scene if prompted.");
    }

    /// <summary>
    /// If the main rendering camera has <see cref="CameraFollow"/>, point it at the Player.
    /// </summary>
    static void WireCameraFollowTarget(Transform playerTransform)
    {
        Camera cam = Camera.main;
        if (cam == null)
            cam = Object.FindFirstObjectByType<Camera>();

        if (cam == null)
            return;

        CameraFollow follow = cam.GetComponent<CameraFollow>();
        if (follow == null)
            return;

        Undo.RecordObject(follow, "Create Basic Player");
        SerializedObject so = new SerializedObject(follow);
        so.FindProperty("target").objectReferenceValue = playerTransform;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(follow);
    }
}
