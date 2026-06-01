#pragma once

#include "raylib.h"

class AssetManager {
public:
  void load();
  void unload();

  Texture2D &getEnemyTexture();
  Texture2D &getWeaponTexture();

  Sound &getShootSound();
  Sound &getHitSound();

private:
  Texture2D enemyTexture();
  Texture2D weaponTexture();

  Sound shootSound();
  Sound hitSound();

  bool loaded = false;
};
