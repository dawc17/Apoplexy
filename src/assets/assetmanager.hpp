#pragma once

#include "raylib.h"

class AssetManager {
public:
  void load();
  void unload();

  const Model &getGunModel() const;
  Shader getMonoShader() const;
  Shader getViewmodelShader() const;
  const Texture2D &getMuzzleFlashTexture() const;

private:
  Model gunModel{};
  Shader monoShader{};
  Shader viewmodelShader{};
  Texture2D muzzleFlashTexture{};
  bool loaded = false;
};
