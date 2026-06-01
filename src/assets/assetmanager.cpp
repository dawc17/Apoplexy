#include "assetmanager.hpp"
#include <raylib.h>

void AssetManager::load() { loaded = true; }

void AssetManager::unload() { loaded = false; }

Texture2D &AssetManager::getEnemyTexture() {
  static Texture2D texture{};
  return texture;
}

Texture2D &AssetManager::getWeaponTexture() {
  static Texture2D texture{};
  return texture;
}

Sound &AssetManager::getShootSound() {
  static Sound sound{};
  return sound;
}

Sound &AssetManager::getHitSound() {
  static Sound sound{};
  return sound;
}
