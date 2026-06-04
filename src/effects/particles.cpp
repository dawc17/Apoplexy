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

void ParticleSystem::spawnEnemyHit(Vector3 position, Vector3 normal) {
  constexpr int count = 10;

  for (int i = 0; i < count; ++i) {
    Vector3 randomDirection{
        static_cast<float>(GetRandomValue(-100, 100)) / 100.0f,
        static_cast<float>(GetRandomValue(20, 120)) / 100.0f,
        static_cast<float>(GetRandomValue(-100, 100)) / 100.0f,
    };

    Vector3 direction = Vector3Normalize(Vector3Add(normal, randomDirection));

    Particle particle{};
    particle.position = position;
    particle.velocity = Vector3Scale(
        direction, static_cast<float>(GetRandomValue(160, 320)) / 100.0f);
    particle.lifetime = 0.35f;
    particle.maxLifetime = particle.lifetime;
    particle.size = static_cast<float>(GetRandomValue(3, 6)) / 100.0f;
    particle.color = {180, 0, 20, 255};

    particles.push_back(particle);
  }
}
