#include "math.hpp"

#include <cmath>

namespace Math {
Vector3 forwardFromYawPitch(float yaw, float pitch) {
  float cp = std::cos(pitch);

  return {
      std::sin(yaw) * cp,
      std::sin(pitch),
      std::cos(yaw) * cp,
  };
}

Vector3 forwardFromYaw(float yaw) {
  return {std::sin(yaw), 0.0f, std::cos(yaw)};
}

float distanceXZ(Vector3 a, Vector3 b) {
  float dx = a.x - b.x;
  float dz = a.z - b.z;

  // cursed much?
  return std::sqrt(dx * dx + dz * dz);
}
} // namespace Math
