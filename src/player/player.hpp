#pragma once

#include "raylib.h"

class Level;

class Player {
public:
  Player();

  void reset(Vector3 spawnPosition);
  void update(float dt, const Level &level);
  void applyDamage(int damage);

  Camera3D getCamera() const;

  Vector3 getPosition() const;
  Vector3 getEyePosition() const; // ja idiota
  float getYaw() const;
  float getPitch() const;
  float getRadius() const;
  int getHealth() const;
  bool isDead() const;
  bool isSprinting() const;
  bool consumeDamageTaken();

private:
  void updateMouseLook(float dt);
  void updateMovement(float dt, const Level &level);

private:
  Vector3 position{};
  Vector3 velocity{};

  float yaw = 0.0f;
  float pitch = 0.0f;

  float sprintFovAmount = 0.0f;

  float headBobTimer = 0.0f;
  float headBobAmount = 0.0f;

  float radius = 0.35f;
  float height = 1.8f;
  float moveSpeed = 6.0f;

  int health = 100;
  int maxHealth = 100;
  bool damageTaken = false;
  bool sprintBlockedByShot = false;
  bool sprinting = false;
};
