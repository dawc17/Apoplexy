#pragma once

#include "proceduralweaponanimation.hpp"

#include "../render/lighting.hpp"
#include "../weapon/weapondata.hpp"

#include "raylib.h"

class AssetManager;

class Viewmodel {
public:
  void reset();
  void update(float dt, bool playerSprinting, bool weaponReloading,
              bool weaponMeleeing, float weaponMeleeProgress,
              const ProceduralWeaponAnimationData &procedural);
  void addRecoil(float amount);
  void clearMelee();

  void draw(const Camera3D &camera, const WeaponData &weapon,
            const ProceduralWeaponAnimationData &procedural,
            const AssetManager &assets, float muzzleFlashTimer,
            float muzzleFlashRotation, float switchAmount,
            const Lighting::SceneLighting &lighting,
            Vector3 pointLightContribution) const;

private:
  float recoilTimer = 1.0f;
  float recoilAmount = 0.0f;
  Vector2 swayOffset{};
  Vector2 swayRotation{};
  float idleBobTimer = 0.0f;
  float idleBobAmount = 0.0f;
  float walkBobTimer = 0.0f;
  float walkBobAmount = 0.0f;
  float sprintAmount = 0.0f;
  float reloadAmount = 0.0f;
  float meleeAmount = 0.0f;
  float meleeProgress = 1.0f;
  Vector3 reloadSpinRotationDegrees{};
};
