#include "editorui.hpp"

#include "editorsettings.hpp"
#include "leveleditor.hpp"

#include "../level/level.hpp"

#include "raygui/raygui.h"
#include "raylib.h"

namespace {
constexpr const char *LEVEL_PATH = "levels/test_arena.json";

int toolToIndex(EditorTool tool) {
  switch (tool) {
  case EditorTool::Select:
    return 0;
  case EditorTool::Wall:
    return 1;
  case EditorTool::EnemySpawn:
    return 2;
  case EditorTool::PlayerSpawn:
    return 3;
  }

  return 1;
}

EditorTool indexToTool(int index) {
  switch (index) {
  case 0:
    return EditorTool::Select;
  case 1:
    return EditorTool::Wall;
  case 2:
    return EditorTool::EnemySpawn;
  case 3:
    return EditorTool::PlayerSpawn;
  default:
    return EditorTool::Wall;
  }
}
} // namespace

namespace EditorUI {
void draw(LevelEditor &editor, Level &level) {
  if (!editor.isEnabled()) {
    return;
  }

  EditorSettings &settings = editor.getSettings();

  Rectangle panel{16.0f, 16.0f, 280.0f, 400.0f};

  DrawRectangleRec(panel, Fade(BLACK, 0.72f));
  DrawRectangleLinesEx(panel, 1.0f, GREEN);

  DrawText("Editor", 28, 28, 24, GREEN);
  DrawText("Select walls, Delete removes", 28, 50, 14, GREEN);

  DrawText("Wall Size", 28, 120, 18, GREEN);

  GuiSliderBar({88.0f, 144.0f, 180.0f, 20.0f}, "X", nullptr,
               &settings.wallSize.x, 1.0f, 12.0f);
  GuiSliderBar({88.0f, 172.0f, 180.0f, 20.0f}, "Y", nullptr,
               &settings.wallSize.y, 1.0f, 8.0f);
  GuiSliderBar({88.0f, 200.0f, 180.0f, 20.0f}, "Z", nullptr,
               &settings.wallSize.z, 1.0f, 12.0f);

  settings.wallSize.x =
      static_cast<float>(static_cast<int>(settings.wallSize.x));
  settings.wallSize.y =
      static_cast<float>(static_cast<int>(settings.wallSize.y));
  settings.wallSize.z =
      static_cast<float>(static_cast<int>(settings.wallSize.z));

  DrawText(TextFormat("%.0f", settings.wallSize.x), 36, 144, 18, GREEN);
  DrawText(TextFormat("%.0f", settings.wallSize.y), 36, 172, 18, GREEN);
  DrawText(TextFormat("%.0f", settings.wallSize.z), 36, 200, 18, GREEN);

  DrawText("Snap", 28, 236, 18, GREEN);
  GuiSliderBar({88.0f, 236.0f, 180.0f, 20.0f}, nullptr, nullptr,
               &settings.snapSize, 0.5f, 4.0f);

  if (settings.snapSize < 0.75f) {
    settings.snapSize = 0.5f;
  } else if (settings.snapSize < 1.5f) {
    settings.snapSize = 1.0f;
  } else if (settings.snapSize < 3.0f) {
    settings.snapSize = 2.0f;
  } else {
    settings.snapSize = 4.0f;
  }

  DrawText(TextFormat("%.1f", settings.snapSize), 36, 260, 18, GREEN);

  GuiCheckBox({28.0f, 288.0f, 20.0f, 20.0f}, "Show grid", &settings.showGrid);
  GuiCheckBox({28.0f, 316.0f, 20.0f, 20.0f}, "Show spawns",
              &settings.showSpawns);

  if (GuiButton({28.0f, 360.0f, 112.0f, 28.0f}, "Reload")) {
    level.loadFromFile(LEVEL_PATH);
  }

  if (GuiButton({156.0f, 360.0f, 112.0f, 28.0f}, "Save")) {
    level.saveToFile(LEVEL_PATH);
  }

  int toolIndex = toolToIndex(settings.tool);
  if (GuiDropdownBox({28.0f, 76.0f, 240.0f, 28.0f},
                     "Select;Wall;Enemy;Player", &toolIndex,
                     settings.toolDropdownOpen)) {
    settings.toolDropdownOpen = !settings.toolDropdownOpen;
    settings.tool = indexToTool(toolIndex);
  }
}
} // namespace EditorUI
