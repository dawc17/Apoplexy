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
};
} // namespace WeaponCatalog
