#pragma once

#include "raylib.h"

class Level;
class Enemy;

#include <vector>

namespace Collision {
bool sphereLevel(Vector3 position, float radius, const Level &level);

Vector3 moveSphereLevel(Vector3 position, Vector3 velocity, float radius,
                        const Level &level, float dt);

bool rayEnemies(Ray ray, std::vector<Enemy> &enemies, int &hitEnemyIndex,
                Vector3 &hitPoint);
} // namespace Collision
