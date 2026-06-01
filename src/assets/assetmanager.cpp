#include "assetmanager.hpp"

void AssetManager::load() {
  if (loaded) {
    return;
  }

  gunModel = LoadModel("models/gun.glb");
  monoShader = LoadShader(0, "shaders/monochrome.fs");

  const Color gunColors[] = {
      {28, 30, 32, 255},
      {54, 57, 60, 255},
      {16, 17, 18, 255},
      {88, 84, 78, 255},
      {120, 118, 110, 255},
      {8, 8, 9, 255},
  };

  for (int i = 0; i < gunModel.materialCount; ++i) {
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

  loaded = false;
}

const Model &AssetManager::getGunModel() const { return gunModel; }

Shader AssetManager::getMonoShader() const { return monoShader; }
