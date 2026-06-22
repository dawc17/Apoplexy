using UnityEditor;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public static class RetroRenderToolkitMaterialFactory
    {
        public static void CreateCutoutMaterials()
        {
            Shader shader = Shader.Find(RetroRenderToolkitInstaller.CutoutShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.CutoutShaderName}'.");
                return;
            }

            Texture2D leaf = RetroRenderToolkitTextureUtility.LoadLeafTexture();
            Texture2D fence = RetroRenderToolkitTextureUtility.LoadFenceTexture();
            Texture2D card = RetroRenderToolkitTextureUtility.LoadCardTexture();

            Material clean = Create(shader, RetroRenderToolkitInstaller.CutoutMaterialFolder, "M_RetroCutout_Clean");
            PSXPS2CutoutShaderGUI.ApplyCleanCutout(clean);
            RetroShaderGUIUtility.SetTexture(clean, "_BaseMap", leaf);
            RetroShaderGUIUtility.SetColor(clean, "_BaseColor", new Color(0.75f, 0.86f, 0.58f, 1f));
            PSXPS2CutoutShaderGUI.SetupMaterial(clean);

            Material crunchy = Create(shader, RetroRenderToolkitInstaller.CutoutMaterialFolder, "M_RetroCutout_CrunchyPSX");
            PSXPS2CutoutShaderGUI.ApplyCrunchyPSXCutout(crunchy);
            RetroShaderGUIUtility.SetTexture(crunchy, "_BaseMap", leaf);
            PSXPS2CutoutShaderGUI.SetupMaterial(crunchy);

            Material fade = Create(shader, RetroRenderToolkitInstaller.CutoutMaterialFolder, "M_RetroCutout_DitherFade");
            PSXPS2CutoutShaderGUI.ApplyDitherFadeCutout(fade);
            RetroShaderGUIUtility.SetTexture(fade, "_BaseMap", card);
            PSXPS2CutoutShaderGUI.SetupMaterial(fade);

            Material fenceMat = Create(shader, RetroRenderToolkitInstaller.CutoutMaterialFolder, "M_RetroCutout_FenceGrate");
            PSXPS2CutoutShaderGUI.ApplyFenceGrate(fenceMat);
            RetroShaderGUIUtility.SetTexture(fenceMat, "_BaseMap", fence);
            RetroShaderGUIUtility.SetColor(fenceMat, "_BaseColor", new Color(0.74f, 0.66f, 0.50f, 1f));
            PSXPS2CutoutShaderGUI.SetupMaterial(fenceMat);

            Material hair = Create(shader, RetroRenderToolkitInstaller.CutoutMaterialFolder, "M_RetroCutout_HairBeard");
            PSXPS2CutoutShaderGUI.ApplyHairBeardCard(hair);
            RetroShaderGUIUtility.SetTexture(hair, "_BaseMap", card);
            RetroShaderGUIUtility.SetColor(hair, "_BaseColor", new Color(0.16f, 0.12f, 0.09f, 1f));
            PSXPS2CutoutShaderGUI.SetupMaterial(hair);
        }

        public static void CreateFoliageMaterials()
        {
            Shader shader = Shader.Find(RetroRenderToolkitInstaller.FoliageShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.FoliageShaderName}'.");
                return;
            }

            Texture2D leaf = RetroRenderToolkitTextureUtility.LoadLeafTexture();
            Texture2D grass = RetroRenderToolkitTextureUtility.LoadGrassTexture();

            CreateFoliage(shader, "M_RetroFoliage_Grass", PSXPS2FoliageShaderGUI.ApplyGrassCard, grass);
            CreateFoliage(shader, "M_RetroFoliage_Bush", PSXPS2FoliageShaderGUI.ApplyBushLeaves, leaf);
            CreateFoliage(shader, "M_RetroFoliage_TreeLeaves", PSXPS2FoliageShaderGUI.ApplyTreeLeaves, leaf);
            CreateFoliage(shader, "M_RetroFoliage_Pine", PSXPS2FoliageShaderGUI.ApplyPineBranch, leaf);
            CreateFoliage(shader, "M_RetroFoliage_DryBush", PSXPS2FoliageShaderGUI.ApplyDeadBush, grass);
            CreateFoliage(shader, "M_RetroFoliage_DarkFantasy", PSXPS2FoliageShaderGUI.ApplyDarkFantasy, leaf);
            CreateFoliage(shader, "M_RetroFoliage_Mobile", PSXPS2FoliageShaderGUI.ApplyMobileFast, leaf);
        }

        public static void CreateUnlitCutoutMaterials()
        {
            Shader shader = Shader.Find(RetroRenderToolkitInstaller.UnlitCutoutShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.UnlitCutoutShaderName}'.");
                return;
            }

            Texture2D leaf = RetroRenderToolkitTextureUtility.LoadLeafTexture();
            Texture2D card = RetroRenderToolkitTextureUtility.LoadCardTexture();

            Material distant = Create(shader, RetroRenderToolkitInstaller.UnlitCutoutMaterialFolder, "M_RetroUnlitCutout_DistantFoliage");
            PSXPS2UnlitCutoutShaderGUI.ApplyDistantFoliage(distant);
            RetroShaderGUIUtility.SetTexture(distant, "_BaseMap", leaf);
            PSXPS2UnlitCutoutShaderGUI.SetupMaterial(distant);

            Material sprite = Create(shader, RetroRenderToolkitInstaller.UnlitCutoutMaterialFolder, "M_RetroUnlitCutout_SpriteProp");
            PSXPS2UnlitCutoutShaderGUI.ApplySpriteProp(sprite);
            RetroShaderGUIUtility.SetTexture(sprite, "_BaseMap", card);
            RetroShaderGUIUtility.SetColor(sprite, "_BaseColor", new Color(0.86f, 0.82f, 0.74f, 1f));
            PSXPS2UnlitCutoutShaderGUI.SetupMaterial(sprite);

            Material mobile = Create(shader, RetroRenderToolkitInstaller.UnlitCutoutMaterialFolder, "M_RetroUnlitCutout_Mobile");
            PSXPS2UnlitCutoutShaderGUI.ApplyMobileFast(mobile);
            RetroShaderGUIUtility.SetTexture(mobile, "_BaseMap", leaf);
            PSXPS2UnlitCutoutShaderGUI.SetupMaterial(mobile);
        }

        public static void CreateWaterMaterials()
        {
            Shader shader = Shader.Find(RetroRenderToolkitInstaller.WaterShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.WaterShaderName}'.");
                return;
            }

            Material clean = Create(shader, RetroRenderToolkitInstaller.WaterMaterialFolder, "M_RetroWater_CleanPS2");
            PSXPS2WaterShaderGUI.ApplyCleanPS2(clean);
            PSXPS2WaterShaderGUI.SetupMaterial(clean);

            Material crunchy = Create(shader, RetroRenderToolkitInstaller.WaterMaterialFolder, "M_RetroWater_CrunchyPSX");
            PSXPS2WaterShaderGUI.ApplyCrunchyPSX(crunchy);
            PSXPS2WaterShaderGUI.SetupMaterial(crunchy);

            Material swamp = Create(shader, RetroRenderToolkitInstaller.WaterMaterialFolder, "M_RetroWater_Swamp");
            PSXPS2WaterShaderGUI.ApplySwamp(swamp);
            PSXPS2WaterShaderGUI.SetupMaterial(swamp);

            Material mobile = Create(shader, RetroRenderToolkitInstaller.WaterMaterialFolder, "M_RetroWater_Mobile");
            PSXPS2WaterShaderGUI.ApplyMobileFast(mobile);
            PSXPS2WaterShaderGUI.SetupMaterial(mobile);
        }

        public static void CreateMaterialOutlineMaterials()
        {
            Shader shader = Shader.Find(RetroRenderToolkitInstaller.MaterialOutlineShaderName);
            if (shader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.MaterialOutlineShaderName}'.");
                return;
            }

            Material black = Create(shader, RetroRenderToolkitInstaller.OutlineMaterialFolder, "M_RetroOutline_Black");
            PSXPS2MaterialOutlineShaderGUI.ApplyBlack(black);
            PSXPS2MaterialOutlineShaderGUI.SetupMaterial(black);

            Material warm = Create(shader, RetroRenderToolkitInstaller.OutlineMaterialFolder, "M_RetroOutline_Warm");
            PSXPS2MaterialOutlineShaderGUI.ApplyWarm(warm);
            PSXPS2MaterialOutlineShaderGUI.SetupMaterial(warm);

            Material crunchy = Create(shader, RetroRenderToolkitInstaller.OutlineMaterialFolder, "M_RetroOutline_Crunchy");
            PSXPS2MaterialOutlineShaderGUI.ApplyCrunchy(crunchy);
            PSXPS2MaterialOutlineShaderGUI.SetupMaterial(crunchy);
        }

        public static void CreateSpriteMaterials()
        {
            Shader litShader = Shader.Find(RetroRenderToolkitInstaller.SpriteLitShaderName);
            Shader unlitShader = Shader.Find(RetroRenderToolkitInstaller.SpriteUnlitShaderName);
            if (litShader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.SpriteLitShaderName}'.");
            }

            if (unlitShader == null)
            {
                Debug.LogError($"Could not find shader '{RetroRenderToolkitInstaller.SpriteUnlitShaderName}'.");
            }

            Texture2D card = RetroRenderToolkitTextureUtility.LoadCardTexture();

            if (litShader != null)
            {
                Material cleanLit = Create(litShader, RetroRenderToolkitInstaller.SpriteMaterialFolder, "M_RetroSpriteLit_Clean");
                PSXPS2SpriteShaderGUI.ApplyCleanSprite(cleanLit);
                RetroShaderGUIUtility.SetTexture(cleanLit, "_MainTex", card);
                PSXPS2SpriteShaderGUI.SetupMaterial(cleanLit);

                Material crunchyLit = Create(litShader, RetroRenderToolkitInstaller.SpriteMaterialFolder, "M_RetroSpriteLit_CrunchyPSX");
                PSXPS2SpriteShaderGUI.ApplyCrunchyPSX(crunchyLit);
                RetroShaderGUIUtility.SetTexture(crunchyLit, "_MainTex", card);
                PSXPS2SpriteShaderGUI.SetupMaterial(crunchyLit);
            }

            if (unlitShader != null)
            {
                Material unlit = Create(unlitShader, RetroRenderToolkitInstaller.SpriteMaterialFolder, "M_RetroSpriteUnlit_Clean");
                PSXPS2SpriteShaderGUI.ApplyCleanSprite(unlit);
                RetroShaderGUIUtility.SetTexture(unlit, "_MainTex", card);
                PSXPS2SpriteShaderGUI.SetupMaterial(unlit);

                Material mobile = Create(unlitShader, RetroRenderToolkitInstaller.SpriteMaterialFolder, "M_RetroSpriteUnlit_Mobile");
                PSXPS2SpriteShaderGUI.ApplyMobileFast(mobile);
                RetroShaderGUIUtility.SetTexture(mobile, "_MainTex", card);
                PSXPS2SpriteShaderGUI.SetupMaterial(mobile);
            }
        }

        public static Material QuickCreate(RetroMaterialTarget target)
        {
            Shader shader = target switch
            {
                RetroMaterialTarget.Terrain => Shader.Find(RetroRenderToolkitInstaller.TerrainShaderName),
                RetroMaterialTarget.Cutout => Shader.Find(RetroRenderToolkitInstaller.CutoutShaderName),
                RetroMaterialTarget.Foliage => Shader.Find(RetroRenderToolkitInstaller.FoliageShaderName),
                RetroMaterialTarget.UnlitCutout => Shader.Find(RetroRenderToolkitInstaller.UnlitCutoutShaderName),
                RetroMaterialTarget.Water => Shader.Find(RetroRenderToolkitInstaller.WaterShaderName),
                RetroMaterialTarget.MaterialOutline => Shader.Find(RetroRenderToolkitInstaller.MaterialOutlineShaderName),
                RetroMaterialTarget.SpriteLit => Shader.Find(RetroRenderToolkitInstaller.SpriteLitShaderName),
                RetroMaterialTarget.SpriteUnlit => Shader.Find(RetroRenderToolkitInstaller.SpriteUnlitShaderName),
                _ => Shader.Find(RetroRenderToolkitInstaller.HybridShaderName)
            };

            string folder = target switch
            {
                RetroMaterialTarget.Terrain => RetroRenderToolkitInstaller.TerrainMaterialFolder,
                RetroMaterialTarget.Cutout => RetroRenderToolkitInstaller.CutoutMaterialFolder,
                RetroMaterialTarget.Foliage => RetroRenderToolkitInstaller.FoliageMaterialFolder,
                RetroMaterialTarget.UnlitCutout => RetroRenderToolkitInstaller.UnlitCutoutMaterialFolder,
                RetroMaterialTarget.Water => RetroRenderToolkitInstaller.WaterMaterialFolder,
                RetroMaterialTarget.MaterialOutline => RetroRenderToolkitInstaller.OutlineMaterialFolder,
                RetroMaterialTarget.SpriteLit => RetroRenderToolkitInstaller.SpriteMaterialFolder,
                RetroMaterialTarget.SpriteUnlit => RetroRenderToolkitInstaller.SpriteMaterialFolder,
                _ => RetroRenderToolkitInstaller.HybridMaterialFolder
            };

            string name = target switch
            {
                RetroMaterialTarget.Terrain => "M_RetroTerrain_New",
                RetroMaterialTarget.Cutout => "M_RetroCutout_New",
                RetroMaterialTarget.Foliage => "M_RetroFoliage_New",
                RetroMaterialTarget.UnlitCutout => "M_RetroUnlitCutout_New",
                RetroMaterialTarget.Water => "M_RetroWater_New",
                RetroMaterialTarget.MaterialOutline => "M_RetroOutline_New",
                RetroMaterialTarget.SpriteLit => "M_RetroSpriteLit_New",
                RetroMaterialTarget.SpriteUnlit => "M_RetroSpriteUnlit_New",
                _ => "M_RetroHybrid_New"
            };

            Material material = shader == null ? null : Create(shader, folder, $"{name}_{System.DateTime.Now:HHmmss}");
            if (material == null)
            {
                return null;
            }

            RetroMaterialConverter.ApplyPreset(material, target, RetroPresetChoice.CleanPS2);
            RetroMaterialConverter.Setup(material, target);
            return material;
        }

        private static void CreateFoliage(Shader shader, string name, System.Action<Material> preset, Texture texture)
        {
            Material material = Create(shader, RetroRenderToolkitInstaller.FoliageMaterialFolder, name);
            preset(material);
            RetroShaderGUIUtility.SetTexture(material, "_BaseMap", texture);
            PSXPS2FoliageShaderGUI.SetupMaterial(material);
        }

        private static Material Create(Shader shader, string folder, string name)
        {
            string path = $"{folder}/{name}.mat";
            Material material = RetroRenderToolkitInstaller.EnsureMaterial(shader, path, name);
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
