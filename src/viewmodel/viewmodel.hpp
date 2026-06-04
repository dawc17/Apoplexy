#pragma once

#include "../weapon/weapondata.hpp"

#include "raylib.h"

class AssetManager;

class Viewmodel {
public:
  void reset();
  void update(float dt);
  void addRecoil(float amount);

  void draw(const Camera3D &camera, const WeaponData &weapon,
            const AssetManager &assets, float muzzleFlashTimer,
            float muzzleFlashRotation) const;

private:
  float recoil = 0.0f;
  Vector2 swayOffset{};
  Vector2 swayRotation{};
  float walkBobTimer = 0.0f;
  float walkBobAmount = 0.0f;
};
