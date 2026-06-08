#pragma once

#include "editorsettings.hpp"
#include "editorselection.hpp"

#include "../level/level.hpp"
#include "raylib.h"

class LevelEditor {
public:
  void update(Level &level, float dt);
  void draw(const Level &level) const;

  bool isEnabled() const;
  Camera3D getCamera() const;

  EditorSettings &getSettings();
  const EditorSettings &getSettings() const;

private:
  void updateCamera();

private:
  Vector3 cursor{0.0f, 0.0f, 0.0f};
  Vector3 cameraTarget{0.0f, 0.0f, 0.0f};
  Camera3D camera{};

  EditorSelection selection{};
  EditorSettings settings{};
  float cameraDistance = 24.0f;
  float cameraYaw = 0.78f;
  float cameraPitch = 0.85f;
  bool enabled = false;
};
