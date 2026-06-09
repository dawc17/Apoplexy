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

bool EditorSelection::hasPlayerSpawn() const {
  return type == EditorSelectionType::PlayerSpawn;
}

bool EditorSelection::hasLight() const {
  return type == EditorSelectionType::Light && selectedIndex >= 0;
}

int EditorSelection::getLightIndex() const { return selectedIndex; }

EditorSelectionType EditorSelection::getType() const { return type; }

bool EditorSelection::hasAny() const {
  return type != EditorSelectionType::None && selectedIndex >= 0;
}

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

  RayCollision playerHit =
      GetRayCollisionSphere(ray, level.getPlayerSpawn(), 0.45f);
  if (playerHit.hit && playerHit.distance >= 0.0f &&
      playerHit.distance < closestDistance) {
    type = EditorSelectionType::PlayerSpawn;
    selectedIndex = 0;
    closestDistance = playerHit.distance;
  }

  const std::vector<Lighting::PointLight> &lights = level.getLights();

  for (int i = 0; i < static_cast<int>(lights.size()); ++i) {
    RayCollision hit = GetRayCollisionSphere(ray, lights[i].position, 0.45f);

    if (!hit.hit) {
      continue;
    }

    if (hit.distance < 0.0f) {
      continue;
    }

    if (hit.distance >= closestDistance) {
      continue;
    }

    type = EditorSelectionType::Light;
    selectedIndex = i;
    closestDistance = hit.distance;
  }

  return type != EditorSelectionType::None;
}
