#include "assetmanager.hpp"
#include <raylib.h>

void AssetManager::load() {
  if (loaded) {
    return;
  }

  gunModel = LoadModel("models/gun.glb");
  monoShader = LoadShader("shaders/monochrome.vs", "shaders/monochrome.fs");
  viewmodelShader =
      LoadShader("shaders/viewmodel.vs", "shaders/viewmodel.fs");
  worldLitShader =
      LoadShader("shaders/world_lit.vs", "shaders/world_lit.fs");
  muzzleFlashTexture = LoadTexture("textures/muzzleflash.png");

  if (FileExists("textures/skybox.png")) {
    skyboxShader = LoadShader("shaders/skybox.vs", "shaders/skybox.fs");
    skyboxShader.locs[SHADER_LOC_MAP_CUBEMAP] =
        GetShaderLocation(skyboxShader, "environmentMap");

    int environmentMap = MATERIAL_MAP_CUBEMAP;
    SetShaderValue(skyboxShader, GetShaderLocation(skyboxShader, "environmentMap"),
                   &environmentMap, SHADER_UNIFORM_INT);

    Image skyboxImage = LoadImage("textures/skybox.png");
    skyboxCubemap =
        LoadTextureCubemap(skyboxImage, CUBEMAP_LAYOUT_AUTO_DETECT);
    UnloadImage(skyboxImage);

    skyboxModel = LoadModelFromMesh(GenMeshCube(1.0f, 1.0f, 1.0f));
    skyboxModel.materials[0].shader = skyboxShader;
    skyboxModel.materials[0].maps[MATERIAL_MAP_CUBEMAP].texture =
        skyboxCubemap;
    skyboxLoaded = true;
  }

  const Color gunColors[] = {
      {52, 55, 58, 255},   {78, 82, 86, 255},    {34, 36, 38, 255},
      {112, 108, 100, 255}, {145, 142, 132, 255}, {24, 25, 27, 255},
  };

  for (int i = 0; i < gunModel.materialCount; ++i) {
    gunModel.materials[i].shader = viewmodelShader;
    gunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].color =
        gunColors[i % (sizeof(gunColors) / sizeof(gunColors[0]))];
  }

  loaded = true;
}

void AssetManager::unload() {
  if (!loaded) {
    return;
  }

  UnloadModel(gunModel);
  UnloadShader(monoShader);
  UnloadShader(viewmodelShader);
  UnloadShader(worldLitShader);
  UnloadTexture(muzzleFlashTexture);
  if (skyboxLoaded) {
    UnloadShader(skyboxShader);
    UnloadTexture(skyboxCubemap);
    UnloadModel(skyboxModel);
    skyboxLoaded = false;
  }

  loaded = false;
}

const Model &AssetManager::getGunModel() const { return gunModel; }

Shader AssetManager::getMonoShader() const { return monoShader; }

Shader AssetManager::getViewmodelShader() const { return viewmodelShader; }

Shader AssetManager::getWorldLitShader() const { return worldLitShader; }

const Texture2D &AssetManager::getMuzzleFlashTexture() const {
  return muzzleFlashTexture;
}

const Model &AssetManager::getSkyboxModel() const { return skyboxModel; }

bool AssetManager::hasSkybox() const { return skyboxLoaded; }
