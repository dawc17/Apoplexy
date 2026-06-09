#pragma once

#include "raylib.h"

class Level;
class Enemy;

#include <vector>

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

bool rayEnemies(Ray ray, std::vector<Enemy> &enemies, int &hitEnemyIndex,
                Vector3 &hitPoint);

bool rayLevel(Ray ray, const Level &level, Vector3 &hitPoint,
              float &hitDistance);
} // namespace Collision
