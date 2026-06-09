#include "assetmanager.hpp"
#include "raylib.h"
#include <raylib.h>

void AssetManager::load() {
  if (loaded) {
    return;
  }

  gunModel = LoadModel("models/USP-S.obj");
  monoShader = LoadShader("shaders/monochrome.vs", "shaders/monochrome.fs");
  viewmodelShader =
      LoadShader("shaders/viewmodel.vs", "shaders/viewmodel.fs");
  worldLitShader =
      LoadShader("shaders/world_lit.vs", "shaders/world_lit.fs");
  muzzleFlashTexture = LoadTexture("textures/muzzleflash.png");

  if (FileExists("textures/USP-S.png")) {
    gunTexture = LoadTexture("textures/USP-S.png");
    gunTextureLoaded = true;
  }

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

  for (int i = 0; i < gunModel.materialCount; ++i) {
    gunModel.materials[i].shader = viewmodelShader;
    gunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].color = WHITE;

    if (gunTextureLoaded) {
      gunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].texture = gunTexture;
    }
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
  if (gunTextureLoaded) {
    UnloadTexture(gunTexture);
    gunTextureLoaded = false;
  }
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
