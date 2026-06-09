#pragma once

#include "raylib.h"
#include "viewmodel/proceduralweaponanimation.hpp"
#include "weapon.hpp"

#include <cstddef>
#include <vector>

class AssetManager;
class Enemy;
class Level;
class ParticleSystem;
class Player;

class WeaponInventory {
public:
  void addWeapon(const WeaponData &weaponData,
                 const ProceduralWeaponAnimationData &proceduralAnimation);

  void reset();

  void update(float dt, const Player &player, std::vector<Enemy> &enemies,
              const Level &level, const Camera3D camera,
              ParticleSystem &particles);

  Weapon &getActiveWeapon();
  const Weapon &getActiveWeapon() const;

  int getActiveWeaponIndex() const;
  int getWeaponCount() const;

private:
  void updateSwitchInput();

private:
  std::vector<Weapon> weapons;
  int activeWeaponIndex = 0;
};
