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
        1.8f, // reloadDuration
        true, // autoReloadWhenEmpty
    },

    // Viewmodel
    {
        {-0.247f, -0.147f, 0.381f}, // holdPosition
        {-2.8f, -7.4f, -1.9f},     // holdRotationDegrees
        0.104f,                     // modelScale
        {0.015f, 0.054f, 0.575f}, // muzzlePoint
        1.478f,                    // muzzleFlashWidth
        0.657f,                    // muzzleFlashHeight
    },
};
} // namespace WeaponCatalog
