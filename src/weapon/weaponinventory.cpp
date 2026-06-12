#include "weaponinventory.hpp"

#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"

#include "audio/audiosystem.hpp"
#include "raylib.h"
#include "viewmodel/proceduralweaponanimation.hpp"
#include "weapon/weapon.hpp"

#include <algorithm>
#include <iostream>
#include <vector>

void WeaponInventory::addWeapon(
    const WeaponData &weaponData,
    const ProceduralWeaponAnimationData &proceduralAnimation) {
  weapons.emplace_back(weaponData, proceduralAnimation);
}

void WeaponInventory::reset() {
  for (Weapon &weapon : weapons) {
    weapon.reset();
  }

  activeWeaponIndex = weapons.empty()
                          ? 0
                          : std::clamp(activeWeaponIndex, 0,
                                       static_cast<int>(weapons.size()) - 1);

  pendingWeaponIndex = activeWeaponIndex;
  switchTimer = 0.0f;
  switchCommited = false;
}

void WeaponInventory::update(float dt, const Player &player,
                             std::vector<Enemy> &enemies, const Level &level,
                             const Camera3D camera, ParticleSystem &particles,
                             AudioSystem &audio) {
  if (switchTimer > 0.0f) {
    switchTimer = std::max(0.0f, switchTimer - dt);

    if (!switchCommited && switchTimer <= switchDuration * 0.5f) {
      activeWeaponIndex = pendingWeaponIndex;
      switchCommited = true;
    }
  }

  updateSwitchInput(audio);

  if (weapons.empty()) {
    return;
  }

  if (switchTimer <= 0.0f) {
    getActiveWeapon().update(dt, player, enemies, level, camera, particles,
                             audio);
  }
}

Weapon &WeaponInventory::getActiveWeapon() {
  return weapons[activeWeaponIndex];
}

const Weapon &WeaponInventory::getActiveWeapon() const {
  return weapons[activeWeaponIndex];
}

int WeaponInventory::getActiveWeaponIndex() const { return activeWeaponIndex; }

int WeaponInventory::getWeaponCount() const {
  return static_cast<int>(weapons.size());
}

float WeaponInventory::getSwitchAmount() const {
  if (switchTimer <= 0.0f || switchDuration <= 0.0f) {
    return 0.0f;
  }

  float t = 1.0f - switchTimer / switchDuration;

  if (t < 0.5f) {
    return t / 0.5f;
  }

  return 1.0f - (t - 0.5f) / 0.5f;
}

void WeaponInventory::updateSwitchInput(AudioSystem &audio) {
  if (weapons.empty()) {
    return;
  }

  if (IsKeyPressed(KEY_ONE)) {
    requestSwitch(0, audio);
  }

  if (IsKeyPressed(KEY_TWO) && weapons.size() >= 2) {
    requestSwitch(1, audio);
  }

  if (IsKeyPressed(KEY_THREE) && weapons.size() >= 3) {
    requestSwitch(2, audio);
  }

  int targetWeaponIndex = activeWeaponIndex;
  float wheel = GetMouseWheelMove();

  if (wheel > 0.0f) {
    --targetWeaponIndex;
  } else if (wheel < 0.0f) {
    ++targetWeaponIndex;
  }

  if (targetWeaponIndex < 0) {
    targetWeaponIndex = static_cast<int>(weapons.size()) - 1;
  }

  if (targetWeaponIndex >= static_cast<int>(weapons.size())) {
    targetWeaponIndex = 0;
  }

  if (targetWeaponIndex != activeWeaponIndex) {
    requestSwitch(targetWeaponIndex, audio);
  }
}

void WeaponInventory::requestSwitch(int index, AudioSystem &audio) {
  if (index < 0 || index >= static_cast<int>(weapons.size())) {
    return;
  }

  if (index == activeWeaponIndex) {
    return;
  }

  getActiveWeapon().cancelReload(audio);
  getActiveWeapon().cancelMelee();
  pendingWeaponIndex = index;
  switchTimer = switchDuration;
  switchCommited = false;
  audio.play(AudioId::WeaponSwitch, {1.0f, 1.0f, 0.0f});
}
