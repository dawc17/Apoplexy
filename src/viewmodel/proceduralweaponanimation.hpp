#pragma once

#include "raylib.h"

struct ProceduralWeaponAnimationData {
  float swayPositionAmount = 0.0008f;
  float swayRotationAmount = 0.045f;
  Vector2 swayPositionClamp{0.035f, 0.025f};
  Vector2 swayRotationClamp{4.0f, 3.0f};
  float swayRollAmount = -0.35f;
  float swayFollowSpeed = 16.0f;
  float swaySettleSpeed = 10.0f;

  float walkBobSpeed = 8.0f;
  float walkBobX = 0.035f;
  float walkBobY = 0.018f;
  float walkBobEaseSpeed = 8.0f;

  float sprintBobSpeedScale = 0.75f;
  float sprintBobX = 0.018f;
  float sprintBobY = 0.020f;

  Vector3 sprintOffset{0.025f, -0.09f, -0.02f};
  Vector3 sprintRotationDegrees{0.0f, 10.0f, -35.0f};
  float sprintEaseSpeed = 10.0f;

  float recoilDuration = 0.36f;
  Vector3 recoilKickOffset{0.0f, -0.030f, -0.06f};
  Vector3 recoilFollowThroughOffset{-0.012f, 0.010f, 0.020f};
  Vector3 recoilKickRotationDegrees{-20.0f, 0.0f, 2.4f};
  Vector3 recoilFollowThroughRotationDegrees{2.5f, -1.6f, -1.2f};

  Vector3 reloadSpinDegreesPerSecond{0.0f, 0.0f, 0.0f};
  float reloadSpinEaseInSpeed = 18.0f;
  float reloadSpinEaseOutSpeed = 12.0f;

  float idleBobSpeed = 1.65f;
  float idleBobY = 0.006f;
  float idleBobEaseSpeed = 3.5f;

  Vector3 meleeExtendOffset{0.04f, 0.10f, 0.22f};
  Vector3 meleeExtendRotationDegrees{-12.0f, -8.0f, 8.0f};
  Vector3 meleeSwingOffset{0.16f, -0.34f, -0.10f};
  Vector3 meleeSwingRotationDegrees{46.0f, 14.0f, -34.0f};
  float meleeEaseInSpeed = 28.0f;
  float meleeEaseOutSpeed = 14.0f;
};
