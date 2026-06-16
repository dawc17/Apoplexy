#pragma once

#include "../render/lighting.hpp"

#include "raylib.h"

#include <string>
#include <vector>

struct Wall {
  Vector3 position{};
  Vector3 size{};
  BoundingBox bounds{};
};

struct WallDecal {
  Vector3 position{};
  Vector3 normal{};
  Vector2 size{1.5f, 1.5f};
  std::string texturePath{};
};

class Level {
public:
  bool loadFromFile(const char *path);
  bool saveToFile(const char *path) const;

  void loadTestArena();
  void unload();

  void draw() const;
  void drawDecals() const;
  void drawWallDecalPreview(Vector3 position, Vector3 normal, Vector2 size,
                            const char *texturePath) const;

  void addWall(Vector3 position, Vector3 size);
  void removeWall(int index);
  void setWallPosition(int index, Vector3 position);
  void addWallDecal(Vector3 position, Vector3 normal, Vector2 size,
                    const char *texturePath);
  void removeWallDecal(int index);
  void setWallDecalPosition(int index, Vector3 position);
  void addEnemySpawn(Vector3 position);
  void removeEnemySpawn(int index);
  void setEnemySpawnPosition(int index, Vector3 position);
  void addLight(Lighting::PointLight light);
  void removeLight(int index);
  void setLightPosition(int index, Vector3 position);
  void setPlayerSpawn(Vector3 position);

  const std::vector<Wall> &getWalls() const;
  const std::vector<WallDecal> &getWallDecals() const;
  const std::vector<Vector3> &getEnemySpawns() const;
  const std::vector<Lighting::PointLight> &getLights() const;
  std::vector<Lighting::PointLight> &getMutableLights();
  const Lighting::SceneLighting &getLighting() const;
  Lighting::SceneLighting &getMutableLighting();
  Vector3 getPlayerSpawn() const;

private:
  std::vector<Wall> walls;
  std::vector<WallDecal> wallDecals;
  std::vector<Vector3> enemySpawns;
  std::vector<Lighting::PointLight> lights;
  Lighting::SceneLighting lighting;

  Vector3 playerSpawn{0.0f, 1.0f, 0.0f};
};
