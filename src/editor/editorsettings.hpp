#pragma once

#include "raylib.h"

enum class EditorTool { Select, Wall, EnemySpawn, PlayerSpawn };

struct EditorSettings {
  EditorTool tool = EditorTool::Wall;

  Vector3 wallSize{2.0f, 2.0f, 2.0f};

  float snapSize = 1.0f;
  bool showGrid = true;
  bool showSpawns = true;
  bool showHelp = true;
  bool toolDropdownOpen = false;
};
