#pragma once

#include "raylib.h"

class AssetManager {
public:
  void load();
  void unload();

  const Model &getGunModel() const;
  Shader getMonoShader() const;
  Shader getViewmodelShader() const;
  Shader getWorldLitShader() const;
  const Texture2D &getMuzzleFlashTexture() const;
  const Model &getSkyboxModel() const;
  bool hasSkybox() const;

private:
  Model gunModel{};
  Model skyboxModel{};
  Shader monoShader{};
  Shader viewmodelShader{};
  Shader worldLitShader{};
  Shader skyboxShader{};
  Texture2D gunTexture{};
  Texture2D muzzleFlashTexture{};
  TextureCubemap skyboxCubemap{};
  bool gunTextureLoaded = false;
  bool skyboxLoaded = false;
  bool loaded = false;
};
