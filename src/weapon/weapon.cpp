#include "weapon.hpp"

#include "../collision/collision.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"

#include "raylib.h"
#include "raymath.h"
#include "rlgl.h"

#include <algorithm>
#include <cfloat>
#include <cmath>
#include <vector>

Weapon::Weapon() { reset(); }

void Weapon::reset() {
  cooldown = 0.0f;
  recoil = 0.0f;
  muzzleFlashTimer = 0.0f;
}

void Weapon::update(float dt, const Player &player, std::vector<Enemy> &enemies,
                    const Level &level, const Camera3D camera) {
  // the documentation for std::max() is amazing. "this does what you think it
  // does" - cpp docs, nineteen ninety-unc
  cooldown = std::max(0.0f, cooldown - dt);
  recoil = std::max(0.0f, recoil - dt * 8.0f);
  muzzleFlashTimer = std::max(0.0f, muzzleFlashTimer - dt);

  if (IsMouseButtonDown(MOUSE_BUTTON_LEFT) && cooldown <= 0.0f) {
    tryShoot(player, enemies, level, camera);
  }
}

void Weapon::drawViewModel(const Camera3D &camera,
                           const AssetManager &assets) const {
  const Model &gun = assets.getGunModel();

  Camera3D viewCamera{};
  viewCamera.position = {0.0f, 0.0f, 0.0f};
  viewCamera.target = {0.0f, 0.0f, 1.0f};
  viewCamera.up = {0.0f, 1.0f, 0.0f};
  viewCamera.fovy = 70;
  viewCamera.projection = CAMERA_PERSPECTIVE;

  float kick = recoil * 0.06f;
  Vector3 position = {-0.20f, -0.10f - recoil * 0.025f, 0.31f - kick};

  rlDisableDepthTest();
  BeginMode3D(viewCamera);
  rlPushMatrix();
  rlTranslatef(position.x, position.y, position.z);
  rlRotatef(-recoil * 14.0f, 1.0f, 0.0f, 0.0f);
  rlRotatef(-90.0f, 0.0f, 1.0f, 0.0f);
  rlRotatef(-6.0f, 1.0f, 0.0f, 0.0f);
  rlRotatef(-4.0f, 0.0f, 0.0f, 1.0f);
  DrawModel(gun, {0.0f, 0.0f, 0.0f}, 0.80f, WHITE);
  rlPopMatrix();
  EndMode3D();
  rlDisableDepthTest();
}

void Weapon::tryShoot(const Player &, std::vector<Enemy> &enemies,
                      const Level &level, const Camera3D &camera) {
  cooldown = 1.0f / fireRate;
  recoil = 1.0f;
  muzzleFlashTimer = 0.05f;

  Ray ray = makeShootRay(camera);
  int hitEnemyIndex = -1;
  Vector3 enemyHitPoint{};
  float enemyHitDistance = FLT_MAX;

  if (!Collision::rayEnemies(ray, enemies, hitEnemyIndex, enemyHitPoint)) {
    return;
  }

  enemyHitDistance = Vector3Distance(ray.position, enemyHitPoint);
  if (enemyHitDistance > range) {
    return;
  }

  Vector3 levelHitPoint{};
  float levelHitDistance = FLT_MAX;
  if (Collision::rayLevel(ray, level, levelHitPoint, levelHitDistance) &&
      levelHitDistance < enemyHitDistance) {
    return;
  }

  if (hitEnemyIndex >= 0) {
    enemies[hitEnemyIndex].applyDamage(damage);
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
