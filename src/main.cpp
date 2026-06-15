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

    if (IsKeyDown(KEY_LEFT_ALT) && IsKeyPressed(KEY_ENTER)) {
      if (!IsWindowFullscreen()) {
        int monitor = GetCurrentMonitor();
        SetWindowSize(GetMonitorWidth(monitor), GetMonitorHeight(monitor));
      } else {
        SetWindowSize(Constants::SCREEN_WIDTH, Constants::SCREEN_HEIGHT);
      }

      // this is a test to see if wakatime is tracking my coding time
      // why is it not tracking my time? what is going on??? why is it fucking
      // broken!!!
      ToggleFullscreen();
    }

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
