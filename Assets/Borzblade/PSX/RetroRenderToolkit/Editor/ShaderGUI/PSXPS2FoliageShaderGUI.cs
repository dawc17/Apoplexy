using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class PSXPS2FoliageShaderGUI : ShaderGUI
    {
        private enum Preset
        {
            GrassCard,
            BushLeaves,
            TreeLeaves,
            PineBranch,
            DeadBush,
            DarkFantasy,
            MobileFast
        }

        private static bool surfaceFoldout = true;
        private static bool cutoutFoldout = true;
        private static bool windFoldout = true;
        private static bool retroFoldout = true;
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
            DrawCutout();
            DrawWind();
            DrawRetroTexture();
            DrawColor();
            DrawLighting();
            DrawFog();
            DrawAdvanced();

            if (EditorGUI.EndChangeCheck())
            {
                ForEachMaterial(SetupMaterial, "Edit Retro Foliage Material");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyBushLeaves(material);
            SetupMaterial(material);
        }

        private void DrawPresets()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Foliage Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Grass", Preset.GrassCard);
                    PresetButton("Bush", Preset.BushLeaves);
                    PresetButton("Tree", Preset.TreeLeaves);
                    PresetButton("Pine", Preset.PineBranch);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Dead", Preset.DeadBush);
                    PresetButton("Dark", Preset.DarkFantasy);
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
                }, $"Apply {label} Foliage Preset");
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
                    RetroShaderGUIUtility.DrawTextureWithScalar(editor, properties, "_BumpMap", "_BumpScale", "Normal Map");
                    RetroShaderGUIUtility.DrawTextureWithColor(editor, properties, "_EmissionMap", "_EmissionColor", "Emission");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ReceiveShadows", "Receive Shadows");
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
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherCutoutFadeEnabled", "Dithered Distance Fade");
                    using (new EditorGUI.DisabledScope(Get("_DitherCutoutFadeEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_DitherFadeAmount", "Fade Amount");
                        RetroShaderGUIUtility.Draw(editor, properties, "_DitherFadeStart", "Fade Start");
                        RetroShaderGUIUtility.Draw(editor, properties, "_DitherFadeEnd", "Fade End");
                        RetroShaderGUIUtility.Draw(editor, properties, "_DistanceFadeEnabled", "Distance Fade");
                        RetroShaderGUIUtility.Draw(editor, properties, "_CameraFadeEnabled", "Camera Fade");
                        RetroShaderGUIUtility.Draw(editor, properties, "_CameraFadeDistance", "Camera Fade Distance");
                    }
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawWind()
        {
            windFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(windFoldout, "Foliage / Wind");
            if (windFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_WindEnabled", "Wind");
                    using (new EditorGUI.DisabledScope(Get("_WindEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindStrength", "Strength");
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindDistance", "Jiggle Amplitude", "Maximum local flutter amplitude. Leaves stay anchored instead of sweeping sideways.");
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindSpeed", "Speed");
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindScale", "Scale");
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindDirection", "Direction");
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindVertexColorMask", "Vertex Alpha Mask");
                        RetroShaderGUIUtility.Draw(editor, properties, "_WindUVHeightMask", "UV Height Mask");
                    }

                    RetroShaderGUIUtility.Draw(editor, properties, "_FoliageGradientEnabled", "Top/Bottom Gradient");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoliageTopColor", "Top Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoliageBottomColor", "Bottom Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoliageVariationEnabled", "Color Variation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoliageVariationStrength", "Variation Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_HueVariation", "Hue Variation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SaturationVariation", "Saturation Variation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_BrightnessVariation", "Brightness Variation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_FoliageMobileLighting", "Mobile Lighting");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRetroTexture()
        {
            retroFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(retroFoldout, "Retro Geometry / Texture");
            if (retroFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapEnabled", "Vertex Snap");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapStrength", "Snap Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapResolution", "Snap Resolution");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSpace", "Snap Space");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSeamReduction", "Snap Seam Reduction");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapUseAnchors", "Use Baked Snap Anchors");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceEnabled", "Vertex Draw Distance");
                    using (new EditorGUI.DisabledScope(Get("_VertexDrawDistanceEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistance", "Draw Distance");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceFade", "Fade Width");
                    }
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleEnabled", "Vertex Wobble");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexWobbleStrength", "Wobble Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelEnabled", "UV Pixelation");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelStrength", "UV Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelResolution", "UV Resolution");
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
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecularIntensity", "PS2 Specular Intensity");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecularPower", "PS2 Specular Power");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroSpecularMode", "Specular Mode");
                    RetroShaderGUIUtility.Draw(editor, properties, "_LightBands", "Light Bands");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ShadowBandStrength", "Band Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimEnabled", "Rim Light");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimColor", "Rim Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimIntensity", "Rim Intensity");
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
                    RetroShaderGUIUtility.Draw(editor, properties, "_TwoSidedEnabled", "Two Sided");
                    RetroShaderGUIUtility.Draw(editor, properties, "_CullMode", "Cull Mode");
                    RetroShaderGUIUtility.Draw(editor, properties, "_BackfaceTintEnabled", "Backface Tint");
                    RetroShaderGUIUtility.Draw(editor, properties, "_BackfaceTint", "Backface Tint Color");
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

        private float Get(string propertyName)
        {
            MaterialProperty property = RetroShaderGUIUtility.Find(properties, propertyName);
            return property == null ? 0f : property.floatValue;
        }

        private static void ApplyPreset(Material material, Preset preset)
        {
            switch (preset)
            {
                case Preset.GrassCard:
                    ApplyGrassCard(material);
                    break;
                case Preset.TreeLeaves:
                    ApplyTreeLeaves(material);
                    break;
                case Preset.PineBranch:
                    ApplyPineBranch(material);
                    break;
                case Preset.DeadBush:
                    ApplyDeadBush(material);
                    break;
                case Preset.DarkFantasy:
                    ApplyDarkFantasy(material);
                    break;
                case Preset.MobileFast:
                    ApplyMobileFast(material);
                    break;
                default:
                    ApplyBushLeaves(material);
                    break;
            }
        }

        public static void ApplyGrassCard(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.38f);
            RetroShaderGUIUtility.SetFloat(material, "_WindStrength", 0.22f);
            RetroShaderGUIUtility.SetFloat(material, "_WindDistance", 0.07f);
            RetroShaderGUIUtility.SetFloat(material, "_WindSpeed", 1.8f);
            RetroShaderGUIUtility.SetColor(material, "_FoliageTopColor", new Color(0.62f, 0.82f, 0.34f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageBottomColor", new Color(0.15f, 0.28f, 0.08f, 1f));
        }

        public static void ApplyBushLeaves(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetFloat(material, "_WindStrength", 0.16f);
            RetroShaderGUIUtility.SetFloat(material, "_WindDistance", 0.05f);
            RetroShaderGUIUtility.SetColor(material, "_FoliageTopColor", new Color(0.42f, 0.68f, 0.28f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageBottomColor", new Color(0.12f, 0.24f, 0.11f, 1f));
        }

        public static void ApplyTreeLeaves(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetFloat(material, "_WindStrength", 0.11f);
            RetroShaderGUIUtility.SetFloat(material, "_WindDistance", 0.035f);
            RetroShaderGUIUtility.SetFloat(material, "_WindScale", 0.7f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularIntensity", 0.25f);
            RetroShaderGUIUtility.SetFloat(material, "_RimIntensity", 0.12f);
        }

        public static void ApplyPineBranch(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.5f);
            RetroShaderGUIUtility.SetColor(material, "_FoliageTopColor", new Color(0.22f, 0.42f, 0.24f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageBottomColor", new Color(0.05f, 0.16f, 0.11f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.2f);
        }

        public static void ApplyDeadBush(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetColor(material, "_BaseColor", new Color(0.86f, 0.66f, 0.39f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageTopColor", new Color(0.72f, 0.52f, 0.27f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageBottomColor", new Color(0.30f, 0.20f, 0.11f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_WindStrength", 0.16f);
            RetroShaderGUIUtility.SetFloat(material, "_WindDistance", 0.045f);
            RetroShaderGUIUtility.SetFloat(material, "_SaturationVariation", 0.18f);
        }

        public static void ApplyDarkFantasy(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetColor(material, "_BaseColor", new Color(0.48f, 0.54f, 0.56f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageTopColor", new Color(0.22f, 0.34f, 0.38f, 1f));
            RetroShaderGUIUtility.SetColor(material, "_FoliageBottomColor", new Color(0.05f, 0.09f, 0.12f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 14f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.22f);
            RetroShaderGUIUtility.SetFloat(material, "_LightBands", 4f);
        }

        public static void ApplyMobileFast(Material material)
        {
            ApplyBaseFoliage(material);
            RetroShaderGUIUtility.SetFloat(material, "_WindStrength", 0.1f);
            RetroShaderGUIUtility.SetFloat(material, "_WindDistance", 0.025f);
            RetroShaderGUIUtility.SetFloat(material, "_FoliageVariationEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_FoliageMobileLighting", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RimEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularIntensity", 0.15f);
        }

        public static void SetupMaterial(Material material)
        {
            RetroShaderGUIUtility.SetupCutoutRenderState(material);
        }

        private static void ApplyBaseFoliage(Material material)
        {
            PSXPS2CutoutShaderGUI.ApplyCleanCutout(material);
            RetroShaderGUIUtility.SetFloat(material, "_TwoSidedEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_WindEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_WindStrength", 0.16f);
            RetroShaderGUIUtility.SetFloat(material, "_WindDistance", 0.05f);
            RetroShaderGUIUtility.SetFloat(material, "_WindSpeed", 1.4f);
            RetroShaderGUIUtility.SetFloat(material, "_WindScale", 1.2f);
            RetroShaderGUIUtility.SetFloat(material, "_WindVertexColorMask", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_WindUVHeightMask", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSeamReduction", 0.1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapUseAnchors", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistance", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceFade", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroSpecularMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_FoliageGradientEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_FoliageVariationEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_FoliageVariationStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_HueVariation", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_SaturationVariation", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_BrightnessVariation", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_FoliageMobileLighting", 0f);
        }
    }
}
