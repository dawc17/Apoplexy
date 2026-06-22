using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public enum RetroMaterialTarget
    {
        Hybrid,
        Terrain,
        Cutout,
        Foliage,
        UnlitCutout,
        Water,
        MaterialOutline,
        SpriteLit,
        SpriteUnlit
    }

    public enum RetroPresetChoice
    {
        None,
        CleanPS2,
        CrunchyPSX,
        MobileFast,
        HorrorPSX,
        GrassCard,
        BushLeaves,
        DitherFadeCutout
    }

    public static class RetroMaterialConverter
    {
        public static bool createCopies = true;
        public static bool backupWhenOverwriting = true;
        public static bool preserveProperties = true;
        public static bool applyPresetAfterConversion = true;
        public static RetroMaterialTarget targetShader = RetroMaterialTarget.Hybrid;
        public static RetroPresetChoice preset = RetroPresetChoice.CleanPS2;

        public static void ConvertSelectedMaterials(RetroMaterialTarget target, bool createCopy, bool preserve, RetroPresetChoice presetChoice)
        {
            List<Material> materials = GetSelectedMaterials();
            ConvertMaterials(materials, target, createCopy, backupWhenOverwriting, preserve, presetChoice);
        }

        public static void ConvertSelectedWithCurrentOptions()
        {
            ConvertMaterials(GetSelectedMaterials(), targetShader, createCopies, backupWhenOverwriting, preserveProperties, applyPresetAfterConversion ? preset : RetroPresetChoice.None);
        }

        public static void ConvertMaterialsInSelectedFolder()
        {
            string folder = GetSelectedFolder();
            if (string.IsNullOrEmpty(folder))
            {
                Debug.LogWarning("Select a project folder before converting materials in a folder.");
                return;
            }

            List<Material> materials = new List<Material>();
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { folder }))
            {
                Material material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (material != null)
                {
                    materials.Add(material);
                }
            }

            ConvertMaterials(materials, targetShader, createCopies, backupWhenOverwriting, preserveProperties, applyPresetAfterConversion ? preset : RetroPresetChoice.None);
        }

        public static string BuildDryRun()
        {
            List<Material> materials = GetSelectedMaterials();
            if (materials.Count == 0)
            {
                return "No selected materials.";
            }

            List<string> lines = new List<string>();
            foreach (Material material in materials)
            {
                lines.Add($"{AssetDatabase.GetAssetPath(material)} -> {GetShaderName(targetShader)}");
            }

            return string.Join("\n", lines);
        }

        public static bool IsToolkitShader(Material material)
        {
            return material != null && material.shader != null && material.shader.name.StartsWith("Borzblade/Retro Render Toolkit", System.StringComparison.Ordinal);
        }

        private static void ConvertMaterials(IReadOnlyList<Material> sourceMaterials, RetroMaterialTarget target, bool createCopy, bool backupOriginal, bool preserve, RetroPresetChoice presetChoice)
        {
            Shader shader = Shader.Find(GetShaderName(target));
            if (shader == null)
            {
                Debug.LogError($"Could not find target shader '{GetShaderName(target)}'.");
                return;
            }

            int converted = 0;
            foreach (Material source in sourceMaterials)
            {
                if (source == null)
                {
                    continue;
                }

                MaterialSnapshot snapshot = MaterialSnapshot.Capture(source);
                Material destination = source;
                string sourcePath = AssetDatabase.GetAssetPath(source);

                if (createCopy)
                {
                    string folder = Path.GetDirectoryName(sourcePath)?.Replace('\\', '/');
                    string copyPath = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{source.name}_{target}.mat");
                    AssetDatabase.CopyAsset(sourcePath, copyPath);
                    destination = AssetDatabase.LoadAssetAtPath<Material>(copyPath);
                }
                else if (backupOriginal)
                {
                    string backupPath = AssetDatabase.GenerateUniqueAssetPath($"{Path.GetDirectoryName(sourcePath)?.Replace('\\', '/')}/{source.name}_Backup.mat");
                    AssetDatabase.CopyAsset(sourcePath, backupPath);
                }

                if (destination == null)
                {
                    continue;
                }

                Undo.RecordObject(destination, "Convert Retro Material");
                destination.shader = shader;

                if (preserve)
                {
                    snapshot.Apply(destination);
                }

                ApplyPreset(destination, target, presetChoice);
                Setup(destination, target);
                EditorUtility.SetDirty(destination);
                converted++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Converted {converted} material(s) to {GetShaderName(target)}.");
        }

        public static void ApplyPreset(Material material, RetroMaterialTarget target, RetroPresetChoice presetChoice)
        {
            if (material == null || presetChoice == RetroPresetChoice.None)
            {
                return;
            }

            switch (target)
            {
                case RetroMaterialTarget.Cutout:
                    if (presetChoice == RetroPresetChoice.CrunchyPSX) PSXPS2CutoutShaderGUI.ApplyCrunchyPSXCutout(material);
                    else if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2CutoutShaderGUI.ApplyMobileCutout(material);
                    else if (presetChoice == RetroPresetChoice.DitherFadeCutout) PSXPS2CutoutShaderGUI.ApplyDitherFadeCutout(material);
                    else PSXPS2CutoutShaderGUI.ApplyCleanCutout(material);
                    break;
                case RetroMaterialTarget.Foliage:
                    if (presetChoice == RetroPresetChoice.GrassCard) PSXPS2FoliageShaderGUI.ApplyGrassCard(material);
                    else if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2FoliageShaderGUI.ApplyMobileFast(material);
                    else if (presetChoice == RetroPresetChoice.HorrorPSX) PSXPS2FoliageShaderGUI.ApplyDarkFantasy(material);
                    else PSXPS2FoliageShaderGUI.ApplyBushLeaves(material);
                    break;
                case RetroMaterialTarget.UnlitCutout:
                    if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2UnlitCutoutShaderGUI.ApplyMobileFast(material);
                    else if (presetChoice == RetroPresetChoice.DitherFadeCutout) PSXPS2UnlitCutoutShaderGUI.ApplyDitherFadeCard(material);
                    else PSXPS2UnlitCutoutShaderGUI.ApplyDistantFoliage(material);
                    break;
                case RetroMaterialTarget.Water:
                    if (presetChoice == RetroPresetChoice.CrunchyPSX || presetChoice == RetroPresetChoice.HorrorPSX) PSXPS2WaterShaderGUI.ApplyCrunchyPSX(material);
                    else if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2WaterShaderGUI.ApplyMobileFast(material);
                    else PSXPS2WaterShaderGUI.ApplyCleanPS2(material);
                    break;
                case RetroMaterialTarget.MaterialOutline:
                    if (presetChoice == RetroPresetChoice.CrunchyPSX || presetChoice == RetroPresetChoice.HorrorPSX) PSXPS2MaterialOutlineShaderGUI.ApplyCrunchy(material);
                    else PSXPS2MaterialOutlineShaderGUI.ApplyBlack(material);
                    break;
                case RetroMaterialTarget.SpriteLit:
                case RetroMaterialTarget.SpriteUnlit:
                    if (presetChoice == RetroPresetChoice.CrunchyPSX || presetChoice == RetroPresetChoice.HorrorPSX) PSXPS2SpriteShaderGUI.ApplyCrunchyPSX(material);
                    else if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2SpriteShaderGUI.ApplyMobileFast(material);
                    else PSXPS2SpriteShaderGUI.ApplyCleanSprite(material);
                    break;
                case RetroMaterialTarget.Terrain:
                    if (presetChoice == RetroPresetChoice.CrunchyPSX) PSXPS2TerrainShaderGUI.ApplyCrunchyPSX(material);
                    else if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2TerrainShaderGUI.ApplyMobileFast(material);
                    else PSXPS2TerrainShaderGUI.ApplyCleanPS2(material);
                    break;
                default:
                    if (presetChoice == RetroPresetChoice.CrunchyPSX || presetChoice == RetroPresetChoice.HorrorPSX) PSXPS2HybridShaderGUI.ApplyCrunchyPSX(material);
                    else if (presetChoice == RetroPresetChoice.MobileFast) PSXPS2HybridShaderGUI.ApplyMobileFast(material);
                    else PSXPS2HybridShaderGUI.ApplyCleanPS2(material);
                    break;
            }
        }

        public static void Setup(Material material, RetroMaterialTarget target)
        {
            switch (target)
            {
                case RetroMaterialTarget.Cutout:
                    PSXPS2CutoutShaderGUI.SetupMaterial(material);
                    break;
                case RetroMaterialTarget.Foliage:
                    PSXPS2FoliageShaderGUI.SetupMaterial(material);
                    break;
                case RetroMaterialTarget.UnlitCutout:
                    PSXPS2UnlitCutoutShaderGUI.SetupMaterial(material);
                    break;
                case RetroMaterialTarget.Terrain:
                    PSXPS2TerrainShaderGUI.SetupMaterial(material);
                    break;
                case RetroMaterialTarget.Water:
                    PSXPS2WaterShaderGUI.SetupMaterial(material);
                    break;
                case RetroMaterialTarget.MaterialOutline:
                    PSXPS2MaterialOutlineShaderGUI.SetupMaterial(material);
                    break;
                case RetroMaterialTarget.SpriteLit:
                case RetroMaterialTarget.SpriteUnlit:
                    PSXPS2SpriteShaderGUI.SetupMaterial(material);
                    break;
                default:
                    PSXPS2HybridShaderGUI.SetupMaterial(material);
                    break;
            }
        }

        public static List<Material> GetSelectedMaterials()
        {
            List<Material> materials = new List<Material>();
            foreach (Object selected in Selection.objects)
            {
                if (selected is Material material)
                {
                    materials.Add(material);
                }
            }

            return materials;
        }

        private static string GetSelectedFolder()
        {
            foreach (Object selected in Selection.objects)
            {
                string path = AssetDatabase.GetAssetPath(selected);
                if (AssetDatabase.IsValidFolder(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static string GetShaderName(RetroMaterialTarget target)
        {
            return target switch
            {
                RetroMaterialTarget.Terrain => RetroRenderToolkitInstaller.TerrainShaderName,
                RetroMaterialTarget.Cutout => RetroRenderToolkitInstaller.CutoutShaderName,
                RetroMaterialTarget.Foliage => RetroRenderToolkitInstaller.FoliageShaderName,
                RetroMaterialTarget.UnlitCutout => RetroRenderToolkitInstaller.UnlitCutoutShaderName,
                RetroMaterialTarget.Water => RetroRenderToolkitInstaller.WaterShaderName,
                RetroMaterialTarget.MaterialOutline => RetroRenderToolkitInstaller.MaterialOutlineShaderName,
                RetroMaterialTarget.SpriteLit => RetroRenderToolkitInstaller.SpriteLitShaderName,
                RetroMaterialTarget.SpriteUnlit => RetroRenderToolkitInstaller.SpriteUnlitShaderName,
                _ => RetroRenderToolkitInstaller.HybridShaderName
            };
        }

        private sealed class MaterialSnapshot
        {
            private Texture baseMap;
            private Color baseColor = Color.white;
            private Texture normalMap;
            private Texture emissionMap;
            private Color emissionColor = Color.black;
            private float metallic;
            private float smoothness;
            private float cutoff = 0.45f;
            private float cull = 2f;
            private float alphaClip;
            private int renderQueue;

            public static MaterialSnapshot Capture(Material material)
            {
                return new MaterialSnapshot
                {
                    baseMap = GetTexture(material, "_BaseMap") ?? GetTexture(material, "_MainTex"),
                    baseColor = GetColor(material, "_BaseColor", GetColor(material, "_Color", Color.white)),
                    normalMap = GetTexture(material, "_BumpMap") ?? GetTexture(material, "_NormalMap"),
                    emissionMap = GetTexture(material, "_EmissionMap"),
                    emissionColor = GetColor(material, "_EmissionColor", Color.black),
                    metallic = GetFloat(material, "_Metallic", 0f),
                    smoothness = GetFloat(material, "_Smoothness", 0.35f),
                    cutoff = GetFloat(material, "_Cutoff", 0.45f),
                    cull = GetFloat(material, "_Cull", 2f),
                    alphaClip = GetFloat(material, "_AlphaClip", GetFloat(material, "_AlphaClipThreshold", 0f)),
                    renderQueue = material.renderQueue
                };
            }

            public void Apply(Material material)
            {
                SetTexture(material, "_BaseMap", baseMap);
                SetTexture(material, "_MainTex", baseMap);
                SetColor(material, "_BaseColor", baseColor);
                SetColor(material, "_Color", baseColor);
                SetTexture(material, "_BumpMap", normalMap);
                SetTexture(material, "_NormalMap", normalMap);
                SetTexture(material, "_EmissionMap", emissionMap);
                SetColor(material, "_EmissionColor", emissionColor);
                SetFloat(material, "_Metallic", metallic);
                SetFloat(material, "_Smoothness", smoothness);
                SetFloat(material, "_Cutoff", cutoff);
                SetFloat(material, "_Cull", cull);
                SetFloat(material, "_CullMode", cull);
                SetFloat(material, "_AlphaClip", alphaClip);
                material.renderQueue = renderQueue;
            }

            private static Texture GetTexture(Material material, string property)
            {
                return material.HasProperty(property) ? material.GetTexture(property) : null;
            }

            private static Color GetColor(Material material, string property, Color fallback)
            {
                return material.HasProperty(property) ? material.GetColor(property) : fallback;
            }

            private static float GetFloat(Material material, string property, float fallback)
            {
                return material.HasProperty(property) ? material.GetFloat(property) : fallback;
            }

            private static void SetTexture(Material material, string property, Texture texture)
            {
                if (texture != null && material.HasProperty(property))
                {
                    material.SetTexture(property, texture);
                }
            }

            private static void SetColor(Material material, string property, Color color)
            {
                if (material.HasProperty(property))
                {
                    material.SetColor(property, color);
                }
            }

            private static void SetFloat(Material material, string property, float value)
            {
                if (material.HasProperty(property))
                {
                    material.SetFloat(property, value);
                }
            }
        }
    }
}
