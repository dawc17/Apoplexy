#pragma once

#include "proceduralweaponanimation.hpp"

namespace ProceduralWeaponAnimationCatalog {
inline const ProceduralWeaponAnimationData Pistol{
    0.0008f,                   // swayPositionAmount
    0.045f,                    // swayRotationAmount
    {0.035f, 0.025f},          // swayPositionClamp
    {4.0f, 3.0f},              // swayRotationClamp
    -0.35f,                    // swayRollAmount
    16.0f,                     // swayFollowSpeed
    10.0f,                     // swaySettleSpeed
    8.0f,                      // walkBobSpeed
    0.035f,                    // walkBobX
    0.018f,                    // walkBobY
    8.0f,                      // walkBobEaseSpeed
    0.75f,                     // sprintBobSpeedScale
    0.018f,                    // sprintBobX
    0.020f,                    // sprintBobY
    {0.025f, -0.09f, -0.02f},  // sprintOffset
    {35.0f, 10.0f, 0.0f},      // sprintRotationDegrees
    10.0f,                     // sprintEaseSpeed
    0.36f,                     // recoilDuration
    {0.0f, -0.030f, -0.06f},   // recoilKickOffset
    {-0.012f, 0.010f, 0.020f}, // recoilFollowThroughOffset
    {-20.0f, 0.0f, 2.4f},      // recoilKickRotationDegrees
    {2.5f, -1.6f, -1.2f},      // recoilFollowThroughRotationDegrees
    {1080.0f, 0.0f, 0.0f},     // reloadSpinDegreesPerSecond
    18.0f,                     // reloadSpinEaseInSpeed
    12.0f,                     // reloadSpinEaseOutSpeed
    1.65f,                     // idleBobSpeed
    0.006f,                    // idleBobY
    3.5f,                      // idleBobEaseSpeed
};
} // namespace ProceduralWeaponAnimationCatalog
