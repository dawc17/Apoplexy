#include "assetmanager.hpp"
#include "raylib.h"
#include "weapon/weapondata.hpp"
#include <raylib.h>

void AssetManager::load() {
  if (loaded) {
    return;
  }

  pistolModel = LoadModel("models/USP-S.obj");
  shotgunModel = LoadModel("models/Shotgun.glb");

  monoShader = LoadShader("shaders/monochrome.vs", "shaders/monochrome.fs");
  viewmodelShader = LoadShader("shaders/viewmodel.vs", "shaders/viewmodel.fs");
  worldLitShader = LoadShader("shaders/world_lit.vs", "shaders/world_lit.fs");
  muzzleFlashTexture = LoadTexture("textures/muzzleflash.png");

  if (FileExists("textures/USP-S.png")) {
    pistolTexture = LoadTexture("textures/USP-S.png");
    pistolTextureLoaded = true;
  }

  if (FileExists("textures/Shotgun.png")) {
    shotgunTexture = LoadTexture("textures/Shotgun.png");
    shotgunTextureLoaded = true;
  }

  if (FileExists("textures/skybox.png")) {
    skyboxShader = LoadShader("shaders/skybox.vs", "shaders/skybox.fs");
    skyboxShader.locs[SHADER_LOC_MAP_CUBEMAP] =
        GetShaderLocation(skyboxShader, "environmentMap");

    int environmentMap = MATERIAL_MAP_CUBEMAP;
    SetShaderValue(skyboxShader,
                   GetShaderLocation(skyboxShader, "environmentMap"),
                   &environmentMap, SHADER_UNIFORM_INT);

    Image skyboxImage = LoadImage("textures/skybox.png");
    skyboxCubemap = LoadTextureCubemap(skyboxImage, CUBEMAP_LAYOUT_AUTO_DETECT);
    UnloadImage(skyboxImage);

    skyboxModel = LoadModelFromMesh(GenMeshCube(1.0f, 1.0f, 1.0f));
    skyboxModel.materials[0].shader = skyboxShader;
    skyboxModel.materials[0].maps[MATERIAL_MAP_CUBEMAP].texture = skyboxCubemap;
    skyboxLoaded = true;
  }

  for (int i = 0; i < pistolModel.materialCount; ++i) {
    pistolModel.materials[i].shader = viewmodelShader;
    pistolModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].color = WHITE;

    if (pistolTextureLoaded) {
      pistolModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].texture =
          pistolTexture;
    }
  }

  for (int i = 0; i < shotgunModel.materialCount; ++i) {
    shotgunModel.materials[i].shader = viewmodelShader;
    shotgunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].color = WHITE;

    if (shotgunTextureLoaded) {
      shotgunModel.materials[i].maps[MATERIAL_MAP_DIFFUSE].texture =
          shotgunTexture;
    }
  }

  loaded = true;
}

void AssetManager::unload() {
  if (!loaded) {
    return;
  }

  UnloadModel(pistolModel);
  UnloadModel(shotgunModel);
  UnloadShader(monoShader);
  UnloadShader(viewmodelShader);
  UnloadShader(worldLitShader);
  UnloadTexture(muzzleFlashTexture);
  if (pistolTextureLoaded) {
    UnloadTexture(pistolTexture);
    pistolTextureLoaded = false;
  }
  if (shotgunTextureLoaded) {
    UnloadTexture(shotgunTexture);
    shotgunTextureLoaded = false;
  }
  if (skyboxLoaded) {
    UnloadShader(skyboxShader);
    UnloadTexture(skyboxCubemap);
    UnloadModel(skyboxModel);
    skyboxLoaded = false;
  }

  loaded = false;
}

const Model &AssetManager::getWeaponModel(WeaponModelId modelId) const {
  switch (modelId) {
  case WeaponModelId::Shotgun:
    return shotgunModel;
  case WeaponModelId::Pistol:
  default:
    return pistolModel;
  }
}

Shader AssetManager::getMonoShader() const { return monoShader; }

Shader AssetManager::getViewmodelShader() const { return viewmodelShader; }

Shader AssetManager::getWorldLitShader() const { return worldLitShader; }

const Texture2D &AssetManager::getMuzzleFlashTexture() const {
  return muzzleFlashTexture;
}

const Model &AssetManager::getSkyboxModel() const { return skyboxModel; }

bool AssetManager::hasSkybox() const { return skyboxLoaded; }
