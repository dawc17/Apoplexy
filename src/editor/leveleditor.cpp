#include "leveleditor.hpp"
#include "editorgrid.hpp"
#include "editorsettings.hpp"
#include "raylib.h"

#include <algorithm>
#include <cmath>

namespace {
constexpr float EDITOR_ZOOM_SPEED = 24.0f;
constexpr float MOUSE_WHEEL_ZOOM_SPEED = 3.0f;
constexpr const char *LEVEL_PATH = "levels/test_arena.json";

constexpr Rectangle EDITOR_PANEL_BOUNDS{16.0f, 16.0f, 280.0f, 382.0f};

bool isMouseOverEditorPanel() {
  return CheckCollisionPointRec(GetMousePosition(), EDITOR_PANEL_BOUNDS);
}

float snapDown(float value, float snapSize) {
  return std::floorf(value / snapSize) * snapSize;
}

Vector3 snapCursor(Vector3 cursor, float snapSize) {
  return {snapDown(cursor.x, snapSize), 0.0f, snapDown(cursor.z, snapSize)};
}

Vector3 wallCenterFromCursor(Vector3 cursor, Vector3 size) {
  return {
      cursor.x + size.x * 0.5f,
      size.y * 0.5f,
      cursor.z + size.z * 0.5f,
  };
}

bool raycastGround(Camera3D camera, Vector3 &hitPoint) {
  Ray ray = GetMouseRay(GetMousePosition(), camera);

  if (std::fabs(ray.direction.y) < 0.0001f) {
    return false;
  }

  float t = -ray.position.y / ray.direction.y;

  if (t < 0.0f) {
    return false;
  }

  hitPoint = {ray.position.x + ray.direction.x * t, 0.0f,
              ray.position.z + ray.direction.z * t};

  return true;
}

Ray mouseRay(Camera3D camera) {
  return GetMouseRay(GetMousePosition(), camera);
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

  float step = settings.snapSize;

  if (IsKeyPressed(KEY_W)) {
    cursor.z -= step;
  }
  if (IsKeyPressed(KEY_S)) {
    cursor.z += step;
  }
  if (IsKeyPressed(KEY_A)) {
    cursor.x -= step;
  }
  if (IsKeyPressed(KEY_D)) {
    cursor.x += step;
  }

  if (!isMouseOverEditorPanel()) {
    Vector3 mouseHit{};
    if (raycastGround(camera, mouseHit)) {
      cursor = snapCursor(mouseHit, settings.snapSize);
    }
  }

  cursor = snapCursor(cursor, settings.snapSize);

  if (IsKeyDown(KEY_Q)) {
    orthoSize += EDITOR_ZOOM_SPEED * dt;
  }
  if (IsKeyDown(KEY_R)) {
    orthoSize -= EDITOR_ZOOM_SPEED * dt;
  }

  float mouseWheel = GetMouseWheelMove();
  if (mouseWheel != 0.0f) {
    orthoSize -= mouseWheel * MOUSE_WHEEL_ZOOM_SPEED;
  }

  orthoSize = std::clamp(orthoSize, 8.0f, 60.0f);

  if (IsKeyPressed(KEY_ONE)) {
    settings.wallSize = {2.0f, 2.0f, 2.0f};
  }
  if (IsKeyPressed(KEY_TWO)) {
    settings.wallSize = {4.0f, 2.0f, 2.0f};
  }
  if (IsKeyPressed(KEY_THREE)) {
    settings.wallSize = {2.0f, 2.0f, 4.0f};
  }

  if (IsKeyPressed(KEY_E)) {
    settings.tool = EditorTool::EnemySpawn;
  }

  if (IsKeyPressed(KEY_P)) {
    settings.tool = EditorTool::PlayerSpawn;
  }

  if (IsKeyPressed(KEY_B)) {
    settings.tool = EditorTool::Wall;
  }

  bool primaryPressed =
      IsKeyPressed(KEY_SPACE) ||
      (!isMouseOverEditorPanel() && IsMouseButtonPressed(MOUSE_BUTTON_LEFT));

  if (primaryPressed) {
    if (settings.tool == EditorTool::Select) {
      selection.pickWall(level, mouseRay(camera));
    } else if (settings.tool == EditorTool::Wall) {
      level.addWall(wallCenterFromCursor(cursor, settings.wallSize),
                    settings.wallSize);
    } else if (settings.tool == EditorTool::EnemySpawn) {
      level.addEnemySpawn({cursor.x, 0.0f, cursor.z});
    } else if (settings.tool == EditorTool::PlayerSpawn) {
      level.setPlayerSpawn({cursor.x, 0.0f, cursor.z});
    }
  }

  if (IsKeyPressed(KEY_DELETE) && selection.hasWall()) {
    level.removeWall(selection.getWallIndex());
    selection.clear();
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

  if (settings.showGrid) {
    EditorGrid::draw(settings.snapSize);
  }

  if (selection.hasWall()) {
    const std::vector<Wall> &walls = level.getWalls();
    int index = selection.getWallIndex();

    if (index >= 0 && index < static_cast<int>(walls.size())) {
      const Wall &wall = walls[index];
      DrawCubeWires(wall.position, wall.size.x, wall.size.y, wall.size.z,
                    YELLOW);
    }
  }

  if (settings.tool == EditorTool::Wall) {
    Vector3 previewCenter = wallCenterFromCursor(cursor, settings.wallSize);
    DrawCubeWires(previewCenter, settings.wallSize.x, settings.wallSize.y,
                  settings.wallSize.z, GREEN);
  } else if (settings.tool == EditorTool::EnemySpawn) {
    DrawSphere({cursor.x, 0.0f, cursor.z}, 0.35f, RED);
  } else if (settings.tool == EditorTool::PlayerSpawn) {
    DrawSphere({cursor.x, 0.0f, cursor.z}, 0.35f, BLUE);
  }

  if (settings.showSpawns) {
    DrawSphere(level.getPlayerSpawn(), 0.35f, BLUE);

    for (Vector3 spawn : level.getEnemySpawns()) {
      DrawSphere(spawn, 0.35f, RED);
    }
  }
}

bool LevelEditor::isEnabled() const { return enabled; }

Camera3D LevelEditor::getCamera() const { return camera; }

EditorSettings &LevelEditor::getSettings() { return settings; }
const EditorSettings &LevelEditor::getSettings() const { return settings; }

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
