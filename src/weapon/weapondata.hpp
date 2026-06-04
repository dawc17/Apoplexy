#pragma once

#include "raylib.h"

struct WeaponData {
  const char *name = "";

  Vector3 holdPosition{};
  Vector3 holdRotationDegrees{};
  float modelScale = 1.0f;

  Vector3 muzzlePoint{};
  float muzzleFlashWidth = 1.0f;
  float muzzleFlashHeight = 1.0f;

  int damage = 15;
  float range = 100.0f;
  float fireRate = 6.0f;

  float recoilKick = 0.06f;
  float recoilPitchDegrees = 14.0f;
};
