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

struct WeaponData {
  const char *name = "";

  WeaponFireData fire;
  WeaponAmmoData ammo;
  WeaponViewModelData viewModel;
};
