#pragma once

#include "../assets/assetmanager.hpp"
#include "../viewmodel/viewmodel.hpp"
#include "weapondata.hpp"

#include "raylib.h"

#include <vector>

class Player;
class Level;
class Enemy;
class AssetManager;
class ParticleSystem;

class Weapon {
public:
  Weapon();

  void reset();

  void update(float dt, const Player &player, std::vector<Enemy> &enemies,
              const Level &level, const Camera3D camera,
              ParticleSystem &particles);

  void drawViewModel(const Camera3D &camera, const AssetManager &assets) const;
  bool consumeShotFired();

private:
  void tryShoot(const Player &player, std::vector<Enemy> &enemies,
                const Level &level, const Camera3D &camera,
                ParticleSystem &particles);

  Ray makeShootRay(const Camera3D &camera) const;

private:
  WeaponData pistol{
      "Pistol",
      {-0.20f, -0.10f, 0.31f},
      {-6.0f, -90.0f, -4.0f},
      0.80f,
      {0.615f, -0.004f, 0.175f},
      1.478f,
      0.657f,
      15,
      100.0f,
      6.0f,
      0.06f,
      20.0f,
  };

  Viewmodel viewmodel;

  float cooldown = 0.0f;
  float muzzleFlashTimer = 0.0f;
  float muzzleFlashRotation = 0.0f;
  bool shotFired = false;
};
