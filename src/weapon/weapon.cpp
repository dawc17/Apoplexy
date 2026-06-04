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

Weapon::Weapon() { reset(); }

void Weapon::reset() {
  cooldown = 0.0f;
  muzzleFlashTimer = 0.0f;
  muzzleFlashRotation = 0.0f;
  shotFired = false;
  viewmodel.reset();
}

void Weapon::update(float dt, const Player &player, std::vector<Enemy> &enemies,
                    const Level &level, const Camera3D camera,
                    ParticleSystem &particles) {
  cooldown = std::max(0.0f, cooldown - dt);
  muzzleFlashTimer = std::max(0.0f, muzzleFlashTimer - dt);
  viewmodel.update(dt);

  if (IsMouseButtonDown(MOUSE_BUTTON_LEFT) && cooldown <= 0.0f) {
    tryShoot(player, enemies, level, camera, particles);
  }
}

void Weapon::drawViewModel(const Camera3D &camera,
                           const AssetManager &assets) const {
  viewmodel.draw(camera, pistol, assets, muzzleFlashTimer, muzzleFlashRotation);
}

void Weapon::tryShoot(const Player &, std::vector<Enemy> &enemies,
                      const Level &level, const Camera3D &camera,
                      ParticleSystem &particles) {
  cooldown = 1.0f / pistol.fireRate;
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
  if (enemyHitDistance > pistol.range) {
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

    enemies[hitEnemyIndex].applyDamage(pistol.damage);
  }
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
