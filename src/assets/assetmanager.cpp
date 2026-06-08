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
  muzzleFlashTexture = LoadTexture("textures/muzzleflash.png");

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
  UnloadTexture(muzzleFlashTexture);

  loaded = false;
}

const Model &AssetManager::getGunModel() const { return gunModel; }

Shader AssetManager::getMonoShader() const { return monoShader; }

Shader AssetManager::getViewmodelShader() const { return viewmodelShader; }

const Texture2D &AssetManager::getMuzzleFlashTexture() const {
  return muzzleFlashTexture;
}
