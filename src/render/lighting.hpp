#pragma once

#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <vector>

namespace Lighting {
constexpr int MAX_POINT_LIGHTS = 8;

struct DirectionalLight {
  Vector3 direction{-0.45f, 0.90f, -0.30f};
  Color color{255, 255, 255, 255};
  float intensity = 0.72f;
};

struct PointLight {
  Vector3 position{};
  Color color{255, 214, 160, 255};
  float intensity = 1.6f;
  float radius = 7.0f;
  bool enabled = true;
};

struct SceneLighting {
  Color ambientColor{255, 255, 255, 255};
  float ambientIntensity = 0.36f;
  DirectionalLight sun{};
  std::vector<PointLight> staticPointLights;
  std::vector<PointLight> dynamicPointLights;
};

inline PointLight makeDefaultPointLight(Vector3 position) {
  PointLight light{};
  light.position = position;
  return light;
}

inline float clampIntensity(float intensity) {
  return std::clamp(intensity, 0.0f, 16.0f);
}

inline float clampRadius(float radius) { return std::max(radius, 0.1f); }

inline Vector3 colorToVec3(Color color) {
  return {
      static_cast<float>(color.r) / 255.0f,
      static_cast<float>(color.g) / 255.0f,
      static_cast<float>(color.b) / 255.0f,
  };
}

inline Vector3 samplePointLightsAt(Vector3 position,
                                   const std::vector<PointLight> &pointLights) {
  Vector3 contribution{};

  for (const PointLight &light : pointLights) {
    if (!light.enabled) {
      continue;
    }

    float radius = clampRadius(light.radius);
    float distanceToLight = Vector3Distance(position, light.position);
    float falloff = std::clamp(1.0f - distanceToLight / radius, 0.0f, 1.0f);
    falloff *= falloff;

    Vector3 color = colorToVec3(light.color);
    float amount = clampIntensity(light.intensity) * falloff;
    contribution = Vector3Add(contribution, Vector3Scale(color, amount));
  }

  return contribution;
}

inline void uploadSceneLighting(Shader shader, const SceneLighting &scene,
                                const std::vector<PointLight> &pointLights) {
  Vector3 ambientColor = colorToVec3(scene.ambientColor);
  Vector3 sunDirection = Vector3Normalize(scene.sun.direction);
  Vector3 sunColor = colorToVec3(scene.sun.color);
  float ambientIntensity = clampIntensity(scene.ambientIntensity);
  float sunIntensity = clampIntensity(scene.sun.intensity);
  int pointCount =
      std::min(static_cast<int>(pointLights.size()), MAX_POINT_LIGHTS);

  SetShaderValue(shader, GetShaderLocation(shader, "ambientColor"),
                 &ambientColor, SHADER_UNIFORM_VEC3);
  SetShaderValue(shader, GetShaderLocation(shader, "ambientIntensity"),
                 &ambientIntensity, SHADER_UNIFORM_FLOAT);
  SetShaderValue(shader, GetShaderLocation(shader, "sunDirection"),
                 &sunDirection, SHADER_UNIFORM_VEC3);
  SetShaderValue(shader, GetShaderLocation(shader, "sunColor"), &sunColor,
                 SHADER_UNIFORM_VEC3);
  SetShaderValue(shader, GetShaderLocation(shader, "sunIntensity"),
                 &sunIntensity, SHADER_UNIFORM_FLOAT);
  SetShaderValue(shader, GetShaderLocation(shader, "pointLightCount"),
                 &pointCount, SHADER_UNIFORM_INT);

  for (int i = 0; i < pointCount; ++i) {
    const PointLight &light = pointLights[i];
    Vector3 color = colorToVec3(light.color);
    float intensity = clampIntensity(light.intensity);
    float radius = clampRadius(light.radius);

    SetShaderValue(shader,
                   GetShaderLocation(
                       shader, TextFormat("pointLights[%i].position", i)),
                   &light.position, SHADER_UNIFORM_VEC3);
    SetShaderValue(shader,
                   GetShaderLocation(shader,
                                     TextFormat("pointLights[%i].color", i)),
                   &color, SHADER_UNIFORM_VEC3);
    SetShaderValue(
        shader,
        GetShaderLocation(shader, TextFormat("pointLights[%i].intensity", i)),
        &intensity, SHADER_UNIFORM_FLOAT);
    SetShaderValue(shader,
                   GetShaderLocation(shader,
                                     TextFormat("pointLights[%i].radius", i)),
                   &radius, SHADER_UNIFORM_FLOAT);
  }
}
} // namespace Lighting
