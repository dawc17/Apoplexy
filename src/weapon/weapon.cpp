#include "weapon.hpp"

#include "../collision/collision.hpp"
#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"

#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cfloat>
#include <iostream>
#include <vector>

Weapon::Weapon(const WeaponData &weaponData,
               const ProceduralWeaponAnimationData &weaponProceduralAnimation)
    : data(&weaponData), proceduralAnimation(&weaponProceduralAnimation) {
  reset();
}

void Weapon::reset() {
  ammoInMagazine = data->ammo.magazineSize;
  reserveAmmo = data->ammo.maxReserveAmmo;

  cooldown = 0.0f;
  reloadTimer = 0.0f;
  muzzleFlashTimer = 0.0f;
  muzzleFlashRotation = 0.0f;

  reloading = false;
  shotFired = false;

  viewmodel.reset();
}

void Weapon::update(float dt, const Player &player, std::vector<Enemy> &enemies,
                    const Level &level, const Camera3D camera,
                    ParticleSystem &particles) {
  cooldown = std::max(0.0f, cooldown - dt);
  muzzleFlashTimer = std::max(0.0f, muzzleFlashTimer - dt);

  if (reloading) {
    reloadTimer = std::max(0.0f, reloadTimer - dt);

    if (reloadTimer <= 0.0f) {
      finishReload();
    }
  }

  viewmodel.update(dt, player.isSprinting(), *proceduralAnimation);

  if (IsKeyPressed(KEY_R)) {
    startReload();
  }

  bool wantsToFire = data->fire.automatic
                         ? IsMouseButtonDown(MOUSE_BUTTON_LEFT)
                         : IsMouseButtonPressed(MOUSE_BUTTON_LEFT);

  if (wantsToFire && cooldown <= 0.0f) {
    tryShoot(player, enemies, level, camera, particles);
  }

  if (!reloading && data->ammo.autoReloadWhenEmpty && ammoInMagazine <= 0 &&
      reserveAmmo > 0) {
    startReload();
  }
}

void Weapon::drawViewModel(const Camera3D &camera,
                           const AssetManager &assets) const {
  viewmodel.draw(camera, *data, *proceduralAnimation, assets, muzzleFlashTimer,
                 muzzleFlashRotation);
}

const WeaponData &Weapon::getData() const { return *data; }

int Weapon::getAmmoInMagazine() const { return ammoInMagazine; }

int Weapon::getReserveAmmo() const { return reserveAmmo; }

int Weapon::getMagazineSize() const { return data->ammo.magazineSize; }

bool Weapon::isReloading() const { return reloading; }

float Weapon::getReloadProgress() const {
  if (!reloading || data->ammo.reloadDuration <= 0.0f) {
    return 0.0f;
  }

  return 1.0f - reloadTimer / data->ammo.reloadDuration;
}

void Weapon::tryShoot(const Player &, std::vector<Enemy> &enemies,
                      const Level &level, const Camera3D &camera,
                      ParticleSystem &particles) {
  if (reloading) {
    return;
  }

  if (ammoInMagazine <= 0) {
    startReload();
    return;
  }

  --ammoInMagazine;

  cooldown = 1.0f / data->fire.fireRate;
  muzzleFlashTimer = 0.025f;
  muzzleFlashRotation = static_cast<float>(GetRandomValue(-25, 25));
  shotFired = true;
  viewmodel.addRecoil(1.0f);

  Ray ray = makeShootRay(camera);
  int hitEnemyIndex = -1;
  Vector3 enemyHitPoint{};
  Vector3 levelHitPoint{};
  float enemyHitDistance = FLT_MAX;
  float levelHitDistance = FLT_MAX;

  if (!Collision::rayEnemies(ray, enemies, hitEnemyIndex, enemyHitPoint)) {
    std::cout << "Hit something else than an enemy" << std::endl;
    return;
  }

  enemyHitDistance = Vector3Distance(ray.position, enemyHitPoint);
  if (enemyHitDistance > data->fire.range) {
    return;
  }

  if (Collision::rayLevel(ray, level, levelHitPoint, levelHitDistance) &&
      levelHitDistance < enemyHitDistance) {
    std::cout << "Hello";
    return;
  }

  if (hitEnemyIndex >= 0) {
    std::cout << "Hit enemy at index: " << hitEnemyIndex << " at a distance of "
              << enemyHitDistance << std::endl;

    Vector3 hitNormal = Vector3Normalize(
        Vector3Subtract(enemyHitPoint, enemies[hitEnemyIndex].getPosition()));

    particles.spawnEnemyHit(enemyHitPoint, hitNormal,
                            enemies[hitEnemyIndex].getVelocity());

    Vector3 enemyPosition = enemies[hitEnemyIndex].getPosition();
    Vector3 enemyVelocity = enemies[hitEnemyIndex].getVelocity();

    if (enemies[hitEnemyIndex].applyDamage(data->fire.damage)) {
      particles.spawnEnemyDeath(
          {enemyPosition.x, enemyPosition.y + 0.75f, enemyPosition.z},
          enemyVelocity);
    }
  }
}

void Weapon::startReload() {
  if (reloading) {
    return;
  }

  if (ammoInMagazine >= data->ammo.magazineSize) {
    return;
  }

  if (reserveAmmo <= 0) {
    return;
  }

  reloading = true;
  reloadTimer = data->ammo.reloadDuration;
}

void Weapon::finishReload() {
  int neededAmmo = data->ammo.magazineSize - ammoInMagazine;
  int ammoToLoad = std::min(neededAmmo, reserveAmmo);

  ammoInMagazine += ammoToLoad;
  reserveAmmo -= ammoToLoad;

  reloading = false;
  reloadTimer = 0.0f;
}

Ray Weapon::makeShootRay(const Camera3D &camera) const {
  Vector3 direction =
      Vector3Normalize(Vector3Subtract(camera.target, camera.position));

  Ray ray{};
  ray.position = camera.position;
  ray.direction = direction;
  return ray;
}

bool Weapon::consumeShotFired() {
  bool fired = shotFired;
  shotFired = false;
  return fired;
}
