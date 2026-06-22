using System.IO;
using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public static class RetroRenderToolkitTextureUtility
    {
        public const string LeafTexturePath = RetroRenderToolkitInstaller.PackageRoot + "/Art/Textures/T_Retro_LeafAlpha.png";
        public const string GrassTexturePath = RetroRenderToolkitInstaller.PackageRoot + "/Art/Textures/T_Retro_GrassAlpha.png";
        public const string FenceTexturePath = RetroRenderToolkitInstaller.PackageRoot + "/Art/Textures/T_Retro_FenceAlpha.png";
        public const string CardTexturePath = RetroRenderToolkitInstaller.PackageRoot + "/Art/Textures/T_Retro_CardAlpha.png";

        public static void EnsurePlaceholderTextures()
        {
            RetroRenderToolkitInstaller.EnsureFolder(RetroRenderToolkitInstaller.PackageRoot + "/Art/Textures");
            CreateIfMissing(LeafTexturePath, DrawLeaf);
            CreateIfMissing(GrassTexturePath, DrawGrass);
            CreateIfMissing(FenceTexturePath, DrawFence);
            CreateIfMissing(CardTexturePath, DrawCard);
            AssetDatabase.Refresh();
        }

        public static Texture2D LoadLeafTexture()
        {
            EnsurePlaceholderTextures();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(LeafTexturePath);
        }

        public static Texture2D LoadGrassTexture()
        {
            EnsurePlaceholderTextures();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(GrassTexturePath);
        }

        public static Texture2D LoadFenceTexture()
        {
            EnsurePlaceholderTextures();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(FenceTexturePath);
        }

        public static Texture2D LoadCardTexture()
        {
            EnsurePlaceholderTextures();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(CardTexturePath);
        }

        private static void CreateIfMissing(string path, System.Func<int, int, Color32> drawer)
        {
            if (File.Exists(path))
            {
                return;
            }

            Texture2D texture = new Texture2D(64, 64, TextureFormat.RGBA32, false)
            {
                name = Path.GetFileNameWithoutExtension(path)
            };

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, drawer(x, y));
                }
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = true;
                importer.filterMode = FilterMode.Point;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.SaveAndReimport();
            }
        }

        private static Color32 DrawLeaf(int x, int y)
        {
            float nx = (x - 31.5f) / 31.5f;
            float ny = (y - 31.5f) / 31.5f;
            float leaf = Mathf.Abs(nx * 0.85f) + Mathf.Abs(ny * 0.32f);
            bool stem = Mathf.Abs(nx) < 0.055f && ny < 0.15f;
            byte alpha = leaf < 0.82f || stem ? (byte)255 : (byte)0;
            return new Color32(180, 230, 94, alpha);
        }

        private static Color32 DrawGrass(int x, int y)
        {
            float nx = x / 63f;
            float ny = y / 63f;
            float wave = Mathf.Sin((nx * 5f + ny * 1.5f) * Mathf.PI) * 0.08f;
            bool blade = Mathf.Abs(nx - 0.2f - wave) < 0.055f || Mathf.Abs(nx - 0.48f + wave) < 0.05f || Mathf.Abs(nx - 0.72f - wave) < 0.045f;
            byte alpha = blade && ny > 0.04f ? (byte)255 : (byte)0;
            return new Color32(128, 202, 73, alpha);
        }

        private static Color32 DrawFence(int x, int y)
        {
            bool vertical = x % 18 < 5;
            bool horizontal = y % 24 < 5;
            byte alpha = vertical || horizontal ? (byte)255 : (byte)0;
            return new Color32(190, 170, 132, alpha);
        }

        private static Color32 DrawCard(int x, int y)
        {
            float nx = (x - 31.5f) / 31.5f;
            float ny = (y - 31.5f) / 31.5f;
            bool diamond = Mathf.Abs(nx) + Mathf.Abs(ny) < 1.05f;
            return new Color32(210, 210, 210, diamond ? (byte)255 : (byte)0);
        }
    }
}
