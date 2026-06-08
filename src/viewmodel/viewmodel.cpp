#include "viewmodel.hpp"

#include "../assets/assetmanager.hpp"

#include "raylib.h"
#include "raymath.h"
#include "rlgl.h"
#include "weapon/weapondata.hpp"

#include <algorithm>
#include <cmath>

namespace {
float easeOutCubic(float t) {
  t = std::clamp(t, 0.0f, 1.0f);
  float inverse = 1.0f - t;
  return 1.0f - inverse * inverse * inverse;
}

float easeInOutCubic(float t) {
  t = std::clamp(t, 0.0f, 1.0f);

  if (t < 0.5f) {
    return 4.0f * t * t * t;
  }

  float f = -2.0f * t + 2.0f;
  return 1.0f - (f * f * f) * 0.5f;
}

float easeInOutSine(float t) {
  t = std::clamp(t, 0.0f, 1.0f);
  return -(std::cosf(3.14159265f * t) - 1.0f) * 0.5f;
}

float recoilKickCurve(float t) {
  if (t < 0.10f) {
    return easeOutCubic(t / 0.10f) * 1.18f;
  }

  if (t < 0.58f) {
    float recover = easeInOutSine((t - 0.10f) / 0.48f);
    return 1.18f + (-0.16f - 1.18f) * recover;
  }

  float settle = easeInOutSine((t - 0.58f) / 0.42f);
  return -0.16f + (0.0f - -0.16f) * settle;
}

float recoilFollowThroughCurve(float t) {
  if (t < 0.08f) {
    return 0.0f;
  }

  if (t < 0.46f) {
    return easeOutCubic((t - 0.08f) / 0.38f);
  }

  float recover = easeInOutSine((t - 0.46f) / 0.54f);
  return 1.0f - recover;
}
} // namespace

void Viewmodel::reset() {
  recoilTimer = recoilDuration;
  recoilAmount = 0.0f;
  swayOffset = {0.0f, 0.0f};
  swayRotation = {0.0f, 0.0f};
  walkBobTimer = 0.0f;
  walkBobAmount = 0.0f;
  sprintAmount = 0.0f;
}

void Viewmodel::update(float dt, bool playerSprinting,
                       const WeaponProceduralAnimationData &procedural) {
  recoilTimer = std::min(recoilDuration, recoilTimer + dt);

  Vector2 mouseDelta = GetMouseDelta();

  Vector2 targetOffset{
      std::clamp(-mouseDelta.x * procedural.swayPositionAmount, -0.035f,
                 0.035f),
      std::clamp(mouseDelta.y * procedural.swayPositionAmount, -0.025f, 0.025f),
  };

  Vector2 targetRotation{
      std::clamp(-mouseDelta.x * procedural.swayRotationAmount, -4.0f, 4.0f),
      std::clamp(-mouseDelta.y * procedural.swayRotationAmount, -3.0f, 3.0f),
  };

  float follow = std::min(1.0f, dt * 16.0f);
  float settle = std::min(1.0f, dt * 10.0f);

  swayOffset.x += (targetOffset.x - swayOffset.x) * follow;
  swayOffset.y += (targetOffset.y - swayOffset.y) * follow;

  swayRotation.x += (targetRotation.x - swayRotation.x) * follow;
  swayRotation.y += (targetRotation.y - swayRotation.y) * follow;

  swayOffset.x += (0.0f - swayOffset.x) * settle;
  swayOffset.y += (0.0f - swayOffset.y) * settle;
  swayRotation.x += (0.0f - swayRotation.x) * settle;
  swayRotation.y += (0.0f - swayRotation.y) * settle;

  bool walking = IsKeyDown(KEY_W) || IsKeyDown(KEY_A) || IsKeyDown(KEY_S) ||
                 IsKeyDown(KEY_D);

  if (walking) {
    walkBobTimer += dt * procedural.walkBobSpeed;
  }

  float targetWalkBobAmount = walking ? 1.0f : 0.0f;
  float walkBobEase = 1.0f - std::expf(-8.0f * dt);
  walkBobAmount += (targetWalkBobAmount - walkBobAmount) * walkBobEase;

  float targetSprintAmount = playerSprinting ? 1.0f : 0.0f;
  float sprintEase = 1.0f - std::expf(-10.0f * dt);
  sprintAmount += (targetSprintAmount - sprintAmount) * sprintEase;
}

void Viewmodel::addRecoil(float amount) {
  recoilTimer = 0.0f;
  recoilAmount = amount;
}

