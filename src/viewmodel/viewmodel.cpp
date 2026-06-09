#include "viewmodel.hpp"

#include "../assets/assetmanager.hpp"
#include "../render/lighting.hpp"
#include "viewmodeldebug.hpp"
#include "raylib.h"
#include "rlgl.h"
#include "external/glad.h"
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
  recoilTimer = 1.0f;
  recoilAmount = 0.0f;
  swayOffset = {0.0f, 0.0f};
  swayRotation = {0.0f, 0.0f};
  walkBobTimer = 0.0f;
  walkBobAmount = 0.0f;
  sprintAmount = 0.0f;
  reloadAmount = 0.0f;
  reloadSpinRotationDegrees = {0.0f, 0.0f, 0.0f};
}

void Viewmodel::update(float dt, bool playerSprinting, bool weaponReloading,
                       const ProceduralWeaponAnimationData &procedural) {
  recoilTimer = std::min(procedural.recoilDuration, recoilTimer + dt);

  Vector2 mouseDelta = GetMouseDelta();

  Vector2 targetOffset{
      std::clamp(-mouseDelta.x * procedural.swayPositionAmount,
                 -procedural.swayPositionClamp.x,
                 procedural.swayPositionClamp.x),
      std::clamp(mouseDelta.y * procedural.swayPositionAmount,
                 -procedural.swayPositionClamp.y,
                 procedural.swayPositionClamp.y),
  };

  Vector2 targetRotation{
      std::clamp(-mouseDelta.x * procedural.swayRotationAmount,
                 -procedural.swayRotationClamp.x,
                 procedural.swayRotationClamp.x),
      std::clamp(-mouseDelta.y * procedural.swayRotationAmount,
                 -procedural.swayRotationClamp.y,
                 procedural.swayRotationClamp.y),
  };

  float follow = std::min(1.0f, dt * procedural.swayFollowSpeed);
  float settle = std::min(1.0f, dt * procedural.swaySettleSpeed);

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
  float walkBobEase = 1.0f - std::expf(-procedural.walkBobEaseSpeed * dt);
  walkBobAmount += (targetWalkBobAmount - walkBobAmount) * walkBobEase;

  float targetSprintAmount = playerSprinting ? 1.0f : 0.0f;
  float sprintEase = 1.0f - std::expf(-procedural.sprintEaseSpeed * dt);
  sprintAmount += (targetSprintAmount - sprintAmount) * sprintEase;

  float targetReloadAmount = weaponReloading ? 1.0f : 0.0f;
  float reloadEaseSpeed = weaponReloading ? procedural.reloadSpinEaseInSpeed
                                          : procedural.reloadSpinEaseOutSpeed;
  float reloadEase = 1.0f - std::expf(-reloadEaseSpeed * dt);
  reloadAmount += (targetReloadAmount - reloadAmount) * reloadEase;

  if (weaponReloading) {
    float spinAmount = reloadAmount * reloadAmount;

    reloadSpinRotationDegrees.x =
        std::fmod(reloadSpinRotationDegrees.x +
                      procedural.reloadSpinDegreesPerSecond.x * dt * spinAmount,
                  360.0f);
    reloadSpinRotationDegrees.y =
        std::fmod(reloadSpinRotationDegrees.y +
                      procedural.reloadSpinDegreesPerSecond.y * dt * spinAmount,
                  360.0f);
    reloadSpinRotationDegrees.z =
        std::fmod(reloadSpinRotationDegrees.z +
                      procedural.reloadSpinDegreesPerSecond.z * dt * spinAmount,
                  360.0f);
  } else if (reloadAmount < 0.001f) {
    reloadAmount = 0.0f;
    reloadSpinRotationDegrees = {0.0f, 0.0f, 0.0f};
  }
}

void Viewmodel::addRecoil(float amount) {
  recoilTimer = 0.0f;
  recoilAmount = amount;
}

