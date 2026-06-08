#pragma once

class Level;

#include "raylib.h"

enum class EditorSelectionType {
  None,
  Wall,
  EnemySpawn,
};

class EditorSelection {
public:
  void clear();

  bool hasWall() const;
  int getWallIndex() const;

  bool hasEnemySpawn() const;
  int getEnemySpawnIndex() const;

  bool pick(const Level &level, Ray ray);

private:
  EditorSelectionType type = EditorSelectionType::None;
  int selectedIndex = -1;
};
