#pragma once

#include "level/level.hpp"
class Level;

#include "raylib.h"

class EditorSelection {
public:
  void clear();

  bool hasWall() const;
  int getWallIndex() const;

  bool pickWall(const Level &level, Ray ray);

private:
  int selectedWallIndex = -1;
};