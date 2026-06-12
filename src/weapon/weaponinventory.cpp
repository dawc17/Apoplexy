#include "weaponinventory.hpp"

#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"

#include "audio/audiosystem.hpp"
#include "raylib.h"
#include "viewmodel/proceduralweaponanimation.hpp"

#include <algorithm>
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
}

void WeaponInventory::update(float dt, const Player &player,
                             std::vector<Enemy> &enemies, const Level &level,
                             const Camera3D camera, ParticleSystem &particles,
                             AudioSystem &audio) {
  updateSwitchInput(audio);

  if (weapons.empty()) {
    return;
  }

  getActiveWeapon().update(dt, player, enemies, level, camera, particles,
                           audio);
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

void WeaponInventory::updateSwitchInput(AudioSystem &audio) {
  if (weapons.empty()) {
    return;
  }

  int previousWeaponIndex = activeWeaponIndex;

  if (IsKeyPressed(KEY_ONE)) {
    activeWeaponIndex = 0;
  }

  if (IsKeyPressed(KEY_TWO) && weapons.size() >= 2) {
    activeWeaponIndex = 1;
  }

  if (IsKeyPressed(KEY_THREE) && weapons.size() >= 3) {
    activeWeaponIndex = 2;
  }

  float wheel = GetMouseWheelMove();

  if (wheel > 0.0f) {
    --activeWeaponIndex;
  } else if (wheel < 0.0f) {
    ++activeWeaponIndex;
  }

  if (activeWeaponIndex < 0) {
    activeWeaponIndex = static_cast<int>(weapons.size()) - 1;
  }

  if (activeWeaponIndex >= static_cast<int>(weapons.size())) {
    activeWeaponIndex = 0;
  }

  if (activeWeaponIndex != previousWeaponIndex) {
    audio.play(AudioId::WeaponSwitch, {1.0f, 1.0f, 0.0f});
  }
}
