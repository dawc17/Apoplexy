#include "weapon.hpp"

#include "../collision/collision.hpp"
#include "../enemy/enemy.hpp"
#include "../player/player.hpp"

#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <vector>

Weapon::Weapon() { reset(); }

void Weapon::reset() {
  cooldown = 0.0f;
  recoil = 0.0f;
  muzzleFlashTimer = 0.0f;
}

void Weapon::update(float dt, const Player &player, std::vector<Enemy> &enemies,
                    const Camera3D camera) {
  // the documentation for std::max() is amazing. "this does what you think it
  // does" - cpp docs, nineteen ninety-unc
  cooldown = std::max(0.0f, cooldown - dt);
  recoil = std::max(0.0f, recoil - dt * 8.0f);
  muzzleFlashTimer = std::max(0.0f, muzzleFlashTimer - dt);

  if (IsMouseButtonDown(MOUSE_BUTTON_LEFT) && cooldown <= 0.0f) {
    tryShoot(player, enemies, camera);
  }
}

void Weapon::drawViewModel() const {
  int screenWidth = GetScreenWidth();
  int screenHeight = GetScreenHeight();

  float kick = recoil * 24.0f;

  // gun model here some day

  if (muzzleFlashTimer > 0.0f) {
    // muzzle flash
  }
}

void Weapon::tryShoot(const Player &, std::vector<Enemy> &enemies,
                      const Camera3D &camera) {
  cooldown = 1.0f / fireRate;
  recoil = 1.0f;
  muzzleFlashTimer = 0.05f;

  int hitEnemyIndex = -1;
  Vector3 hitPoint{};

  if (Collision::rayEnemies(makeShootRay(camera), enemies, hitEnemyIndex,
                            hitPoint)) {
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
