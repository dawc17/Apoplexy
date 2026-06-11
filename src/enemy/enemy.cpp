#include "enemy.hpp"

#include "../collision/collision.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"
#include "../util/math.hpp"

#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cmath>

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
  stateTimer = std::max(0.0f, stateTimer - dt);

  float distance = Math::distanceXZ(position, player.getPosition());
  bool seesPlayer = canSeePlayer(player, level);

  if (seesPlayer) {
    lastKnownPlayerPosition = player.getPosition();
    timeSinceSeenPlayer = 0.0f;
  } else {
    timeSinceSeenPlayer += dt;
  }

  switch (state) {
  case EnemyState::Idle:
    stopHorizontalMovement(dt);

    if (seesPlayer) {
      state = EnemyState::Alert;
      stateTimer = alertDuration;
    }
    break;

  case EnemyState::Alert:
    stopHorizontalMovement(dt);

    if (!seesPlayer) {
      state = EnemyState::Search;
      stateTimer = searchDuration;
    } else if (stateTimer <= 0.0f) {
      state = EnemyState::Chase;
    }
    break;

  case EnemyState::Chase:
    if (seesPlayer && distance <= attackRange && attackCooldown <= 0.0f) {
      state = EnemyState::AttackWindup;
      stateTimer = attackWindupDuration;
      stopHorizontalMovement(dt);
    } else if (!seesPlayer && timeSinceSeenPlayer > loseSightGrace) {
      state = EnemyState::Search;
      stateTimer = searchDuration;
    } else {
      chasePlayer(dt, player, level);
    }
    break;

  case EnemyState::Search:
    if (seesPlayer) {
      state = EnemyState::Alert;
      stateTimer = alertDuration;
      break;
    }

    moveToward(lastKnownPlayerPosition, searchSpeed, dt);

    if (Math::distanceXZ(position, lastKnownPlayerPosition) <= 0.65f ||
        stateTimer <= 0.0f) {
      state = EnemyState::Idle;
    }
    break;

  case EnemyState::AttackWindup:
    stopHorizontalMovement(dt);

    if (stateTimer <= 0.0f) {
      attackPlayer(dt, player);
      state = EnemyState::AttackRecovery;
      stateTimer = attackRecoveryDuration;
    }
    break;

  case EnemyState::AttackRecovery:
    stopHorizontalMovement(dt);

    if (stateTimer <= 0.0f) {
      state = seesPlayer ? EnemyState::Chase : EnemyState::Search;
      stateTimer = seesPlayer ? 0.0f : searchDuration;
    }
    break;

  case EnemyState::Dead:
    break;
  }

  if (!grounded || velocity.y > 0.0f) {
    velocity.y -= gravity * dt;
  } else {
    velocity.y = 0.0f;
  }

  Collision::MoveResult move = Collision::moveCylinderLevel(
      position, velocity, radius, height, level, dt);
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
  Color color = MAROON;

  if (state == EnemyState::Alert || state == EnemyState::Search) {
    color = ORANGE;
  } else if (state == EnemyState::Chase) {
    color = RED;
  } else if (state == EnemyState::AttackWindup) {
    color = YELLOW;
  } else if (state == EnemyState::AttackRecovery) {
    color = DARKPURPLE;
  }

  if (hitFlashTimer > 0.0f) {
    color = WHITE;
  }

  DrawCube({position.x, position.y + height * 0.5f, position.z}, radius * 2.0f,
           height, radius * 2.0f, color);

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

bool Enemy::canSeePlayer(const Player &player, const Level &level) const {
  Vector3 eye{position.x, position.y + height * 0.7f, position.z};
  Vector3 target = player.getEyePosition();
  Vector3 toPlayer = Vector3Subtract(target, eye);
  float distance = Vector3Length(toPlayer);

  if (distance <= 0.001f || distance > visionRange) {
    return false;
  }

  Ray ray{};
  ray.position = eye;
  ray.direction = Vector3Scale(toPlayer, 1.0f / distance);

  Vector3 wallHit{};
  float wallDistance = 0.0f;

  if (Collision::rayLevel(ray, level, wallHit, wallDistance) &&
      wallDistance < distance - 0.05f) {
    return false;
  }

  return true;
}

void Enemy::updateHitbox() {
  hitbox.min = {position.x - radius, position.y, position.z - radius};
  hitbox.max = {position.x + radius, position.y + height, position.z + radius};
}

void Enemy::chasePlayer(float dt, const Player &player, const Level &level) {
  (void)level;
  moveToward(lastKnownPlayerPosition, speed, dt);
}

void Enemy::moveToward(Vector3 target, float targetSpeed, float dt) {
  Vector3 toTarget = Vector3Subtract(target, position);
  toTarget.y = 0.0f;

  if (Vector3Length(toTarget) <= 0.001f) {
    stopHorizontalMovement(dt);
    return;
  }

  Vector3 direction = Vector3Normalize(toTarget);
  Vector3 targetVelocity{direction.x * targetSpeed, velocity.y,
                         direction.z * targetSpeed};
  float ease = 1.0f - std::expf(-acceleration * dt);

  velocity.x += (targetVelocity.x - velocity.x) * ease;
  velocity.z += (targetVelocity.z - velocity.z) * ease;
}

void Enemy::stopHorizontalMovement(float dt) {
  float ease = 1.0f - std::expf(-acceleration * dt);
  velocity.x += (0.0f - velocity.x) * ease;
  velocity.z += (0.0f - velocity.z) * ease;
}

void Enemy::attackPlayer(float, Player &player) {
  if (attackCooldown <= 0.0f) {
    player.applyDamage(10);
    attackCooldown = 0.8f;
  }
}
