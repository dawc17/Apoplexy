using System.Collections.Generic;
using Borzblade.RetroRenderToolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Legacy entry points kept for existing editor scripts. Public menus live on
// RetroRenderToolkitInstaller under Tools/Borzblade/Retro Render Toolkit.
public static class PSXPS2RetroInstaller
{
    public static void InstallRetroPass() => Borzblade.RetroRenderToolkit.Editor.RetroRenderToolkitInstaller.InstallRetroRendererFeature();
    public static void CreateRetroMaterials() => Borzblade.RetroRenderToolkit.Editor.RetroRenderToolkitInstaller.CreateDefaultMaterials();
    public static void CreateTerrainMaterial() => Borzblade.RetroRenderToolkit.Editor.RetroRenderToolkitInstaller.CreateTerrainMaterial();
}

namespace Borzblade.RetroRenderToolkit.Editor
{
    public static class RetroRenderToolkitInstaller
    {
        public const string PackageRoot = "Assets/Borzblade/PSX/RetroRenderToolkit";
        public const string HybridShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Hybrid Lit";
        public const string TerrainShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Terrain Lit";
        public const string CutoutShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Cutout Lit";
        public const string FoliageShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Foliage Lit";
        public const string UnlitCutoutShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Unlit Cutout";
        public const string WaterShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Water";
        public const string MaterialOutlineShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Material Outline";
        public const string SpriteLitShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Sprite Lit";
        public const string SpriteUnlitShaderName = "Borzblade/Retro Render Toolkit/PSX PS2 Sprite Unlit";
        public const string GlobalShaderName = RetroRendererFeature.ShaderName;
        public const string ScreenOutlineShaderName = PSXPS2ScreenOutlineRendererFeature.ShaderName;
        public const string DepthFogShaderName = PSXPS2DepthFogRendererFeature.ShaderName;

        public const string HybridMaterialFolder = PackageRoot + "/Materials/Hybrid";
        public const string TerrainMaterialFolder = PackageRoot + "/Materials/Terrain";
        public const string CutoutMaterialFolder = PackageRoot + "/Materials/Cutout";
        public const string FoliageMaterialFolder = PackageRoot + "/Materials/Foliage";
        public const string UnlitCutoutMaterialFolder = PackageRoot + "/Materials/UnlitCutout";
        public const string WaterMaterialFolder = PackageRoot + "/Materials/Water";
        public const string OutlineMaterialFolder = PackageRoot + "/Materials/Outline";
        public const string SpriteMaterialFolder = PackageRoot + "/Materials/Sprites";
        public const string GlobalProfileFolder = PackageRoot + "/Settings/GlobalProfiles";
        public const string UserPresetFolder = PackageRoot + "/Settings/UserPresets";
        public const string SnapAnchorFolder = PackageRoot + "/Generated/SnapAnchors";
        public const string GlobalMaterialPath = GlobalProfileFolder + "/PSXPS2_GlobalPass.mat";
        public const string DepthFogMaterialPath = GlobalProfileFolder + "/PSXPS2_DepthFog.mat";

