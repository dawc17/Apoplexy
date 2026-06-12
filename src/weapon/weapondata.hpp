#pragma once

#include "raylib.h"

enum class WeaponModelId { Pistol, Shotgun, Count };

struct WeaponAmmoData {
  int magazineSize = 12;
  int maxReserveAmmo = 48;
  float reloadDuration = 1.1f;
  bool autoReloadWhenEmpty = true;
  bool reloadOneRoundAtATime = false;
};

struct WeaponFireData {
  int damage = 15;
  float range = 100.0f;
  float fireRate = 6.0f;
  bool automatic = false;
  int pelletCount = 1;
  float spreadDegrees = 0.25f;
  float movingSpreadDegrees = 0.75f;
  float sprintSpreadDegrees = 1.25f;
  float spreadBloomPerShot = 0.35f;
  float maxSpreadBloomDegrees = 2.5f;
  float spreadRecoverySpeed = 7.0f;
};

struct WeaponMeleeData {
  int damage = 5;
  float range = 1.65f;
  float radius = 0.45f;
  float windupDuration = 0.10f;
  float activeDuration = 0.08f;
  float recoveryDuration = 0.32f;
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
  WeaponModelId modelId = WeaponModelId::Pistol;

  WeaponFireData fire;
  WeaponAmmoData ammo;
  WeaponMeleeData melee;
  WeaponViewModelData viewModel;
};
