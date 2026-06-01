#pragma once

#include "raylib.h"

namespace Math {
Vector3 forwardFromYawPitch(float yaw, float pitch);
Vector3 forwardFromYaw(float yaw);
float distanceXZ(Vector3 a, Vector3 b);
} // namespace Math
