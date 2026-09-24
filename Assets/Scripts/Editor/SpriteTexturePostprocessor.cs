using UnityEditor;
using UnityEngine;

/// <summary>
/// Автоматически настраивает все изображения в папке Assets/Sprites как 2D Sprite (UI).
/// </summary>
public class SpriteTexturePostprocessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath.StartsWith("Assets/Sprites/"))
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
        }
    }
}
