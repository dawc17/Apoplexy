#pragma once

#include "raylib.h"

enum class EditorTool { Select, Wall, EnemySpawn, PlayerSpawn, Light, Decal };

struct EditorSettings {
  EditorTool tool = EditorTool::Wall;

  Vector3 wallSize{2.0f, 2.0f, 2.0f};
  Vector2 decalSize{2.0f, 1.2f};
  char decalTexturePath[128] = "textures/decal.png";

  float snapSize = 1.0f;
  bool showGrid = true;
  bool showSpawns = true;
  bool showHelp = true;
  bool toolDropdownOpen = false;
  bool decalPathEditMode = false;
};
