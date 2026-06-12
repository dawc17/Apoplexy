#pragma once

#include "../weapon/weapondata.hpp"

#include <array>

namespace ViewmodelDebug {
struct Entry {
  Vector3 position{};
  Vector3 rotationDegrees{};
  float scale = 1.0f;
  Vector3 muzzlePoint{};
  float muzzleFlashWidth = 1.0f;
  float muzzleFlashHeight = 1.0f;
  bool initialized = false;
};

inline bool panelOpen = false;
inline std::array<Entry, static_cast<int>(WeaponModelId::Count)> entries{};

inline int indexFor(WeaponModelId modelId) {
  return static_cast<int>(modelId);
}

inline Entry &entryFor(WeaponModelId modelId) {
  return entries[indexFor(modelId)];
}

inline const Entry &constEntryFor(WeaponModelId modelId) {
  return entries[indexFor(modelId)];
}

inline void syncFromWeapon(const WeaponData &weapon) {
  Entry &entry = entryFor(weapon.modelId);

  if (entry.initialized) {
    return;
  }

  entry.position = weapon.viewModel.holdPosition;
  entry.rotationDegrees = weapon.viewModel.holdRotationDegrees;
  entry.scale = weapon.viewModel.modelScale;
  entry.muzzlePoint = weapon.viewModel.muzzlePoint;
  entry.muzzleFlashWidth = weapon.viewModel.muzzleFlashWidth;
  entry.muzzleFlashHeight = weapon.viewModel.muzzleFlashHeight;
  entry.initialized = true;
}

inline void reset(const WeaponData &weapon) {
  Entry &entry = entryFor(weapon.modelId);
  entry.position = weapon.viewModel.holdPosition;
  entry.rotationDegrees = weapon.viewModel.holdRotationDegrees;
  entry.scale = weapon.viewModel.modelScale;
  entry.muzzlePoint = weapon.viewModel.muzzlePoint;
  entry.muzzleFlashWidth = weapon.viewModel.muzzleFlashWidth;
  entry.muzzleFlashHeight = weapon.viewModel.muzzleFlashHeight;
  entry.initialized = true;
}

inline WeaponViewModelData viewModelFor(const WeaponData &weapon) {
  const Entry &entry = constEntryFor(weapon.modelId);

  if (!entry.initialized) {
    return weapon.viewModel;
  }

  WeaponViewModelData viewModel = weapon.viewModel;
  viewModel.holdPosition = entry.position;
  viewModel.holdRotationDegrees = entry.rotationDegrees;
  viewModel.modelScale = entry.scale;
  viewModel.muzzlePoint = entry.muzzlePoint;
  viewModel.muzzleFlashWidth = entry.muzzleFlashWidth;
  viewModel.muzzleFlashHeight = entry.muzzleFlashHeight;
  return viewModel;
}
} // namespace ViewmodelDebug
