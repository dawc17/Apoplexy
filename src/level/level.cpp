#include "level.hpp"
#include <raylib.h>
#include <vector>

void Level::loadTestArena() {
  unload();

  playerSpawn = {0.0f, 0.0f, 0.0f};

  addWall({0.0f, 1.5f, -12.0f}, {24.0f, 3.0f, 1.0f});
  addWall({0.0f, 1.5f, 12.0f}, {24.0f, 3.0f, 1.0f});
  addWall({-12.0f, 1.5f, 0.0f}, {1.0f, 3.0f, 24.0f});
  addWall({12.0f, 1.5f, 0.0f}, {1.0f, 3.0f, 24.0f});

  addWall({-4.0f, 1.0f, -3.0f}, {2.0f, 2.0f, 5.0f});
  addWall({5.0f, 1.0f, 3.0f}, {4.0f, 2.0f, 2.0f});
  addWall({1.0f, 1.0f, 7.0f}, {6.0f, 2.0f, 1.0f});

  enemySpawns.push_back({-8.0f, 0.0f, -8.0f});
  enemySpawns.push_back({8.0f, 0.0f, -7.0f});
  enemySpawns.push_back({7.0f, 0.0f, 8.0f});
  enemySpawns.push_back({-7.0f, 0.0f, 7.0f});
}

void Level::unload() {
  walls.clear();
  enemySpawns.clear();
}

void Level::draw() const {
  DrawPlane({0.0f, 0.0f, 0.0f}, {28.0f, 28.0f}, LIGHTGRAY);

  // true nophono subfoid graybox
  for (const Wall &wall : walls) {
    DrawCube(wall.position, wall.size.x, wall.size.y, wall.size.z, GRAY);
    DrawCubeWires(wall.position, wall.size.x, wall.size.y, wall.size.z,
                  DARKGRAY);
  }
}

const std::vector<Wall> &Level::getWalls() const { return walls; }

const std::vector<Vector3> &Level::getEnemySpawns() const {
  return enemySpawns;
}

Vector3 Level::getPlayerSpawn() const { return playerSpawn; }

void Level::addWall(Vector3 position, Vector3 size) {
  Wall wall{};
  wall.position = position;
  wall.size = size;

  Vector3 half{
      size.x * 0.5f,
      size.y * 0.5f,
      size.z * 0.5f,
  };

  wall.bounds.min = {
      position.x - half.x,
      position.y - half.y,
      position.z - half.z,
  };

  wall.bounds.max = {
      position.x + half.x,
      position.y + half.y,
      position.z + half.z,
  };

  walls.push_back(wall);
}
