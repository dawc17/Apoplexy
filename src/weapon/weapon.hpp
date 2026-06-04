#pragma once

#include "../assets/assetmanager.hpp"
#include "raylib.h"

#include <vector>

class Player;
class Level;
class Enemy;
class AssetManager;

class Weapon {
public:
  Weapon();

  void reset();

  void update(float dt, const Player &player, std::vector<Enemy> &enemies,
              const Level &level, const Camera3D camera);

  void drawViewModel(const Camera3D &camera, const AssetManager &assets) const;

private:
  void tryShoot(const Player &player, std::vector<Enemy> &enemies,
                const Level &level, const Camera3D &camera);
  Ray makeShootRay(const Camera3D &camera) const;

private:
  int damage = 15;
  float range = 100.0f;
  float fireRate = 6.0f;
  float cooldown = 0.0f;

  float recoil = 0.0f;
  float muzzleFlashTimer = 0.0f;
  float muzzleFlashRotation = 0.0f;
};
