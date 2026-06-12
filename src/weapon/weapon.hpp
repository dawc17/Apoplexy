#pragma once

#include "../assets/assetmanager.hpp"
#include "../audio/audiosystem.hpp"
#include "../render/lighting.hpp"
#include "../viewmodel/proceduralweaponanimation.hpp"
#include "../viewmodel/viewmodel.hpp"
#include "weapondata.hpp"

#include "raylib.h"

#include <vector>

struct DebugShotRay {
  Vector3 start{};
  Vector3 end{};
  bool hit = false;
  float timer = 0.0f;
};

class Player;
class Level;
class Enemy;
class AssetManager;
class ParticleSystem;

class Weapon {
public:
  Weapon(const WeaponData &data,
         const ProceduralWeaponAnimationData &proceduralAnimation);

  void reset();

  void update(float dt, const Player &player, std::vector<Enemy> &enemies,
              const Level &level, const Camera3D camera,
              ParticleSystem &particles, AudioSystem &audio);

  void drawViewModel(const Camera3D &camera, const AssetManager &assets,
                     const Lighting::SceneLighting &lighting,
                     Vector3 pointLightContribution) const;
  void drawDebugRays() const;

  const WeaponData &getData() const;

  int getAmmoInMagazine() const;
  int getReserveAmmo() const;
  int getMagazineSize() const;
  bool isReloading() const;
  float getReloadProgress() const;
  float getCurrentSpreadDegrees(const Player &player) const;

  bool consumeShotFired();

  static bool debugRaysEnabled;

private:
  void tryShoot(const Player &player, std::vector<Enemy> &enemies,
                const Level &level, const Camera3D &camera,
                ParticleSystem &particles, AudioSystem &audio);

  bool startReload();
  void finishReload(AudioSystem &audio);

  Ray makeShootRay(const Camera3D &camera, float spreadDegrees) const;
  void firePelletRay(Ray ray, std::vector<Enemy> &enemies, const Level &level,
                     ParticleSystem &particles, AudioSystem &audio);
  void addDebugRay(Ray ray, float distance, bool hit);

private:
  const WeaponData *data = nullptr;
  const ProceduralWeaponAnimationData *proceduralAnimation = nullptr;

  Viewmodel viewmodel;

  int ammoInMagazine = 0;
  int reserveAmmo = 0;

  float cooldown = 0.0f;
  float reloadTimer = 0.0f;
  float muzzleFlashTimer = 0.0f;
  float muzzleFlashRotation = 0.0f;
  float spreadBloom = 0.0f;
  bool reloading = false;
  bool shotFired = false;
  std::vector<DebugShotRay> debugRays;
};
