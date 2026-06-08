#include "editorselection.hpp"

#include "../level/level.hpp"

#include "raylib.h"

#include <cfloat>
#include <vector>

void EditorSelection::clear() { selectedWallIndex = -1; }

bool EditorSelection::hasWall() const { return selectedWallIndex >= 0; }

int EditorSelection::getWallIndex() const { return selectedWallIndex; }

bool EditorSelection::pickWall(const Level &level, Ray ray) {
  selectedWallIndex = -1;

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

    selectedWallIndex = i;
    closestDistance = hit.distance;
  }

  return selectedWallIndex >= 0;
}