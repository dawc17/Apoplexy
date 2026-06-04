#pragma once

#include "raylib.h"
#include <vector>

struct Particle {
  Vector3 position{};
  Vector3 velocity{};
  float lifetime = 0.0f;
  float maxLifetime = 0.0f;
  float size = 0.04f;
  Color color = RED;
};

class ParticleSystem {
public:
  void update(float dt);
  void draw() const;
  void spawnEnemyHit(Vector3 position, Vector3 normal);

private:
  std::vector<Particle> particles;
};
