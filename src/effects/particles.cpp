#include "particles.hpp"
#include "raylib.h"
#include "raymath.h"
#include <algorithm>

void ParticleSystem::update(float dt) {
  for (Particle &particle : particles) {
    particle.lifetime -= dt;
    particle.velocity.y -= 9.8f * dt;
    particle.position =
        Vector3Add(particle.position, Vector3Scale(particle.velocity, dt));
  }

  particles.erase(std::remove_if(particles.begin(), particles.end(),
                                 [](const Particle &particle) {
                                   return particle.lifetime <= 0.0f;
                                 }),
                  particles.end());
}

void ParticleSystem::draw() const {
  for (const Particle &particle : particles) {
    float t = particle.lifetime / particle.maxLifetime;
    Color color = Fade(particle.color, t);

    DrawCube(particle.position, particle.size, particle.size, particle.size,
             color);
  }
}

void ParticleSystem::spawnEnemyHit(Vector3 position, Vector3 normal,
                                   Vector3 inheritedVelocity) {
  constexpr int count = 14;

  if (Vector3Length(normal) <= 0.001f) {
    normal = {0.0f, 1.0f, 0.0f};
  } else {
    normal = Vector3Normalize(normal);
  }

  Vector3 spawnPosition = Vector3Add(position, Vector3Scale(normal, 0.08f));

  for (int i = 0; i < count; ++i) {
    Vector3 randomDirection{
        static_cast<float>(GetRandomValue(-50, 50)) / 100.0f,
        static_cast<float>(GetRandomValue(20, 120)) / 100.0f,
        static_cast<float>(GetRandomValue(-50, 50)) / 100.0f,
    };

    Vector3 direction =
        Vector3Normalize(Vector3Add(Vector3Scale(normal, 1.8f), randomDirection));

    Particle particle{};
    particle.position = spawnPosition;
    particle.velocity = Vector3Add(
        inheritedVelocity,
        Vector3Scale(direction,
                     static_cast<float>(GetRandomValue(240, 430)) / 100.0f));
    particle.lifetime = 0.45f;
    particle.maxLifetime = particle.lifetime;
    particle.size = static_cast<float>(GetRandomValue(4, 8)) / 100.0f;
    particle.color = {185, 8, 20, 255};

    particles.push_back(particle);
  }
}

void ParticleSystem::spawnEnemyDeath(Vector3 position,
                                     Vector3 inheritedVelocity) {
  constexpr int count = 42;

  for (int i = 0; i < count; ++i) {
    Vector3 randomDirection{
        static_cast<float>(GetRandomValue(-100, 100)) / 100.0f,
        static_cast<float>(GetRandomValue(15, 130)) / 100.0f,
        static_cast<float>(GetRandomValue(-100, 100)) / 100.0f,
    };

    if (Vector3Length(randomDirection) <= 0.001f) {
      randomDirection = {0.0f, 1.0f, 0.0f};
    }

    Vector3 direction = Vector3Normalize(randomDirection);

    Particle particle{};
    particle.position = Vector3Add(
        position, Vector3Scale(direction,
                               static_cast<float>(GetRandomValue(0, 12)) /
                                   100.0f));
    particle.velocity = Vector3Add(
        inheritedVelocity,
        Vector3Scale(direction,
                     static_cast<float>(GetRandomValue(260, 620)) / 100.0f));
    particle.lifetime = static_cast<float>(GetRandomValue(45, 80)) / 100.0f;
    particle.maxLifetime = particle.lifetime;
    particle.size = static_cast<float>(GetRandomValue(5, 11)) / 100.0f;
    particle.color = {190, 8, 24, 255};

    particles.push_back(particle);
  }
}
