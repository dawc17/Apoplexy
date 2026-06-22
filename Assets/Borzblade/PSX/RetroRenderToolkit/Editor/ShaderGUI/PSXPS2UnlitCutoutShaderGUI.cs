using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class PSXPS2UnlitCutoutShaderGUI : ShaderGUI
    {
        private enum Preset
        {
            DistantFoliage,
            SpriteProp,
            DitherFadeCard,
            MobileFast
        }

        private static bool surfaceFoldout = true;
        private static bool cutoutFoldout = true;
        private static bool retroFoldout = true;
        private static bool fogFoldout;
        private static bool advancedFoldout;

        private MaterialEditor editor;
        private MaterialProperty[] properties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] materialProperties)
        {
            editor = materialEditor;
            properties = materialProperties;

            DrawPresets();
            EditorGUI.BeginChangeCheck();
            DrawSurface();
            DrawCutout();
            DrawRetro();
            DrawFog();
            DrawAdvanced();

            if (EditorGUI.EndChangeCheck())
            {
                ForEachMaterial(SetupMaterial, "Edit Retro Unlit Cutout Material");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyDistantFoliage(material);
            SetupMaterial(material);
        }

        private void DrawPresets()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Unlit Cutout Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Distant", Preset.DistantFoliage);
                    PresetButton("Sprite", Preset.SpriteProp);
                    PresetButton("Dither Fade", Preset.DitherFadeCard);
                    PresetButton("Mobile", Preset.MobileFast);
                }
            }
        }

        private void PresetButton(string label, Preset preset)
        {
            if (GUILayout.Button(label))
            {
                ForEachMaterial(material =>
                {
                    ApplyPreset(material, preset);
                    SetupMaterial(material);
                }, $"Apply {label} Unlit Cutout Preset");
            }
        }

        private void DrawSurface()
        {
            surfaceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(surfaceFoldout, "Surface");
            if (surfaceFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.DrawTextureWithColor(editor, properties, "_BaseMap", "_BaseColor", "Base Map");
                    RetroShaderGUIUtility.DrawTextureWithColor(editor, properties, "_EmissionMap", "_EmissionColor", "Emission");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawFog()
        {
            fogFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(fogFoldout, "Retro Fog");
            if (fogFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogEnabled", "Retro Fog");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogColor", "Fog Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogStart", "Start");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogEnd", "End");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogDensity", "Density");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogSteps", "Steps");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogBlendMode", "Blend Mode");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawCutout()
        {
            cutoutFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(cutoutFoldout, "Cutout");
            if (cutoutFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_Cutoff", "Alpha Cutoff");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ShadowCutoff", "Shadow Cutoff");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherCutoutFadeEnabled", "Dithered Fade");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherFadeAmount", "Fade Amount");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherFadeStart", "Fade Start");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherFadeEnd", "Fade End");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRetro()
        {
            retroFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(retroFoldout, "Retro Texture / Color");
            if (retroFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelEnabled", "UV Pixelation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelStrength", "UV Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelResolution", "UV Resolution");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelAspect", "UV Aspect");
                    RetroShaderGUIUtility.Draw(editor, properties, "_MipBias", "Mip Bias");
                    RetroShaderGUIUtility.Draw(editor, properties, "_PosterizeEnabled", "Posterize");
                    RetroShaderGUIUtility.Draw(editor, properties, "_PosterizeSteps", "Posterize Steps");
                    RetroShaderGUIUtility.Draw(editor, properties, "_PaletteStrength", "Palette Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_PaletteSteps", "Palette Steps");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherEnabled", "Ordered Dither");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherStrength", "Dither Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherScale", "Dither Scale");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAdvanced()
        {
            advancedFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(advancedFoldout, "Advanced");
            if (advancedFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_TwoSidedEnabled", "Two Sided");
                    RetroShaderGUIUtility.Draw(editor, properties, "_CullMode", "Cull Mode");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AlphaToMask", "Alpha To Coverage");
                    RetroShaderGUIUtility.Draw(editor, properties, "_QueueOffset", "Queue Offset");
                    editor.EnableInstancingField();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void ForEachMaterial(System.Action<Material> action, string undoName)
        {
            Undo.RecordObjects(editor.targets, undoName);
            foreach (Object target in editor.targets)
            {
                if (target is Material material)
                {
                    action(material);
                    EditorUtility.SetDirty(material);
                }
            }
        }

        private static void ApplyPreset(Material material, Preset preset)
        {
            switch (preset)
            {
                case Preset.SpriteProp:
                    ApplySpriteProp(material);
                    break;
                case Preset.DitherFadeCard:
                    ApplyDitherFadeCard(material);
                    break;
                case Preset.MobileFast:
                    ApplyMobileFast(material);
                    break;
                default:
                    ApplyDistantFoliage(material);
                    break;
            }
        }

        public static void ApplyDistantFoliage(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.42f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 22f);
        }

        public static void ApplySpriteProp(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 32f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
        }

        public static void ApplyDitherFadeCard(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_DitherCutoutFadeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeAmount", 0.8f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeStart", 22f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeEnd", 70f);
        }

        public static void ApplyMobileFast(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 18f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.08f);
        }

        public static void SetupMaterial(Material material)
        {
            RetroShaderGUIUtility.SetupCutoutRenderState(material, true);
        }

        private static void ApplyBase(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_AlphaClip", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_AlphaToMask", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_ShadowCutoff", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_TwoSidedEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.2f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelResolution", 256f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelAspect", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_MipBias", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 24f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteSteps", 32f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherScale", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogStart", 18f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnd", 70f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogDensity", 0.035f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogSteps", 6f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherCutoutFadeEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeAmount", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeStart", 25f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeEnd", 60f);
        }
    }
}
