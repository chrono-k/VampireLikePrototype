using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Shared editor helper: writes a small PNG under Assets, imports it as a Sprite, returns the Sprite reference.
/// Used by setup tools so placeholder art is not duplicated in multiple files.
/// </summary>
public static class GameSetupPlaceholderTexture
{
    /// <summary>
    /// Creates the PNG on first use; later runs reuse the file (same as enemy/player setup tools).
    /// </summary>
    public static Sprite GetOrCreateSquareSprite(string assetPath, Color fill, int size = 32, float pixelsPerUnit = 32f)
    {
        EnsureParentFoldersExist(assetPath);

        string fullPath = FullPath(assetPath);
        bool createdNewFile = !File.Exists(fullPath);
        if (createdNewFile)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, fill);
            tex.Apply();

            byte[] png = tex.EncodeToPNG();
            Object.DestroyImmediate(tex);

            File.WriteAllBytes(fullPath, png);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            ConfigureTextureAsSprite(assetPath, pixelsPerUnit);
        }

        Sprite sprite = LoadFirstSpriteAtPath(assetPath);
        if (sprite == null)
        {
            ConfigureTextureAsSprite(assetPath, pixelsPerUnit);
            sprite = LoadFirstSpriteAtPath(assetPath);
        }

        return sprite;
    }

    static void EnsureParentFoldersExist(string assetPath)
    {
        int last = assetPath.LastIndexOf('/');
        if (last <= 0)
            return;

        string folder = assetPath.Substring(0, last);
        if (AssetDatabase.IsValidFolder(folder))
            return;

        string[] parts = folder.Split('/');
        if (parts.Length < 2 || parts[0] != "Assets")
            return;

        string current = "Assets";
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static void ConfigureTextureAsSprite(string assetPath, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Point;
        importer.mipmapEnabled = false;
        importer.SaveAndReimport();
    }

    static Sprite LoadFirstSpriteAtPath(string assetPath)
    {
        foreach (Object obj in AssetDatabase.LoadAllAssetsAtPath(assetPath))
        {
            if (obj is Sprite s)
                return s;
        }

        return null;
    }

    static string FullPath(string assetPath)
    {
        string relative = assetPath.StartsWith("Assets/")
            ? assetPath.Substring("Assets/".Length)
            : assetPath;
        return Path.Combine(Application.dataPath, relative);
    }
}
