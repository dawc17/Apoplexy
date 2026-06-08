#pragma once

#include "weapondata.hpp"

namespace WeaponCatalog {
inline const WeaponData Pistol{
    "Pistol",

    // Fire
    {
        15,     // damage
        100.0f, // range
        6.0f,   // fireRate
        false,  // automatic
    },

    // Ammo
    {
        12,   // magazineSize
        48,   // maxReserveAmmo
        1.1f, // reloadDuration
        true, // autoReloadWhenEmpty
    },

    // Viewmodel
    {
        {-0.20f, -0.10f, 0.31f},   // holdPosition
        {-6.0f, -90.0f, -4.0f},    // holdRotationDegrees
        0.80f,                     // modelScale
        {0.615f, -0.004f, 0.175f}, // muzzlePoint
        1.478f,                    // muzzleFlashWidth
        0.657f,                    // muzzleFlashHeight
    },

    // Recoil
    {
        0.06f, // kick
        20.0f, // pitchDegrees
    },

    // Procedural animation
    {
        0.0008f,                  // swayPositionAmount
        0.045f,                   // swayRotationAmount
        8.0f,                     // walkBobSpeed
        0.035f,                   // walkBobX
        0.018f,                   // walkBobY
        {0.025f, -0.09f, -0.02f}, // sprintOffset
        {0.0f, 10.0f, -35.0f},    // sprintRotationDegrees
    },
};
} // namespace WeaponCatalog
