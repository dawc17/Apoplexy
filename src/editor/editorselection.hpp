#pragma once

class Level;

#include "raylib.h"

enum class EditorSelectionType {
  None,
  Wall,
  EnemySpawn,
  PlayerSpawn,
  Light,
  WallDecal,
};

class EditorSelection {
public:
  void clear();

  bool hasWall() const;
  int getWallIndex() const;

  bool hasEnemySpawn() const;
  int getEnemySpawnIndex() const;

  bool hasPlayerSpawn() const;

  bool hasLight() const;
  int getLightIndex() const;

  bool hasWallDecal() const;
  int getWallDecalIndex() const;

  EditorSelectionType getType() const;
  bool hasAny() const;

  bool pick(const Level &level, Ray ray);

private:
  EditorSelectionType type = EditorSelectionType::None;
  int selectedIndex = -1;
};
