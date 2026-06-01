#include "raylib.h"

#include "../include/resource_dir.h" // utility header for SearchAndSetResourceDir
#include "core/constants.hpp"
#include "core/game.hpp"

int main() {
  SetConfigFlags(FLAG_VSYNC_HINT | FLAG_WINDOW_HIGHDPI);

  InitWindow(Constants::SCREEN_WIDTH, Constants::SCREEN_HEIGHT, "Apoplexy");
  InitAudioDevice();
  SearchAndSetResourceDir("resources");

  SetTargetFPS(Constants::TARGET_FPS);
  DisableCursor();

  Game game;

  while (!WindowShouldClose()) {
    float dt = GetFrameTime();

    game.update(dt);

    BeginDrawing();
    ClearBackground(BLACK);

    game.draw();

    EndDrawing();
  }

  // cleanup
  CloseAudioDevice();
  CloseWindow();

  return 0;
}
