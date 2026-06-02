#include "player.hpp"

#include "../collision/collision.hpp"
#include "../core/constants.hpp"
#include "../level/level.hpp"
#include "../util/math.hpp"

#include <algorithm>
#include <cmath>
#include <raylib.h>
#include <raymath.h>

Player::Player() { reset({0.0f, 1.0f, 0.0f}); }

void Player::reset(Vector3 spawnPostion) {
  position = spawnPostion;
  velocity = {0.0f, 0.0f, 0.0f};

  yaw = 0.0f;
  pitch = 0.0f;

  health = maxHealth;
}

void Player::update(float dt, const Level &level) {
  updateMouseLook(dt);
  updateMovement(dt, level);
}

void Player::applyDamage(int damage) {
  health -= damage;

  if (health < 0) {
    health = 0;
  }
}

Camera3D Player::getCamera() const {
  Vector3 eye = getEyePosition();
  Vector3 forward = Math::forwardFromYawPitch(yaw, pitch);

  Camera3D camera{};
  camera.position = eye;
  camera.target = Vector3Add(eye, forward);
  camera.up = {0.0f, 1.0f, 0.0f};
  camera.fovy = 90.0f;
  camera.projection = CAMERA_PERSPECTIVE;

  return camera;
}

Vector3 Player::getPosition() const { return position; }

Vector3 Player::getEyePosition() const {
  return {position.x, position.y + height * 0.85f, position.z};
}

// getters
float Player::getYaw() const { return yaw; }

float Player::getPitch() const { return pitch; }

float Player::getRadius() const { return radius; }

int Player::getHealth() const { return health; }

bool Player::isDead() const { return health <= 0; }

void Player::updateMouseLook(float dt) {
  Vector2 mouseDelta = GetMouseDelta();

  yaw -= mouseDelta.x * Constants::MOUSE_SENSITIVITY;
  pitch -= mouseDelta.y * Constants::MOUSE_SENSITIVITY;

  pitch = std::clamp(pitch, -1.5f, 1.5f);
}

void Player::updateMovement(float dt, const Level &level) {
  Vector3 moveDir{0.0f, 0.0f, 0.0f};

  Vector3 forward{std::sin(yaw), 0.0f, std::cos(yaw)};

  Vector3 right{forward.z, 0.0f, -forward.x};

  if (IsKeyDown(KEY_W)) {
    moveDir = Vector3Add(moveDir, forward);
  }

  if (IsKeyDown(KEY_S)) {
    moveDir = Vector3Subtract(moveDir, forward);
  }

  if (IsKeyDown(KEY_D)) {
    moveDir = Vector3Subtract(moveDir, right);
  }

  if (IsKeyDown(KEY_A)) {
    moveDir = Vector3Add(moveDir, right);
  }

  // stopping the player from moving faster on diagonals, classic
  if (Vector3Length(moveDir) > 0.0f) {
    moveDir = Vector3Normalize(moveDir);
  }

  velocity.x = moveDir.x * moveSpeed;
  velocity.z = moveDir.z * moveSpeed;

  position = Collision::moveSphereLevel(position, velocity, radius, level, dt);
}
