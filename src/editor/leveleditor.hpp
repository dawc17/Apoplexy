#pragma once

#include "editorsettings.hpp"
#include "editorselection.hpp"

#include "../level/level.hpp"
#include "raylib.h"

#include <string_view>

class LevelEditor {
public:
  enum class GizmoAxis { None, X, Y, Z };
  enum class Mode { Build, Test };

  struct NoiseEvent {
    Vector3 position{};
    float radius = 0.0f;
  };

  void update(Level &level, float dt, std::string_view levelPath);
  void draw(const Level &level) const;

  bool isEnabled() const;
  bool isTestMode() const;
  Mode getMode() const;
  Camera3D getCamera() const;
  bool consumeNoiseEvent(NoiseEvent &event);
  Vector3 getLastTestNoisePosition() const;
  float getLastTestNoiseRadius() const;
  bool hasLastTestNoise() const;

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
  Mode mode = Mode::Build;
  float cameraDistance = 24.0f;
  float cameraYaw = 0.78f;
  float cameraPitch = 0.85f;
  float testNoiseRadius = 28.0f;
  Vector3 lastTestNoisePosition{};
  float lastTestNoiseRadius = 0.0f;
  Vector3 gizmoDragStartPosition{};
  Vector3 gizmoDragStartObjectPosition{};
  GizmoAxis activeGizmoAxis = GizmoAxis::None;
  NoiseEvent pendingNoiseEvent{};
  bool enabled = false;
  bool pendingNoiseEventValid = false;
  bool lastTestNoiseValid = false;
};
