#include "leveleditor.hpp"
#include "editorgrid.hpp"
#include "editorsettings.hpp"
#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cmath>

namespace {
constexpr float EDITOR_ZOOM_SPEED = 18.0f;
constexpr float MOUSE_WHEEL_ZOOM_SPEED = 2.0f;
constexpr float EDITOR_ROTATE_SPEED = 0.008f;
constexpr const char *LEVEL_PATH = "levels/test_arena.json";

constexpr Rectangle EDITOR_PANEL_BOUNDS{16.0f, 16.0f, 280.0f, 400.0f};

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

Vector3 normalizedGroundVector(Vector3 value) {
  value.y = 0.0f;

  if (Vector3Length(value) <= 0.0001f) {
    return {0.0f, 0.0f, 0.0f};
  }

  return Vector3Normalize(value);
}

bool isRotateDragDown() {
  return IsMouseButtonDown(MOUSE_BUTTON_MIDDLE) ||
         (IsKeyDown(KEY_LEFT_ALT) && IsMouseButtonDown(MOUSE_BUTTON_LEFT));
}
} // namespace

void LevelEditor::update(Level &level, float dt) {
  if (IsKeyPressed(KEY_F1)) {
    enabled = !enabled;

    if (enabled) {
      EnableCursor();
    } else {
      DisableCursor();
    }

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

  if (IsKeyDown(KEY_Q)) {
    cameraDistance += EDITOR_ZOOM_SPEED * dt;
  }
  if (IsKeyDown(KEY_R)) {
    cameraDistance -= EDITOR_ZOOM_SPEED * dt;
  }

  float mouseWheel = GetMouseWheelMove();
  if (mouseWheel != 0.0f) {
    cameraDistance -= mouseWheel * MOUSE_WHEEL_ZOOM_SPEED;
  }

  cameraDistance = std::clamp(cameraDistance, 5.0f, 80.0f);

  if (IsMouseButtonDown(MOUSE_BUTTON_RIGHT) && !isMouseOverEditorPanel()) {
    Vector2 mouseDelta = GetMouseDelta();
    float panScale = cameraDistance / static_cast<float>(GetScreenHeight());

    Vector3 forward =
        Vector3Normalize(Vector3Subtract(camera.target, camera.position));
    Vector3 right =
        normalizedGroundVector(Vector3CrossProduct(forward, camera.up));
    Vector3 forwardGround = normalizedGroundVector(forward);

    cameraTarget =
        Vector3Subtract(cameraTarget, Vector3Scale(right, mouseDelta.x * panScale));
    cameraTarget = Vector3Add(cameraTarget,
                              Vector3Scale(forwardGround,
                                           mouseDelta.y * panScale));
  }

  if (isRotateDragDown() && !isMouseOverEditorPanel()) {
    Vector2 mouseDelta = GetMouseDelta();

    cameraYaw -= mouseDelta.x * EDITOR_ROTATE_SPEED;
    cameraPitch += mouseDelta.y * EDITOR_ROTATE_SPEED;
    cameraPitch = std::clamp(cameraPitch, 0.15f, 1.45f);
  }

  updateCamera();

  if (!IsMouseButtonDown(MOUSE_BUTTON_RIGHT) && !isMouseOverEditorPanel()) {
    Vector3 mouseHit{};
    if (raycastGround(camera, mouseHit)) {
      cursor = snapCursor(mouseHit, settings.snapSize);
    }
  }

  cursor = snapCursor(cursor, settings.snapSize);

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
      selection.pick(level, mouseRay(camera));
    } else if (settings.tool == EditorTool::Wall) {
      level.addWall(wallCenterFromCursor(cursor, settings.wallSize),
                    settings.wallSize);
    } else if (settings.tool == EditorTool::EnemySpawn) {
      level.addEnemySpawn({cursor.x, 0.0f, cursor.z});
    } else if (settings.tool == EditorTool::PlayerSpawn) {
      level.setPlayerSpawn({cursor.x, 0.0f, cursor.z});
    }
  }

  if (IsKeyPressed(KEY_DELETE)) {
    if (selection.hasWall()) {
      level.removeWall(selection.getWallIndex());
    } else if (selection.hasEnemySpawn()) {
      level.removeEnemySpawn(selection.getEnemySpawnIndex());
    }

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

  if (selection.hasEnemySpawn()) {
    const std::vector<Vector3> &enemySpawns = level.getEnemySpawns();
    int index = selection.getEnemySpawnIndex();

    if (index >= 0 && index < static_cast<int>(enemySpawns.size())) {
      DrawSphereWires(enemySpawns[index], 0.55f, 12, 12, YELLOW);
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
  float horizontalDistance = std::cosf(cameraPitch) * cameraDistance;

  camera.position = {
      cameraTarget.x + std::sinf(cameraYaw) * horizontalDistance,
      cameraTarget.y + std::sinf(cameraPitch) * cameraDistance,
      cameraTarget.z + std::cosf(cameraYaw) * horizontalDistance,
  };
  camera.target = cameraTarget;
  camera.up = {0.0f, 1.0f, 0.0f};
  camera.fovy = 55.0f;
  camera.projection = CAMERA_PERSPECTIVE;
}
