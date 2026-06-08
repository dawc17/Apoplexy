#include "renderer.hpp"

#include "../core/game.hpp"
#include "../enemy/enemy.hpp"

#include "lighting.hpp"

#include "raylib.h"

namespace Renderer {
void drawWorld(const Game &game) {
  Camera3D camera = game.getCamera();

  Lighting::SceneLight light{};
  light.directionalDirection = {-0.45f, 0.90f, -0.30f};
  light.ambient = 0.36f;
  light.directionalStrength = 0.72f;
  Lighting::setSceneLight(light);

  BeginMode3D(camera);

  game.getLevel().draw();

  for (const Enemy &enemy : game.getEnemies()) {
    enemy.draw();
  }

  game.getParticles().draw();
  game.getLevelEditor().draw(game.getLevel());

  EndMode3D();

  if (!game.isEditorEnabled()) {
    game.getWeapon().drawViewModel(game.getCamera(), game.getAssets());
  }
}
} // namespace Renderer
