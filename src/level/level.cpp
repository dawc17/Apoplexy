#include "level.hpp"

#include "nlohmann/json.hpp"
#include "raylib.h"

#include <cstdint>
#include <fstream>

using json = nlohmann::json;

namespace {
Vector3 readVec3(const json &value) {
  return {value.at(0).get<float>(), value.at(1).get<float>(),
          value.at(2).get<float>()};
}
json writeVec3(Vector3 value) {
  return json::array({value.x, value.y, value.z});
}
Color readColor(const json &value) {
  unsigned char alpha =
      value.size() > 3 ? value.at(3).get<unsigned char>() : 255;
  return {value.at(0).get<unsigned char>(), value.at(1).get<unsigned char>(),
          value.at(2).get<unsigned char>(), alpha};
}
json writeColor(Color value) {
  return json::array({value.r, value.g, value.b, value.a});
}
} // namespace

bool Level::loadFromFile(const char *path) {
  std::ifstream file(path);
  if (!file) {
    return false;
  }

  json data = json::parse(file, nullptr, false);
  if (data.is_discarded()) {
    return false;
  }

  unload();

  playerSpawn = readVec3(data.at("playerSpawn"));

  if (data.contains("lighting") && data.at("lighting").is_object()) {
    const json &lightingData = data.at("lighting");
    lighting.ambientColor =
        readColor(lightingData.value("ambientColor",
                                     json::array({255, 255, 255, 255})));
    lighting.ambientIntensity =
        lightingData.value("ambientIntensity", lighting.ambientIntensity);

    if (lightingData.contains("sun") && lightingData.at("sun").is_object()) {
      const json &sunData = lightingData.at("sun");
      lighting.sun.direction =
          readVec3(sunData.value("direction", writeVec3(lighting.sun.direction)));
      lighting.sun.color =
          readColor(sunData.value("color", json::array({255, 255, 255, 255})));
      lighting.sun.intensity =
          sunData.value("intensity", lighting.sun.intensity);
    }
  }

  for (const json &wall : data.at("walls")) {
    addWall((readVec3(wall.at("position"))), readVec3(wall.at("size")));
  }

  for (const json &spawn : data.at("enemySpawns")) {
    enemySpawns.push_back(readVec3(spawn));
  }

  if (data.contains("lights") && data.at("lights").is_array()) {
    for (const json &lightData : data.at("lights")) {
      Lighting::PointLight light{};
      light.position = readVec3(lightData.at("position"));
      light.color =
          readColor(lightData.value("color",
                                    json::array({255, 214, 160, 255})));
      light.intensity = lightData.value("intensity", light.intensity);
      light.radius = lightData.value("radius", light.radius);
      light.enabled = lightData.value("enabled", light.enabled);
      addLight(light);
    }
  }

  return true;
}

bool Level::saveToFile(const char *path) const {
  json data;
  data["playerSpawn"] = writeVec3(playerSpawn);
  data["walls"] = json::array();
  data["enemySpawns"] = json::array();
  data["lights"] = json::array();
  data["lighting"] = {
      {"ambientColor", writeColor(lighting.ambientColor)},
      {"ambientIntensity", lighting.ambientIntensity},
      {"sun",
       {
           {"direction", writeVec3(lighting.sun.direction)},
           {"color", writeColor(lighting.sun.color)},
           {"intensity", lighting.sun.intensity},
       }},
  };

  for (const Wall &wall : walls) {
    data["walls"].push_back({
      {"position", writeVec3(wall.position)},
      {"size", writeVec3(wall.size)},
    });
  }

  for (Vector3 spawn : enemySpawns) {
    data["enemySpawns"].push_back(writeVec3(spawn));
  }

  for (const Lighting::PointLight &light : lights) {
    data["lights"].push_back({
        {"position", writeVec3(light.position)},
        {"color", writeColor(light.color)},
        {"intensity", light.intensity},
        {"radius", light.radius},
        {"enabled", light.enabled},
    });
  }

  std::ofstream file(path);
  if (!file) {
    return false;
  }

  file << data.dump(2);
  return true;
}

void Level::addEnemySpawn(Vector3 position) {
  enemySpawns.push_back(position);
}

void Level::removeEnemySpawn(int index) {
  if (index < 0 || index >= static_cast<int>(enemySpawns.size())) {
    return;
  }

  enemySpawns.erase(enemySpawns.begin() + index);
}

void Level::setEnemySpawnPosition(int index, Vector3 position) {
  if (index < 0 || index >= static_cast<int>(enemySpawns.size())) {
    return;
  }

  enemySpawns[index] = position;
}

void Level::addLight(Lighting::PointLight light) {
  light.intensity = Lighting::clampIntensity(light.intensity);
  light.radius = Lighting::clampRadius(light.radius);
  lights.push_back(light);
}

void Level::removeLight(int index) {
  if (index < 0 || index >= static_cast<int>(lights.size())) {
    return;
  }

  lights.erase(lights.begin() + index);
}

void Level::setLightPosition(int index, Vector3 position) {
  if (index < 0 || index >= static_cast<int>(lights.size())) {
    return;
  }

  lights[index].position = position;
}

void Level::setPlayerSpawn(Vector3 position) {
  playerSpawn = position;
}

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
  lights.clear();
  lighting = Lighting::SceneLighting{};
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

const std::vector<Lighting::PointLight> &Level::getLights() const {
  return lights;
}

std::vector<Lighting::PointLight> &Level::getMutableLights() { return lights; }

const Lighting::SceneLighting &Level::getLighting() const { return lighting; }

Lighting::SceneLighting &Level::getMutableLighting() { return lighting; }

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

void Level::removeWall(int index) {
  if (index < 0 || index >= static_cast<int>(walls.size())) {
    return;
  }

  walls.erase(walls.begin() + index);
}

void Level::setWallPosition(int index, Vector3 position) {
  if (index < 0 || index >= static_cast<int>(walls.size())) {
    return;
  }

  Wall &wall = walls[index];
  wall.position = position;

  Vector3 half{
      wall.size.x * 0.5f,
      wall.size.y * 0.5f,
      wall.size.z * 0.5f,
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
}
