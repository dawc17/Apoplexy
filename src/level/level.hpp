#pragma once

#include "raylib.h"

#include <vector>

struct Wall {
  Vector3 position{};
  Vector3 size{};
  BoundingBox bounds{};
};

class Level {
public:
  void loadTestArena();
  void unload();

  void draw() const;

  const std::vector<Wall> &getWalls() const;
  const std::vector<Vector3> &getEnemySpawns() const;
  Vector3 getPlayerSpawn() const;

private:
  void addWall(Vector3 position, Vector3 size);

private:
  std::vector<Wall> walls;
  std::vector<Vector3> enemySpawns;

  Vector3 playerSpawn{0.0f, 1.0f, 0.0f};
};
