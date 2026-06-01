#pragma once

#include "raylib.h"

class AssetManager {
public:
  void load();
  void unload();

  const Model &getGunModel() const;
  Shader getMonoShader() const;

private:
  Model gunModel{};
  Shader monoShader{};
  bool loaded = false;
};
