#pragma once

#include "raylib.h"

class Player;
class Level;

enum class EnemyState {
  Idle,
  Suspicious,
  Alert,
  Chase,
  Search,
  AttackWindup,
  AttackRecovery,
  Dead
};

class Enemy {
public:
  explicit Enemy(Vector3 spawnPosition);

  void update(float dt, Player &player, const Level &level);
  void updateEditorTest(float dt, const Level &level);
  void draw(const Model &model) const;

  void resolveOverlap(Enemy &other);

  bool hearNoise(Vector3 sourcePosition, float noiseRadius, const Level &level);

  bool applyDamage(int damage);
  bool applyDamage(int damage, Vector3 threatPosition);
  void applyKnockback(Vector3 direction, float impulse, float lift);

  bool isAlive() const;
  EnemyState getState() const;
  BoundingBox getHitbox() const;
  Vector3 getPosition() const;
  Vector3 getVelocity() const;
  Vector3 getInvestigationTarget() const;

private:
  void updateHitbox();
  bool canSeePlayer(const Player &player, const Level &level) const;
  void reactToThreat(Vector3 threatPosition);
  void chasePlayer(float dt, const Player &player, const Level &level);
  void moveToward(Vector3 target, float targetSpeed, float dt,
                  const Level &level);
  Vector3 steerAroundWalls(Vector3 desiredDirection, const Level &level) const;
  void stopHorizontalMovement(float dt);
  void attackPlayer(float dt, Player &player);

private:
  Vector3 position{};
  Vector3 velocity{};
  Vector3 facingDirection{0.0f, 0.0f, 1.0f};

  float radius = 0.4f;
  float height = 1.88f;
  float speed = 2.5f;
  float searchSpeed = 1.8f;
  float acceleration = 14.0f;
  float gravity = 24.0f;
  bool grounded = false;

  int health = 30;
  int maxHealth = 30;

  float hitFlashTimer = 0.0f;
  float hitFlashDuration = 0.10f;

  float attackRange = 1.4f;
  float attackCooldown = 0.0f;
  float attackWindupDuration = 0.34f;
  float attackRecoveryDuration = 0.52f;

  float visionRange = 18.0f;
  float visionHalfAngleDegrees = 55.0f;
  float suspicionDuration = 0.65f;
  float loseSightGrace = 0.45f;
  float searchDuration = 2.2f;
  float alertDuration = 0.22f;
  float stateTimer = 0.0f;
  float timeSinceSeenPlayer = 999.0f;
  Vector3 lastKnownPlayerPosition{};

  EnemyState state = EnemyState::Idle;

  BoundingBox hitbox{};
};
