#include "enemy.hpp"

#include "../collision/collision.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"
#include "../util/math.hpp"

#include "raylib.h"
#include "raymath.h"

#include <algorithm>

Enemy::Enemy(Vector3 spawnPosition) : position(spawnPosition) {
  updateHitbox();
}

void Enemy::update(float dt, Player &player, const Level &level) {
  if (!isAlive()) {
    state = EnemyState::Dead;
    return;
  }

  attackCooldown = std::max(0.0f, attackCooldown - dt);
  hitFlashTimer = std::max(0.0f, hitFlashTimer - dt);

  float distance = Math::distanceXZ(position, player.getPosition());

  if (distance <= attackRange) {
    state = EnemyState::Attack;
    attackPlayer(dt, player);
  } else {
    state = EnemyState::Chase;
    chasePlayer(dt, player, level);
  }

  if (!grounded || velocity.y > 0.0f) {
    velocity.y -= gravity * dt;
  } else {
    velocity.y = 0.0f;
  }

  Collision::MoveResult move =
      Collision::moveCylinderLevel(position, velocity, radius, height, level, dt);
  position = move.position;
  velocity = move.velocity;
  grounded = move.grounded;

  updateHitbox();
}

void Enemy::draw() const {
  if (!isAlive()) {
    return;
  }

  // woke enemy foid out to kill you (changes color)
  Color color = state == EnemyState::Attack ? RED : MAROON;

  if (hitFlashTimer > 0.0f) {
    color = WHITE;
  }

  DrawCube({position.x, position.y + height * 0.5f, position.z},
           radius * 2.0f, height, radius * 2.0f, color);

  DrawCubeWires({position.x, position.y + height * 0.5f, position.z},
                radius * 2.0f, height, radius * 2.0f, BLACK);
}

bool Enemy::applyDamage(int damage) {
  if (!isAlive()) {
    return false;
  }

  health -= damage;
  hitFlashTimer = hitFlashDuration;

  if (health <= 0) {
    health = 0;
    state = EnemyState::Dead;
    return true;
  }

  return false;
}

bool Enemy::isAlive() const { return health > 0; }

BoundingBox Enemy::getHitbox() const { return hitbox; }

Vector3 Enemy::getPosition() const { return position; }

Vector3 Enemy::getVelocity() const { return velocity; }

void Enemy::updateHitbox() {
  hitbox.min = {position.x - radius, position.y, position.z - radius};
  hitbox.max = {position.x + radius, position.y + height, position.z + radius};
}

void Enemy::chasePlayer(float dt, const Player &player, const Level &level) {
  Vector3 toPlayer = Vector3Subtract(player.getPosition(), position);
  toPlayer.y = 0.0f;

  if (Vector3Length(toPlayer) <= 0.001f) {
    velocity.x = 0.0f;
    velocity.z = 0.0f;
    return;
  }

  Vector3 direction = Vector3Normalize(toPlayer);
  velocity.x = direction.x * speed;
  velocity.z = direction.z * speed;
}

void Enemy::attackPlayer(float, Player &player) {
  velocity.x = 0.0f;
  velocity.z = 0.0f;

  if (attackCooldown <= 0.0f) {
    player.applyDamage(10);
    attackCooldown = 0.8f;
  }
}
