#pragma once

#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cmath>

namespace Lighting {
struct SceneLight {
  Vector3 directionalDirection{-0.35f, 0.85f, -0.45f};
  float ambient = 0.36f;
  float directionalStrength = 0.72f;
};

inline SceneLight sceneLight{};

inline void setSceneLight(const SceneLight &light) { sceneLight = light; }

inline Color shade(Color color, Vector3 position, Vector3 normal) {
  (void)position;

  Vector3 directionalDirection =
      Vector3Normalize(sceneLight.directionalDirection);

  float directional =
      std::max(0.0f, Vector3DotProduct(normal, directionalDirection)) *
      sceneLight.directionalStrength;

  float amount = std::clamp(sceneLight.ambient + directional, 0.0f, 1.25f);

  return {
      static_cast<unsigned char>(
          std::clamp(std::round(color.r * amount), 0.0f, 255.0f)),
      static_cast<unsigned char>(
          std::clamp(std::round(color.g * amount), 0.0f, 255.0f)),
      static_cast<unsigned char>(
          std::clamp(std::round(color.b * amount), 0.0f, 255.0f)),
      color.a,
  };
}

inline void drawFace(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 normal,
                     Color color) {
  Vector3 center{
      (a.x + b.x + c.x + d.x) * 0.25f,
      (a.y + b.y + c.y + d.y) * 0.25f,
      (a.z + b.z + c.z + d.z) * 0.25f,
  };
  Color faceColor = shade(color, center, normal);
  DrawTriangle3D(a, b, c, faceColor);
  DrawTriangle3D(a, c, d, faceColor);
  DrawTriangle3D(c, b, a, faceColor);
  DrawTriangle3D(d, c, a, faceColor);
}

inline void drawCube(Vector3 center, Vector3 size, Color color) {
  Vector3 half{size.x * 0.5f, size.y * 0.5f, size.z * 0.5f};

  float minX = center.x - half.x;
  float maxX = center.x + half.x;
  float minY = center.y - half.y;
  float maxY = center.y + half.y;
  float minZ = center.z - half.z;
  float maxZ = center.z + half.z;

  Vector3 v000{minX, minY, minZ};
  Vector3 v001{minX, minY, maxZ};
  Vector3 v010{minX, maxY, minZ};
  Vector3 v011{minX, maxY, maxZ};
  Vector3 v100{maxX, minY, minZ};
  Vector3 v101{maxX, minY, maxZ};
  Vector3 v110{maxX, maxY, minZ};
  Vector3 v111{maxX, maxY, maxZ};

  drawFace(v010, v110, v111, v011, {0.0f, 1.0f, 0.0f}, color);
  drawFace(v000, v001, v101, v100, {0.0f, -1.0f, 0.0f}, color);
  drawFace(v100, v101, v111, v110, {1.0f, 0.0f, 0.0f}, color);
  drawFace(v000, v010, v011, v001, {-1.0f, 0.0f, 0.0f}, color);
  drawFace(v001, v011, v111, v101, {0.0f, 0.0f, 1.0f}, color);
  drawFace(v000, v100, v110, v010, {0.0f, 0.0f, -1.0f}, color);
}

inline void drawPlane(Vector3 center, Vector2 size, Color color) {
  Vector3 half{size.x * 0.5f, 0.0f, size.y * 0.5f};

  Vector3 a{center.x - half.x, center.y, center.z - half.z};
  Vector3 b{center.x - half.x, center.y, center.z + half.z};
  Vector3 c{center.x + half.x, center.y, center.z + half.z};
  Vector3 d{center.x + half.x, center.y, center.z - half.z};

  drawFace(a, b, c, d, {0.0f, 1.0f, 0.0f}, color);
}
} // namespace Lighting
