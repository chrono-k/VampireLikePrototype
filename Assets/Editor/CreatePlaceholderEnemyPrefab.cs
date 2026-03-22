using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates or updates <c>Assets/Prefabs/Enemy.prefab</c> with everything needed for chase + trigger contact damage.
/// Safe to run many times: existing prefabs are opened and merged (missing components added, key settings refreshed).
/// Menu: Tools/Game Setup/Create Placeholder Enemy Prefab
/// </summary>
public static class CreatePlaceholderEnemyPrefab
{
    const string MenuPath = "Tools/Game Setup/Create Placeholder Enemy Prefab";

    const string PrefabPath = "Assets/Prefabs/Enemy.prefab";

    /// <summary>PNG next to the prefab; reused on later runs so we do not spam duplicate textures.</summary>
    const string PlaceholderTexturePath = "Assets/Prefabs/EnemyPlaceholder.png";

    [MenuItem(MenuPath)]
    public static void Run()
    {
        Sprite sprite = GameSetupPlaceholderTexture.GetOrCreateSquareSprite(
            PlaceholderTexturePath,
            new Color(0.78f, 0.2f, 0.2f));
        if (sprite == null)
        {
            Debug.LogWarning(
                "[Game Setup] Could not create or load a placeholder sprite. " +
                "The enemy prefab will still be saved — assign a Sprite on the Sprite Renderer when ready.");
        }

        bool updatingExisting = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null;
        GameObject root;

        if (updatingExisting)
        {
            // Open the asset on disk so we can merge components/settings without destroying child objects you may have added.
            root = PrefabUtility.LoadPrefabContents(PrefabPath);
        }
        else
        {
            root = new GameObject("Enemy");
        }

        try
        {
            EnsureEnemyRootSupportsContactDamage(root, sprite);
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            if (updatingExisting)
                PrefabUtility.UnloadPrefabContents(root);
            else
                Object.DestroyImmediate(root);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        string action = updatingExisting ? "Updated" : "Created";
        Debug.Log($"[Game Setup] {action} enemy prefab at {PrefabPath} (trigger hurtbox + EnemyContactDamage).");
    }

    /// <summary>
    /// Ensures the root has visuals, top-down Rigidbody2D, a trigger Collider2D, chase, and contact damage.
    /// </summary>
    static void EnsureEnemyRootSupportsContactDamage(GameObject root, Sprite sprite)
    {
        // --- Visual (optional placeholder sprite) ---
        SpriteRenderer sr = root.GetComponent<SpriteRenderer>();
        if (sr == null)
            sr = root.AddComponent<SpriteRenderer>();
        if (sprite != null)
            sr.sprite = sprite;
        sr.color = Color.white;

        // --- Physics: top-down — no gravity, no spin from collisions ---
        Rigidbody2D rb = root.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = root.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // --- Hurtbox: trigger overlap drives EnemyContactDamage (player keeps a non-trigger body) ---
        Collider2D[] colliders = root.GetComponents<Collider2D>();
        if (colliders.Length == 0)
        {
            CircleCollider2D circle = root.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
        }
        else
        {
            // If the root ever has more than one collider, keep them all triggers so contact stays consistent.
            foreach (Collider2D c in colliders)
                c.isTrigger = true;
        }

        if (root.GetComponent<EnemyFollow>() == null)
            root.AddComponent<EnemyFollow>();

        if (root.GetComponent<EnemyContactDamage>() == null)
            root.AddComponent<EnemyContactDamage>();
    }
}
