#include "math.hpp"

#include <cmath>

namespace Math {
Vector3 forwardFromYawPitch(float yaw, float pitch) {
  float cp = std::cosf(pitch);

  return {
      std::sinf(yaw) * cp,
      std::sinf(pitch),
      std::cosf(yaw) * cp,
  };
}

Vector3 forwardFromYaw(float yaw) {
  return {std::sinf(yaw), 0.0f, std::cosf(yaw)};
}

float distanceXZ(Vector3 a, Vector3 b) {
  float dx = a.x - b.x;
  float dz = a.z - b.z;

  // cursed much?
  return std::sqrtf(dx * dx + dz * dz);
}
} // namespace Math
