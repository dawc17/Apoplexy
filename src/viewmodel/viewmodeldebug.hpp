#pragma once

#include "raylib.h"

namespace ViewmodelDebug {
inline bool panelOpen = false;
inline Vector3 positionOffset{0.0f, 0.0f, 0.0f};
inline Vector3 rotationOffsetDegrees{0.0f, 0.0f, 0.0f};
inline float scaleMultiplier = 1.0f;

inline void reset() {
  positionOffset = {0.0f, 0.0f, 0.0f};
  rotationOffsetDegrees = {0.0f, 0.0f, 0.0f};
  scaleMultiplier = 1.0f;
}
} // namespace ViewmodelDebug
