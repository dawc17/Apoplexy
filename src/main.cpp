#include "raylib.h"

#include "../include/resource_dir.h" // utility header for SearchAndSetResourceDir

const int WINDOW_HEIGHT = 600;
const int WINDOW_WIDTH = 800;

int main() {
  SetConfigFlags(FLAG_VSYNC_HINT | FLAG_WINDOW_HIGHDPI);

  InitWindow(WINDOW_WIDTH, WINDOW_HEIGHT, "Apoplexy");

  // Utility function from resource_dir.h to find the resources folder and set
  // it as the current working directory so we can load from it
  SearchAndSetResourceDir("resources");

  Texture wabbit = LoadTexture("wabbit_alpha.png");

  // game loop
  while (!WindowShouldClose()) {
    BeginDrawing();

    ClearBackground(BLACK);

    DrawText("Apoplexy", WINDOW_WIDTH / 2, WINDOW_HEIGHT / 2, 20, WHITE);

    DrawTexture(wabbit, 400, 200, WHITE);

    EndDrawing();
  }

  // cleanup
  UnloadTexture(wabbit);

  CloseWindow();
  return 0;
}
