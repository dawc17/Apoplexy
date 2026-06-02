#include "collision.hpp"

#include "../enemy/enemy.hpp"
#include "../level/level.hpp"

#include "raymath.h"

#include <cfloat>
#include <raylib.h>

namespace {
bool sphereAabbXZ(Vector3 position, float radius, BoundingBox box) {
  float closestX = Clamp(position.x, box.min.x, box.max.x);
  float closestZ = Clamp(position.z, box.min.z, box.max.z);

  float dx = position.x - closestX;
  float dz = position.z - closestZ;

  // seriously, what even is math?
  return dx * dx + dz * dz < radius * radius;
}
} // namespace

namespace Collision {
bool sphereLevel(Vector3 position, float radius, const Level &level) {
  for (const Wall &wall : level.getWalls()) {
    if (sphereAabbXZ(position, radius, wall.bounds)) {
      return true;
    }
  }

  return false;
}

Vector3 moveSphereLevel(Vector3 position, Vector3 velocity, float radius,
                        const Level &level, float dt) {
  Vector3 next = position;

  next.x += velocity.x * dt;
  if (sphereLevel(next, radius, level)) {
    next.x = position.x;
  }

  next.z += velocity.z * dt;
  if (sphereLevel(next, radius, level)) {
    next.z = position.z;
  }

  return next;
}

bool rayEnemies(Ray ray, std::vector<Enemy> &enemies, int &hitEnemyIndex,
                Vector3 &hitPoint) {
  hitEnemyIndex = -1;
  float closestDistance = FLT_MAX;

  for (int i = 0; i < static_cast<int>(enemies.size()); ++i) {
    if (!enemies[i].isAlive()) {
      continue;
    }

    RayCollision hit = GetRayCollisionBox(ray, enemies[i].getHitbox());

    if (hit.hit && hit.distance < closestDistance) {
      closestDistance = hit.distance;
      hitEnemyIndex = i;
      hitPoint = hit.point;
    }
  }

  return hitEnemyIndex >= 0;
}

bool rayLevel(Ray ray, const Level &level, Vector3 &hitPoint,
              float &hitDistance) {
  bool didHit = false;
  float closestDistance = FLT_MAX;

  for (const Wall &wall : level.getWalls()) {
    RayCollision collision = GetRayCollisionBox(ray, wall.bounds);

    if (!collision.hit) {
      continue;
    }

    if (collision.distance < 0.0f) {
      continue;
    }

    if (collision.distance >= closestDistance) {
      continue;
    }

    didHit = true;
    closestDistance = collision.distance;
    hitPoint = collision.point;
  }

  hitDistance = closestDistance;
  return didHit;
}
} // namespace Collision
