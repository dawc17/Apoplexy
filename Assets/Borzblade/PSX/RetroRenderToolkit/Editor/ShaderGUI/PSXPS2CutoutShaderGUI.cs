using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class PSXPS2CutoutShaderGUI : ShaderGUI
    {
        private enum Preset
        {
            CleanCutout,
            CrunchyPSXCutout,
            DitherFadeCutout,
            HairBeardCard,
            FenceGrate,
            MobileCutout
        }

        private static bool surfaceFoldout = true;
        private static bool cutoutFoldout = true;
        private static bool geometryFoldout;
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
            DrawCutout();
            DrawGeometry();
            DrawTexture();
            DrawColor();
            DrawLighting();
            DrawFog();
            DrawAdvanced();

            if (EditorGUI.EndChangeCheck())
            {
                ForEachMaterial(SetupMaterial, "Edit Retro Cutout Material");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyCleanCutout(material);
            SetupMaterial(material);
        }

        private void DrawPresets()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Cutout Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Clean", Preset.CleanCutout);
                    PresetButton("Crunchy", Preset.CrunchyPSXCutout);
                    PresetButton("Dither Fade", Preset.DitherFadeCutout);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Hair/Beard", Preset.HairBeardCard);
                    PresetButton("Fence/Grate", Preset.FenceGrate);
                    PresetButton("Mobile", Preset.MobileCutout);
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
                }, $"Apply {label} Cutout Preset");
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
                    RetroShaderGUIUtility.DrawResetHeader("Cutout", () => ForEachMaterial(ResetCutout, "Reset Cutout"));
                    RetroShaderGUIUtility.Draw(editor, properties, "_Cutoff", "Alpha Cutoff");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ShadowCutoff", "Shadow Cutoff");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherCutoutFadeEnabled", "Dithered Fade");
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

        private void DrawGeometry()
        {
            geometryFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(geometryFoldout, "Retro Geometry");
            if (geometryFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.DrawResetHeader("Geometry", () => ForEachMaterial(ResetGeometry, "Reset Geometry"));
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapEnabled", "Vertex Snap");
                    using (new EditorGUI.DisabledScope(Get("_VertexSnapEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapStrength", "Strength");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapResolution", "Resolution");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSpace", "Snap Space");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapDistanceFade", "Distance Fade");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSeamReduction", "Snap Seam Reduction", "Reduces crack-prone snap offsets while keeping the PSX wobble feel.");
                        RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapUseAnchors", "Use Baked Snap Anchors", "Uses UV4 snap-anchor data baked by the toolkit when available.");
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
                    RetroShaderGUIUtility.DrawResetHeader("Texture", () => ForEachMaterial(ResetTexture, "Reset Texture"));
                    RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelEnabled", "UV Pixelation");
                    using (new EditorGUI.DisabledScope(Get("_UvPixelEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelStrength", "Strength");
                        RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelResolution", "Resolution");
                        RetroShaderGUIUtility.Draw(editor, properties, "_UvPixelAspect", "Aspect");
                    }

                    RetroShaderGUIUtility.Draw(editor, properties, "_MipBias", "Mip Bias");
                    RetroShaderGUIUtility.Draw(editor, properties, "_AffineEnabled", "Affine Warp");
                    using (new EditorGUI.DisabledScope(Get("_AffineEnabled") < 0.5f))
                    {
                        RetroShaderGUIUtility.Draw(editor, properties, "_AffineStrength", "Strength");
                        RetroShaderGUIUtility.Draw(editor, properties, "_AffineMode", "Mode");
                    }
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
                    RetroShaderGUIUtility.DrawResetHeader("Color", () => ForEachMaterial(ResetColor, "Reset Color"));
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
                    RetroShaderGUIUtility.DrawResetHeader("Lighting", () => ForEachMaterial(ResetLighting, "Reset Lighting"));
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroLightingModel", "Lighting Model");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecColor", "Specular Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_Smoothness", "Smoothness");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecularIntensity", "PS2 Specular Intensity");
                    RetroShaderGUIUtility.Draw(editor, properties, "_SpecularPower", "PS2 Specular Power");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RetroSpecularMode", "Specular Mode");
                    RetroShaderGUIUtility.Draw(editor, properties, "_LightBands", "Light Bands");
                    RetroShaderGUIUtility.Draw(editor, properties, "_ShadowBandStrength", "Band Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimEnabled", "Rim Light");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimColor", "Rim Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimIntensity", "Rim Intensity");
                    RetroShaderGUIUtility.Draw(editor, properties, "_RimPower", "Rim Power");
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
                case Preset.CrunchyPSXCutout:
                    ApplyCrunchyPSXCutout(material);
                    break;
                case Preset.DitherFadeCutout:
                    ApplyDitherFadeCutout(material);
                    break;
                case Preset.HairBeardCard:
                    ApplyHairBeardCard(material);
                    break;
                case Preset.FenceGrate:
                    ApplyFenceGrate(material);
                    break;
                case Preset.MobileCutout:
                    ApplyMobileCutout(material);
                    break;
                default:
                    ApplyCleanCutout(material);
                    break;
            }
        }

        public static void ApplyCleanCutout(Material material)
        {
            ResetCutout(material);
            ResetGeometry(material);
            ResetTexture(material);
            ResetColor(material);
            ResetLighting(material);
            RetroShaderGUIUtility.SetFloat(material, "_TwoSidedEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroFogEnabled", 0f);
        }

        public static void ApplyCrunchyPSXCutout(Material material)
        {
            ApplyCleanCutout(material);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.28f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapResolution", 180f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSeamReduction", 0.15f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.65f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelResolution", 128f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineStrength", 0.28f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 10f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.5f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_LightBands", 5f);
            RetroShaderGUIUtility.SetFloat(material, "_RimEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroSpecularMode", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 0f);
        }

        public static void ApplyDitherFadeCutout(Material material)
        {
            ApplyCleanCutout(material);
            RetroShaderGUIUtility.SetFloat(material, "_DitherCutoutFadeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DistanceFadeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeAmount", 0.75f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeStart", 18f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeEnd", 50f);
        }

        public static void ApplyHairBeardCard(Material material)
        {
            ApplyCleanCutout(material);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.32f);
            RetroShaderGUIUtility.SetFloat(material, "_ShadowCutoff", 0.42f);
            RetroShaderGUIUtility.SetFloat(material, "_TwoSidedEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_RimIntensity", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularIntensity", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroSpecularMode", 0f);
        }

        public static void ApplyFenceGrate(Material material)
        {
            ApplyCleanCutout(material);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.55f);
            RetroShaderGUIUtility.SetFloat(material, "_ShadowCutoff", 0.55f);
            RetroShaderGUIUtility.SetFloat(material, "_TwoSidedEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_CullMode", (float)UnityEngine.Rendering.CullMode.Back);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.16f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
        }

        public static void ApplyMobileCutout(Material material)
        {
            ApplyCleanCutout(material);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.14f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 20f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RimEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularIntensity", 0.2f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroSpecularMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 0f);
        }

        public static void SetupMaterial(Material material)
        {
            RetroShaderGUIUtility.SetupCutoutRenderState(material);
        }

        private static void ResetCutout(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_AlphaClip", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_AlphaToMask", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_Cutoff", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_ShadowCutoff", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherCutoutFadeEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeAmount", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeStart", 25f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherFadeEnd", 60f);
            RetroShaderGUIUtility.SetFloat(material, "_CameraFadeEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DistanceFadeEnabled", 0f);
        }

        private static void ResetGeometry(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapResolution", 320f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapDistanceFade", 0.6f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSeamReduction", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapUseAnchors", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistance", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceFade", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleStrength", 0.04f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleSpeed", 1.5f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexWobbleScale", 4f);
        }

        private static void ResetTexture(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelStrength", 0.16f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelResolution", 384f);
            RetroShaderGUIUtility.SetFloat(material, "_UvPixelAspect", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_MipBias", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_AffineMode", 0f);
        }

        private static void ResetColor(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_PosterizeSteps", 24f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteStrength", 0.15f);
            RetroShaderGUIUtility.SetFloat(material, "_PaletteSteps", 32f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.12f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherScale", 1f);
        }

        private static void ResetLighting(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_Smoothness", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularIntensity", 0.45f);
            RetroShaderGUIUtility.SetFloat(material, "_SpecularPower", 28f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroSpecularMode", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_RetroLightingModel", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_LightBands", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_ShadowBandStrength", 0.25f);
            RetroShaderGUIUtility.SetFloat(material, "_RimEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_RimIntensity", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_RimPower", 2.5f);
        }
    }
}