void Viewmodel::draw(const Camera3D &, const WeaponData &weapon,
                     const AssetManager &assets, float muzzleFlashTimer,
                     float muzzleFlashRotation) const {
  const Model &gun = assets.getGunModel();
  const Texture2D &flash = assets.getMuzzleFlashTexture();

  Camera3D viewCamera{};
  viewCamera.position = {0.0f, 0.0f, 0.0f};
  viewCamera.target = {0.0f, 0.0f, 1.0f};
  viewCamera.up = {0.0f, 1.0f, 0.0f};
  viewCamera.fovy = 70;
  viewCamera.projection = CAMERA_PERSPECTIVE;

  float recoilProgress =
      recoilDuration > 0.0f ? recoilTimer / recoilDuration : 1.0f;
  float recoilKick = recoilKickCurve(recoilProgress) * recoilAmount;
  float recoilFollowThrough =
      recoilFollowThroughCurve(recoilProgress) * recoilAmount;

  float kick = recoilKick * weapon.recoil.kick;

  float bobX =
      std::sinf(walkBobTimer) * weapon.procedural.walkBobX * walkBobAmount;
  float bobY = std::fabs(std::cosf(walkBobTimer)) * weapon.procedural.walkBobY *
               walkBobAmount;

  bobX += std::sinf(walkBobTimer * 0.75f) * 0.018f * sprintAmount;
  bobY += std::fabs(std::cosf(walkBobTimer * 0.75f)) * 0.020f * sprintAmount;

  Vector3 sprintOffset{
      weapon.procedural.sprintOffset.x * sprintAmount,
      weapon.procedural.sprintOffset.y * sprintAmount,
      weapon.procedural.sprintOffset.z * sprintAmount,
  };

  Vector3 position{
      weapon.viewModel.holdPosition.x + sprintOffset.x + swayOffset.x + bobX -
          recoilFollowThrough * 0.012f,
      weapon.viewModel.holdPosition.y + sprintOffset.y + swayOffset.y - bobY -
          recoilKick * 0.030f + recoilFollowThrough * 0.010f,
      weapon.viewModel.holdPosition.z + sprintOffset.z - kick +
          recoilFollowThrough * 0.020f,
  };

  rlDrawRenderBatchActive();
  BeginMode3D(viewCamera);
  rlDrawRenderBatchActive();
  rlDisableDepthTest();

  rlPushMatrix();
  rlTranslatef(position.x, position.y, position.z);

  rlRotatef(swayRotation.y, 1.0f, 0.0f, 0.0f);
  rlRotatef(swayRotation.x, 0.0f, 1.0f, 0.0f);
  rlRotatef(-swayRotation.x * 0.35f, 0.0f, 0.0f, 1.0f);

  rlRotatef(-recoilKick * weapon.recoil.pitchDegrees +
                recoilFollowThrough * 2.5f,
            1.0f, 0.0f, 0.0f);

  rlRotatef(-recoilFollowThrough * 1.6f, 0.0f, 1.0f, 0.0f);
  rlRotatef(recoilKick * 2.4f - recoilFollowThrough * 1.2f, 0.0f, 0.0f, 1.0f);

  rlRotatef(weapon.viewModel.holdRotationDegrees.y, 0.0f, 1.0f, 0.0f);
  rlRotatef(weapon.viewModel.holdRotationDegrees.x, 1.0f, 0.0f, 0.0f);
  rlRotatef(weapon.viewModel.holdRotationDegrees.z, 0.0f, 0.0f, 1.0f);

  rlRotatef(0.0f * sprintAmount, 1.0f, 0.0f, 0.0f);
  rlRotatef(weapon.procedural.spritRotationDegrees.y * sprintAmount, 0.0f, 1.0f,
            0.0f);
  rlRotatef(weapon.procedural.spritRotationDegrees.z * sprintAmount, 0.0f, 0.0f,
            1.0f);
  DrawModel(gun, {0.0f, 0.0f, 0.0f}, weapon.viewModel.modelScale, WHITE);

  if (muzzleFlashTimer > 0.0f) {
    float t = muzzleFlashTimer / 0.05f;

    Rectangle source{0.0f, 0.0f, static_cast<float>(flash.width),
                     static_cast<float>(flash.height)};

    Vector2 size{weapon.viewModel.muzzleFlashWidth * t,
                 weapon.viewModel.muzzleFlashHeight * t};
    Vector2 origin{size.x * 0.5f, size.y * 0.5f};

    BeginBlendMode(BLEND_ADDITIVE);
    DrawBillboardPro(viewCamera, flash, source, weapon.viewModel.muzzlePoint,
                     {0.0f, 1.0f, 0.0f}, size, origin, muzzleFlashRotation,
                     Fade(WHITE, 0.95f * t));
    EndBlendMode();
  }

  rlPopMatrix();
  EndMode3D();

  rlEnableDepthTest();
}
