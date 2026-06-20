#pragma once

#include "raylib.h"
#include <optional>
#include <span>

class Level;
class Enemy;

namespace Collision {
struct MoveResult {
  Vector3 position{};
  Vector3 velocity{};
  bool grounded = false;
};

bool cylinderLevel(Vector3 position, float radius, float height,
                   const Level &level);

MoveResult moveCylinderLevel(Vector3 position, Vector3 velocity, float radius,
                             float height, const Level &level, float dt);

struct EnemyHit {
  int enemyIndex = -1;
  Vector3 point{};
  float distance = 0.0f;
};

struct LevelHit {
  Vector3 point{};
  float distance = 0.0f;
};

std::optional<EnemyHit> rayEnemies(Ray ray, std::span<Enemy> enemies);
std::optional<LevelHit> rayLevel(Ray ray, const Level &level);

} // namespace Collision
