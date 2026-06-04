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
  // const Texture2D &flash = assets.getMuzzleFlashTexture();

  Camera3D viewCamera{};
  viewCamera.position = {0.0f, 0.0f, 0.0f};
  viewCamera.target = {0.0f, 0.0f, 1.0f};
  viewCamera.up = {0.0f, 1.0f, 0.0f};
  viewCamera.fovy = 70;
  viewCamera.projection = CAMERA_PERSPECTIVE;

  float kick = recoil * 0.06f;
  Vector3 position = {-0.20f, -0.10f - recoil * 0.025f, 0.31f - kick};

  // gun camera
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
  rlEnableDepthTest();

  // my muzzle flashes a lot what about yours
  // if (muzzleFlashTimer > 0.0f) {
  //
  //   float t = muzzleFlashTimer / 0.05f;
  //
  //   float screenW = static_cast<float>(GetScreenWidth());
  //   float screenH = static_cast<float>(GetScreenHeight());
  //
  //   Vector2 center{screenW * 0.515f, screenH * 0.615f + recoil * 18.0f};
  //
  //   float size = 110.0f * t;
  //
  //   float width = 180.0f * t;
  //   float height = 100.0f * t;
  //
  //   Rectangle source{0.0f, 0.0f, static_cast<float>(flash.width),
  //                    static_cast<float>(flash.height)};
  //
  //   Rectangle dest{center.x, center.y, width, height};
  //
  //   Vector2 origin{size * 0.5f, size * 0.5f};
  //
  //   DrawTexturePro(flash, source, dest, origin, muzzleFlashRotation, WHITE);
  // }
  // DrawText(TextFormat("Flash texture: %d x %d", flash.width, flash.height),
  // 20,
  //          200, 24, RED);
  // DrawTextureEx(flash, {20.0f, 150.0f}, 0.0f, 2.0f, WHITE);
}

void Weapon::tryShoot(const Player &, std::vector<Enemy> &enemies,
                      const Level &level, const Camera3D &camera) {
  cooldown = 1.0f / fireRate;
  recoil = 1.0f;
  muzzleFlashTimer = 0.05f;
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
