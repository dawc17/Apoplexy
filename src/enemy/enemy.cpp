#include "enemy.hpp"

#include "../collision/collision.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"
#include "../util/math.hpp"

#include "raylib.h"
#include "raymath.h"
#include "rlgl.h"

#include <algorithm>
#include <cmath>

// niche constant
namespace {
constexpr float DEG2RAD_LOCAL = 0.01745329251f;
}

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

    Vector3 look = Vector3Subtract(player.getPosition(), position);
    look.y = 0.0f;

    if (Vector3LengthSqr(look) > 0.001f) {
      facingDirection = Vector3Normalize(look);
    }
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

    moveToward(lastKnownPlayerPosition, searchSpeed, dt, level);

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

  Vector3 forward{facingDirection.x, 0.0f, facingDirection.z};
  if (Vector3LengthSqr(forward) <= 0.001f) {
    forward = {0.0f, 0.0f, 1.0f};
  }
  forward = Vector3Normalize(forward);

  float yawDegrees = std::atan2f(forward.x, forward.z) * RAD2DEG;

  rlPushMatrix();
  rlTranslatef(position.x, position.y + height * 0.5f, position.z);
  rlRotatef(yawDegrees, 0.0f, 1.0f, 0.0f);

  DrawCube({0.0f, 0.0f, 0.0f}, radius * 2.0f, height, radius * 2.0f, color);
  DrawCubeWires({0.0f, 0.0f, 0.0f}, radius * 2.0f, height, radius * 2.0f,
                BLACK);

  DrawCube({-0.13f, height * 0.22f, radius + 0.025f}, 0.10f, 0.10f, 0.035f,
           YELLOW);
  DrawCube({0.13f, height * 0.22f, radius + 0.025f}, 0.10f, 0.10f, 0.035f,
           YELLOW);

  rlPopMatrix();
}

void Enemy::resolveOverlap(Enemy &other) {
  if (!isAlive() || !other.isAlive()) {
    return;
  }

  Vector3 delta = Vector3Subtract(other.position, position);
  delta.y = 0.0f;

  float minDistance = radius + other.radius;
  float distanceSqr = Vector3LengthSqr(delta);

  if (distanceSqr >= minDistance * minDistance) {
    return;
  }

  Vector3 direction{};
  float distance = 0.0f;

  if (distanceSqr <= 0.0001f) {
    direction = {1.0f, 0.0f, 0.0f};
  } else {
    distance = std::sqrtf(distanceSqr);
    direction = Vector3Scale(delta, 1.0f / distance);
  }

  float overlap = minDistance - distance;
  Vector3 push = Vector3Scale(direction, overlap * 0.5f);

  position = Vector3Subtract(position, push);
  other.position = Vector3Add(other.position, push);

  float myVelocityIntoOther =
      velocity.x * direction.x + velocity.z * direction.z;
  if (myVelocityIntoOther > 0.0f) {
    velocity.x -= direction.x * myVelocityIntoOther;
    velocity.z -= direction.z * myVelocityIntoOther;
  }

  float otherVelocityIntoMe =
      other.velocity.x * -direction.x + other.velocity.z * -direction.z;
  if (otherVelocityIntoMe > 0.0f) {
    other.velocity.x += direction.x * otherVelocityIntoMe;
    other.velocity.z += direction.z * otherVelocityIntoMe;
  }

  updateHitbox();
  other.updateHitbox();
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

  Vector3 flatToPlayer{toPlayer.x, 0.0f, toPlayer.z};
  if (Vector3LengthSqr(flatToPlayer) <= 0.001f) {
    return true;
  }

  Vector3 viewDirection = Vector3Normalize(flatToPlayer);
  Vector3 flatFacing{facingDirection.x, 0.0f, facingDirection.z};

  if (Vector3LengthSqr(flatFacing) <= 0.001f) {
    flatFacing = {0.0f, 0.0f, 1.0f};
  }

  flatFacing = Vector3Normalize(flatFacing);

  float minVisionDot = std::cosf(visionHalfAngleDegrees * DEG2RAD_LOCAL);
  if (Vector3DotProduct(flatFacing, viewDirection) < minVisionDot) {
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
  moveToward(lastKnownPlayerPosition, speed, dt, level);
}

void Enemy::moveToward(Vector3 target, float targetSpeed, float dt,
                       const Level &level) {
  Vector3 toTarget = Vector3Subtract(target, position);
  toTarget.y = 0.0f;

  if (Vector3Length(toTarget) <= 0.001f) {
    stopHorizontalMovement(dt);
    return;
  }

  Vector3 direction = Vector3Normalize(toTarget);
  direction = steerAroundWalls(direction, level);
  facingDirection = direction;
  Vector3 targetVelocity{direction.x * targetSpeed, velocity.y,
                         direction.z * targetSpeed};
  float ease = 1.0f - std::expf(-acceleration * dt);

  velocity.x += (targetVelocity.x - velocity.x) * ease;
  velocity.z += (targetVelocity.z - velocity.z) * ease;
}

Vector3 Enemy::steerAroundWalls(Vector3 desiredDirection,
                                const Level &level) const {
  Vector3 origin{position.x, position.y + height * 0.45f, position.z};
  Vector3 forward{desiredDirection.x, 0.0f, desiredDirection.z};

  if (Vector3LengthSqr(forward) <= 0.001f) {
    return {0.0f, 0.0f, 1.0f};
  }

  forward = Vector3Normalize(forward);
  Vector3 right{forward.z, 0.0f, -forward.x};

  constexpr float feelerLength = 2.2f;
  constexpr float sideFeelerLength = 1.45f;
  constexpr float sideOffset = 0.45f;

  Vector3 steering = forward;

  auto addAvoidance = [&](Vector3 rayOrigin, Vector3 rayDirection,
                          float rayLength, Vector3 avoidDirection,
                          float weight) {
    Ray ray{};
    ray.position = rayOrigin;
    ray.direction = Vector3Normalize(rayDirection);

    Vector3 hitPoint{};
    float hitDistance = 0.0f;

    if (!Collision::rayLevel(ray, level, hitPoint, hitDistance)) {
      return;
    }

    if (hitDistance > rayLength) {
      return;
    }

    float urgency = 1.0f - hitDistance / rayLength;
    steering =
        Vector3Add(steering, Vector3Scale(avoidDirection, urgency * weight));
  };

  addAvoidance(origin, forward, feelerLength, Vector3Scale(forward, -1.0f),
               0.75f);
  addAvoidance(Vector3Add(origin, Vector3Scale(right, sideOffset)), forward,
               sideFeelerLength, Vector3Scale(right, -1.0f), 1.25f);
  addAvoidance(Vector3Subtract(origin, Vector3Scale(right, sideOffset)),
               forward, sideFeelerLength, right, 1.25f);

  steering.y = 0.0f;

  if (Vector3LengthSqr(steering) <= 0.001f) {
    return forward;
  }

  return Vector3Normalize(steering);
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
