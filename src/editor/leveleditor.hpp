#pragma once

#include "../level/level.hpp"
#include "raylib.h"

class LevelEditor {
public:
  void update(Level &level, float dt);
  void draw(const Level &level) const;

  bool isEnabled() const;
  Camera3D getCamera() const;

private:
  void updateCamera();

private:
  Vector3 cursor{0.0f, 0.0f, 0.0f};
  Vector3 wallSize{2.0f, 2.0f, 2.0f};
  Camera3D camera{};
  float orthoSize = 35.0f;
  bool enabled = false;
};
