#pragma once

#include "../weapon/weapondata.hpp"
#include "raylib.h"

class AssetManager {
public:
  void load();
  void unload();

  const Model &getWeaponModel(WeaponModelId modelId) const;
  Shader getMonoShader() const;
  Shader getViewmodelShader() const;
  Shader getWorldLitShader() const;
  const Texture2D &getMuzzleFlashTexture() const;
  const Model &getSkyboxModel() const;
  bool hasSkybox() const;

private:
  Model pistolModel{};
  Model shotgunModel{};
  Model skyboxModel{};
  Shader monoShader{};
  Shader viewmodelShader{};
  Shader worldLitShader{};
  Shader skyboxShader{};
  Texture2D pistolTexture{};
  Texture2D shotgunTexture{};
  Texture2D muzzleFlashTexture{};
  TextureCubemap skyboxCubemap{};
  bool pistolTextureLoaded = false;
  bool shotgunTextureLoaded = false;
  bool skyboxLoaded = false;
  bool loaded = false;
};
