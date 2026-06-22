using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class RetroMaterialPreset : ScriptableObject
    {
        [SerializeField] private string shaderName;
        [SerializeField] private List<FloatEntry> floats = new List<FloatEntry>();
        [SerializeField] private List<ColorEntry> colors = new List<ColorEntry>();
        [SerializeField] private List<VectorEntry> vectors = new List<VectorEntry>();

        [Serializable]
        public struct FloatEntry
        {
            public string name;
            public float value;
        }

        [Serializable]
        public struct ColorEntry
        {
            public string name;
            public Color value;
        }

        [Serializable]
        public struct VectorEntry
        {
            public string name;
            public Vector4 value;
        }

        [Serializable]
        private sealed class PresetData
        {
            public string name;
            public string shaderName;
            public List<FloatEntry> floats = new List<FloatEntry>();
            public List<ColorEntry> colors = new List<ColorEntry>();
            public List<VectorEntry> vectors = new List<VectorEntry>();
        }

        public string ShaderName => shaderName;

        public void Capture(Material material)
        {
            shaderName = material != null && material.shader != null ? material.shader.name : string.Empty;
            floats.Clear();
            colors.Clear();
            vectors.Clear();

            if (material == null || material.shader == null)
            {
                return;
            }

            int count = material.shader.GetPropertyCount();
            for (int i = 0; i < count; i++)
            {
                string propertyName = material.shader.GetPropertyName(i);
                if (ShouldSkip(propertyName))
                {
                    continue;
                }

                ShaderPropertyType type = material.shader.GetPropertyType(i);
                if ((type == ShaderPropertyType.Float || type == ShaderPropertyType.Range) && material.HasProperty(propertyName))
                {
                    floats.Add(new FloatEntry { name = propertyName, value = material.GetFloat(propertyName) });
                }
                else if (type == ShaderPropertyType.Color && material.HasProperty(propertyName))
                {
                    colors.Add(new ColorEntry { name = propertyName, value = material.GetColor(propertyName) });
                }
                else if (type == ShaderPropertyType.Vector && material.HasProperty(propertyName))
                {
                    vectors.Add(new VectorEntry { name = propertyName, value = material.GetVector(propertyName) });
                }
            }
        }

        public void ApplyTo(Material material)
        {
            if (material == null)
            {
                return;
            }

            Undo.RecordObject(material, "Apply Retro Material Preset");

            foreach (FloatEntry entry in floats)
            {
                if (material.HasProperty(entry.name))
                {
                    material.SetFloat(entry.name, entry.value);
                }
            }

            foreach (ColorEntry entry in colors)
            {
                if (material.HasProperty(entry.name))
                {
                    material.SetColor(entry.name, entry.value);
                }
            }

            foreach (VectorEntry entry in vectors)
            {
                if (material.HasProperty(entry.name))
                {
                    material.SetVector(entry.name, entry.value);
                }
            }

            RetroMaterialConverter.Setup(material, DetectTarget(material));
            EditorUtility.SetDirty(material);
        }

        public string ToJson()
        {
            PresetData data = ToData();
            return JsonUtility.ToJson(data, true);
        }

        public void FromJson(string json)
        {
            PresetData data = JsonUtility.FromJson<PresetData>(json);
            if (data == null)
            {
                return;
            }

            shaderName = data.shaderName;
            floats = data.floats ?? new List<FloatEntry>();
            colors = data.colors ?? new List<ColorEntry>();
            vectors = data.vectors ?? new List<VectorEntry>();
        }

        public void ExportJson(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, ToJson());
            }
        }

        public static RetroMaterialPreset ImportJson(string path, string assetPath)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(assetPath) || !File.Exists(path))
            {
                return null;
            }

            RetroMaterialPreset preset = CreateInstance<RetroMaterialPreset>();
            preset.FromJson(File.ReadAllText(path));
            preset.name = Path.GetFileNameWithoutExtension(assetPath);
            AssetDatabase.CreateAsset(preset, assetPath);
            AssetDatabase.SaveAssets();
            return preset;
        }

        public static RetroMaterialPreset SaveFromMaterial(Material material, string presetName)
        {
            if (material == null)
            {
                return null;
            }

            RetroRenderToolkitInstaller.EnsureFolder(RetroRenderToolkitInstaller.UserPresetFolder);
            string safeName = string.IsNullOrWhiteSpace(presetName) ? $"{material.name}_Preset" : presetName.Trim();
            foreach (char invalid in Path.GetInvalidFileNameChars())
            {
                safeName = safeName.Replace(invalid, '_');
            }

            string path = AssetDatabase.GenerateUniqueAssetPath($"{RetroRenderToolkitInstaller.UserPresetFolder}/{safeName}.asset");
            RetroMaterialPreset preset = CreateInstance<RetroMaterialPreset>();
            preset.name = safeName;
            preset.Capture(material);
            AssetDatabase.CreateAsset(preset, path);
            AssetDatabase.SaveAssets();
            return preset;
        }

        private PresetData ToData()
        {
            return new PresetData
            {
                name = name,
                shaderName = shaderName,
                floats = new List<FloatEntry>(floats),
                colors = new List<ColorEntry>(colors),
                vectors = new List<VectorEntry>(vectors)
            };
        }

        private static bool ShouldSkip(string propertyName)
        {
            return propertyName == "_BaseMap" ||
                   propertyName == "_MainTex" ||
                   propertyName == "_BaseColor" ||
                   propertyName == "_Color" ||
                   propertyName.EndsWith("_TexelSize", StringComparison.Ordinal) ||
                   propertyName.EndsWith("_ST", StringComparison.Ordinal);
        }

        private static RetroMaterialTarget DetectTarget(Material material)
        {
            string currentShaderName = material != null && material.shader != null ? material.shader.name : string.Empty;
            if (currentShaderName == RetroRenderToolkitInstaller.TerrainShaderName) return RetroMaterialTarget.Terrain;
            if (currentShaderName == RetroRenderToolkitInstaller.CutoutShaderName) return RetroMaterialTarget.Cutout;
            if (currentShaderName == RetroRenderToolkitInstaller.FoliageShaderName) return RetroMaterialTarget.Foliage;
            if (currentShaderName == RetroRenderToolkitInstaller.UnlitCutoutShaderName) return RetroMaterialTarget.UnlitCutout;
            if (currentShaderName == RetroRenderToolkitInstaller.WaterShaderName) return RetroMaterialTarget.Water;
            if (currentShaderName == RetroRenderToolkitInstaller.MaterialOutlineShaderName) return RetroMaterialTarget.MaterialOutline;
            if (currentShaderName == RetroRenderToolkitInstaller.SpriteLitShaderName) return RetroMaterialTarget.SpriteLit;
            if (currentShaderName == RetroRenderToolkitInstaller.SpriteUnlitShaderName) return RetroMaterialTarget.SpriteUnlit;
            return RetroMaterialTarget.Hybrid;
        }
    }
}
