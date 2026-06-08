#pragma once

#include "raylib.h"

struct WeaponAmmoData {
  int magazineSize = 12;
  int maxReserveAmmo = 48;
  float reloadDuration = 1.1f;
  bool autoReloadWhenEmpty = true;
};

struct WeaponFireData {
  int damage = 15;
  float range = 100.0f;
  float fireRate = 6.0f;
  bool automatic = false;
};

struct WeaponViewModelData {
  Vector3 holdPosition{};
  Vector3 holdRotationDegrees{};
  float modelScale = 1.0f;

  Vector3 muzzlePoint{};
  float muzzleFlashWidth = 1.0f;
  float muzzleFlashHeight = 1.0f;
};

struct WeaponRecoilData {
  float kick = 0.06f;
  float pitchDegrees = 14.0f;
};

struct WeaponProceduralAnimationData {
  float swayPositionAmount = 0.0008f;
  float swayRotationAmount = 0.045f;

  float walkBobSpeed = 8.0f;
  float walkBobX = 0.035f;
  float walkBobY = 0.018f;

  Vector3 sprintOffset{0.025f, -0.09f, -0.02f};
  Vector3 spritRotationDegrees{0.0f, 10.0f, -35.0f};
};

struct WeaponData {
  const char *name = "";

  WeaponFireData fire;
  WeaponAmmoData ammo;
  WeaponViewModelData viewModel;
  WeaponRecoilData recoil;
  WeaponProceduralAnimationData procedural;
};
