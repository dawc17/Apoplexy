#include "collision.hpp"

#include "../enemy/enemy.hpp"
#include "../level/level.hpp"

#include "raymath.h"

#include <limits>
#include <raylib.h>

namespace {
constexpr float GROUND_Y = 0.0f;
constexpr float SURFACE_EPSILON = 0.001f;

bool cylinderAabb(Vector3 position, float radius, float height,
                  BoundingBox box) {
  float bodyMinY = position.y;
  float bodyMaxY = position.y + height;

  if (bodyMaxY <= box.min.y || bodyMinY >= box.max.y) {
    return false;
  }

  float closestX = Clamp(position.x, box.min.x, box.max.x);
  float closestZ = Clamp(position.z, box.min.z, box.max.z);

  float dx = position.x - closestX;
  float dz = position.z - closestZ;

  // seriously, what even is math?
  return dx * dx + dz * dz < radius * radius;
}

bool cylinderOverlapsAabbXZ(Vector3 position, float radius, BoundingBox box) {
  float closestX = Clamp(position.x, box.min.x, box.max.x);
  float closestZ = Clamp(position.z, box.min.z, box.max.z);

  float dx = position.x - closestX;
  float dz = position.z - closestZ;

  return dx * dx + dz * dz < radius * radius;
}
} // namespace

namespace Collision {
bool cylinderLevel(Vector3 position, float radius, float height,
                   const Level &level) {
  for (const Wall &wall : level.getWalls()) {
    if (cylinderAabb(position, radius, height, wall.bounds)) {
      return true;
    }
  }

  return false;
}

MoveResult moveCylinderLevel(Vector3 position, Vector3 velocity, float radius,
                             float height, const Level &level, float dt) {
  Vector3 next = position;
  Vector3 nextVelocity = velocity;
  bool grounded = false;

  next.x += velocity.x * dt;
  if (cylinderLevel(next, radius, height, level)) {
    next.x = position.x;
    nextVelocity.x = 0.0f;
  }

  next.z += velocity.z * dt;
  if (cylinderLevel(next, radius, height, level)) {
    next.z = position.z;
    nextVelocity.z = 0.0f;
  }

  next.y += velocity.y * dt;

  if (next.y <= GROUND_Y) {
    next.y = GROUND_Y;
    nextVelocity.y = 0.0f;
    grounded = true;
  }

  for (const Wall &wall : level.getWalls()) {
    if (!cylinderOverlapsAabbXZ(next, radius, wall.bounds)) {
      continue;
    }

    float previousBottom = position.y;
    float previousTop = position.y + height;
    float nextBottom = next.y;
    float nextTop = next.y + height;

    if (velocity.y <= 0.0f && previousBottom >= wall.bounds.max.y &&
        nextBottom <= wall.bounds.max.y + SURFACE_EPSILON) {
      next.y = wall.bounds.max.y + SURFACE_EPSILON;
      nextVelocity.y = 0.0f;
      grounded = true;
      continue;
    }

    if (velocity.y > 0.0f && previousTop <= wall.bounds.min.y &&
        nextTop >= wall.bounds.min.y) {
      next.y = wall.bounds.min.y - height - SURFACE_EPSILON;
      nextVelocity.y = 0.0f;
    }
  }

  return {next, nextVelocity, grounded};
}

std::optional<EnemyHit> rayEnemies(Ray ray, std::span<Enemy> enemies) {
  std::optional<EnemyHit> closestHit;
  float closestDistance = std::numeric_limits<float>::max();

  for (int i = 0; i < static_cast<int>(enemies.size()); ++i) {
    if (!enemies[i].isAlive()) {
      continue;
    }

    RayCollision hit = GetRayCollisionBox(ray, enemies[i].getHitbox());

    if (hit.hit && hit.distance < closestDistance) {
      closestDistance = hit.distance;
      closestHit = EnemyHit{i, hit.point, hit.distance};
    }
  }

  return closestHit;
}

std::optional<LevelHit> rayLevel(Ray ray, const Level &level) {
  std::optional<LevelHit> closestHit;
  float closestDistance = std::numeric_limits<float>::max();

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

    closestDistance = collision.distance;
    closestHit = LevelHit{collision.point, collision.distance};
  }

  return closestHit;
}
} // namespace Collision
