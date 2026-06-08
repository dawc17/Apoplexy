#include "renderer.hpp"

#include "../core/game.hpp"
#include "../enemy/enemy.hpp"

#include "raylib.h"

namespace Renderer {
void drawWorld(const Game &game) {
  BeginMode3D(game.getCamera());

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
