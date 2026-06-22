using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class PSXPS2WaterShaderGUI : ShaderGUI
    {
        private enum Preset
        {
            CleanPS2,
            CrunchyPSX,
            Swamp,
            MobileFast
        }

        private static bool surfaceFoldout = true;
        private static bool motionFoldout = true;
        private static bool depthFoldout = true;
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
            DrawMotion();
            DrawDepthFoam();
            DrawRetro();
            DrawFog();
            DrawAdvanced();

            if (EditorGUI.EndChangeCheck())
            {
                ForEachMaterial(SetupMaterial, "Edit Retro Water Material");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyCleanPS2(material);
            SetupMaterial(material);
        }

        private void DrawPresets()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Water Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Clean PS2", Preset.CleanPS2);
                    PresetButton("Crunchy PSX", Preset.CrunchyPSX);
                    PresetButton("Swamp", Preset.Swamp);
                    PresetButton("Mobile", Preset.MobileFast);
                }
            }
        }

        private void PresetButton(string label, Preset preset)
        {
            if (!GUILayout.Button(label))
            {
                return;
            }

            ForEachMaterial(material =>
            {
                switch (preset)
                {
                    case Preset.CrunchyPSX:
                        ApplyCrunchyPSX(material);
                        break;
                    case Preset.Swamp:
                        ApplySwamp(material);
                        break;
                    case Preset.MobileFast:
                        ApplyMobileFast(material);
                        break;
                    default:
                        ApplyCleanPS2(material);
                        break;
                }
                SetupMaterial(material);
            }, $"Apply {label} Water Preset");
        }

        private void DrawSurface()
        {
            surfaceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(surfaceFoldout, "Surface");
            if (surfaceFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.DrawTextureWithColor(editor, properties, "_BaseMap", "_BaseColor", "Surface Map");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ShallowColor", "Shallow Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DeepColor", "Deep Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_Alpha", "Alpha");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FresnelColor", "Fresnel Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FresnelIntensity", "Fresnel Intensity");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FresnelPower", "Fresnel Power");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawMotion()
        {
            motionFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(motionFoldout, "Wave Motion");
            if (motionFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.DrawTextureWithScalar(editor, properties, "_NoiseMap", "_WaveStrength", "Wave Noise");
                    RetroShaderGUIUtility.Draw(editor, properties, "_WaveSpeed", "Speed");
                    RetroShaderGUIUtility.Draw(editor, properties, "_WaveScale", "Scale");
                    RetroShaderGUIUtility.Draw(editor, properties, "_WaveSteps", "Steps");
                    RetroShaderGUIUtility.Draw(editor, properties, "_WaveDirection", "Direction");
                    RetroShaderGUIUtility.Draw(editor, properties, "_NormalDistortion", "Normal Distortion");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawDepthFoam()
        {
            depthFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(depthFoldout, "Depth / Foam");
            if (depthFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_DepthFadeEnabled", "Depth Fade");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DepthFadeDistance", "Depth Distance");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoamColor", "Foam Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoamDistance", "Foam Distance");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoamStrength", "Foam Strength");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRetro()
        {
            retroFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(retroFoldout, "Retro Controls");
            if (retroFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapEnabled", "Vertex Snap");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapStrength", "Snap Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapResolution", "Snap Resolution");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSpace", "Snap Space");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapDistanceFade", "Snap Distance Fade");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapFadeStart", "Snap Fade Start");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapFadeEnd", "Snap Fade End");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSeamReduction", "Snap Seam Reduction");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceEnabled", "Vertex Draw Distance");
                    using (new EditorGUI.DisabledScope(Get("_VertexDrawDistanceEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistance", "Draw Distance");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceFade", "Fade Width");
                    }
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleEnabled", "Vertex Wobble");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleStrength", "Wobble Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleSpeed", "Wobble Speed");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleScale", "Wobble Scale");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelEnabled", "UV Pixelation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelStrength", "UV Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelResolution", "UV Resolution");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelAspect", "UV Aspect");
                    RetroShaderGUIUtility.Draw(editor, properties, "_MipBias", "Mip Bias");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineEnabled", "Affine Warp");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineStrength", "Affine Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineMode", "Affine Mode");
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

        private void DrawFog()
        {
            fogFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(fogFoldout, "Material Fog");
            if (fogFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogEnabled", "Retro Fog", "Per-material stylized fog. Use the depth fog renderer feature for main scene fog.");
                    using (new EditorGUI.DisabledScope((RetroShaderGUIUtility.Find(properties, "_RetroFogEnabled")?.floatValue ?? 0f) < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogColor", "Fog Color");
                        RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogStart", "Start");
                        RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogEnd", "End");
                        RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogDensity", "Density");
                        RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogSteps", "Steps");
                        RetroShaderGUIUtility.Draw(editor, properties, "_RetroFogBlendMode", "Blend Mode");
                    }
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
                    RetroShaderGUIUtility.Draw(editor, properties, "_Cull", "Cull");
                    RetroShaderGUIUtility.Draw(editor, properties, "_QueueOffset", "Queue Offset");
                    editor.EnableInstancingField();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private float Get(string propertyName)
        {
            MaterialProperty property = RetroShaderGUIUtility.Find(properties, propertyName);
            return property == null ? 0f : property.floatValue;
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

        public static void ApplyCleanPS2(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_Alpha", 0.52f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 34f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.04f);
        }

        public static void ApplyCrunchyPSX(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_Alpha", 0.62f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveSteps", 4f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.22f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.58f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineStrength", 0.22f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 12f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.28f);
        }

        public static void ApplySwamp(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetColor(material, "_ShallowColor", new Color(0.25f, 0.42f, 0.22f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_DeepColor", new Color(0.05f, 0.13f, 0.10f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoamColor", new Color(0.58f, 0.68f, 0.38f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_Alpha", 0.68f);
            RetroShaderGUIUtility.SetFloat(material, "_FoamStrength", 0.28f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 16f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.18f);
        }

        public static void ApplyMobileFast(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_DepthFadeEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveStrength", 0.06f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 22f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
        }

        public static void SetupMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent + Mathf.Clamp((int)RetroShaderGUIUtility.GetFloat(material, "_QueueOffset"), -50, 50);

            RetroShaderGUIUtility.SetKeyword(material, "_WATER_DEPTH_FADE", RetroShaderGUIUtility.GetFloat(material, "_DepthFadeEnabled", 1f) > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_VERTEX_SNAP", RetroShaderGUIUtility.GetFloat(material, "_VertexSnapEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_VERTEX_WOBBLE", RetroShaderGUIUtility.GetFloat(material, "_VertexWobbleEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_UV_PIXEL", RetroShaderGUIUtility.GetFloat(material, "_UvPixelEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_AFFINE", RetroShaderGUIUtility.GetFloat(material, "_AffineEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_POSTERIZE", RetroShaderGUIUtility.GetFloat(material, "_PosterizeEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_DITHER", RetroShaderGUIUtility.GetFloat(material, "_DitherEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_FOG", RetroShaderGUIUtility.GetFloat(material, "_RetroFogEnabled") > 0.5f);
        }

        private static void ApplyBase(Material material)
        {
            RetroShaderGUIUtility.SetColor(material, "_BaseColor", new Color(0.72f, 0.90f, 0.92f, 0.55f));
            RetroShaderGUIUtility.SetColor(material, "_ShallowColor", new Color(0.26f, 0.70f, 0.72f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_DeepColor", new Color(0.05f, 0.20f, 0.34f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoamColor", new Color(0.88f, 0.96f, 0.88f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_Alpha", 0.58f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveSpeed", 1.1f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveScale", 1.8f);
            RetroShaderGUIUtility.SetFloat(material, "_WaveSteps", 6f);
            RetroShaderGUIUtility.SetFloat(material, "_DepthFadeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DepthFadeDistance", 2.4f);
            RetroShaderGUIUtility.SetFloat(material, "_FoamDistance", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_FoamStrength", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_FresnelIntensity", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_FresnelPower", 2.5f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapDistanceFade", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapFadeStart", 5f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapFadeEnd", 45f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSeamReduction", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistance", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceFade", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleScale", 3f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.28f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelResolution", 192f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelAspect", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 24f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteSteps", 32f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherScale", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnabled", 0f);
            RetroShaderGUIUtility.SetColor(material, "_RetroFogColor", new Color(0.42f, 0.46f, 0.50f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogStart", 18f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnd", 70f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogDensity", 0.035f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogSteps", 6f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogBlendMode", 0f);
        }
    }
}
