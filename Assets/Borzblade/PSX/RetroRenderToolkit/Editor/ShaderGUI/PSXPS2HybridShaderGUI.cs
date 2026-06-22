using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
public sealed class PSXPS2HybridShaderGUI : ShaderGUI
{
    private enum SurfaceType
    {
        Opaque,
        Transparent
    }

    private enum BlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
    }

    private enum RetroPreset
    {
        HybridHigh,
        CrunchyPSX,
        CleanPS2,
        MobileFast
    }

    private static bool surfaceFoldout = true;
    private static bool geometryFoldout = true;
    private static bool textureFoldout = true;
    private static bool colorFoldout = true;
    private static bool lightingFoldout = true;
    private static bool fogFoldout;
    private static bool advancedFoldout;

    private MaterialEditor materialEditor;
    private MaterialProperty[] properties;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditor;
        this.properties = properties;

        Material material = materialEditor.target as Material;
        if (material == null)
        {
            return;
        }

        DrawPresetToolbar();

        EditorGUI.BeginChangeCheck();
        DrawSurfaceOptions();
        DrawGeometryModifiers();
        DrawTextureModifiers();
        DrawColorModifiers();
        DrawLightingModifiers();
        DrawFogModifiers();
        DrawAdvancedOptions();

        if (EditorGUI.EndChangeCheck())
        {
            foreach (Object target in materialEditor.targets)
            {
                if (target is Material changedMaterial)
                {
                    SetupMaterial(changedMaterial);
                }
            }
        }
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        SetupMaterial(material);
    }

    private void DrawPresetToolbar()
    {
        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("PSX / PS2 Hybrid Presets", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Hybrid High"))
                {
                    ApplyPreset(RetroPreset.HybridHigh);
                }

                if (GUILayout.Button("Crunchy PSX"))
                {
                    ApplyPreset(RetroPreset.CrunchyPSX);
                }

                if (GUILayout.Button("Clean PS2"))
                {
                    ApplyPreset(RetroPreset.CleanPS2);
                }

                if (GUILayout.Button("Mobile Fast"))
                {
                    ApplyPreset(RetroPreset.MobileFast);
                }
            }
        }
    }

    private void DrawSurfaceOptions()
    {
        surfaceFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(surfaceFoldout, "Surface");
        if (surfaceFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                MaterialProperty baseMap = Find("_BaseMap");
                MaterialProperty baseColor = Find("_BaseColor");
                if (baseMap != null && baseColor != null)
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Base Map"), baseMap, baseColor);
                    materialEditor.TextureScaleOffsetProperty(baseMap);
                    RetroShaderGUIUtility.DrawPointFilterWarning(baseMap);
                }

                DrawTextureWithScalar("_BumpMap", "_BumpScale", "Normal Map");
                DrawTextureWithColor("_EmissionMap", "_EmissionColor", "Emission");

                DrawSurfacePopup();
                DrawBlendPopup();
                DrawProperty("_AlphaClip", "Alpha Clip");
                using (new EditorGUI.DisabledScope(Find("_AlphaClip")?.floatValue < 0.5f))
                {
                    DrawProperty("_Cutoff", "Clip Threshold");
                }

                DrawCullPopup();
                DrawProperty("_ReceiveShadows", "Receive Shadows");
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawGeometryModifiers()
    {
        geometryFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(geometryFoldout, "Geometry Modifiers");
        if (geometryFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawGroupHeaderReset("Geometry", ResetGeometry);
                DrawProperty("_VertexSnapEnabled", "Vertex Snap");
                using (new EditorGUI.DisabledScope(Find("_VertexSnapEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_VertexSnapStrength", "Strength");
                    DrawProperty("_VertexSnapResolution", "Resolution");
                    DrawProperty("_VertexSnapSpace", "Snap Space");
                    DrawProperty("_VertexSnapDistanceFade", "Distance Fade");
                    DrawProperty("_VertexSnapFadeStart", "Fade Start");
                    DrawProperty("_VertexSnapFadeEnd", "Fade End");
                    DrawProperty("_VertexSnapSeamReduction", "Snap Seam Reduction");
                    DrawProperty("_VertexSnapUseAnchors", "Use Baked Snap Anchors");
                }

                EditorGUILayout.Space(4f);
                DrawProperty("_VertexDrawDistanceEnabled", "Vertex Draw Distance");
                using (new EditorGUI.DisabledScope(Find("_VertexDrawDistanceEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_VertexDrawDistance", "Distance");
                    DrawProperty("_VertexDrawDistanceFade", "Fade Width");
                }

                EditorGUILayout.Space(4f);
                DrawProperty("_VertexWobbleEnabled", "Vertex Wobble");
                using (new EditorGUI.DisabledScope(Find("_VertexWobbleEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_VertexWobbleStrength", "Strength");
                    DrawProperty("_VertexWobbleSpeed", "Speed");
                    DrawProperty("_VertexWobbleScale", "Scale");
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawTextureModifiers()
    {
        textureFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(textureFoldout, "Texture Modifiers");
        if (textureFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawGroupHeaderReset("Texture", ResetTexture);
                DrawProperty("_UvPixelEnabled", "UV Pixelation");
                using (new EditorGUI.DisabledScope(Find("_UvPixelEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_UvPixelStrength", "Strength");
                    DrawProperty("_UvPixelResolution", "Resolution");
                    DrawProperty("_UvPixelAspect", "Aspect");
                }

                DrawProperty("_MipBias", "Mip Bias");

                EditorGUILayout.Space(4f);
                DrawProperty("_AffineEnabled", "Affine Warp");
                using (new EditorGUI.DisabledScope(Find("_AffineEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_AffineStrength", "Strength");
                    DrawProperty("_AffineMode", "Mode");
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawColorModifiers()
    {
        colorFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(colorFoldout, "Color Modifiers");
        if (colorFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawGroupHeaderReset("Color", ResetColor);
                DrawProperty("_PosterizeEnabled", "Posterize");
                using (new EditorGUI.DisabledScope(Find("_PosterizeEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_PosterizeSteps", "Posterize Steps");
                    DrawProperty("_PaletteStrength", "Palette Strength");
                    DrawProperty("_PaletteSteps", "Palette Steps");
                }

                EditorGUILayout.Space(4f);
                DrawProperty("_DitherEnabled", "Ordered Dither");
                using (new EditorGUI.DisabledScope(Find("_DitherEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_DitherStrength", "Strength");
                    DrawProperty("_DitherScale", "Scale");
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawLightingModifiers()
    {
        lightingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(lightingFoldout, "Lighting Modifiers");
        if (lightingFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawGroupHeaderReset("Lighting", ResetLighting);
                DrawProperty("_RetroLightingModel", "Lighting Model");
                DrawProperty("_SpecColor", "Specular Color");
                DrawProperty("_Smoothness", "Smoothness");
                DrawProperty("_SpecularIntensity", "PS2 Specular Intensity");
                DrawProperty("_SpecularPower", "PS2 Specular Power");
                DrawProperty("_RetroSpecularMode", "Specular Mode");
                DrawProperty("_LightBands", "Shadow/Light Bands");
                DrawProperty("_ShadowBandStrength", "Band Strength");

                EditorGUILayout.Space(4f);
                DrawProperty("_RimEnabled", "Rim Light");
                using (new EditorGUI.DisabledScope(Find("_RimEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_RimColor", "Rim Color");
                    DrawProperty("_RimIntensity", "Rim Intensity");
                    DrawProperty("_RimPower", "Rim Power");
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawFogModifiers()
    {
        fogFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(fogFoldout, "Retro Fog");
        if (fogFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawProperty("_RetroFogEnabled", "Retro Fog");
                using (new EditorGUI.DisabledScope(Find("_RetroFogEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_RetroFogColor", "Fog Color");
                    DrawProperty("_RetroFogStart", "Start");
                    DrawProperty("_RetroFogEnd", "End");
                    DrawProperty("_RetroFogDensity", "Density");
                    DrawProperty("_RetroFogSteps", "Steps");
                    DrawProperty("_RetroFogBlendMode", "Blend Mode");
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawAdvancedOptions()
    {
        advancedFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(advancedFoldout, "Advanced");
        if (advancedFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawProperty("_QueueOffset", "Queue Offset");
                materialEditor.EnableInstancingField();
                materialEditor.DoubleSidedGIField();

                if (GUILayout.Button("Sync Keywords And Render State"))
                {
                    foreach (Object target in materialEditor.targets)
                    {
                        if (target is Material changedMaterial)
                        {
                            SetupMaterial(changedMaterial);
                            EditorUtility.SetDirty(changedMaterial);
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawSurfacePopup()
    {
        MaterialProperty surface = Find("_Surface");
        if (surface == null)
        {
            return;
        }

        EditorGUI.showMixedValue = surface.hasMixedValue;
        SurfaceType value = (SurfaceType)Mathf.Clamp((int)surface.floatValue, 0, 1);
        EditorGUI.BeginChangeCheck();
        value = (SurfaceType)EditorGUILayout.EnumPopup("Surface Type", value);
        if (EditorGUI.EndChangeCheck())
        {
            surface.floatValue = (float)value;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawBlendPopup()
    {
        MaterialProperty surface = Find("_Surface");
        MaterialProperty blend = Find("_Blend");
        if (surface == null || blend == null || surface.floatValue < 0.5f)
        {
            return;
        }

        EditorGUI.showMixedValue = blend.hasMixedValue;
        BlendMode value = (BlendMode)Mathf.Clamp((int)blend.floatValue, 0, 3);
        EditorGUI.BeginChangeCheck();
        value = (BlendMode)EditorGUILayout.EnumPopup("Blend Mode", value);
        if (EditorGUI.EndChangeCheck())
        {
            blend.floatValue = (float)value;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawCullPopup()
    {
        MaterialProperty cull = Find("_Cull");
        if (cull == null)
        {
            return;
        }

        EditorGUI.showMixedValue = cull.hasMixedValue;
        CullMode value = (CullMode)Mathf.Clamp((int)cull.floatValue, 0, 2);
        EditorGUI.BeginChangeCheck();
        value = (CullMode)EditorGUILayout.EnumPopup("Cull", value);
        if (EditorGUI.EndChangeCheck())
        {
            cull.floatValue = (float)value;
        }
        EditorGUI.showMixedValue = false;
    }

    private void DrawTextureWithScalar(string textureName, string scalarName, string label)
    {
        MaterialProperty texture = Find(textureName);
        MaterialProperty scalar = Find(scalarName);
        if (texture != null)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent(label), texture, scalar);
            RetroShaderGUIUtility.DrawPointFilterWarning(texture);
        }
    }

    private void DrawTextureWithColor(string textureName, string colorName, string label)
    {
        MaterialProperty texture = Find(textureName);
        MaterialProperty color = Find(colorName);
        if (texture != null && color != null)
        {
            materialEditor.TexturePropertySingleLine(new GUIContent(label), texture, color);
            RetroShaderGUIUtility.DrawPointFilterWarning(texture);
        }
    }

    private void DrawProperty(string propertyName, string label)
    {
        MaterialProperty property = Find(propertyName);
        if (property != null)
        {
            materialEditor.ShaderProperty(property, label);
        }
    }

    private void DrawGroupHeaderReset(string label, System.Action resetAction)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, EditorStyles.miniBoldLabel);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Width(64f)))
            {
                resetAction?.Invoke();
            }
        }
    }

    private MaterialProperty Find(string propertyName)
    {
        return FindProperty(propertyName, properties, false);
    }

    private void ApplyPreset(RetroPreset preset)
    {
        Undo.RecordObjects(materialEditor.targets, $"Apply {preset} Retro Preset");
        foreach (Object target in materialEditor.targets)
        {
            Material material = target as Material;
            if (material == null)
            {
                continue;
            }

            switch (preset)
            {
                case RetroPreset.HybridHigh:
                    ApplyHybridHigh(material);
                    break;
                case RetroPreset.CrunchyPSX:
                    ApplyCrunchyPSX(material);
                    break;
                case RetroPreset.CleanPS2:
                    ApplyCleanPS2(material);
                    break;
                case RetroPreset.MobileFast:
                    ApplyMobileFast(material);
                    break;
            }

            SetupMaterial(material);
            EditorUtility.SetDirty(material);
        }
    }

    private void ResetGeometry()
    {
        ForEachMaterial(material =>
        {
            SetFloat(material, "_VertexSnapEnabled", 1f);
            SetFloat(material, "_VertexSnapStrength", 0.35f);
            SetFloat(material, "_VertexSnapResolution", 240f);
            SetFloat(material, "_VertexSnapSpace", 0f);
            SetFloat(material, "_VertexSnapDistanceFade", 0.35f);
            SetFloat(material, "_VertexSnapFadeStart", 5f);
            SetFloat(material, "_VertexSnapFadeEnd", 45f);
            SetFloat(material, "_VertexSnapSeamReduction", 0f);
            SetFloat(material, "_VertexSnapUseAnchors", 0f);
            SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            SetFloat(material, "_VertexDrawDistance", 0f);
            SetFloat(material, "_VertexDrawDistanceFade", 0f);
            SetFloat(material, "_VertexWobbleEnabled", 1f);
            SetFloat(material, "_VertexWobbleStrength", 0.12f);
            SetFloat(material, "_VertexWobbleSpeed", 2.5f);
            SetFloat(material, "_VertexWobbleScale", 4f);
        }, "Reset Retro Geometry");
    }

    private void ResetTexture()
    {
        ForEachMaterial(material =>
        {
            SetFloat(material, "_UvPixelEnabled", 1f);
            SetFloat(material, "_UvPixelStrength", 0.35f);
            SetFloat(material, "_UvPixelResolution", 256f);
            SetFloat(material, "_UvPixelAspect", 1f);
            SetFloat(material, "_MipBias", 0f);
            SetFloat(material, "_AffineEnabled", 0f);
            SetFloat(material, "_AffineStrength", 0.18f);
            SetFloat(material, "_AffineMode", 0f);
        }, "Reset Retro Texture");
    }

    private void ResetColor()
    {
        ForEachMaterial(material =>
        {
            SetFloat(material, "_PosterizeEnabled", 1f);
            SetFloat(material, "_PosterizeSteps", 18f);
            SetFloat(material, "_PaletteStrength", 0.2f);
            SetFloat(material, "_PaletteSteps", 32f);
            SetFloat(material, "_DitherEnabled", 1f);
            SetFloat(material, "_DitherStrength", 0.18f);
            SetFloat(material, "_DitherScale", 1f);
        }, "Reset Retro Color");
    }

    private void ResetLighting()
    {
        ForEachMaterial(material =>
        {
            SetColor(material, "_SpecColor", new Color(0.5f, 0.5f, 0.5f, 1f));
            SetFloat(material, "_Smoothness", 0.45f);
            SetFloat(material, "_SpecularIntensity", 0.65f);
            SetFloat(material, "_SpecularPower", 36f);
            SetFloat(material, "_RetroSpecularMode", 0f);
            SetFloat(material, "_RetroLightingModel", 0f);
            SetFloat(material, "_LightBands", 0f);
            SetFloat(material, "_ShadowBandStrength", 0.25f);
            SetFloat(material, "_RimEnabled", 1f);
            SetColor(material, "_RimColor", new Color(0.45f, 0.55f, 0.8f, 1f));
            SetFloat(material, "_RimIntensity", 0.25f);
            SetFloat(material, "_RimPower", 2.5f);
        }, "Reset Retro Lighting");
    }

    private void ForEachMaterial(System.Action<Material> action, string undoName)
    {
        Undo.RecordObjects(materialEditor.targets, undoName);
        foreach (Object target in materialEditor.targets)
        {
            if (target is Material material)
            {
                action(material);
                SetupMaterial(material);
                EditorUtility.SetDirty(material);
            }
        }
    }

    public static void ApplyHybridHigh(Material material)
    {
        SetFloat(material, "_VertexSnapEnabled", 1f);
        SetFloat(material, "_VertexSnapStrength", 0.25f);
        SetFloat(material, "_VertexSnapResolution", 320f);
        SetFloat(material, "_VertexSnapSpace", 0f);
        SetFloat(material, "_VertexSnapDistanceFade", 0.5f);
        SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
        SetFloat(material, "_VertexWobbleEnabled", 1f);
        SetFloat(material, "_VertexWobbleStrength", 0.07f);
        SetFloat(material, "_UvPixelEnabled", 1f);
        SetFloat(material, "_UvPixelStrength", 0.22f);
        SetFloat(material, "_UvPixelResolution", 384f);
        SetFloat(material, "_AffineEnabled", 0f);
        SetFloat(material, "_AffineMode", 0f);
        SetFloat(material, "_PosterizeEnabled", 1f);
        SetFloat(material, "_PosterizeSteps", 24f);
        SetFloat(material, "_PaletteStrength", 0.18f);
        SetFloat(material, "_DitherEnabled", 1f);
        SetFloat(material, "_DitherStrength", 0.12f);
        SetFloat(material, "_LightBands", 0f);
        SetFloat(material, "_RimEnabled", 1f);
        SetFloat(material, "_RimIntensity", 0.25f);
        SetFloat(material, "_SpecularIntensity", 0.65f);
        SetFloat(material, "_SpecularPower", 36f);
        SetFloat(material, "_RetroSpecularMode", 2f);
        SetFloat(material, "_RetroLightingModel", 0f);
        SetFloat(material, "_RetroFogEnabled", 0f);
    }

    public static void ApplyCrunchyPSX(Material material)
    {
        SetFloat(material, "_VertexSnapEnabled", 1f);
        SetFloat(material, "_VertexSnapStrength", 0.8f);
        SetFloat(material, "_VertexSnapResolution", 160f);
        SetFloat(material, "_VertexSnapSpace", 0f);
        SetFloat(material, "_VertexSnapDistanceFade", 0.1f);
        SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
        SetFloat(material, "_VertexWobbleEnabled", 1f);
        SetFloat(material, "_VertexWobbleStrength", 0.35f);
        SetFloat(material, "_UvPixelEnabled", 1f);
        SetFloat(material, "_UvPixelStrength", 0.75f);
        SetFloat(material, "_UvPixelResolution", 128f);
        SetFloat(material, "_AffineEnabled", 1f);
        SetFloat(material, "_AffineStrength", 0.45f);
        SetFloat(material, "_AffineMode", 1f);
        SetFloat(material, "_PosterizeEnabled", 1f);
        SetFloat(material, "_PosterizeSteps", 10f);
        SetFloat(material, "_PaletteStrength", 0.55f);
        SetFloat(material, "_PaletteSteps", 16f);
        SetFloat(material, "_DitherEnabled", 1f);
        SetFloat(material, "_DitherStrength", 0.42f);
        SetFloat(material, "_LightBands", 5f);
        SetFloat(material, "_ShadowBandStrength", 0.55f);
        SetFloat(material, "_RimEnabled", 0f);
        SetFloat(material, "_SpecularIntensity", 0.25f);
        SetFloat(material, "_RetroSpecularMode", 1f);
        SetFloat(material, "_RetroLightingModel", 0f);
        SetFloat(material, "_RetroFogEnabled", 0f);
    }

    public static void ApplyCleanPS2(Material material)
    {
        SetFloat(material, "_VertexSnapEnabled", 0f);
        SetFloat(material, "_VertexSnapSpace", 0f);
        SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
        SetFloat(material, "_VertexWobbleEnabled", 0f);
        SetFloat(material, "_UvPixelEnabled", 1f);
        SetFloat(material, "_UvPixelStrength", 0.08f);
        SetFloat(material, "_UvPixelResolution", 512f);
        SetFloat(material, "_AffineEnabled", 0f);
        SetFloat(material, "_AffineMode", 0f);
        SetFloat(material, "_PosterizeEnabled", 1f);
        SetFloat(material, "_PosterizeSteps", 36f);
        SetFloat(material, "_PaletteStrength", 0.05f);
        SetFloat(material, "_DitherEnabled", 0f);
        SetFloat(material, "_LightBands", 0f);
        SetFloat(material, "_RimEnabled", 1f);
        SetFloat(material, "_RimIntensity", 0.18f);
        SetFloat(material, "_SpecularIntensity", 1.05f);
        SetFloat(material, "_SpecularPower", 56f);
        SetFloat(material, "_RetroSpecularMode", 2f);
        SetFloat(material, "_RetroLightingModel", 0f);
        SetFloat(material, "_RetroFogEnabled", 0f);
    }

    public static void ApplyMobileFast(Material material)
    {
        SetFloat(material, "_VertexSnapEnabled", 1f);
        SetFloat(material, "_VertexSnapStrength", 0.2f);
        SetFloat(material, "_VertexSnapResolution", 240f);
        SetFloat(material, "_VertexSnapSpace", 0f);
        SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
        SetFloat(material, "_VertexWobbleEnabled", 0f);
        SetFloat(material, "_UvPixelEnabled", 1f);
        SetFloat(material, "_UvPixelStrength", 0.2f);
        SetFloat(material, "_UvPixelResolution", 256f);
        SetFloat(material, "_AffineEnabled", 0f);
        SetFloat(material, "_AffineMode", 0f);
        SetFloat(material, "_PosterizeEnabled", 1f);
        SetFloat(material, "_PosterizeSteps", 20f);
        SetFloat(material, "_PaletteStrength", 0.1f);
        SetFloat(material, "_DitherEnabled", 0f);
        SetFloat(material, "_LightBands", 0f);
        SetFloat(material, "_RimEnabled", 0f);
        SetFloat(material, "_SpecularIntensity", 0.35f);
        SetFloat(material, "_RetroSpecularMode", 0f);
        SetFloat(material, "_RetroLightingModel", 0f);
        SetFloat(material, "_RetroFogEnabled", 0f);
    }

    public static void SetupMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        bool alphaClip = GetFloat(material, "_AlphaClip") > 0.5f;
        bool transparent = GetFloat(material, "_Surface") > 0.5f;
        int queueOffset = Mathf.Clamp((int)GetFloat(material, "_QueueOffset"), -50, 50);

        SetKeyword(material, "_ALPHATEST_ON", alphaClip);
        SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", transparent);
        SetKeyword(material, "_RECEIVE_SHADOWS_OFF", GetFloat(material, "_ReceiveShadows", 1f) < 0.5f);
        SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") != null);
        SetKeyword(material, "_EMISSION", HasEmission(material));

        SetKeyword(material, "_RETRO_VERTEX_SNAP", GetFloat(material, "_VertexSnapEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_SNAP_ANCHORS", GetFloat(material, "_VertexSnapUseAnchors") > 0.5f);
        SetKeyword(material, "_RETRO_VERTEX_WOBBLE", GetFloat(material, "_VertexWobbleEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_UV_PIXEL", GetFloat(material, "_UvPixelEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_AFFINE", GetFloat(material, "_AffineEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_POSTERIZE", GetFloat(material, "_PosterizeEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_DITHER", GetFloat(material, "_DitherEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_RIM", GetFloat(material, "_RimEnabled") > 0.5f);
        SetKeyword(material, "_RETRO_FOG", GetFloat(material, "_RetroFogEnabled") > 0.5f);

        SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
        SetKeyword(material, "_ALPHAMODULATE_ON", false);

        if (transparent)
        {
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent + queueOffset;

            BlendMode blendMode = (BlendMode)Mathf.Clamp((int)GetFloat(material, "_Blend"), 0, 3);
            switch (blendMode)
            {
                case BlendMode.Premultiply:
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    SetKeyword(material, "_ALPHAPREMULTIPLY_ON", true);
                    break;
                case BlendMode.Additive:
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
                    break;
                case BlendMode.Multiply:
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
                    SetKeyword(material, "_ALPHAMODULATE_ON", true);
                    break;
                default:
                    material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    break;
            }

            material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        }
        else
        {
            material.SetOverrideTag("RenderType", alphaClip ? "TransparentCutout" : "Opaque");
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.Zero);
            material.SetFloat("_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            material.SetFloat("_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.Zero);
            material.SetFloat("_ZWrite", 1f);
            material.renderQueue = (alphaClip ? (int)RenderQueue.AlphaTest : (int)RenderQueue.Geometry) + queueOffset;
        }

        material.SetFloat("_AlphaToMask", alphaClip ? 1f : 0f);
        material.SetShaderPassEnabled("ShadowCaster", true);
    }

    private static bool HasEmission(Material material)
    {
        Color emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
        return emission.maxColorComponent > 0.0001f || material.GetTexture("_EmissionMap") != null;
    }

    private static void SetKeyword(Material material, string keyword, bool enabled)
    {
        if (enabled)
        {
            material.EnableKeyword(keyword);
        }
        else
        {
            material.DisableKeyword(keyword);
        }
    }

    private static float GetFloat(Material material, string propertyName, float fallback = 0f)
    {
        return material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
    }

    private static void SetFloat(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static void SetColor(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }
}
}
