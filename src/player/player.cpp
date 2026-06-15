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

  headBobTimer = 0.0f;
  headBobAmount = 0.0f;
  sprintFovAmount = 0.0f;
  crouchAmount = 0.0f;

  health = maxHealth;
  damageTaken = false;
  sprintBlockedByShot = false;
  sprinting = false;
  grounded = false;
  crouching = false;
}

void Player::update(float dt, const Level &level) {
  updateMouseLook(dt);
  updateMovement(dt, level);
}

void Player::applyDamage(int damage) {
  if (damage <= 0 || health <= 0) {
    return;
  }

  health -= damage;
  damageTaken = true;

  if (health < 0) {
    health = 0;
  }
}

Camera3D Player::getCamera() const {
  Vector3 eye = getEyePosition();

  float bobY = std::sinf(headBobTimer) * 0.06f * headBobAmount;
  float bobX = std::cosf(headBobTimer * 0.5f) * 0.032f * headBobAmount;

  Vector3 right{std::cosf(yaw), 0.0f, -std::sinf(yaw)};

  eye.y += bobY;
  eye = Vector3Add(eye, Vector3Scale(right, bobX));

  Vector3 forward = Math::forwardFromYawPitch(yaw, pitch);

  Camera3D camera{};
  camera.position = eye;
  camera.target = Vector3Add(eye, forward);
  camera.up = {0.0f, 1.0f, 0.0f};
  camera.fovy = 90.0f + 10.0f * sprintFovAmount;
  camera.projection = CAMERA_PERSPECTIVE;

  return camera;
}

Vector3 Player::getPosition() const { return position; }

Vector3 Player::getEyePosition() const {
  float standingEyeHeight = height * 0.85f;
  float crouchingEyeHeight = height * 0.5f;
  float eyeHeight =
      standingEyeHeight + (crouchingEyeHeight - standingEyeHeight) * crouchAmount;
  return {position.x, position.y + eyeHeight, position.z};
}

// getters
float Player::getYaw() const { return yaw; }

float Player::getPitch() const { return pitch; }

float Player::getRadius() const { return radius; }

float Player::getHorizontalSpeed() const {
  return std::sqrtf(velocity.x * velocity.x + velocity.z * velocity.z);
}

int Player::getHealth() const { return health; }

bool Player::isDead() const { return health <= 0; }

bool Player::isGrounded() const { return grounded; }

bool Player::isSprinting() const { return sprinting; }

bool Player::isCrouching() const { return crouching; }

bool Player::consumeDamageTaken() {
  bool taken = damageTaken;
  damageTaken = false;
  return taken;
}

void Player::updateMouseLook(float dt) {
  Vector2 mouseDelta = GetMouseDelta();

  yaw -= mouseDelta.x * Constants::MOUSE_SENSITIVITY;
  pitch -= mouseDelta.y * Constants::MOUSE_SENSITIVITY;

  pitch = std::clamp(pitch, -1.5f, 1.5f);
}

void Player::updateMovement(float dt, const Level &level) {
  Vector3 moveDir{0.0f, 0.0f, 0.0f};

  Vector3 forward{std::sinf(yaw), 0.0f, std::cosf(yaw)};

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

  float targetSpeed = moveSpeed;
  bool shooting = IsMouseButtonDown(MOUSE_BUTTON_LEFT);
  bool holdingSprint = IsKeyDown(KEY_LEFT_SHIFT);
  bool holdingCrouch = IsKeyDown(KEY_LEFT_CONTROL) || IsKeyDown(KEY_C);

  crouching = holdingCrouch && grounded;

  if (!holdingSprint) {
    sprintBlockedByShot = false;
  } else if (shooting) {
    sprintBlockedByShot = true;
  }

  sprinting =
      holdingSprint && !crouching && !sprintBlockedByShot && Vector3Length(moveDir) > 0.0f;

  if (sprinting) {
    targetSpeed = moveSpeed * 1.45f;
  } else if (crouching) {
    targetSpeed = moveSpeed * 0.42f;
  }

  float targetSprintFovAmount = sprinting ? 1.0f : 0.0f;
  float sprintFovEase = 1.0f - std::expf(-7.0 * dt);
  sprintFovAmount += (targetSprintFovAmount - sprintFovAmount) * sprintFovEase;

  float targetCrouchAmount = crouching ? 1.0f : 0.0f;
  float crouchEase = 1.0f - std::expf(-12.0f * dt);
  crouchAmount += (targetCrouchAmount - crouchAmount) * crouchEase;

  Vector3 targetVelocity{moveDir.x * targetSpeed, velocity.y,
                         moveDir.z * targetSpeed};

  float acceleration = Vector3Length(moveDir) > 0.0f ? 32.0f : 42.0f;
  float velocityEase = 1.0f - std::expf(-acceleration * dt);

  velocity.x += (targetVelocity.x - velocity.x) * velocityEase;
  velocity.z += (targetVelocity.z - velocity.z) * velocityEase;

  if (!grounded || velocity.y > 0.0f) {
    velocity.y -= gravity * dt;
  } else {
    velocity.y = 0.0f;
  }

  float horizontalSpeed =
      std::sqrtf(velocity.x * velocity.x + velocity.z * velocity.z);

  float speedPercent = std::clamp(horizontalSpeed / targetSpeed, 0.0f, 1.0f);

  if (horizontalSpeed > 0.1f) {
    // bob speed
    headBobTimer += dt * (10.0f + 8.0f * speedPercent);
  }

  float sprintBobBoost = sprinting ? 1.18f : 1.0f;
  float crouchBobScale = crouching ? 0.35f : 1.0f;
  float targetHeadBobAmount = speedPercent * sprintBobBoost;
  targetHeadBobAmount *= crouchBobScale;
  float headBobEase = 1.0f - std::expf(-8.0f * dt);
  headBobAmount += (targetHeadBobAmount - headBobAmount) * headBobEase;

  Collision::MoveResult move = Collision::moveCylinderLevel(
      position, velocity, radius, height, level, dt);
  position = move.position;
  velocity = move.velocity;
  grounded = move.grounded;
}
