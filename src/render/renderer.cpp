#include "renderer.hpp"

#include "../core/game.hpp"
#include "../enemy/enemy.hpp"

#include "lighting.hpp"

#include "raylib.h"
#include "raymath.h"
#include "rlgl.h"

#include <algorithm>
#include <vector>

namespace Renderer {
namespace {
std::vector<Lighting::PointLight> dynamicPointLights;

float distanceSquared(Vector3 a, Vector3 b) {
  float dx = a.x - b.x;
  float dy = a.y - b.y;
  float dz = a.z - b.z;
  return dx * dx + dy * dy + dz * dz;
}

std::vector<Lighting::PointLight>
gatherPointLights(const Lighting::SceneLighting &scene, Vector3 cameraPosition) {
  std::vector<Lighting::PointLight> pointLights;

  for (const Lighting::PointLight &light : scene.staticPointLights) {
    if (light.enabled) {
      pointLights.push_back(light);
    }
  }

  for (const Lighting::PointLight &light : scene.dynamicPointLights) {
    if (light.enabled) {
      pointLights.push_back(light);
    }
  }

  std::sort(pointLights.begin(), pointLights.end(),
            [cameraPosition](const Lighting::PointLight &a,
                             const Lighting::PointLight &b) {
              return distanceSquared(a.position, cameraPosition) <
                     distanceSquared(b.position, cameraPosition);
            });

  if (pointLights.size() > Lighting::MAX_POINT_LIGHTS) {
    pointLights.resize(Lighting::MAX_POINT_LIGHTS);
  }

  return pointLights;
}

void drawSkybox(const Game &game, Camera3D camera) {
  if (!game.getAssets().hasSkybox()) {
    return;
  }

  rlDisableDepthMask();
  rlDisableBackfaceCulling();
  DrawModel(game.getAssets().getSkyboxModel(), {0.0f, 0.0f, 0.0f}, 1.0f,
            WHITE);
  rlEnableBackfaceCulling();
  rlEnableDepthMask();
}
} // namespace

void submitDynamicPointLight(Lighting::PointLight light) {
  light.intensity = Lighting::clampIntensity(light.intensity);
  light.radius = Lighting::clampRadius(light.radius);
  dynamicPointLights.push_back(light);
}

void clearDynamicPointLights() { dynamicPointLights.clear(); }

void drawWorld(const Game &game) {
  Camera3D camera = game.getCamera();

  Lighting::SceneLighting scene = game.getLevel().getLighting();
  scene.staticPointLights = game.getLevel().getLights();
  scene.dynamicPointLights = dynamicPointLights;

  std::vector<Lighting::PointLight> pointLights =
      gatherPointLights(scene, camera.position);

  BeginMode3D(camera);

  drawSkybox(game, camera);

  Shader worldShader = game.getAssets().getWorldLitShader();
  Lighting::uploadSceneLighting(worldShader, scene, pointLights);

  BeginShaderMode(worldShader);

  game.getLevel().draw();

  for (const Enemy &enemy : game.getEnemies()) {
    enemy.draw();
  }

  EndShaderMode();

  game.getParticles().draw();
  game.getWeapon().drawDebugRays();
  game.getLevelEditor().draw(game.getLevel());

  EndMode3D();

  clearDynamicPointLights();

  if (!game.isEditorEnabled()) {
    Vector3 viewmodelPointLight =
        Lighting::samplePointLightsAt(camera.position, pointLights);
    game.getWeapon().drawViewModel(game.getCamera(), game.getAssets(),
                                   scene, viewmodelPointLight);
  }
}
} // namespace Renderer
