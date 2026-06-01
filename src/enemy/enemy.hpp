#pragma once

#include "raylib.h"

class Player;
class Level;

enum class EnemyState { Idle, Chase, Attack, Dead };

class Enemy {
public:
  explicit Enemy(Vector3 spawnPosition);

  void update(float dt, Player &player, const Level &level);
  void draw() const;

  void applyDamage(int damage);

  bool isAlive() const;
  BoundingBox getHitbox() const;
  Vector3 getPosition() const;

private:
  void updateHitbox();
  void chasePlayer(float dt, const Player &player, const Level &level);
  void attackPlayer(float dt, Player &player);

private:
  Vector3 position{};
  Vector3 velocity{};

  float radius = 0.4f;
  float height = 1.5f;
  float speed = 2.5f;

  int health = 30;
  int maxHealth = 30;

  float attackRange = 1.4f;
  float attackCooldown = 0.0f;

  EnemyState state = EnemyState::Idle;

  BoundingBox hitbox{};
};
