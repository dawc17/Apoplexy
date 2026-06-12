#pragma once

#include "audio/audiosystem.hpp"
#include "raylib.h"
#include "viewmodel/proceduralweaponanimation.hpp"
#include "weapon.hpp"

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
              ParticleSystem &particles, AudioSystem &audio);

  Weapon &getActiveWeapon();
  const Weapon &getActiveWeapon() const;

  int getActiveWeaponIndex() const;
  int getWeaponCount() const;
  float getSwitchAmount() const;

private:
  void updateSwitchInput(AudioSystem &audio);
  void requestSwitch(int index, AudioSystem &audio);

private:
  std::vector<Weapon> weapons;
  int activeWeaponIndex = 0;
  int pendingWeaponIndex = 0;
  float switchTimer = 0.0f;
  float switchDuration = 0.34f;
  bool switchCommited = false;
};
