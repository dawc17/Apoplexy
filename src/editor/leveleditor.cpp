#include "leveleditor.hpp"
#include "editorgrid.hpp"
#include "editorsettings.hpp"
#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cfloat>
#include <cmath>

namespace {
constexpr float EDITOR_ZOOM_SPEED = 18.0f;
constexpr float MOUSE_WHEEL_ZOOM_SPEED = 2.0f;
constexpr float EDITOR_ROTATE_SPEED = 0.008f;
constexpr const char *LEVEL_PATH = "levels/test_arena.json";

constexpr Rectangle EDITOR_PANEL_BOUNDS{16.0f, 16.0f, 320.0f, 680.0f};
constexpr float GIZMO_AXIS_LENGTH = 2.5f;
constexpr float GIZMO_PICK_PIXELS = 12.0f;

bool isMouseOverEditorPanel() {
  return CheckCollisionPointRec(GetMousePosition(), EDITOR_PANEL_BOUNDS);
}

float snapDown(float value, float snapSize) {
  return std::floorf(value / snapSize) * snapSize;
}

Vector3 snapCursor(Vector3 cursor, float snapSize) {
  return {snapDown(cursor.x, snapSize), 0.0f, snapDown(cursor.z, snapSize)};
}

float snapNearest(float value, float snapSize) {
  return std::roundf(value / snapSize) * snapSize;
}

Vector3 snapPosition(Vector3 position, float snapSize) {
  return {snapNearest(position.x, snapSize), snapNearest(position.y, snapSize),
          snapNearest(position.z, snapSize)};
}

Vector3 wallCenterFromCursor(Vector3 cursor, Vector3 size) {
  return {
      cursor.x + size.x * 0.5f,
      size.y * 0.5f,
      cursor.z + size.z * 0.5f,
  };
}

bool raycastGround(Camera3D camera, Vector3 &hitPoint) {
  Ray ray = GetScreenToWorldRay(GetMousePosition(), camera);

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

bool raycastWallFace(const Level &level, Ray ray, Vector3 &hitPoint,
                     Vector3 &hitNormal) {
  float closestDistance = FLT_MAX;
  bool found = false;

  for (const Wall &wall : level.getWalls()) {
    RayCollision hit = GetRayCollisionBox(ray, wall.bounds);

    if (!hit.hit || hit.distance < 0.0f || hit.distance >= closestDistance) {
      continue;
    }

    closestDistance = hit.distance;
    hitPoint = hit.point;
    hitNormal = hit.normal;
    found = true;
  }

  return found;
}

Ray mouseRay(Camera3D camera) {
  return GetScreenToWorldRay(GetMousePosition(), camera);
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

Vector3 axisDirection(LevelEditor::GizmoAxis axis) {
  switch (axis) {
  case LevelEditor::GizmoAxis::X:
    return {1.0f, 0.0f, 0.0f};
  case LevelEditor::GizmoAxis::Y:
    return {0.0f, 1.0f, 0.0f};
  case LevelEditor::GizmoAxis::Z:
    return {0.0f, 0.0f, 1.0f};
  case LevelEditor::GizmoAxis::None:
    return {0.0f, 0.0f, 0.0f};
  }

  return {0.0f, 0.0f, 0.0f};
}

Color axisColor(LevelEditor::GizmoAxis axis) {
  switch (axis) {
  case LevelEditor::GizmoAxis::X:
    return RED;
  case LevelEditor::GizmoAxis::Y:
    return GREEN;
  case LevelEditor::GizmoAxis::Z:
    return BLUE;
  case LevelEditor::GizmoAxis::None:
    return WHITE;
  }

  return WHITE;
}

float distancePointSegment(Vector2 point, Vector2 a, Vector2 b) {
  Vector2 ab = Vector2Subtract(b, a);
  float lengthSquared = Vector2DotProduct(ab, ab);

  if (lengthSquared <= 0.0001f) {
    return Vector2Distance(point, a);
  }

  float t =
      std::clamp(Vector2DotProduct(Vector2Subtract(point, a), ab) /
                     lengthSquared,
                 0.0f, 1.0f);
  Vector2 closest = Vector2Add(a, Vector2Scale(ab, t));
  return Vector2Distance(point, closest);
}

float axisParameterFromRay(Vector3 axisOrigin, Vector3 axisDirection, Ray ray) {
  Vector3 between = Vector3Subtract(axisOrigin, ray.position);
  float axisRayDot = Vector3DotProduct(axisDirection, ray.direction);
  float denominator = 1.0f - axisRayDot * axisRayDot;

  if (std::fabs(denominator) <= 0.0001f) {
    return Vector3DotProduct(Vector3Subtract(ray.position, axisOrigin),
                             axisDirection);
  }

  float axisBetweenDot = Vector3DotProduct(axisDirection, between);
  float rayBetweenDot = Vector3DotProduct(ray.direction, between);
  return (axisRayDot * rayBetweenDot - axisBetweenDot) / denominator;
}

bool selectedObjectPosition(const Level &level, const EditorSelection &selection,
                            Vector3 &position) {
  if (selection.hasWall()) {
    const std::vector<Wall> &walls = level.getWalls();
    int index = selection.getWallIndex();

    if (index < 0 || index >= static_cast<int>(walls.size())) {
      return false;
    }

    position = walls[index].position;
    return true;
  }

  if (selection.hasEnemySpawn()) {
    const std::vector<Vector3> &enemySpawns = level.getEnemySpawns();
    int index = selection.getEnemySpawnIndex();

    if (index < 0 || index >= static_cast<int>(enemySpawns.size())) {
      return false;
    }

    position = enemySpawns[index];
    return true;
  }

  if (selection.hasPlayerSpawn()) {
    position = level.getPlayerSpawn();
    return true;
  }

  if (selection.hasLight()) {
    const std::vector<Lighting::PointLight> &lights = level.getLights();
    int index = selection.getLightIndex();

    if (index < 0 || index >= static_cast<int>(lights.size())) {
      return false;
    }

    position = lights[index].position;
    return true;
  }

  if (selection.hasWallDecal()) {
    const std::vector<WallDecal> &wallDecals = level.getWallDecals();
    int index = selection.getWallDecalIndex();

    if (index < 0 || index >= static_cast<int>(wallDecals.size())) {
      return false;
    }

    position = wallDecals[index].position;
    return true;
  }

  return false;
}

void setSelectedObjectPosition(Level &level, const EditorSelection &selection,
                               Vector3 position) {
  if (selection.hasWall()) {
    level.setWallPosition(selection.getWallIndex(), position);
  } else if (selection.hasEnemySpawn()) {
    level.setEnemySpawnPosition(selection.getEnemySpawnIndex(), position);
  } else if (selection.hasPlayerSpawn()) {
    level.setPlayerSpawn(position);
  } else if (selection.hasLight()) {
    level.setLightPosition(selection.getLightIndex(), position);
  } else if (selection.hasWallDecal()) {
    level.setWallDecalPosition(selection.getWallDecalIndex(), position);
  }
}

LevelEditor::GizmoAxis pickGizmoAxis(Camera3D camera, Vector3 origin) {
  Vector2 mouse = GetMousePosition();
  float closestDistance = GIZMO_PICK_PIXELS;
  LevelEditor::GizmoAxis closestAxis = LevelEditor::GizmoAxis::None;

  for (LevelEditor::GizmoAxis axis :
       {LevelEditor::GizmoAxis::X, LevelEditor::GizmoAxis::Y,
        LevelEditor::GizmoAxis::Z}) {
    Vector3 direction = axisDirection(axis);
    Vector2 start = GetWorldToScreen(origin, camera);
    Vector2 end = GetWorldToScreen(
        Vector3Add(origin, Vector3Scale(direction, GIZMO_AXIS_LENGTH)), camera);
    float distance = distancePointSegment(mouse, start, end);

    if (distance < closestDistance) {
      closestDistance = distance;
      closestAxis = axis;
    }
  }

  return closestAxis;
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

  if (settings.decalPathEditMode) {
    updateCamera();
    return;
  }

  if (IsKeyPressed(KEY_T)) {
    mode = mode == Mode::Build ? Mode::Test : Mode::Build;
    activeGizmoAxis = GizmoAxis::None;
    selection.clear();
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

  if (activeGizmoAxis == GizmoAxis::None &&
      !IsMouseButtonDown(MOUSE_BUTTON_RIGHT) && !isMouseOverEditorPanel()) {
    Vector3 mouseHit{};
    if (raycastGround(camera, mouseHit)) {
      cursor = snapCursor(mouseHit, settings.snapSize);
    }
  }

  cursor = snapCursor(cursor, settings.snapSize);

  if (mode == Mode::Test) {
    if (!isMouseOverEditorPanel() && IsMouseButtonPressed(MOUSE_BUTTON_LEFT)) {
      Vector3 mouseHit{};

      if (raycastGround(camera, mouseHit)) {
        pendingNoiseEvent = {mouseHit, testNoiseRadius};
        pendingNoiseEventValid = true;
        lastTestNoisePosition = mouseHit;
        lastTestNoiseRadius = testNoiseRadius;
        lastTestNoiseValid = true;
      }
    }

    updateCamera();
    return;
  }

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

  if (IsKeyPressed(KEY_L)) {
    settings.tool = EditorTool::Light;
  }

  if (IsKeyPressed(KEY_G)) {
    settings.tool = EditorTool::Decal;
  }

  bool gizmoConsumedInput = updateGizmo(level);

  bool primaryPressed =
      IsKeyPressed(KEY_SPACE) ||
      (!isMouseOverEditorPanel() && IsMouseButtonPressed(MOUSE_BUTTON_LEFT));

  if (primaryPressed && !gizmoConsumedInput) {
    if (settings.tool == EditorTool::Select) {
      selection.pick(level, mouseRay(camera));
    } else if (settings.tool == EditorTool::Wall) {
      level.addWall(wallCenterFromCursor(cursor, settings.wallSize),
                    settings.wallSize);
    } else if (settings.tool == EditorTool::EnemySpawn) {
      level.addEnemySpawn({cursor.x, 0.0f, cursor.z});
    } else if (settings.tool == EditorTool::PlayerSpawn) {
      level.setPlayerSpawn({cursor.x, 0.0f, cursor.z});
    } else if (settings.tool == EditorTool::Light) {
      level.addLight(
          Lighting::makeDefaultPointLight({cursor.x, 1.5f, cursor.z}));
    } else if (settings.tool == EditorTool::Decal) {
      Vector3 hitPoint{};
      Vector3 hitNormal{};

      if (raycastWallFace(level, mouseRay(camera), hitPoint, hitNormal)) {
        level.addWallDecal(Vector3Add(hitPoint, Vector3Scale(hitNormal, 0.025f)),
                           hitNormal, settings.decalSize,
                           settings.decalTexturePath);
      }
    }
  }

  if (IsKeyPressed(KEY_DELETE)) {
    if (selection.hasWall()) {
      level.removeWall(selection.getWallIndex());
    } else if (selection.hasEnemySpawn()) {
      level.removeEnemySpawn(selection.getEnemySpawnIndex());
    } else if (selection.hasLight()) {
      level.removeLight(selection.getLightIndex());
    } else if (selection.hasWallDecal()) {
      level.removeWallDecal(selection.getWallDecalIndex());
    }

    selection.clear();
  }

  if (IsKeyPressed(KEY_F5)) {
    if (auto result = level.loadFromFile(LEVEL_PATH); !result) {
      TraceLog(LOG_WARNING, "%s", result.error().c_str());
    }
  }

  if (IsKeyPressed(KEY_F6)) {
    if (auto result = level.saveToFile(LEVEL_PATH); !result) {
      TraceLog(LOG_WARNING, "%s", result.error().c_str());
    }
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

  if (selection.hasPlayerSpawn()) {
    DrawSphereWires(level.getPlayerSpawn(), 0.55f, 12, 12, YELLOW);
  }

  if (selection.hasLight()) {
    const std::vector<Lighting::PointLight> &lights = level.getLights();
    int index = selection.getLightIndex();

    if (index >= 0 && index < static_cast<int>(lights.size())) {
      const Lighting::PointLight &light = lights[index];
      DrawSphereWires(light.position, 0.45f, 12, 12, YELLOW);
      DrawCircle3D(light.position, light.radius, {1.0f, 0.0f, 0.0f}, 90.0f,
                   YELLOW);
    }
  }

  if (selection.hasWallDecal()) {
    const std::vector<WallDecal> &wallDecals = level.getWallDecals();
    int index = selection.getWallDecalIndex();

    if (index >= 0 && index < static_cast<int>(wallDecals.size())) {
      const WallDecal &decal = wallDecals[index];
      DrawSphereWires(decal.position, 0.32f, 12, 12, YELLOW);
    }
  }

  if (mode == Mode::Test) {
    DrawSphere({cursor.x, 0.0f, cursor.z}, 0.22f, SKYBLUE);
  } else if (settings.tool == EditorTool::Wall) {
    Vector3 previewCenter = wallCenterFromCursor(cursor, settings.wallSize);
    DrawCubeWires(previewCenter, settings.wallSize.x, settings.wallSize.y,
                  settings.wallSize.z, GREEN);
  } else if (settings.tool == EditorTool::EnemySpawn) {
    DrawSphere({cursor.x, 0.0f, cursor.z}, 0.35f, RED);
  } else if (settings.tool == EditorTool::PlayerSpawn) {
    DrawSphere({cursor.x, 0.0f, cursor.z}, 0.35f, BLUE);
  } else if (settings.tool == EditorTool::Light) {
    DrawSphere({cursor.x, 1.5f, cursor.z}, 0.25f, YELLOW);
    DrawCircle3D({cursor.x, 1.5f, cursor.z}, 7.0f, {1.0f, 0.0f, 0.0f},
                 90.0f, Fade(YELLOW, 0.55f));
  } else if (settings.tool == EditorTool::Decal) {
    Vector3 hitPoint{};
    Vector3 hitNormal{};

    if (raycastWallFace(level, mouseRay(camera), hitPoint, hitNormal)) {
      Vector3 previewPosition =
          Vector3Add(hitPoint, Vector3Scale(hitNormal, 0.025f));
      level.drawWallDecalPreview(previewPosition, hitNormal, settings.decalSize,
                                 settings.decalTexturePath);
    }
  }

  if (settings.showSpawns) {
    DrawSphere(level.getPlayerSpawn(), 0.35f, BLUE);

    for (Vector3 spawn : level.getEnemySpawns()) {
      DrawSphere(spawn, 0.35f, RED);
    }
  }

  for (const Lighting::PointLight &light : level.getLights()) {
    Color color = light.enabled ? YELLOW : DARKGRAY;
    DrawSphere(light.position, 0.22f, color);
    DrawCircle3D(light.position, light.radius, {1.0f, 0.0f, 0.0f}, 90.0f,
                 Fade(color, 0.35f));
  }

  if (mode == Mode::Build) {
    drawMoveGizmo(level);
  }
}

bool LevelEditor::isEnabled() const { return enabled; }

bool LevelEditor::isTestMode() const { return enabled && mode == Mode::Test; }

LevelEditor::Mode LevelEditor::getMode() const { return mode; }

Camera3D LevelEditor::getCamera() const { return camera; }

bool LevelEditor::consumeNoiseEvent(NoiseEvent &event) {
  if (!pendingNoiseEventValid) {
    return false;
  }

  event = pendingNoiseEvent;
  pendingNoiseEventValid = false;
  return true;
}

Vector3 LevelEditor::getLastTestNoisePosition() const {
  return lastTestNoisePosition;
}

float LevelEditor::getLastTestNoiseRadius() const { return lastTestNoiseRadius; }

bool LevelEditor::hasLastTestNoise() const { return lastTestNoiseValid; }

EditorSettings &LevelEditor::getSettings() { return settings; }
const EditorSettings &LevelEditor::getSettings() const { return settings; }
EditorSelection &LevelEditor::getSelection() { return selection; }
const EditorSelection &LevelEditor::getSelection() const { return selection; }

bool LevelEditor::updateGizmo(Level &level) {
  if (settings.tool != EditorTool::Select || !selection.hasAny() ||
      isMouseOverEditorPanel()) {
    activeGizmoAxis = GizmoAxis::None;
    return false;
  }

  Vector3 objectPosition{};
  if (!selectedObjectPosition(level, selection, objectPosition)) {
    activeGizmoAxis = GizmoAxis::None;
    return false;
  }

  if (activeGizmoAxis == GizmoAxis::None &&
      IsMouseButtonPressed(MOUSE_BUTTON_LEFT)) {
    activeGizmoAxis = pickGizmoAxis(camera, objectPosition);

    if (activeGizmoAxis != GizmoAxis::None) {
      Ray ray = mouseRay(camera);
      Vector3 axis = axisDirection(activeGizmoAxis);
      float axisParameter = axisParameterFromRay(objectPosition, axis, ray);
      gizmoDragStartPosition =
          Vector3Add(objectPosition, Vector3Scale(axis, axisParameter));
      gizmoDragStartObjectPosition = objectPosition;
      return true;
    }
  }

  if (activeGizmoAxis == GizmoAxis::None) {
    return false;
  }

  if (!IsMouseButtonDown(MOUSE_BUTTON_LEFT)) {
    activeGizmoAxis = GizmoAxis::None;
    return false;
  }

  Ray ray = mouseRay(camera);
  Vector3 axis = axisDirection(activeGizmoAxis);
  float axisParameter =
      axisParameterFromRay(gizmoDragStartObjectPosition, axis, ray);
  Vector3 currentAxisPosition =
      Vector3Add(gizmoDragStartObjectPosition, Vector3Scale(axis, axisParameter));
  Vector3 delta = Vector3Subtract(currentAxisPosition, gizmoDragStartPosition);
  Vector3 movedPosition = Vector3Add(gizmoDragStartObjectPosition, delta);

  bool freeMove = IsKeyDown(KEY_LEFT_SHIFT) || IsKeyDown(KEY_RIGHT_SHIFT);
  if (!freeMove) {
    movedPosition = snapPosition(movedPosition, settings.snapSize);
  }

  setSelectedObjectPosition(level, selection, movedPosition);
  return true;
}

void LevelEditor::drawMoveGizmo(const Level &level) const {
  if (settings.tool != EditorTool::Select || !selection.hasAny()) {
    return;
  }

  Vector3 origin{};
  if (!selectedObjectPosition(level, selection, origin)) {
    return;
  }

  for (GizmoAxis axis : {GizmoAxis::X, GizmoAxis::Y, GizmoAxis::Z}) {
    Vector3 direction = axisDirection(axis);
    Color color =
        axis == activeGizmoAxis ? YELLOW : Fade(axisColor(axis), 0.9f);
    Vector3 end = Vector3Add(origin, Vector3Scale(direction, GIZMO_AXIS_LENGTH));
    DrawLine3D(origin, end, color);
    DrawSphere(end, 0.12f, color);
  }
}

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
