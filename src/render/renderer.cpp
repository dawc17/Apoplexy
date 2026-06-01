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

  EndMode3D();

  game.getWeapon().drawViewModel(game.getCamera(), game.getAssets());
}
} // namespace Renderer