        private static readonly string[] KnownRendererPaths =
        {
            "Assets/Settings/PC_Renderer.asset",
            "Assets/Settings/Mobile_Renderer.asset"
        };

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Open Toolkit")]
        public static void OpenToolkit()
        {
            RetroRenderToolkitWindow.ShowWindow();
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Install Retro Renderer Feature")]
        public static void InstallRetroRendererFeature()
        {
            EnsurePackageFolders();
            Material globalMaterial = EnsureGlobalMaterial();
            if (globalMaterial == null)
            {
                return;
            }

            int installedCount = 0;
            foreach (ScriptableRendererData rendererData in FindRendererDataAssets())
            {
                if (rendererData == null)
                {
                    continue;
                }

                RetroRendererFeature feature = FindFeature(rendererData);
                if (feature == null)
                {
                    var legacyFeature = ScriptableObject.CreateInstance<global::PSXPS2RetroRendererFeature>();
                    legacyFeature.name = "PSX PS2 Retro Pass";
                    AssetDatabase.AddObjectToAsset(legacyFeature, rendererData);
                    AddFeatureReference(rendererData, legacyFeature);
                    feature = legacyFeature;
                }

                Undo.RecordObject(feature, "Install Retro Renderer Feature");
                feature.passMaterial = globalMaterial;
                feature.settings ??= new PSXPS2RetroGlobalSettings();
                feature.settings.enabled = true;
                feature.SetActive(true);
                feature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(feature);
                EditorUtility.SetDirty(rendererData);
                installedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Installed Retro Render Toolkit renderer feature on {installedCount} URP renderer asset(s).");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Install Screen Outline Feature")]
        public static void InstallScreenOutlineFeature()
        {
            int installedCount = 0;
            foreach (ScriptableRendererData rendererData in FindRendererDataAssets())
            {
                if (rendererData == null)
                {
                    continue;
                }

                PSXPS2ScreenOutlineRendererFeature feature = FindScreenOutlineFeature(rendererData);
                if (feature == null)
                {
                    feature = ScriptableObject.CreateInstance<PSXPS2ScreenOutlineRendererFeature>();
                    feature.name = "PSX PS2 Screen Outline";
                    AssetDatabase.AddObjectToAsset(feature, rendererData);
                    AddFeatureReference(rendererData, feature);
                }

                Undo.RecordObject(feature, "Install Screen Outline Feature");
                feature.settings ??= new PSXPS2ScreenOutlineSettings();
                feature.settings.enabled = true;
                feature.SetActive(true);
                feature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(feature);
                EditorUtility.SetDirty(rendererData);
                installedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Installed Retro Render Toolkit screen outline feature on {installedCount} URP renderer asset(s).");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Install Depth Fog Feature")]
        public static void InstallDepthFogFeature()
        {
            EnsurePackageFolders();
            Material depthFogMaterial = EnsureDepthFogMaterial();
            if (depthFogMaterial == null)
            {
                return;
            }

            int installedCount = 0;
            foreach (ScriptableRendererData rendererData in FindRendererDataAssets())
            {
                if (rendererData == null)
                {
                    continue;
                }

                PSXPS2DepthFogRendererFeature feature = FindDepthFogFeature(rendererData);
                if (feature == null)
                {
                    feature = ScriptableObject.CreateInstance<PSXPS2DepthFogRendererFeature>();
                    feature.name = "PSX PS2 Depth Fog";
                    AssetDatabase.AddObjectToAsset(feature, rendererData);
                    AddFeatureReference(rendererData, feature);
                }

                Undo.RecordObject(feature, "Install Depth Fog Feature");
                feature.passMaterial = depthFogMaterial;
                feature.settings ??= new PSXPS2DepthFogSettings();
                feature.settings.enabled = true;
                feature.SetActive(true);
                feature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(feature);
                EditorUtility.SetDirty(rendererData);
                installedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Installed Retro Render Toolkit depth fog feature on {installedCount} URP renderer asset(s).");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Create Default Materials")]
        public static void CreateDefaultMaterials()
        {
            EnsurePackageFolders();
            EnsureGlobalMaterial();
            EnsureDepthFogMaterial();
            EnsureHybridMaterials();
            EnsureTerrainMaterial();
            CreateCutoutFoliageMaterials();
            CreateWaterAndOutlineMaterials();
            CreateSpriteMaterials();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created/updated Retro Render Toolkit default materials.");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Create Terrain Material")]
        public static void CreateTerrainMaterial()
        {
            EnsurePackageFolders();
            EnsureTerrainMaterial();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created/updated Retro Render Toolkit terrain material.");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Create Cutout/Foliage Materials")]
        public static void CreateCutoutFoliageMaterialsMenu()
        {
            EnsurePackageFolders();
            CreateCutoutFoliageMaterials();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created/updated Retro Render Toolkit cutout, foliage, and unlit cutout materials.");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Create Water/Outline Materials")]
        public static void CreateWaterAndOutlineMaterialsMenu()
        {
            EnsurePackageFolders();
            CreateWaterAndOutlineMaterials();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created/updated Retro Render Toolkit water and outline materials.");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Create Sprite Materials")]
        public static void CreateSpriteMaterialsMenu()
        {
            EnsurePackageFolders();
            CreateSpriteMaterials();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Created/updated Retro Render Toolkit sprite materials.");
        }

        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Convert Selected Materials")]
        public static void ConvertSelectedMaterials()
        {
            RetroMaterialConverter.ConvertSelectedMaterials(RetroMaterialTarget.Hybrid, false, true, RetroPresetChoice.CleanPS2);
        }

        public static IReadOnlyList<ScriptableRendererData> FindRendererDataAssets()
        {
            List<ScriptableRendererData> renderers = new List<ScriptableRendererData>();
            HashSet<ScriptableRendererData> seen = new HashSet<ScriptableRendererData>();

            foreach (string knownPath in KnownRendererPaths)
            {
                AddRendererAtPath(knownPath, renderers, seen);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:ScriptableRendererData"))
            {
                AddRendererAtPath(AssetDatabase.GUIDToAssetPath(guid), renderers, seen);
            }

            foreach (string guid in AssetDatabase.FindAssets("t:UniversalRendererData"))
            {
                AddRendererAtPath(AssetDatabase.GUIDToAssetPath(guid), renderers, seen);
            }

            return renderers;
        }

        public static RetroRendererFeature FindFeature(ScriptableRendererData rendererData)
        {
            if (rendererData == null)
            {
                return null;
            }

            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature is RetroRendererFeature retroFeature)
                {
                    return retroFeature;
                }
            }

            return null;
        }

        public static PSXPS2ScreenOutlineRendererFeature FindScreenOutlineFeature(ScriptableRendererData rendererData)
        {
            if (rendererData == null)
            {
                return null;
            }

            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature is PSXPS2ScreenOutlineRendererFeature outlineFeature)
                {
                    return outlineFeature;
                }
            }

            return null;
        }

        public static PSXPS2DepthFogRendererFeature FindDepthFogFeature(ScriptableRendererData rendererData)
        {
            if (rendererData == null)
            {
                return null;
            }

            foreach (ScriptableRendererFeature feature in rendererData.rendererFeatures)
            {
                if (feature is PSXPS2DepthFogRendererFeature depthFogFeature)
                {
                    return depthFogFeature;
                }
            }

            return null;
        }

        public static void EnsurePackageFolders()
        {
            string[] folders =
            {
                HybridMaterialFolder,
                TerrainMaterialFolder,
                CutoutMaterialFolder,
                FoliageMaterialFolder,
                UnlitCutoutMaterialFolder,
                WaterMaterialFolder,
                OutlineMaterialFolder,
                SpriteMaterialFolder,
                GlobalProfileFolder,
                UserPresetFolder,
                SnapAnchorFolder,
                PackageRoot + "/Art/Textures",
                PackageRoot + "/Documentation"
            };

            foreach (string folder in folders)
            {
                EnsureFolder(folder);
            }
        }

        public static Material EnsureGlobalMaterial()
        {
            Shader shader = Shader.Find(GlobalShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{GlobalShaderName}'. The global pass material was not created.");
                return null;
            }

            Material material = EnsureMaterial(shader, GlobalMaterialPath, "PSXPS2_GlobalPass");
            material.SetFloat("_Intensity", 0.65f);
            material.SetFloat("_PixelScale", 1f);
            material.SetFloat("_ColorSteps", 28f);
            material.SetFloat("_DitherStrength", 0.12f);
            material.SetFloat("_ScanlineStrength", 0.08f);
            material.SetFloat("_VignetteStrength", 0.12f);
            material.SetFloat("_Saturation", 1.05f);
            material.SetFloat("_Contrast", 1.05f);
            material.SetFloat("_Bleed", 0.08f);
            material.SetColor("_ColorTint", Color.white);
            material.SetFloat("_Gamma", 1f);
            material.SetFloat("_BlackLevel", 0f);
            material.SetFloat("_DitherScale", 1f);
            material.SetFloat("_PixelationMode", 0f);
            material.SetFloat("_FixedVerticalResolution", 240f);
            material.SetFloat("_DitherPatternMode", 0f);
            material.SetFloat("_DitherPatternScale", 1f);
            material.SetFloat("_DitherThreshold", 0.5f);
            material.SetFloat("_CrtMaskStrength", 0f);
            material.SetFloat("_ChromaticOffset", 0f);
            material.SetFloat("_NoiseStrength", 0f);
            material.SetFloat("_HorizontalJitter", 0f);
            material.SetFloat("_Curvature", 0f);
            material.SetFloat("_GlobalFogEnabled", 0f);
            material.SetColor("_GlobalFogColor", new Color(0.42f, 0.46f, 0.50f, 1f));
            material.SetFloat("_GlobalFogIntensity", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material EnsureDepthFogMaterial()
        {
            Shader shader = Shader.Find(DepthFogShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{DepthFogShaderName}'. The depth fog material was not created.");
                return null;
            }

            Material material = EnsureMaterial(shader, DepthFogMaterialPath, "PSXPS2_DepthFog");
            material.SetColor("_FogColor", new Color(0.42f, 0.46f, 0.50f, 1f));
            material.SetFloat("_Intensity", 0.45f);
            material.SetFloat("_StartDistance", 18f);
            material.SetFloat("_EndDistance", 85f);
            material.SetFloat("_Density", 0.035f);
            material.SetFloat("_BlendMode", 0f);
            material.SetFloat("_Steps", 8f);
            material.SetFloat("_DitherStrength", 0.08f);
            material.SetFloat("_DitherScale", 1f);
            material.SetFloat("_AffectSky", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static void EnsureHybridMaterials()
        {
            Shader shader = Shader.Find(HybridShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{HybridShaderName}'. Hybrid materials were not created.");
                return;
            }

            Material hybrid = EnsureMaterial(shader, HybridMaterialFolder + "/PSXPS2_HybridHigh.mat", "PSXPS2_HybridHigh");
            PSXPS2HybridShaderGUI.ApplyHybridHigh(hybrid);
            PSXPS2HybridShaderGUI.SetupMaterial(hybrid);

            Material crunchy = EnsureMaterial(shader, HybridMaterialFolder + "/PSXPS2_CrunchyPSX.mat", "PSXPS2_CrunchyPSX");
            PSXPS2HybridShaderGUI.ApplyCrunchyPSX(crunchy);
            PSXPS2HybridShaderGUI.SetupMaterial(crunchy);

            Material clean = EnsureMaterial(shader, HybridMaterialFolder + "/PSXPS2_CleanPS2.mat", "PSXPS2_CleanPS2");
            PSXPS2HybridShaderGUI.ApplyCleanPS2(clean);
            PSXPS2HybridShaderGUI.SetupMaterial(clean);

            Material mobile = EnsureMaterial(shader, HybridMaterialFolder + "/PSXPS2_MobileFast.mat", "PSXPS2_MobileFast");
            PSXPS2HybridShaderGUI.ApplyMobileFast(mobile);
            PSXPS2HybridShaderGUI.SetupMaterial(mobile);
        }

        public static void EnsureTerrainMaterial()
        {
            Shader terrainShader = Shader.Find(TerrainShaderName);
            if (terrainShader == null)
            {
                Debug.LogError($"Could not find shader '{TerrainShaderName}'. Terrain material was not created.");
                return;
            }

            Material terrain = EnsureMaterial(terrainShader, TerrainMaterialFolder + "/PSXPS2_TerrainHybrid.mat", "PSXPS2_TerrainHybrid");
            PSXPS2TerrainShaderGUI.ApplyHybridTerrain(terrain);
            PSXPS2TerrainShaderGUI.SetupMaterial(terrain);
        }

        public static void CreateCutoutFoliageMaterials()
        {
            RetroRenderToolkitTextureUtility.EnsurePlaceholderTextures();
            RetroRenderToolkitMaterialFactory.CreateCutoutMaterials();
            RetroRenderToolkitMaterialFactory.CreateFoliageMaterials();
            RetroRenderToolkitMaterialFactory.CreateUnlitCutoutMaterials();
        }

        public static void CreateWaterAndOutlineMaterials()
        {
            RetroRenderToolkitMaterialFactory.CreateWaterMaterials();
            RetroRenderToolkitMaterialFactory.CreateMaterialOutlineMaterials();
        }

        public static void CreateSpriteMaterials()
        {
            RetroRenderToolkitMaterialFactory.CreateSpriteMaterials();
        }

        public static Material EnsureMaterial(Shader shader, string path, string name)
        {
            EnsureFolder(System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/'));
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader)
                {
                    name = name
                };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        public static void EnsureFolder(string assetFolder)
        {
            if (string.IsNullOrEmpty(assetFolder))
            {
                return;
            }

            string[] parts = assetFolder.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static void AddRendererAtPath(string path, List<ScriptableRendererData> renderers, HashSet<ScriptableRendererData> seen)
        {
            ScriptableRendererData rendererData = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
            if (rendererData != null && seen.Add(rendererData))
            {
                renderers.Add(rendererData);
            }
        }

        private static void AddFeatureReference(ScriptableRendererData rendererData, ScriptableRendererFeature feature)
        {
            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out string _, out long localId);

            SerializedObject serializedRenderer = new SerializedObject(rendererData);
            SerializedProperty features = serializedRenderer.FindProperty("m_RendererFeatures");
            SerializedProperty featureMap = serializedRenderer.FindProperty("m_RendererFeatureMap");

            int index = features.arraySize;
            features.InsertArrayElementAtIndex(index);
            features.GetArrayElementAtIndex(index).objectReferenceValue = feature;

            featureMap.InsertArrayElementAtIndex(index);
            featureMap.GetArrayElementAtIndex(index).longValue = localId;

            serializedRenderer.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
