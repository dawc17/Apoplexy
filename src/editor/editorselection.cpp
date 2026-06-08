#include "editorselection.hpp"

#include "../level/level.hpp"

#include "raylib.h"

#include <cfloat>
#include <vector>

void EditorSelection::clear() {
  type = EditorSelectionType::None;
  selectedIndex = -1;
}

bool EditorSelection::hasWall() const {
  return type == EditorSelectionType::Wall && selectedIndex >= 0;
}

int EditorSelection::getWallIndex() const { return selectedIndex; }

bool EditorSelection::hasEnemySpawn() const {
  return type == EditorSelectionType::EnemySpawn && selectedIndex >= 0;
}

int EditorSelection::getEnemySpawnIndex() const { return selectedIndex; }

bool EditorSelection::pick(const Level &level, Ray ray) {
  clear();

  float closestDistance = FLT_MAX;
  const std::vector<Wall> &walls = level.getWalls();

  for (int i = 0; i < static_cast<int>(walls.size()); ++i) {
    RayCollision hit = GetRayCollisionBox(ray, walls[i].bounds);

    if (!hit.hit) {
      continue;
    }

    if (hit.distance < 0.0f) {
      continue;
    }

    if (hit.distance >= closestDistance) {
      continue;
    }

    type = EditorSelectionType::Wall;
    selectedIndex = i;
    closestDistance = hit.distance;
  }

  const std::vector<Vector3> &enemySpawns = level.getEnemySpawns();

  for (int i = 0; i < static_cast<int>(enemySpawns.size()); ++i) {
    RayCollision hit = GetRayCollisionSphere(ray, enemySpawns[i], 0.45f);

    if (!hit.hit) {
      continue;
    }

    if (hit.distance < 0.0f) {
      continue;
    }

    if (hit.distance >= closestDistance) {
      continue;
    }

    type = EditorSelectionType::EnemySpawn;
    selectedIndex = i;
    closestDistance = hit.distance;
  }

  return type != EditorSelectionType::None;
}
