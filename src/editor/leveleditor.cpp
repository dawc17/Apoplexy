#include "leveleditor.hpp"

#include "raylib.h"

#include <algorithm>

namespace {
constexpr float GRID_SIZE = 1.0f;
constexpr float EDITOR_ZOOM_SPEED = 24.0f;
constexpr const char *LEVEL_PATH = "levels/test_arena.json";

Vector3 wallCenterFromCursor(Vector3 cursor, Vector3 size) {
  return {
      cursor.x + size.x * 0.5f,
      size.y * 0.5f,
      cursor.z + size.z * 0.5f,
  };
}
} // namespace

void LevelEditor::update(Level &level, float dt) {
  if (IsKeyPressed(KEY_F1)) {
    enabled = !enabled;
    updateCamera();
  }

  if (!enabled) {
    return;
  }

  if (IsKeyPressed(KEY_W)) {
    cursor.z -= GRID_SIZE;
  }
  if (IsKeyPressed(KEY_S)) {
    cursor.z += GRID_SIZE;
  }
  if (IsKeyPressed(KEY_A)) {
    cursor.x -= GRID_SIZE;
  }
  if (IsKeyPressed(KEY_D)) {
    cursor.x += GRID_SIZE;
  }

  if (IsKeyDown(KEY_Q)) {
    orthoSize += EDITOR_ZOOM_SPEED * dt;
  }
  if (IsKeyDown(KEY_R)) {
    orthoSize -= EDITOR_ZOOM_SPEED * dt;
  }

  orthoSize = std::clamp(orthoSize, 8.0f, 60.0f);

  if (IsKeyPressed(KEY_ONE)) {
    wallSize = {2.0f, 2.0f, 2.0f};
  }
  if (IsKeyPressed(KEY_TWO)) {
    wallSize = {4.0f, 2.0f, 2.0f};
  }
  if (IsKeyPressed(KEY_THREE)) {
    wallSize = {2.0f, 2.0f, 4.0f};
  }

  if (IsKeyPressed(KEY_SPACE)) {
    level.addWall(wallCenterFromCursor(cursor, wallSize), wallSize);
  }

  if (IsKeyPressed(KEY_E)) {
    level.addEnemySpawn({cursor.x, 0.0f, cursor.z});
  }

  if (IsKeyPressed(KEY_P)) {
    level.setPlayerSpawn({cursor.x, 0.0f, cursor.z});
  }

  if (IsKeyPressed(KEY_F5)) {
    level.loadFromFile(LEVEL_PATH);
  }

  if (IsKeyPressed(KEY_F6)) {
    level.saveToFile(LEVEL_PATH);
  }

  updateCamera();
}

void LevelEditor::draw(const Level &level) const {
  if (!enabled) {
    return;
  }

  Vector3 previewCenter = wallCenterFromCursor(cursor, wallSize);
  DrawCubeWires(previewCenter, wallSize.x, wallSize.y, wallSize.z, GREEN);
  DrawSphere(level.getPlayerSpawn(), 0.35f, BLUE);

  for (Vector3 spawn : level.getEnemySpawns()) {
    DrawSphere(spawn, 0.3f, RED);
  }
}

bool LevelEditor::isEnabled() const { return enabled; }

Camera3D LevelEditor::getCamera() const { return camera; }

void LevelEditor::updateCamera() {
  Vector3 target{cursor.x, 0.0f, cursor.z};
  constexpr float cameraDistance = 18.0f;

  camera.position = {
      target.x + cameraDistance,
      target.y + cameraDistance * 1.15f,
      target.z + cameraDistance,
  };
  camera.target = target;
  camera.up = {0.0f, 1.0f, 0.0f};
  camera.fovy = orthoSize;
  camera.projection = CAMERA_ORTHOGRAPHIC;
}
