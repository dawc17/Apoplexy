#include "weapon.hpp"

#include "../collision/collision.hpp"
#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"

#include "raylib.h"
#include "raymath.h"
#include "rlgl.h"

#include <algorithm>
#include <cfloat>
#include <iostream>
#include <vector>

Weapon::Weapon() { reset(); }

void Weapon::reset() {
  cooldown = 0.0f;
  recoil = 0.0f;
  muzzleFlashTimer = 0.0f;
  muzzleFlashRotation = 0.0f;
}

void Weapon::update(float dt, const Player &player, std::vector<Enemy> &enemies,
                    const Level &level, const Camera3D camera,
                    ParticleSystem &particles) {
  // the documentation for std::max() is amazing. "this does what you think it
  // does" - cpp docs, nineteen ninety-unc
  cooldown = std::max(0.0f, cooldown - dt);
  recoil = std::max(0.0f, recoil - dt * 8.0f);
  muzzleFlashTimer = std::max(0.0f, muzzleFlashTimer - dt);

  if (IsMouseButtonDown(MOUSE_BUTTON_LEFT) && cooldown <= 0.0f) {
    tryShoot(player, enemies, level, camera, particles);
  }
}

void Weapon::drawViewModel(const Camera3D &camera,
                           const AssetManager &assets) const {
  const Model &gun = assets.getGunModel();
  const Texture2D &flash = assets.getMuzzleFlashTexture();

  Camera3D viewCamera{};
  viewCamera.position = {0.0f, 0.0f, 0.0f};
  viewCamera.target = {0.0f, 0.0f, 1.0f};
  viewCamera.up = {0.0f, 1.0f, 0.0f};
  viewCamera.fovy = 70;
  viewCamera.projection = CAMERA_PERSPECTIVE;

  float kick = recoil * 0.06f;
  Vector3 position = {-0.20f, -0.10f - recoil * 0.025f, 0.31f - kick};

  // gun camera
  rlDrawRenderBatchActive();
  BeginMode3D(viewCamera);
  rlDrawRenderBatchActive();
  rlDisableDepthTest();
  rlPushMatrix();
  rlTranslatef(position.x, position.y, position.z);
  rlRotatef(-recoil * 14.0f, 1.0f, 0.0f, 0.0f);
  rlRotatef(-90.0f, 0.0f, 1.0f, 0.0f);
  rlRotatef(-6.0f, 1.0f, 0.0f, 0.0f);
  rlRotatef(-4.0f, 0.0f, 0.0f, 1.0f);
  DrawModel(gun, {0.0f, 0.0f, 0.0f}, 0.80f, WHITE);

  if (muzzleFlashTimer > 0.0f) {
    float t = muzzleFlashTimer / 0.05f;

    Rectangle source{0.0f, 0.0f, static_cast<float>(flash.width),
                     static_cast<float>(flash.height)};

    Vector2 size{muzzleFlashWidth * t, muzzleFlashHeight * t};

    Vector2 origin{size.x * 0.5f, size.y * 0.5f};

    BeginBlendMode(BLEND_ADDITIVE);
    DrawBillboardPro(viewCamera, flash, source, muzzlePoint, {0.0f, 1.0f, 0.0f},
                     size, origin, muzzleFlashRotation, Fade(WHITE, 0.95f * t));
    EndBlendMode();
  }

  rlPopMatrix();
  EndMode3D();
  rlEnableDepthTest();
}

void Weapon::tryShoot(const Player &, std::vector<Enemy> &enemies,
                      const Level &level, const Camera3D &camera,
                      ParticleSystem &particles) {
  cooldown = 1.0f / fireRate;
  recoil = 1.0f;
  muzzleFlashTimer = 0.025f;
  muzzleFlashRotation = static_cast<float>(GetRandomValue(-25, 25));

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
  if (enemyHitDistance > range) {
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
    Vector3 hitNormal = Vector3Normalize(Vector3Subtract(
        enemyHitPoint, enemies[hitEnemyIndex].getPosition()));
    particles.spawnEnemyHit(enemyHitPoint, hitNormal);
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
