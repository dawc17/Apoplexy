using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class PSXPS2SpriteShaderGUI : ShaderGUI
    {
        private enum Preset
        {
            CleanSprite,
            CrunchyPSX,
            MobileFast
        }

        private static bool surfaceFoldout = true;
        private static bool geometryFoldout = true;
        private static bool textureFoldout = true;
        private static bool colorFoldout = true;
        private static bool lightingFoldout = true;
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
            DrawGeometry();
            DrawTexture();
            DrawColor();
            DrawLighting();
            DrawFog();
            DrawAdvanced();

            if (EditorGUI.EndChangeCheck())
            {
                ForEachMaterial(SetupMaterial, "Edit Retro Sprite Material");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyCleanSprite(material);
            SetupMaterial(material);
        }

        private void DrawPresets()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Sprite Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Clean", Preset.CleanSprite);
                    PresetButton("Crunchy PSX", Preset.CrunchyPSX);
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
                    case Preset.MobileFast:
                        ApplyMobileFast(material);
                        break;
                    default:
                        ApplyCleanSprite(material);
                        break;
                }

                SetupMaterial(material);
            }, $"Apply {label} Sprite Preset");
        }

        private void DrawSurface()
        {
            surfaceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(surfaceFoldout, "Surface");
            if (surfaceFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.DrawTextureWithColor(editor, properties, "_MainTex", "_Color", "Sprite Texture");
                    RetroShaderGUIUtility.DrawTextureWithScalar(editor, properties, "_BumpMap", "_BumpScale", "Normal Map");
                    RetroShaderGUIUtility.DrawTextureWithColor(editor, properties, "_EmissionMap", "_EmissionColor", "Emission");
                    RetroShaderGUIUtility.Draw(editor, properties, "_Cutoff", "Alpha Cutoff");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ReceiveShadows", "Receive Shadows");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawGeometry()
        {
            geometryFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(geometryFoldout, "Retro Geometry");
            if (geometryFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapEnabled", "Vertex Snap");
                    using (new EditorGUI.DisabledScope(Get("_VertexSnapEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapStrength", "Strength");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapResolution", "Resolution");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSpace", "Snap Space");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapDistanceFade", "Distance Fade");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapFadeStart", "Fade Start");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapFadeEnd", "Fade End");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSeamReduction", "Seam Reduction");
                    }

                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceEnabled", "Vertex Draw Distance");
                    using (new EditorGUI.DisabledScope(Get("_VertexDrawDistanceEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistance", "Distance");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceFade", "Fade Width");
                    }

                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleEnabled", "Vertex Wobble");
                    using (new EditorGUI.DisabledScope(Get("_VertexWobbleEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleStrength", "Strength");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleSpeed", "Speed");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleScale", "Scale");
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawTexture()
        {
            textureFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(textureFoldout, "Retro Texture");
            if (textureFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelEnabled", "UV Pixelation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelStrength", "UV Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelResolution", "UV Resolution");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelAspect", "UV Aspect");
                    RetroShaderGUIUtility.Draw(editor, properties, "_MipBias", "Mip Bias");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineEnabled", "Affine Warp");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineStrength", "Affine Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineMode", "Affine Mode");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawColor()
        {
            colorFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(colorFoldout, "Retro Color");
            if (colorFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
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

        private void DrawLighting()
        {
            lightingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(lightingFoldout, "Lighting");
            if (lightingFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroLightingModel", "Lighting Model");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecColor", "Specular Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_Smoothness", "Smoothness");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecularIntensity", "Specular Intensity");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecularPower", "Specular Power");
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
                    using (new EditorGUI.DisabledScope(Get("_RetroFogEnabled") < 0.5f))
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

        private float Get(string propertyName)
        {
            MaterialProperty property = RetroShaderGUIUtility.Find(properties, propertyName);
            return property == null ? 0f : property.floatValue;
        }

        public static void ApplyCleanSprite(Material material)
        {
            SetCommonDefaults(material);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 28f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.08f);
        }

        public static void ApplyCrunchyPSX(Material material)
        {
            SetCommonDefaults(material);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapResolution", 180f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.55f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelResolution", 128f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineStrength", 0.22f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 12f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.32f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 1f);
        }

        public static void ApplyMobileFast(Material material)
        {
            SetCommonDefaults(material);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 24f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 3f);
        }

        public static void SetupMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            int queueOffset = Mathf.Clamp((int)RetroShaderGUIUtility.GetFloat(material, "_QueueOffset"), -50, 50);
            RetroShaderGUIUtility.SetFloat(material, "_SrcBlend", (float)BlendMode.SrcAlpha);
            RetroShaderGUIUtility.SetFloat(material, "_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            RetroShaderGUIUtility.SetFloat(material, "_ZWrite", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.renderQueue = (int)RenderQueue.Transparent + queueOffset;

            RetroShaderGUIUtility.SetKeyword(material, "_NORMALMAP", material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null);
            RetroShaderGUIUtility.SetKeyword(material, "_EMISSION", RetroShaderGUIUtility.HasEmission(material));
            RetroShaderGUIUtility.SetKeyword(material, "_RECEIVE_SHADOWS_OFF", RetroShaderGUIUtility.GetFloat(material, "_ReceiveShadows", 0f) < 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_VERTEX_SNAP", RetroShaderGUIUtility.GetFloat(material, "_VertexSnapEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_VERTEX_WOBBLE", RetroShaderGUIUtility.GetFloat(material, "_VertexWobbleEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_UV_PIXEL", RetroShaderGUIUtility.GetFloat(material, "_UvPixelEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_AFFINE", RetroShaderGUIUtility.GetFloat(material, "_AffineEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_POSTERIZE", RetroShaderGUIUtility.GetFloat(material, "_PosterizeEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_DITHER", RetroShaderGUIUtility.GetFloat(material, "_DitherEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_FOG", RetroShaderGUIUtility.GetFloat(material, "_RetroFogEnabled") > 0.5f);
            material.SetShaderPassEnabled("ShadowCaster", false);
        }

        private static void SetCommonDefaults(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.01f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapResolution", 240f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapDistanceFade", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapFadeStart", 5f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapFadeEnd", 45f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSeamReduction", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistance", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceFade", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleStrength", 0.04f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleSpeed", 2.5f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleScale", 4f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.2f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelResolution", 256f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelAspect", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_MipBias", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 24f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteSteps", 32f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherScale", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularIntensity", 0.25f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularPower", 24f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnabled", 0f);
        }
    }
}
