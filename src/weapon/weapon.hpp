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
  explicit Weapon(const WeaponData &data);

  void reset();

  void update(float dt, const Player &player, std::vector<Enemy> &enemies,
              const Level &level, const Camera3D camera,
              ParticleSystem &particles);

  void drawViewModel(const Camera3D &camera, const AssetManager &assets) const;

  const WeaponData &getData() const;

  int getAmmoInMagazine() const;
  int getReserveAmmo() const;
  int getMagazineSize() const;
  bool isReloading() const;
  float getReloadProgress() const;

  bool consumeShotFired();

private:
  void tryShoot(const Player &player, std::vector<Enemy> &enemies,
                const Level &level, const Camera3D &camera,
                ParticleSystem &particles);

  void startReload();
  void finishReload();

  Ray makeShootRay(const Camera3D &camera) const;

private:
  const WeaponData *data = nullptr;

  Viewmodel viewmodel;

  int ammoInMagazine = 0;
  int reserveAmmo = 0;

  float cooldown = 0.0f;
  float reloadTimer = 0.0f;
  float muzzleFlashTimer = 0.0f;
  float muzzleFlashRotation = 0.0f;
  bool reloading = false;
  bool shotFired = false;
};
