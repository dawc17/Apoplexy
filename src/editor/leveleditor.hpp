#pragma once

#include "editorsettings.hpp"
#include "editorselection.hpp"

#include "../level/level.hpp"
#include "raylib.h"

class LevelEditor {
public:
  enum class GizmoAxis { None, X, Y, Z };

  void update(Level &level, float dt);
  void draw(const Level &level) const;

  bool isEnabled() const;
  Camera3D getCamera() const;

  EditorSettings &getSettings();
  const EditorSettings &getSettings() const;
  EditorSelection &getSelection();
  const EditorSelection &getSelection() const;

private:
  void updateCamera();
  bool updateGizmo(Level &level);
  void drawMoveGizmo(const Level &level) const;

private:
  Vector3 cursor{0.0f, 0.0f, 0.0f};
  Vector3 cameraTarget{0.0f, 0.0f, 0.0f};
  Camera3D camera{};

  EditorSelection selection{};
  EditorSettings settings{};
  float cameraDistance = 24.0f;
  float cameraYaw = 0.78f;
  float cameraPitch = 0.85f;
  Vector3 gizmoDragStartPosition{};
  Vector3 gizmoDragStartObjectPosition{};
  GizmoAxis activeGizmoAxis = GizmoAxis::None;
  bool enabled = false;
};
