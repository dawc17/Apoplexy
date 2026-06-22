using System.Collections.Generic;
using System.IO;
using Borzblade.RetroRenderToolkit;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class RetroRenderToolkitWindow : EditorWindow
    {
        private static readonly string[] Tabs =
        {
            "Setup",
            "Renderer",
            "Materials",
            "Converter",
            "Presets",
            "Terrain",
            "Cutout/Foliage",
            "Demo/Docs",
            "Diagnostics"
        };

        private int selectedTab;
        private Vector2 scroll;
        private ScriptableRendererData selectedRenderer;
        private RetroRendererFeature selectedFeature;
        private PSXPS2DepthFogRendererFeature selectedDepthFogFeature;
        private PSXPS2ScreenOutlineRendererFeature selectedOutlineFeature;
        private string dryRunText = "Press Dry Run to preview selected material conversions.";
        private string userPresetName = "New Retro Preset";
        private RetroMaterialPreset selectedUserPreset;
        private readonly List<string> diagnostics = new List<string>();

        public static void ShowWindow()
        {
            RetroRenderToolkitWindow window = GetWindow<RetroRenderToolkitWindow>("Retro Render Toolkit");
            window.minSize = new Vector2(580f, 440f);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshRendererSelection();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            selectedTab = GUILayout.Toolbar(selectedTab, Tabs);
            EditorGUILayout.Space(6f);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            switch (selectedTab)
            {
                case 1:
                    DrawRendererTab();
                    break;
                case 2:
                    DrawMaterialsTab();
                    break;
                case 3:
                    DrawConverterTab();
                    break;
                case 4:
                    DrawPresetsTab();
                    break;
                case 5:
                    DrawTerrainTab();
                    break;
                case 6:
                    DrawCutoutFoliageTab();
                    break;
                case 7:
                    DrawDemoDocsTab();
                    break;
                case 8:
                    DrawDiagnosticsTab();
                    break;
                default:
                    DrawSetupTab();
                    break;
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawSetupTab()
        {
            DrawTitle("Setup");
            RenderPipelineAsset pipelineAsset = GraphicsSettings.currentRenderPipeline;
            EditorGUILayout.LabelField("Current Render Pipeline", pipelineAsset != null ? pipelineAsset.name : "None", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(pipelineAsset == null ? "No render pipeline asset is active. Assign a URP asset before using URP shaders." : "URP detected or a render pipeline asset is active.", pipelineAsset == null ? MessageType.Warning : MessageType.Info);

            IReadOnlyList<ScriptableRendererData> renderers = RetroRenderToolkitInstaller.FindRendererDataAssets();
            EditorGUILayout.LabelField("Renderer Data Assets", renderers.Count.ToString());
            EditorGUILayout.LabelField("Retro Feature Installed", HasAnyInstalledFeature(renderers) ? "Yes" : "No");

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Install Retro Renderer Feature"))
                {
                    RetroRenderToolkitInstaller.InstallRetroRendererFeature();
                    RefreshRendererSelection();
                }

                if (GUILayout.Button("Create Default Materials"))
                {
                    RetroRenderToolkitInstaller.CreateDefaultMaterials();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Terrain Material"))
                {
                    RetroRenderToolkitInstaller.CreateTerrainMaterial();
                }

                if (GUILayout.Button("Create Cutout/Foliage Materials"))
                {
                    RetroRenderToolkitInstaller.CreateCutoutFoliageMaterialsMenu();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Install Depth Fog Feature"))
                {
                    RetroRenderToolkitInstaller.InstallDepthFogFeature();
                    RefreshRendererSelection();
                }

                if (GUILayout.Button("Install Screen Outline Feature"))
                {
                    RetroRenderToolkitInstaller.InstallScreenOutlineFeature();
                    RefreshRendererSelection();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Bake Snap Anchors For Selection"))
                {
                    RetroSnapAnchorBaker.BakeSelection();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Documentation Folder"))
                {
                    PingOrOpen(RetroRenderToolkitInstaller.PackageRoot + "/Documentation", false);
                }

                if (GUILayout.Button("Ping Package Root"))
                {
                    PingOrOpen(RetroRenderToolkitInstaller.PackageRoot, true);
                }
            }

            if (!HasAnyInstalledFeature(renderers))
            {
                EditorGUILayout.HelpBox("The fullscreen retro renderer feature is not installed on any discovered URP renderer asset.", MessageType.Warning);
            }
        }

        private void DrawRendererTab()
        {
            DrawTitle("Renderer");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Refresh Renderer List", GUILayout.Width(180f)))
                {
                    RefreshRendererSelection();
                }

                if (GUILayout.Button("Install Feature", GUILayout.Width(140f)))
                {
                    RetroRenderToolkitInstaller.InstallRetroRendererFeature();
                    RefreshRendererSelection();
                }
            }

            IReadOnlyList<ScriptableRendererData> renderers = RetroRenderToolkitInstaller.FindRendererDataAssets();
            foreach (ScriptableRendererData rendererData in renderers)
            {
                RetroRendererFeature feature = RetroRenderToolkitInstaller.FindFeature(rendererData);
                PSXPS2DepthFogRendererFeature depthFogFeature = RetroRenderToolkitInstaller.FindDepthFogFeature(rendererData);
                PSXPS2ScreenOutlineRendererFeature outlineFeature = RetroRenderToolkitInstaller.FindScreenOutlineFeature(rendererData);
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    bool selected = rendererData == selectedRenderer;
                    if (GUILayout.Toggle(selected, rendererData.name, "Button"))
                    {
                        selectedRenderer = rendererData;
                        selectedFeature = feature;
                        selectedDepthFogFeature = depthFogFeature;
                        selectedOutlineFeature = outlineFeature;
                    }

                    EditorGUILayout.LabelField(feature != null ? "Retro" : "Missing", GUILayout.Width(64f));
                    EditorGUILayout.LabelField(depthFogFeature != null ? "Fog" : "No Fog", GUILayout.Width(58f));
                    EditorGUILayout.LabelField(outlineFeature != null ? "Outline" : "No Outline", GUILayout.Width(78f));
                    if (GUILayout.Button("Ping", GUILayout.Width(54f)))
                    {
                        EditorGUIUtility.PingObject(rendererData);
                        Selection.activeObject = rendererData;
                    }
                }
            }

            if (selectedRenderer == null)
            {
                EditorGUILayout.HelpBox("No URP renderer data asset is selected.", MessageType.Info);
                return;
            }

            selectedFeature = RetroRenderToolkitInstaller.FindFeature(selectedRenderer);
            selectedDepthFogFeature = RetroRenderToolkitInstaller.FindDepthFogFeature(selectedRenderer);
            selectedOutlineFeature = RetroRenderToolkitInstaller.FindScreenOutlineFeature(selectedRenderer);
            if (selectedFeature == null)
            {
                EditorGUILayout.HelpBox("Selected renderer does not have the Retro Renderer Feature. Install it first.", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.Space(8f);
                DrawRendererFeatureEditor(selectedFeature);
                DrawRendererPresetButtons(selectedFeature);
            }

            EditorGUILayout.Space(8f);
            DrawDepthFogFeatureEditor(selectedRenderer, selectedDepthFogFeature);

            EditorGUILayout.Space(8f);
            DrawScreenOutlineFeatureEditor(selectedRenderer, selectedOutlineFeature);
        }

        private void DrawRendererFeatureEditor(RetroRendererFeature feature)
        {
            SerializedObject serializedFeature = new SerializedObject(feature);
            serializedFeature.Update();

            SerializedProperty active = serializedFeature.FindProperty("m_Active");
            SerializedProperty settings = serializedFeature.FindProperty("settings");
            Undo.RecordObject(feature, "Edit Retro Renderer Feature");

            if (active != null)
            {
                EditorGUILayout.PropertyField(active, new GUIContent("Enabled"));
            }

            if (settings != null)
            {
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("enabled"), new GUIContent("Effect Enabled"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("intensity"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("pixelScale"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("pixelationMode"));
                using (new EditorGUI.DisabledScope(settings.FindPropertyRelative("pixelationMode").enumValueIndex == 0))
                {
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative("fixedVerticalResolution"));
                }
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("colorSteps"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherStrength"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherPatternMode"));
                using (new EditorGUI.DisabledScope(settings.FindPropertyRelative("ditherPatternMode").enumValueIndex == 0))
                {
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherPatternTexture"));
                }
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherPatternScale"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherThreshold"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("scanlineStrength"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("vignetteStrength"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("saturation"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("contrast"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("bleed"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("colorTint"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("gamma"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("blackLevel"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherScale"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("crtMaskStrength"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("chromaticOffset"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("noiseStrength"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("horizontalJitter"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("curvature"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("globalFogEnabled"));
                using (new EditorGUI.DisabledScope(!settings.FindPropertyRelative("globalFogEnabled").boolValue))
                {
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative("globalFogColor"));
                    EditorGUILayout.PropertyField(settings.FindPropertyRelative("globalFogIntensity"));
                }
            }

            if (serializedFeature.ApplyModifiedProperties())
            {
                feature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(feature);
                if (selectedRenderer != null)
                {
                    EditorUtility.SetDirty(selectedRenderer);
                }
            }
        }

        private void DrawScreenOutlineFeatureEditor(ScriptableRendererData rendererData, PSXPS2ScreenOutlineRendererFeature outlineFeature)
        {
            EditorGUILayout.LabelField("Screen Outline", EditorStyles.boldLabel);
            if (outlineFeature == null)
            {
                EditorGUILayout.HelpBox("Screen-space outline is not installed on the selected renderer.", MessageType.Info);
                if (GUILayout.Button("Install Screen Outline Feature", GUILayout.Width(220f)))
                {
                    RetroRenderToolkitInstaller.InstallScreenOutlineFeature();
                    RefreshRendererSelection();
                }
                return;
            }

            SerializedObject serializedFeature = new SerializedObject(outlineFeature);
            serializedFeature.Update();

            SerializedProperty active = serializedFeature.FindProperty("m_Active");
            SerializedProperty settings = serializedFeature.FindProperty("settings");
            Undo.RecordObject(outlineFeature, "Edit Screen Outline Feature");

            if (active != null)
            {
                EditorGUILayout.PropertyField(active, new GUIContent("Enabled"));
            }

            if (settings != null)
            {
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("enabled"), new GUIContent("Effect Enabled"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("passEvent"), new GUIContent("Pass Event"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("outlineColor"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("intensity"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("thickness"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("depthSensitivity"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("normalSensitivity"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("distanceFadeStart"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("distanceFadeEnd"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("blend"));
            }

            if (serializedFeature.ApplyModifiedProperties())
            {
                outlineFeature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(outlineFeature);
                if (rendererData != null)
                {
                    EditorUtility.SetDirty(rendererData);
                }
            }
        }

        private void DrawDepthFogFeatureEditor(ScriptableRendererData rendererData, PSXPS2DepthFogRendererFeature depthFogFeature)
        {
            EditorGUILayout.LabelField("Depth Fog", EditorStyles.boldLabel);
            if (depthFogFeature == null)
            {
                EditorGUILayout.HelpBox("Depth-based fog is not installed on the selected renderer.", MessageType.Info);
                if (GUILayout.Button("Install Depth Fog Feature", GUILayout.Width(220f)))
                {
                    RetroRenderToolkitInstaller.InstallDepthFogFeature();
                    RefreshRendererSelection();
                }
                return;
            }

            SerializedObject serializedFeature = new SerializedObject(depthFogFeature);
            serializedFeature.Update();

            SerializedProperty active = serializedFeature.FindProperty("m_Active");
            SerializedProperty settings = serializedFeature.FindProperty("settings");
            Undo.RecordObject(depthFogFeature, "Edit Depth Fog Feature");

            if (active != null)
            {
                EditorGUILayout.PropertyField(active, new GUIContent("Enabled"));
            }

            if (settings != null)
            {
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("enabled"), new GUIContent("Effect Enabled"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("passEvent"), new GUIContent("Pass Event"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("fogColor"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("intensity"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("startDistance"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("endDistance"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("density"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("blendMode"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("steps"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherStrength"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("ditherScale"));
                EditorGUILayout.PropertyField(settings.FindPropertyRelative("affectSky"));
            }

            if (serializedFeature.ApplyModifiedProperties())
            {
                depthFogFeature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(depthFogFeature);
                if (rendererData != null)
                {
                    EditorUtility.SetDirty(rendererData);
                }
            }
        }

        private void DrawRendererPresetButtons(RetroRendererFeature feature)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Renderer Presets", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                RendererPresetButton("Clean PS2", feature, ApplyCleanRenderer);
                RendererPresetButton("Crunchy PSX", feature, ApplyCrunchyRenderer);
                RendererPresetButton("Horror PSX", feature, ApplyHorrorRenderer);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                RendererPresetButton("Dark Fantasy", feature, ApplyDarkFantasyRenderer);
                RendererPresetButton("Mobile Fast", feature, ApplyMobileRenderer);
                RendererPresetButton("Off/Neutral", feature, ApplyOffRenderer);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                RendererPresetButton("VHS / CRT", feature, ApplyVhsCrtRenderer);
            }
        }

        private void DrawMaterialsTab()
        {
            DrawTitle("Materials");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Default Materials")) RetroRenderToolkitInstaller.CreateDefaultMaterials();
                if (GUILayout.Button("Create Cutout/Foliage Materials")) RetroRenderToolkitInstaller.CreateCutoutFoliageMaterialsMenu();
                if (GUILayout.Button("Create Water/Outline Materials")) RetroRenderToolkitInstaller.CreateWaterAndOutlineMaterialsMenu();
                if (GUILayout.Button("Create Sprite Materials")) RetroRenderToolkitInstaller.CreateSpriteMaterialsMenu();
            }

            DrawFolderButtons("Hybrid", RetroRenderToolkitInstaller.HybridMaterialFolder);
            DrawFolderButtons("Terrain", RetroRenderToolkitInstaller.TerrainMaterialFolder);
            DrawFolderButtons("Cutout", RetroRenderToolkitInstaller.CutoutMaterialFolder);
            DrawFolderButtons("Foliage", RetroRenderToolkitInstaller.FoliageMaterialFolder);
            DrawFolderButtons("Unlit Cutout", RetroRenderToolkitInstaller.UnlitCutoutMaterialFolder);
            DrawFolderButtons("Water", RetroRenderToolkitInstaller.WaterMaterialFolder);
            DrawFolderButtons("Outline", RetroRenderToolkitInstaller.OutlineMaterialFolder);
            DrawFolderButtons("Sprites", RetroRenderToolkitInstaller.SpriteMaterialFolder);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Quick Create Material", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                QuickCreateButton("Hybrid Lit", RetroMaterialTarget.Hybrid);
                QuickCreateButton("Terrain Lit", RetroMaterialTarget.Terrain);
                QuickCreateButton("Cutout Lit", RetroMaterialTarget.Cutout);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                QuickCreateButton("Foliage Lit", RetroMaterialTarget.Foliage);
                QuickCreateButton("Unlit Cutout", RetroMaterialTarget.UnlitCutout);
                QuickCreateButton("Water", RetroMaterialTarget.Water);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                QuickCreateButton("Material Outline", RetroMaterialTarget.MaterialOutline);
                QuickCreateButton("Sprite Lit", RetroMaterialTarget.SpriteLit);
                QuickCreateButton("Sprite Unlit", RetroMaterialTarget.SpriteUnlit);
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Existing Toolkit Materials", EditorStyles.boldLabel);
            foreach (string guid in AssetDatabase.FindAssets("t:Material", new[] { RetroRenderToolkitInstaller.PackageRoot + "/Materials" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
                EditorGUILayout.ObjectField(material, typeof(Material), false);
            }
        }

        private void DrawConverterTab()
        {
            DrawTitle("Converter");
            RetroMaterialConverter.targetShader = (RetroMaterialTarget)EditorGUILayout.EnumPopup("Target Shader", RetroMaterialConverter.targetShader);
            RetroMaterialConverter.createCopies = EditorGUILayout.Toggle("Create Copies", RetroMaterialConverter.createCopies);
            RetroMaterialConverter.preserveProperties = EditorGUILayout.Toggle("Preserve Properties", RetroMaterialConverter.preserveProperties);
            RetroMaterialConverter.backupWhenOverwriting = EditorGUILayout.Toggle("Backup If Overwriting", RetroMaterialConverter.backupWhenOverwriting);
            RetroMaterialConverter.applyPresetAfterConversion = EditorGUILayout.Toggle("Apply Preset", RetroMaterialConverter.applyPresetAfterConversion);
            using (new EditorGUI.DisabledScope(!RetroMaterialConverter.applyPresetAfterConversion))
            {
                RetroMaterialConverter.preset = (RetroPresetChoice)EditorGUILayout.EnumPopup("Preset", RetroMaterialConverter.preset);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Convert Selected Materials"))
                {
                    RetroMaterialConverter.ConvertSelectedWithCurrentOptions();
                }

                if (GUILayout.Button("Convert Materials In Selected Folder"))
                {
                    RetroMaterialConverter.ConvertMaterialsInSelectedFolder();
                }

                if (GUILayout.Button("Dry Run"))
                {
                    dryRunText = RetroMaterialConverter.BuildDryRun();
                }
            }

            EditorGUILayout.HelpBox(dryRunText, MessageType.None);
        }

        private void DrawPresetsTab()
        {
            DrawTitle("Presets");
            EditorGUILayout.LabelField("Apply to selected materials", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                ApplyToSelectionButton("Clean PS2", RetroPresetChoice.CleanPS2);
                ApplyToSelectionButton("Crunchy PSX", RetroPresetChoice.CrunchyPSX);
                ApplyToSelectionButton("Mobile Fast", RetroPresetChoice.MobileFast);
                ApplyToSelectionButton("Horror PSX", RetroPresetChoice.HorrorPSX);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                ApplyToSelectionButton("Grass Card", RetroPresetChoice.GrassCard);
                ApplyToSelectionButton("Bush Leaves", RetroPresetChoice.BushLeaves);
                ApplyToSelectionButton("Dither Fade Cutout", RetroPresetChoice.DitherFadeCutout);
            }

            EditorGUILayout.Space(10f);
            DrawUserPresets();
        }

        private void DrawUserPresets()
        {
            EditorGUILayout.LabelField("My Presets", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                userPresetName = EditorGUILayout.TextField("Preset Name", userPresetName);
                selectedUserPreset = (RetroMaterialPreset)EditorGUILayout.ObjectField("Selected Preset", selectedUserPreset, typeof(RetroMaterialPreset), false);

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(RetroMaterialConverter.GetSelectedMaterials().Count == 0))
                    {
                        if (GUILayout.Button("Save Preset"))
                        {
                            SaveUserPreset();
                        }
                    }

                    using (new EditorGUI.DisabledScope(selectedUserPreset == null || RetroMaterialConverter.GetSelectedMaterials().Count == 0))
                    {
                        if (GUILayout.Button("Load Preset"))
                        {
                            ApplyUserPreset();
                        }
                    }

                    using (new EditorGUI.DisabledScope(selectedUserPreset == null))
                    {
                        if (GUILayout.Button("Delete"))
                        {
                            DeleteUserPreset();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(selectedUserPreset == null))
                    {
                        if (GUILayout.Button("Export JSON"))
                        {
                            ExportUserPreset();
                        }
                    }

                    if (GUILayout.Button("Import JSON"))
                    {
                        ImportUserPreset();
                    }
                }

                string[] presetGuids = AssetDatabase.FindAssets("t:RetroMaterialPreset", new[] { RetroRenderToolkitInstaller.UserPresetFolder });
                if (presetGuids.Length == 0)
                {
                    EditorGUILayout.HelpBox("No user presets saved yet.", MessageType.Info);
                }
                else
                {
                    foreach (string guid in presetGuids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        RetroMaterialPreset preset = AssetDatabase.LoadAssetAtPath<RetroMaterialPreset>(path);
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.ObjectField(preset, typeof(RetroMaterialPreset), false);
                            if (GUILayout.Button("Select", GUILayout.Width(72f)))
                            {
                                selectedUserPreset = preset;
                                Selection.activeObject = preset;
                                EditorGUIUtility.PingObject(preset);
                            }
                        }
                    }
                }
            }
        }

        private void DrawTerrainTab()
        {
            DrawTitle("Terrain");
            if (GUILayout.Button("Create Terrain Material"))
            {
                RetroRenderToolkitInstaller.CreateTerrainMaterial();
            }

            Material terrainMaterial = AssetDatabase.LoadAssetAtPath<Material>(RetroRenderToolkitInstaller.TerrainMaterialFolder + "/PSXPS2_TerrainHybrid.mat");
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.ObjectField("Terrain Material", terrainMaterial, typeof(Material), false);
                if (GUILayout.Button("Select", GUILayout.Width(80f)) && terrainMaterial != null)
                {
                    Selection.activeObject = terrainMaterial;
                    EditorGUIUtility.PingObject(terrainMaterial);
                }
            }

            EditorGUILayout.LabelField("Terrain Presets", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                TerrainPresetButton("Clean PS2 Terrain", PSXPS2TerrainShaderGUI.ApplyCleanPS2);
                TerrainPresetButton("Crunchy PSX Terrain", PSXPS2TerrainShaderGUI.ApplyCrunchyPSX);
                TerrainPresetButton("Mountain Path", PSXPS2TerrainShaderGUI.ApplyHybridTerrain);
                TerrainPresetButton("Mobile Terrain", PSXPS2TerrainShaderGUI.ApplyMobileFast);
            }

            EditorGUILayout.HelpBox("Assign the terrain material to the Terrain component material/template slot. TerrainLayers remain in Unity's Terrain painting system.", MessageType.Info);
        }

        private void DrawCutoutFoliageTab()
        {
            DrawTitle("Cutout / Foliage");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Create Cutout Materials")) RetroRenderToolkitMaterialFactory.CreateCutoutMaterials();
                if (GUILayout.Button("Create Foliage Materials")) RetroRenderToolkitMaterialFactory.CreateFoliageMaterials();
                if (GUILayout.Button("Create Unlit Cutout Materials")) RetroRenderToolkitMaterialFactory.CreateUnlitCutoutMaterials();
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Best Practices", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use alpha clipping for leaves, grass, fences, and hair cards. Use dithered fade for distance or camera fading. Keep vertex snap subtle on foliage, and use two-sided rendering for leaves and grass cards.", MessageType.Info);
        }

        private void SaveUserPreset()
        {
            List<Material> selectedMaterials = RetroMaterialConverter.GetSelectedMaterials();
            if (selectedMaterials.Count == 0)
            {
                return;
            }

            selectedUserPreset = RetroMaterialPreset.SaveFromMaterial(selectedMaterials[0], userPresetName);
            if (selectedUserPreset != null)
            {
                Selection.activeObject = selectedUserPreset;
                EditorGUIUtility.PingObject(selectedUserPreset);
            }
        }

        private void ApplyUserPreset()
        {
            if (selectedUserPreset == null)
            {
                return;
            }

            foreach (Material material in RetroMaterialConverter.GetSelectedMaterials())
            {
                selectedUserPreset.ApplyTo(material);
            }

            AssetDatabase.SaveAssets();
        }

        private void DeleteUserPreset()
        {
            if (selectedUserPreset == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(selectedUserPreset);
            if (EditorUtility.DisplayDialog("Delete Retro Preset", $"Delete '{selectedUserPreset.name}'?", "Delete", "Cancel"))
            {
                selectedUserPreset = null;
                AssetDatabase.DeleteAsset(path);
                AssetDatabase.SaveAssets();
            }
        }

        private void ExportUserPreset()
        {
            if (selectedUserPreset == null)
            {
                return;
            }

            string path = EditorUtility.SaveFilePanel("Export Retro Preset", Application.dataPath, $"{selectedUserPreset.name}.json", "json");
            selectedUserPreset.ExportJson(path);
        }

        private void ImportUserPreset()
        {
            string sourcePath = EditorUtility.OpenFilePanel("Import Retro Preset", Application.dataPath, "json");
            if (string.IsNullOrEmpty(sourcePath))
            {
                return;
            }

            RetroRenderToolkitInstaller.EnsureFolder(RetroRenderToolkitInstaller.UserPresetFolder);
            string assetName = Path.GetFileNameWithoutExtension(sourcePath);
            string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{RetroRenderToolkitInstaller.UserPresetFolder}/{assetName}.asset");
            selectedUserPreset = RetroMaterialPreset.ImportJson(sourcePath, assetPath);
            if (selectedUserPreset != null)
            {
                Selection.activeObject = selectedUserPreset;
                EditorGUIUtility.PingObject(selectedUserPreset);
            }
        }

        private void DrawDemoDocsTab()
        {
            DrawTitle("Demo / Docs");
            DrawFolderButtons("Materials", RetroRenderToolkitInstaller.PackageRoot + "/Materials");
            DrawFolderButtons("Shaders", RetroRenderToolkitInstaller.PackageRoot + "/Shaders");
            DrawFolderButtons("Demo", RetroRenderToolkitInstaller.PackageRoot + "/Demo");
            DrawFolderButtons("Documentation", RetroRenderToolkitInstaller.PackageRoot + "/Documentation");

            string[] scenes = AssetDatabase.FindAssets("t:Scene", new[] { RetroRenderToolkitInstaller.PackageRoot + "/Demo/Scenes" });
            if (scenes.Length == 0)
            {
                EditorGUILayout.HelpBox("No demo scene is present yet. Demo buttons will appear here when scenes are added under Demo/Scenes.", MessageType.Info);
            }
        }

        private void DrawDiagnosticsTab()
        {
            DrawTitle("Diagnostics");
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Diagnostics"))
                {
                    RunDiagnostics();
                }

                if (GUILayout.Button("Ping Package Root"))
                {
                    PingOrOpen(RetroRenderToolkitInstaller.PackageRoot, true);
                }

                if (GUILayout.Button("Open Package Root"))
                {
                    PingOrOpen(RetroRenderToolkitInstaller.PackageRoot, false);
                }

                if (GUILayout.Button("Select All Package Files"))
                {
                    SelectAllPackageFiles();
                }
            }

            if (diagnostics.Count == 0)
            {
                RunDiagnostics();
            }

            foreach (string diagnostic in diagnostics)
            {
                EditorGUILayout.LabelField(diagnostic, EditorStyles.wordWrappedLabel);
            }
        }

        private void RunDiagnostics()
        {
            diagnostics.Clear();
            diagnostics.Add($"Unity Version: {Application.unityVersion}");
            diagnostics.Add($"Render Pipeline Asset: {(GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.name : "None")}");
            diagnostics.Add($"Selected Materials: {RetroMaterialConverter.GetSelectedMaterials().Count}");

            CheckFolder(RetroRenderToolkitInstaller.PackageRoot);
            CheckFolder(RetroRenderToolkitInstaller.PackageRoot + "/Shaders");
            CheckFolder(RetroRenderToolkitInstaller.PackageRoot + "/Runtime");
            CheckFolder(RetroRenderToolkitInstaller.PackageRoot + "/Editor");
            CheckFolder(RetroRenderToolkitInstaller.PackageRoot + "/Materials");
            CheckFolder(RetroRenderToolkitInstaller.PackageRoot + "/Documentation");
            CheckFolder(RetroRenderToolkitInstaller.GlobalProfileFolder);
            CheckFolder(RetroRenderToolkitInstaller.UserPresetFolder);
            CheckFolder(RetroRenderToolkitInstaller.SnapAnchorFolder);

            CheckShader(RetroRenderToolkitInstaller.HybridShaderName);
            CheckShader(RetroRenderToolkitInstaller.TerrainShaderName);
            CheckShader(RetroRenderToolkitInstaller.CutoutShaderName);
            CheckShader(RetroRenderToolkitInstaller.FoliageShaderName);
            CheckShader(RetroRenderToolkitInstaller.UnlitCutoutShaderName);
            CheckShader(RetroRenderToolkitInstaller.WaterShaderName);
            CheckShader(RetroRenderToolkitInstaller.MaterialOutlineShaderName);
            CheckShader(RetroRenderToolkitInstaller.SpriteLitShaderName);
            CheckShader(RetroRenderToolkitInstaller.SpriteUnlitShaderName);
            CheckShader(RetroRenderToolkitInstaller.GlobalShaderName);
            CheckShader(RetroRenderToolkitInstaller.DepthFogShaderName);
            CheckShader(RetroRenderToolkitInstaller.ScreenOutlineShaderName);
            CheckAsset(RetroRenderToolkitInstaller.DepthFogMaterialPath);

            IReadOnlyList<ScriptableRendererData> renderers = RetroRenderToolkitInstaller.FindRendererDataAssets();
            diagnostics.Add($"Renderer Feature Installed: {(HasAnyInstalledFeature(renderers) ? "Yes" : "No")}");
            diagnostics.Add($"Depth Fog Installed: {(HasAnyInstalledDepthFogFeature(renderers) ? "Yes" : "No")}");
            diagnostics.Add($"Screen Outline Installed: {(HasAnyInstalledOutlineFeature(renderers) ? "Yes" : "No")}");
            diagnostics.Add($"Discovered Renderer Assets: {renderers.Count}");

            CheckOldFolder("Assets/Shaders/Retro");
            CheckOldFolder("Assets/Scripts/Rendering");
            CheckOldFolder("Assets/Scripts/Editor/Rendering");
            CheckOldFolder("Assets/Materials/Retro");

            foreach (Material material in RetroMaterialConverter.GetSelectedMaterials())
            {
                diagnostics.Add($"{material.name}: {(RetroMaterialConverter.IsToolkitShader(material) ? "Toolkit shader" : "Other shader")}");
            }
        }

        private void CheckFolder(string path)
        {
            diagnostics.Add($"{(AssetDatabase.IsValidFolder(path) ? "OK" : "Missing")}: {path}");
        }

        private void CheckShader(string shaderName)
        {
            diagnostics.Add($"{(Shader.Find(shaderName) != null ? "OK" : "Missing")}: {shaderName}");
        }

        private void CheckAsset(string path)
        {
            diagnostics.Add($"{(AssetDatabase.LoadAssetAtPath<Object>(path) != null ? "OK" : "Missing")}: {path}");
        }

        private void CheckOldFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                diagnostics.Add($"OK: old folder not present: {path}");
                return;
            }

            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { path });
            diagnostics.Add(guids.Length == 0 ? $"OK: old folder empty: {path}" : $"Warning: old folder still has {guids.Length} asset(s): {path}");
        }

        private void RefreshRendererSelection()
        {
            IReadOnlyList<ScriptableRendererData> renderers = RetroRenderToolkitInstaller.FindRendererDataAssets();
            selectedRenderer = renderers.Count > 0 ? renderers[0] : null;
            selectedFeature = selectedRenderer != null ? RetroRenderToolkitInstaller.FindFeature(selectedRenderer) : null;
            selectedDepthFogFeature = selectedRenderer != null ? RetroRenderToolkitInstaller.FindDepthFogFeature(selectedRenderer) : null;
            selectedOutlineFeature = selectedRenderer != null ? RetroRenderToolkitInstaller.FindScreenOutlineFeature(selectedRenderer) : null;
        }

        private static bool HasAnyInstalledFeature(IReadOnlyList<ScriptableRendererData> renderers)
        {
            foreach (ScriptableRendererData rendererData in renderers)
            {
                if (RetroRenderToolkitInstaller.FindFeature(rendererData) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAnyInstalledOutlineFeature(IReadOnlyList<ScriptableRendererData> renderers)
        {
            foreach (ScriptableRendererData rendererData in renderers)
            {
                if (RetroRenderToolkitInstaller.FindScreenOutlineFeature(rendererData) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasAnyInstalledDepthFogFeature(IReadOnlyList<ScriptableRendererData> renderers)
        {
            foreach (ScriptableRendererData rendererData in renderers)
            {
                if (RetroRenderToolkitInstaller.FindDepthFogFeature(rendererData) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static void RendererPresetButton(string label, RetroRendererFeature feature, System.Action<PSXPS2RetroGlobalSettings> apply)
        {
            if (GUILayout.Button(label))
            {
                Undo.RecordObject(feature, $"Apply {label} Renderer Preset");
                feature.settings ??= new PSXPS2RetroGlobalSettings();
                apply(feature.settings);
                feature.ApplySettingsToMaterial();
                EditorUtility.SetDirty(feature);
                AssetDatabase.SaveAssets();
            }
        }

        private static void ApplyCleanRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = true; s.intensity = 0.35f; s.pixelScale = 1f; s.colorSteps = 40f; s.ditherStrength = 0.04f; s.scanlineStrength = 0.02f; s.vignetteStrength = 0.06f; s.saturation = 1.03f; s.contrast = 1.03f; s.bleed = 0.02f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = Color.white; s.gamma = 1f; s.blackLevel = 0f; s.ditherScale = 1f; s.crtMaskStrength = 0f; s.chromaticOffset = 0f; s.noiseStrength = 0f; s.horizontalJitter = 0f; s.curvature = 0f;
            ResetGlobalFog(s);
        }

        private static void ApplyCrunchyRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = true; s.intensity = 0.85f; s.pixelScale = 3f; s.colorSteps = 12f; s.ditherStrength = 0.34f; s.scanlineStrength = 0.15f; s.vignetteStrength = 0.18f; s.saturation = 0.95f; s.contrast = 1.14f; s.bleed = 0.14f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = Color.white; s.gamma = 1.05f; s.blackLevel = 0.02f; s.ditherScale = 1f; s.crtMaskStrength = 0.08f; s.chromaticOffset = 0.08f; s.noiseStrength = 0.08f; s.horizontalJitter = 0.04f; s.curvature = 0f;
            ResetGlobalFog(s);
        }

        private static void ApplyHorrorRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = true; s.intensity = 0.78f; s.pixelScale = 2f; s.colorSteps = 16f; s.ditherStrength = 0.28f; s.scanlineStrength = 0.12f; s.vignetteStrength = 0.38f; s.saturation = 0.78f; s.contrast = 1.22f; s.bleed = 0.1f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = new Color(0.92f, 0.96f, 1f, 1f); s.gamma = 1.08f; s.blackLevel = 0.04f; s.ditherScale = 1f; s.crtMaskStrength = 0.05f; s.chromaticOffset = 0.06f; s.noiseStrength = 0.11f; s.horizontalJitter = 0.035f; s.curvature = 0f;
            s.globalFogEnabled = true; s.globalFogColor = new Color(0.28f, 0.30f, 0.35f, 1f); s.globalFogIntensity = 0.12f;
        }

        private static void ApplyDarkFantasyRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = true; s.intensity = 0.6f; s.pixelScale = 1.5f; s.colorSteps = 20f; s.ditherStrength = 0.18f; s.scanlineStrength = 0.06f; s.vignetteStrength = 0.26f; s.saturation = 0.9f; s.contrast = 1.12f; s.bleed = 0.08f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = new Color(0.94f, 0.98f, 1f, 1f); s.gamma = 1.02f; s.blackLevel = 0.025f; s.ditherScale = 1f; s.crtMaskStrength = 0.02f; s.chromaticOffset = 0.03f; s.noiseStrength = 0.05f; s.horizontalJitter = 0.015f; s.curvature = 0f;
            s.globalFogEnabled = true; s.globalFogColor = new Color(0.34f, 0.38f, 0.42f, 1f); s.globalFogIntensity = 0.08f;
        }

        private static void ApplyMobileRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = true; s.intensity = 0.28f; s.pixelScale = 1f; s.colorSteps = 28f; s.ditherStrength = 0f; s.scanlineStrength = 0f; s.vignetteStrength = 0.05f; s.saturation = 1f; s.contrast = 1f; s.bleed = 0f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = Color.white; s.gamma = 1f; s.blackLevel = 0f; s.ditherScale = 1f; s.crtMaskStrength = 0f; s.chromaticOffset = 0f; s.noiseStrength = 0f; s.horizontalJitter = 0f; s.curvature = 0f;
            ResetGlobalFog(s);
        }

        private static void ApplyOffRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = false; s.intensity = 0f; s.pixelScale = 1f; s.colorSteps = 64f; s.ditherStrength = 0f; s.scanlineStrength = 0f; s.vignetteStrength = 0f; s.saturation = 1f; s.contrast = 1f; s.bleed = 0f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = Color.white; s.gamma = 1f; s.blackLevel = 0f; s.ditherScale = 1f; s.crtMaskStrength = 0f; s.chromaticOffset = 0f; s.noiseStrength = 0f; s.horizontalJitter = 0f; s.curvature = 0f;
            ResetGlobalFog(s);
        }

        private static void ApplyVhsCrtRenderer(PSXPS2RetroGlobalSettings s)
        {
            s.enabled = true; s.intensity = 0.72f; s.pixelScale = 2f; s.colorSteps = 18f; s.ditherStrength = 0.2f; s.scanlineStrength = 0.18f; s.vignetteStrength = 0.18f; s.saturation = 0.92f; s.contrast = 1.1f; s.bleed = 0.18f;
            ResetPixelAndDitherPattern(s);
            s.colorTint = new Color(1f, 0.97f, 0.92f, 1f); s.gamma = 1.04f; s.blackLevel = 0.025f; s.ditherScale = 1f; s.crtMaskStrength = 0.18f; s.chromaticOffset = 0.12f; s.noiseStrength = 0.09f; s.horizontalJitter = 0.08f; s.curvature = 0.15f;
            ResetGlobalFog(s);
        }

        private static void ResetPixelAndDitherPattern(PSXPS2RetroGlobalSettings s)
        {
            s.pixelationMode = PSXPS2PixelationMode.Scale;
            s.fixedVerticalResolution = 240f;
            s.ditherPatternMode = PSXPS2DitherPatternMode.Procedural;
            s.ditherPatternTexture = null;
            s.ditherPatternScale = 1f;
            s.ditherThreshold = 0.5f;
        }

        private static void ResetGlobalFog(PSXPS2RetroGlobalSettings s)
        {
            s.globalFogEnabled = false;
            s.globalFogColor = new Color(0.42f, 0.46f, 0.50f, 1f);
            s.globalFogIntensity = 0f;
        }

        private static void DrawTitle(string title)
        {
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.Space(3f);
        }

        private static void DrawFolderButtons(string label, string path)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(110f));
                if (GUILayout.Button("Select", GUILayout.Width(80f))) PingOrOpen(path, true);
                if (GUILayout.Button("Open", GUILayout.Width(80f))) PingOrOpen(path, false);
                EditorGUILayout.SelectableLabel(path, GUILayout.Height(EditorGUIUtility.singleLineHeight));
            }
        }

        private static void QuickCreateButton(string label, RetroMaterialTarget target)
        {
            if (GUILayout.Button(label))
            {
                Material material = RetroRenderToolkitMaterialFactory.QuickCreate(target);
                if (material != null)
                {
                    Selection.activeObject = material;
                    EditorGUIUtility.PingObject(material);
                    EditorUtility.SetDirty(material);
                    AssetDatabase.SaveAssets();
                }
            }
        }

        private static void ApplyToSelectionButton(string label, RetroPresetChoice preset)
        {
            if (!GUILayout.Button(label))
            {
                return;
            }

            foreach (Material material in RetroMaterialConverter.GetSelectedMaterials())
            {
                Undo.RecordObject(material, $"Apply {label} Retro Preset");
                RetroMaterialTarget target = DetectTarget(material);
                RetroMaterialConverter.ApplyPreset(material, target, preset);
                RetroMaterialConverter.Setup(material, target);
                EditorUtility.SetDirty(material);
            }

            AssetDatabase.SaveAssets();
        }

        private static RetroMaterialTarget DetectTarget(Material material)
        {
            string shaderName = material != null && material.shader != null ? material.shader.name : string.Empty;
            if (shaderName == RetroRenderToolkitInstaller.TerrainShaderName) return RetroMaterialTarget.Terrain;
            if (shaderName == RetroRenderToolkitInstaller.CutoutShaderName) return RetroMaterialTarget.Cutout;
            if (shaderName == RetroRenderToolkitInstaller.FoliageShaderName) return RetroMaterialTarget.Foliage;
            if (shaderName == RetroRenderToolkitInstaller.UnlitCutoutShaderName) return RetroMaterialTarget.UnlitCutout;
            if (shaderName == RetroRenderToolkitInstaller.WaterShaderName) return RetroMaterialTarget.Water;
            if (shaderName == RetroRenderToolkitInstaller.MaterialOutlineShaderName) return RetroMaterialTarget.MaterialOutline;
            if (shaderName == RetroRenderToolkitInstaller.SpriteLitShaderName) return RetroMaterialTarget.SpriteLit;
            if (shaderName == RetroRenderToolkitInstaller.SpriteUnlitShaderName) return RetroMaterialTarget.SpriteUnlit;
            return RetroMaterialTarget.Hybrid;
        }

        private static void TerrainPresetButton(string label, System.Action<Material> apply)
        {
            if (!GUILayout.Button(label))
            {
                return;
            }

            foreach (Material material in RetroMaterialConverter.GetSelectedMaterials())
            {
                Undo.RecordObject(material, $"Apply {label}");
                apply(material);
                PSXPS2TerrainShaderGUI.SetupMaterial(material);
                EditorUtility.SetDirty(material);
            }

            AssetDatabase.SaveAssets();
        }

        private static void PingOrOpen(string path, bool pingOnly)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (asset != null)
            {
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }

            if (!pingOnly)
            {
                string fullPath = Path.GetFullPath(path);
                EditorUtility.RevealInFinder(fullPath);
            }
        }

        private static void SelectAllPackageFiles()
        {
            List<Object> assets = new List<Object>();
            foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { RetroRenderToolkitInstaller.PackageRoot }))
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            Selection.objects = assets.ToArray();
        }
    }
}
