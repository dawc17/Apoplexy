#include "viewmodel.hpp"

#include "../assets/assetmanager.hpp"

#include "raymath.h"
#include "rlgl.h"

#include <algorithm>
#include <cmath>

void Viewmodel::reset() {
  recoil = 0.0f;
  swayOffset = {0.0f, 0.0f};
  swayRotation = {0.0f, 0.0f};
  walkBobTimer = 0.0f;
  walkBobAmount = 0.0f;
}

void Viewmodel::update(float dt) {
  recoil = std::max(0.0f, recoil - dt * 8.0f);

  Vector2 mouseDelta = GetMouseDelta();

  Vector2 targetOffset{
      std::clamp(-mouseDelta.x * 0.0008f, -0.035f, 0.035f),
      std::clamp(mouseDelta.y * 0.0008f, -0.025f, 0.025f),
  };

  Vector2 targetRotation{
      std::clamp(-mouseDelta.x * 0.045f, -4.0f, 4.0f),
      std::clamp(-mouseDelta.y * 0.035f, -3.0f, 3.0f),
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
    walkBobTimer += dt * 8.0f;
    walkBobAmount = std::min(1.0f, walkBobAmount + dt * 8.0f);
  } else {
    walkBobAmount = std::max(0.0f, walkBobAmount - dt * 8.0f);
  }
}

void Viewmodel::addRecoil(float amount) { recoil = amount; }

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

  float kick = recoil * weapon.recoilKick;
  float bobX = std::sinf(walkBobTimer) * 0.035f * walkBobAmount;
  float bobY = std::fabs(std::cosf(walkBobTimer)) * 0.018f * walkBobAmount;

  Vector3 position{
      weapon.holdPosition.x + swayOffset.x + bobX,
      weapon.holdPosition.y + swayOffset.y - bobY - recoil * 0.025f,
      weapon.holdPosition.z - kick,
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

  rlRotatef(-recoil * weapon.recoilPitchDegrees, 1.0f, 0.0f, 0.0f);
  rlRotatef(weapon.holdRotationDegrees.y, 0.0f, 1.0f, 0.0f);
  rlRotatef(weapon.holdRotationDegrees.x, 1.0f, 0.0f, 0.0f);
  rlRotatef(weapon.holdRotationDegrees.z, 0.0f, 0.0f, 1.0f);

  DrawModel(gun, {0.0f, 0.0f, 0.0f}, weapon.modelScale, WHITE);

  if (muzzleFlashTimer > 0.0f) {
    float t = muzzleFlashTimer / 0.05f;

    Rectangle source{0.0f, 0.0f, static_cast<float>(flash.width),
                     static_cast<float>(flash.height)};

    Vector2 size{weapon.muzzleFlashWidth * t, weapon.muzzleFlashHeight * t};
    Vector2 origin{size.x * 0.5f, size.y * 0.5f};

    BeginBlendMode(BLEND_ADDITIVE);
    DrawBillboardPro(viewCamera, flash, source, weapon.muzzlePoint,
                     {0.0f, 1.0f, 0.0f}, size, origin, muzzleFlashRotation,
                     Fade(WHITE, 0.95f * t));
    EndBlendMode();
  }

  rlPopMatrix();
  EndMode3D();

  rlEnableDepthTest();
}