void Viewmodel::draw(const Camera3D &, const WeaponData &weapon,
                     const ProceduralWeaponAnimationData &procedural,
                     const AssetManager &assets, float muzzleFlashTimer,
                     float muzzleFlashRotation,
                     const Lighting::SceneLighting &lighting,
                     Vector3 pointLightContribution) const {
  const Model &gun = assets.getGunModel();
  const Texture2D &flash = assets.getMuzzleFlashTexture();
  Shader viewmodelShader = assets.getViewmodelShader();

  Vector2 virtualResolution{320.0f, 180.0f};
  Vector3 lightDirection = lighting.sun.direction;
  float ambientStrength = Lighting::clampIntensity(lighting.ambientIntensity);
  float diffuseStrength = Lighting::clampIntensity(lighting.sun.intensity);

  SetShaderValue(viewmodelShader,
                 GetShaderLocation(viewmodelShader, "virtualResolution"),
                 &virtualResolution, SHADER_UNIFORM_VEC2);
  SetShaderValue(viewmodelShader,
                 GetShaderLocation(viewmodelShader, "lightDirection"),
                 &lightDirection, SHADER_UNIFORM_VEC3);
  SetShaderValue(viewmodelShader,
                 GetShaderLocation(viewmodelShader, "ambientStrength"),
                 &ambientStrength, SHADER_UNIFORM_FLOAT);
  SetShaderValue(viewmodelShader,
                 GetShaderLocation(viewmodelShader, "diffuseStrength"),
                 &diffuseStrength, SHADER_UNIFORM_FLOAT);
  SetShaderValue(viewmodelShader,
                 GetShaderLocation(viewmodelShader, "pointLightContribution"),
                 &pointLightContribution, SHADER_UNIFORM_VEC3);

  Camera3D viewCamera{};
  viewCamera.position = {0.0f, 0.0f, 0.0f};
  viewCamera.target = {0.0f, 0.0f, 1.0f};
  viewCamera.up = {0.0f, 1.0f, 0.0f};
  viewCamera.fovy = 70;
  viewCamera.projection = CAMERA_PERSPECTIVE;

  float recoilProgress =
      procedural.recoilDuration > 0.0f
          ? recoilTimer / procedural.recoilDuration
          : 1.0f;
  float recoilKick = recoilKickCurve(recoilProgress) * recoilAmount;
  float recoilFollowThrough =
      recoilFollowThroughCurve(recoilProgress) * recoilAmount;

  float bobX = std::sinf(walkBobTimer) * procedural.walkBobX * walkBobAmount;
  float bobY =
      std::fabs(std::cosf(walkBobTimer)) * procedural.walkBobY * walkBobAmount;

  bobX += std::sinf(walkBobTimer * procedural.sprintBobSpeedScale) *
          procedural.sprintBobX * sprintAmount;
  bobY += std::fabs(std::cosf(walkBobTimer * procedural.sprintBobSpeedScale)) *
          procedural.sprintBobY * sprintAmount;

  Vector3 sprintOffset{
      procedural.sprintOffset.x * sprintAmount,
      procedural.sprintOffset.y * sprintAmount,
      procedural.sprintOffset.z * sprintAmount,
  };

  float reloadPose = easeInOutCubic(reloadAmount);

  Vector3 position{
      weapon.viewModel.holdPosition.x + sprintOffset.x + swayOffset.x + bobX -
          recoilKick * procedural.recoilKickOffset.x +
          recoilFollowThrough * procedural.recoilFollowThroughOffset.x,
      weapon.viewModel.holdPosition.y + sprintOffset.y + swayOffset.y - bobY -
          recoilKick * procedural.recoilKickOffset.y +
          recoilFollowThrough * procedural.recoilFollowThroughOffset.y,
      weapon.viewModel.holdPosition.z + sprintOffset.z +
          recoilKick * procedural.recoilKickOffset.z +
          recoilFollowThrough * procedural.recoilFollowThroughOffset.z,
  };
  position.x += ViewmodelDebug::positionOffset.x;
  position.y += ViewmodelDebug::positionOffset.y;
  position.z += ViewmodelDebug::positionOffset.z;

  rlDrawRenderBatchActive();
  BeginMode3D(viewCamera);
  rlDrawRenderBatchActive();
  glClear(GL_DEPTH_BUFFER_BIT);
  rlEnableDepthTest();

  rlPushMatrix();
  rlTranslatef(position.x, position.y, position.z);

  rlRotatef(swayRotation.y, 1.0f, 0.0f, 0.0f);
  rlRotatef(swayRotation.x, 0.0f, 1.0f, 0.0f);
  rlRotatef(swayRotation.x * procedural.swayRollAmount, 0.0f, 0.0f, 1.0f);

  rlRotatef(recoilKick * procedural.recoilKickRotationDegrees.x +
                recoilFollowThrough *
                    procedural.recoilFollowThroughRotationDegrees.x,
            1.0f, 0.0f, 0.0f);
  rlRotatef(recoilKick * procedural.recoilKickRotationDegrees.y +
                recoilFollowThrough *
                    procedural.recoilFollowThroughRotationDegrees.y,
            0.0f, 1.0f, 0.0f);
  rlRotatef(recoilKick * procedural.recoilKickRotationDegrees.z +
                recoilFollowThrough *
                    procedural.recoilFollowThroughRotationDegrees.z,
            0.0f, 0.0f, 1.0f);

  rlRotatef(weapon.viewModel.holdRotationDegrees.y, 0.0f, 1.0f, 0.0f);
  rlRotatef(weapon.viewModel.holdRotationDegrees.x, 1.0f, 0.0f, 0.0f);
  rlRotatef(weapon.viewModel.holdRotationDegrees.z, 0.0f, 0.0f, 1.0f);

  rlRotatef(ViewmodelDebug::rotationOffsetDegrees.y, 0.0f, 1.0f, 0.0f);
  rlRotatef(ViewmodelDebug::rotationOffsetDegrees.x, 1.0f, 0.0f, 0.0f);
  rlRotatef(ViewmodelDebug::rotationOffsetDegrees.z, 0.0f, 0.0f, 1.0f);

  rlRotatef(reloadSpinRotationDegrees.y * reloadPose, 0.0f, 1.0f, 0.0f);
  rlRotatef(reloadSpinRotationDegrees.x * reloadPose, 1.0f, 0.0f, 0.0f);
  rlRotatef(reloadSpinRotationDegrees.z * reloadPose, 0.0f, 0.0f, 1.0f);

  rlRotatef(procedural.sprintRotationDegrees.x * sprintAmount, 1.0f, 0.0f,
            0.0f);
  rlRotatef(procedural.sprintRotationDegrees.y * sprintAmount, 0.0f, 1.0f,
            0.0f);
  rlRotatef(procedural.sprintRotationDegrees.z * sprintAmount, 0.0f, 0.0f,
            1.0f);
  DrawModel(gun, {0.0f, 0.0f, 0.0f},
            weapon.viewModel.modelScale * ViewmodelDebug::scaleMultiplier,
            WHITE);

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
