#pragma once

#include "weapondata.hpp"

namespace WeaponCatalog {
inline const WeaponData Pistol{
    "Pistol",
    WeaponModelId::Pistol,

    // Fire
    {
        15,     // damage
        100.0f, // range
        6.0f,   // fireRate
        false,  // automatic
        1,      // pelletCount
        0.15f,  // spreadDegrees
        0.45f,  // movingSpreadDegrees
        0.95f,  // sprintSpreadDegrees
        0.25f,  // spreadBloomPerShot
        1.25f,  // maxSpreadBloomDegrees
        8.0f    // spreadRecoverySpeed
    },

    // Ammo
    {
        12,   // magazineSize
        48,   // maxReserveAmmo
        1.8f, // reloadDuration
        true, // autoReloadWhenEmpty
        false // reloadOneRoundAtATime
    },

    // Melee
    {},

    // Viewmodel
    {
        {-0.247f, -0.147f, 0.381f}, // holdPosition
        {-2.8f, -7.4f, -1.9f},      // holdRotationDegrees
        0.104f,                     // modelScale
        {0.015f, 0.054f, 0.575f},   // muzzlePoint
        1.478f,                     // muzzleFlashWidth
        0.657f,                     // muzzleFlashHeight
    },
};

inline const WeaponData Shotgun{
    "Shotgun",
    WeaponModelId::Shotgun,
    {
        8,
        34.0f,
        1.15f,
        false,
        10,
        4.2f,
        1.2f,
        2.2f,
        1.1f,
        3.5f,
        5.5f,
    },
    {
        6,
        60,
        0.72f,
        true,
        true,
    },
    {},
    {
        {-0.176f, -0.194f, 0.428f}, // holdPosition
        {0.0f, -180.0f, 0.0f},      // holdRotationDegrees
        1.0f,                       // modelScale
        {0.015f, 0.054f, -0.575f},  // muzzlePoint
        1.478f,                     // muzzleFlashWidth
        0.657f,
    }};
} // namespace WeaponCatalog
