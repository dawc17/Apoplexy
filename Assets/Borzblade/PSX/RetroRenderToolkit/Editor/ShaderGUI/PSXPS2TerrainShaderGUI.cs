using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
public sealed class PSXPS2TerrainShaderGUI : ShaderGUI, ITerrainLayerCustomUI
{
    private enum TerrainPreset
    {
        HybridTerrain,
        CrunchyPSX,
        CleanPS2,
        MobileFast
    }

    private static bool terrainFoldout = true;
    private static bool geometryFoldout = true;
    private static bool textureFoldout = true;
    private static bool colorFoldout = true;
    private static bool lightingFoldout = true;
    private static bool fogFoldout;
    private static bool advancedFoldout;
    private static bool layerRemapFoldout;

    private MaterialEditor materialEditor;
    private MaterialProperty[] properties;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.materialEditor = materialEditor;
        this.properties = properties;

        DrawPresetToolbar();

        EditorGUI.BeginChangeCheck();
        DrawTerrainOptions();
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
                if (target is Material material)
                {
                    SetupMaterial(material);
                }
            }
        }
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);
        SetupMaterial(material);
    }

    public bool OnTerrainLayerGUI(TerrainLayer terrainLayer, Terrain terrain)
    {
        if (terrainLayer == null)
        {
            return false;
        }

        Undo.RecordObject(terrainLayer, "Edit Terrain Layer");
        EditorGUI.BeginChangeCheck();

        terrainLayer.diffuseTexture = EditorGUILayout.ObjectField("Diffuse", terrainLayer.diffuseTexture, typeof(Texture2D), false) as Texture2D;
        TerrainLayerUtility.ValidateDiffuseTextureUI(terrainLayer.diffuseTexture);

        Vector4 diffuseRemapMin = terrainLayer.diffuseRemapMin;
        Vector4 diffuseRemapMax = terrainLayer.diffuseRemapMax;
        Color diffuseTint = new Color(diffuseRemapMax.x, diffuseRemapMax.y, diffuseRemapMax.z, 1f);
        diffuseTint = EditorGUILayout.ColorField(new GUIContent("Color Tint"), diffuseTint, true, false, false);
        diffuseRemapMin.x = 0f;
        diffuseRemapMin.y = 0f;
        diffuseRemapMin.z = 0f;
        diffuseRemapMax.x = diffuseTint.r;
        diffuseRemapMax.y = diffuseTint.g;
        diffuseRemapMax.z = diffuseTint.b;
        diffuseRemapMax.w = 1f;

        terrainLayer.normalMapTexture = EditorGUILayout.ObjectField("Normal Map", terrainLayer.normalMapTexture, typeof(Texture2D), false) as Texture2D;
        TerrainLayerUtility.ValidateNormalMapTextureUI(terrainLayer.normalMapTexture, TerrainLayerUtility.CheckNormalMapTextureType(terrainLayer.normalMapTexture));
        using (new EditorGUI.DisabledScope(terrainLayer.normalMapTexture == null))
        {
            terrainLayer.normalScale = EditorGUILayout.Slider("Normal Scale", terrainLayer.normalScale, 0f, 8f);
        }

        terrainLayer.maskMapTexture = EditorGUILayout.ObjectField("Mask Map", terrainLayer.maskMapTexture, typeof(Texture2D), false) as Texture2D;
        TerrainLayerUtility.ValidateMaskMapTextureUI(terrainLayer.maskMapTexture);

        layerRemapFoldout = EditorGUILayout.Foldout(layerRemapFoldout, terrainLayer.maskMapTexture == null ? "Layer Defaults" : "Mask Channel Remap");
        if (layerRemapFoldout)
        {
            EditorGUI.indentLevel++;
            Vector4 maskMin = terrainLayer.maskMapRemapMin;
            Vector4 maskMax = terrainLayer.maskMapRemapMax;

            if (terrainLayer.maskMapTexture != null)
            {
                DrawMinMax("R: Metallic", ref maskMin.x, ref maskMax.x);
                DrawMinMax("G: AO", ref maskMin.y, ref maskMax.y);
                DrawMinMax("B: Height", ref maskMin.z, ref maskMax.z);
                DrawMinMax("A: Smoothness", ref maskMin.w, ref maskMax.w);
            }
            else
            {
                terrainLayer.metallic = EditorGUILayout.Slider("R: Metallic", terrainLayer.metallic, 0f, 1f);
                maskMax.y = EditorGUILayout.Slider("G: AO", maskMax.y, 0f, 1f);
                maskMin.y = Mathf.Min(maskMin.y, maskMax.y);
                maskMax.z = EditorGUILayout.FloatField("B: Height", maskMax.z);
                maskMin.z = Mathf.Min(Mathf.Max(0f, maskMin.z), maskMax.z);
                terrainLayer.smoothness = EditorGUILayout.Slider("A: Smoothness", terrainLayer.smoothness, 0f, 1f);
            }

            terrainLayer.maskMapRemapMin = maskMin;
            terrainLayer.maskMapRemapMax = maskMax;
            EditorGUI.indentLevel--;
        }

        TerrainLayerUtility.TilingSettingsUI(terrainLayer);

        if (EditorGUI.EndChangeCheck())
        {
            terrainLayer.diffuseRemapMin = diffuseRemapMin;
            terrainLayer.diffuseRemapMax = diffuseRemapMax;
            EditorUtility.SetDirty(terrainLayer);
        }

        return true;
    }

    private void DrawPresetToolbar()
    {
        EditorGUILayout.Space(4f);
        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.LabelField("PSX / PS2 Terrain Presets", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Hybrid"))
                {
                    ApplyPreset(TerrainPreset.HybridTerrain);
                }

                if (GUILayout.Button("Crunchy"))
                {
                    ApplyPreset(TerrainPreset.CrunchyPSX);
                }

                if (GUILayout.Button("Clean PS2"))
                {
                    ApplyPreset(TerrainPreset.CleanPS2);
                }

                if (GUILayout.Button("Mobile"))
                {
                    ApplyPreset(TerrainPreset.MobileFast);
                }
            }

            EditorGUILayout.HelpBox("Assign this material to a Terrain component's Material Template. TerrainLayer textures, normals, masks, tiling, and height data still come from the terrain layers.", MessageType.Info);
        }
    }

    private void DrawTerrainOptions()
    {
        terrainFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(terrainFoldout, "Terrain");
        if (terrainFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawProperty("_EnableHeightBlend", "Height Blend");
                using (new EditorGUI.DisabledScope(Find("_EnableHeightBlend")?.floatValue < 0.5f))
                {
                    DrawProperty("_HeightTransition", "Height Transition");
                    EditorGUILayout.HelpBox("Unity disables reliable height blending when a terrain needs extra add passes for more than four layers.", MessageType.None);
                }

                DrawProperty("_EnableInstancedPerPixelNormal", "Instanced Per-Pixel Normal");
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
                DrawProperty("_TerrainRetroVertexSnapEnabled", "Vertex Snap");
                using (new EditorGUI.DisabledScope(Find("_TerrainRetroVertexSnapEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_TerrainRetroVertexSnapStrength", "Strength");
                    DrawProperty("_TerrainRetroVertexSnapResolution", "Resolution");
                    DrawProperty("_TerrainRetroVertexSnapDistanceFade", "Distance Fade");
                    DrawProperty("_TerrainRetroVertexSnapFadeStart", "Fade Start");
                    DrawProperty("_TerrainRetroVertexSnapFadeEnd", "Fade End");
                    DrawProperty("_TerrainRetroVertexSnapSeamReduction", "Snap Seam Reduction");
                    DrawProperty("_TerrainRetroVertexSnapSpace", "Snap Space");
                }

                EditorGUILayout.Space(4f);
                DrawProperty("_TerrainRetroVertexWobbleEnabled", "Vertex Wobble");
                using (new EditorGUI.DisabledScope(Find("_TerrainRetroVertexWobbleEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_TerrainRetroVertexWobbleStrength", "Strength");
                    DrawProperty("_TerrainRetroVertexWobbleSpeed", "Speed");
                    DrawProperty("_TerrainRetroVertexWobbleScale", "Scale");
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
                DrawProperty("_TerrainRetroUvPixelEnabled", "UV Pixelation");
                using (new EditorGUI.DisabledScope(Find("_TerrainRetroUvPixelEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_TerrainRetroUvPixelStrength", "Strength");
                    DrawProperty("_TerrainRetroUvPixelResolution", "Resolution");
                    DrawProperty("_TerrainRetroUvPixelAspect", "Aspect");
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
                DrawProperty("_TerrainRetroPosterizeEnabled", "Posterize");
                using (new EditorGUI.DisabledScope(Find("_TerrainRetroPosterizeEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_TerrainRetroPosterizeSteps", "Posterize Steps");
                    DrawProperty("_TerrainRetroPaletteStrength", "Palette Strength");
                    DrawProperty("_TerrainRetroPaletteSteps", "Palette Steps");
                }

                EditorGUILayout.Space(4f);
                DrawProperty("_TerrainRetroDitherEnabled", "Ordered Dither");
                using (new EditorGUI.DisabledScope(Find("_TerrainRetroDitherEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_TerrainRetroDitherStrength", "Strength");
                    DrawProperty("_TerrainRetroDitherScale", "Scale");
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
                DrawProperty("_TerrainRetroLightBandsEnabled", "Light Banding");
                using (new EditorGUI.DisabledScope(Find("_TerrainRetroLightBandsEnabled")?.floatValue < 0.5f))
                {
                    DrawProperty("_TerrainRetroLightBands", "Bands");
                    DrawProperty("_TerrainRetroBandStrength", "Strength");
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void DrawFogModifiers()
    {
        fogFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(fogFoldout, "Material Fog");
        if (fogFoldout)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                DrawGroupHeaderReset("Fog", ResetFog);
                DrawProperty("_TerrainRetroFogEnabled", "Retro Fog");
                using (new EditorGUI.DisabledScope((Find("_TerrainRetroFogEnabled")?.floatValue ?? 0f) < 0.5f))
                {
                    DrawProperty("_TerrainRetroFogColor", "Fog Color");
                    DrawProperty("_TerrainRetroFogStart", "Start");
                    DrawProperty("_TerrainRetroFogEnd", "End");
                    DrawProperty("_TerrainRetroFogDensity", "Density");
                    DrawProperty("_TerrainRetroFogSteps", "Steps");
                    DrawProperty("_TerrainRetroFogBlendMode", "Blend Mode");
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
                materialEditor.EnableInstancingField();
                materialEditor.DoubleSidedGIField();

                if (GUILayout.Button("Sync Terrain Keywords"))
                {
                    ForEachMaterial(SetupMaterial);
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    private void ApplyPreset(TerrainPreset preset)
    {
        ForEachMaterial(material =>
        {
            Undo.RecordObject(material, "Apply PSX PS2 Terrain Preset");

            switch (preset)
            {
                case TerrainPreset.HybridTerrain:
                    ApplyHybridTerrain(material);
                    break;
                case TerrainPreset.CrunchyPSX:
                    ApplyCrunchyPSX(material);
                    break;
                case TerrainPreset.CleanPS2:
                    ApplyCleanPS2(material);
                    break;
                case TerrainPreset.MobileFast:
                    ApplyMobileFast(material);
                    break;
            }

            SetupMaterial(material);
            EditorUtility.SetDirty(material);
        });
    }

    public static void ApplyHybridTerrain(Material material)
    {
        Set(material, "_TerrainRetroVertexSnapEnabled", 1f);
        Set(material, "_TerrainRetroVertexSnapStrength", 0.25f);
        Set(material, "_TerrainRetroVertexSnapResolution", 300f);
        Set(material, "_TerrainRetroVertexSnapDistanceFade", 0.4f);
        Set(material, "_TerrainRetroVertexSnapFadeStart", 10f);
        Set(material, "_TerrainRetroVertexSnapFadeEnd", 90f);
        Set(material, "_TerrainRetroVertexSnapSeamReduction", 0.1f);
        Set(material, "_TerrainRetroVertexSnapSpace", 0f);
        Set(material, "_TerrainRetroVertexWobbleEnabled", 0f);
        Set(material, "_TerrainRetroVertexWobbleStrength", 0.08f);
        Set(material, "_TerrainRetroVertexWobbleSpeed", 1.5f);
        Set(material, "_TerrainRetroVertexWobbleScale", 3f);
        Set(material, "_TerrainRetroUvPixelEnabled", 1f);
        Set(material, "_TerrainRetroUvPixelStrength", 0.18f);
        Set(material, "_TerrainRetroUvPixelResolution", 384f);
        Set(material, "_TerrainRetroUvPixelAspect", 1f);
        Set(material, "_TerrainRetroPosterizeEnabled", 1f);
        Set(material, "_TerrainRetroPosterizeSteps", 24f);
        Set(material, "_TerrainRetroPaletteStrength", 0.14f);
        Set(material, "_TerrainRetroPaletteSteps", 32f);
        Set(material, "_TerrainRetroDitherEnabled", 1f);
        Set(material, "_TerrainRetroDitherStrength", 0.08f);
        Set(material, "_TerrainRetroDitherScale", 1f);
        Set(material, "_TerrainRetroLightBandsEnabled", 0f);
        Set(material, "_TerrainRetroLightBands", 6f);
        Set(material, "_TerrainRetroBandStrength", 0.2f);
        ResetFogValues(material);
    }

    public static void ApplyCrunchyPSX(Material material)
    {
        ApplyHybridTerrain(material);
        Set(material, "_TerrainRetroVertexSnapStrength", 0.55f);
        Set(material, "_TerrainRetroVertexSnapResolution", 180f);
        Set(material, "_TerrainRetroVertexWobbleEnabled", 1f);
        Set(material, "_TerrainRetroVertexWobbleStrength", 0.18f);
        Set(material, "_TerrainRetroUvPixelStrength", 0.55f);
        Set(material, "_TerrainRetroUvPixelResolution", 128f);
        Set(material, "_TerrainRetroPosterizeSteps", 12f);
        Set(material, "_TerrainRetroPaletteStrength", 0.4f);
        Set(material, "_TerrainRetroPaletteSteps", 16f);
        Set(material, "_TerrainRetroDitherStrength", 0.2f);
        Set(material, "_TerrainRetroLightBandsEnabled", 1f);
        Set(material, "_TerrainRetroLightBands", 4f);
        Set(material, "_TerrainRetroBandStrength", 0.45f);
    }

    public static void ApplyCleanPS2(Material material)
    {
        ApplyHybridTerrain(material);
        Set(material, "_TerrainRetroVertexSnapStrength", 0.08f);
        Set(material, "_TerrainRetroVertexSnapResolution", 640f);
        Set(material, "_TerrainRetroUvPixelStrength", 0.05f);
        Set(material, "_TerrainRetroPosterizeSteps", 40f);
        Set(material, "_TerrainRetroPaletteStrength", 0.06f);
        Set(material, "_TerrainRetroDitherStrength", 0.03f);
        Set(material, "_TerrainRetroLightBandsEnabled", 0f);
    }

    public static void ApplyMobileFast(Material material)
    {
        ApplyHybridTerrain(material);
        Set(material, "_TerrainRetroVertexSnapEnabled", 1f);
        Set(material, "_TerrainRetroVertexSnapStrength", 0.2f);
        Set(material, "_TerrainRetroVertexSnapResolution", 256f);
        Set(material, "_TerrainRetroVertexWobbleEnabled", 0f);
        Set(material, "_TerrainRetroUvPixelEnabled", 0f);
        Set(material, "_TerrainRetroPosterizeEnabled", 1f);
        Set(material, "_TerrainRetroPosterizeSteps", 20f);
        Set(material, "_TerrainRetroPaletteStrength", 0.1f);
        Set(material, "_TerrainRetroDitherEnabled", 0f);
        Set(material, "_TerrainRetroLightBandsEnabled", 0f);
        Set(material, "_EnableInstancedPerPixelNormal", 0f);
    }

    public static void SetupMaterial(Material material)
    {
        if (material == null)
        {
            return;
        }

        SetKeyword(material, "_TERRAIN_BLEND_HEIGHT", Get(material, "_EnableHeightBlend") > 0.5f);
        SetKeyword(material, "_TERRAIN_INSTANCED_PERPIXEL_NORMAL", Get(material, "_EnableInstancedPerPixelNormal") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_VERTEX_SNAP", Get(material, "_TerrainRetroVertexSnapEnabled") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_VERTEX_WOBBLE", Get(material, "_TerrainRetroVertexWobbleEnabled") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_UV_PIXEL", Get(material, "_TerrainRetroUvPixelEnabled") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_POSTERIZE", Get(material, "_TerrainRetroPosterizeEnabled") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_DITHER", Get(material, "_TerrainRetroDitherEnabled") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_LIGHT_BANDS", Get(material, "_TerrainRetroLightBandsEnabled") > 0.5f);
        SetKeyword(material, "_TERRAIN_RETRO_FOG", Get(material, "_TerrainRetroFogEnabled") > 0.5f);
        material.SetOverrideTag("RenderType", "Opaque");
    }

    private void ResetGeometry()
    {
        ForEachMaterial(material =>
        {
            Set(material, "_TerrainRetroVertexSnapEnabled", 1f);
            Set(material, "_TerrainRetroVertexSnapStrength", 0.25f);
            Set(material, "_TerrainRetroVertexSnapResolution", 300f);
            Set(material, "_TerrainRetroVertexSnapDistanceFade", 0.4f);
            Set(material, "_TerrainRetroVertexSnapFadeStart", 10f);
            Set(material, "_TerrainRetroVertexSnapFadeEnd", 90f);
            Set(material, "_TerrainRetroVertexSnapSeamReduction", 0.1f);
            Set(material, "_TerrainRetroVertexSnapSpace", 0f);
            Set(material, "_TerrainRetroVertexWobbleEnabled", 0f);
            Set(material, "_TerrainRetroVertexWobbleStrength", 0.08f);
            Set(material, "_TerrainRetroVertexWobbleSpeed", 1.5f);
            Set(material, "_TerrainRetroVertexWobbleScale", 3f);
            SetupMaterial(material);
        });
    }

    private void ResetTexture()
    {
        ForEachMaterial(material =>
        {
            Set(material, "_TerrainRetroUvPixelEnabled", 1f);
            Set(material, "_TerrainRetroUvPixelStrength", 0.18f);
            Set(material, "_TerrainRetroUvPixelResolution", 384f);
            Set(material, "_TerrainRetroUvPixelAspect", 1f);
            SetupMaterial(material);
        });
    }

    private void ResetColor()
    {
        ForEachMaterial(material =>
        {
            Set(material, "_TerrainRetroPosterizeEnabled", 1f);
            Set(material, "_TerrainRetroPosterizeSteps", 24f);
            Set(material, "_TerrainRetroPaletteStrength", 0.14f);
            Set(material, "_TerrainRetroPaletteSteps", 32f);
            Set(material, "_TerrainRetroDitherEnabled", 1f);
            Set(material, "_TerrainRetroDitherStrength", 0.08f);
            Set(material, "_TerrainRetroDitherScale", 1f);
            SetupMaterial(material);
        });
    }

    private void ResetLighting()
    {
        ForEachMaterial(material =>
        {
            Set(material, "_TerrainRetroLightBandsEnabled", 0f);
            Set(material, "_TerrainRetroLightBands", 6f);
            Set(material, "_TerrainRetroBandStrength", 0.2f);
            SetupMaterial(material);
        });
    }

    private void ResetFog()
    {
        ForEachMaterial(material =>
        {
            ResetFogValues(material);
            SetupMaterial(material);
        });
    }

    private static void ResetFogValues(Material material)
    {
        Set(material, "_TerrainRetroFogEnabled", 0f);
        SetColor(material, "_TerrainRetroFogColor", new Color(0.42f, 0.46f, 0.50f, 1f));
        Set(material, "_TerrainRetroFogStart", 18f);
        Set(material, "_TerrainRetroFogEnd", 70f);
        Set(material, "_TerrainRetroFogDensity", 0.035f);
        Set(material, "_TerrainRetroFogSteps", 6f);
        Set(material, "_TerrainRetroFogBlendMode", 0f);
    }

    private void DrawGroupHeaderReset(string label, System.Action resetAction)
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (GUILayout.Button("Reset", GUILayout.Width(72f)))
            {
                resetAction?.Invoke();
            }
        }
    }

    private void DrawMinMax(string label, ref float min, ref float max)
    {
        EditorGUILayout.MinMaxSlider(label, ref min, ref max, 0f, 1f);
        min = Mathf.Clamp01(min);
        max = Mathf.Clamp01(max);
    }

    private void DrawProperty(string propertyName, string label)
    {
        MaterialProperty property = Find(propertyName);
        if (property != null)
        {
            materialEditor.ShaderProperty(property, label);
        }
    }

    private MaterialProperty Find(string propertyName)
    {
        return FindProperty(propertyName, properties, false);
    }

    private void ForEachMaterial(System.Action<Material> action)
    {
        foreach (Object target in materialEditor.targets)
        {
            if (target is Material material)
            {
                Undo.RecordObject(material, "Edit PSX PS2 Terrain Material");
                action(material);
                EditorUtility.SetDirty(material);
            }
        }
    }

    private static void Set(Material material, string propertyName, float value)
    {
        if (material != null && material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }

    private static void SetColor(Material material, string propertyName, Color value)
    {
        if (material != null && material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static float Get(Material material, string propertyName)
    {
        return material != null && material.HasProperty(propertyName) ? material.GetFloat(propertyName) : 0f;
    }

    private static void SetKeyword(Material material, string keyword, bool enabled)
    {
        CoreUtils.SetKeyword(material, keyword, enabled);
    }
}
}
