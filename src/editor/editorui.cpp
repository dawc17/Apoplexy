#include "editorui.hpp"

#include "editorsettings.hpp"
#include "leveleditor.hpp"

#include "../level/level.hpp"

#include "raygui/raygui.h"
#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <vector>

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
  case EditorTool::Light:
    return 4;
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
  case 4:
    return EditorTool::Light;
  default:
      return EditorTool::Wall;
  }
}

const char *modeLabel(LevelEditor::Mode mode) {
  return mode == LevelEditor::Mode::Test ? "Test" : "Build";
}
} // namespace

namespace EditorUI {
void draw(LevelEditor &editor, Level &level) {
  if (!editor.isEnabled()) {
    return;
  }

  EditorSettings &settings = editor.getSettings();

  Rectangle panel{16.0f, 16.0f, 320.0f, 680.0f};

  DrawRectangleRec(panel, Fade(BLACK, 0.72f));
  DrawRectangleLinesEx(panel, 1.0f, GREEN);

  DrawText("Editor", 28, 28, 24, GREEN);
  DrawText(TextFormat("Mode: %s (T)", modeLabel(editor.getMode())), 28, 50, 14,
           editor.isTestMode() ? SKYBLUE : GREEN);
  DrawText(editor.isTestMode() ? "Left click emits test noise"
                               : "Shift-drag gizmo disables snap",
           28, 66, 14, editor.isTestMode() ? SKYBLUE : GREEN);

  DrawText("Wall Size", 28, 120, 18, GREEN);

  GuiSliderBar({100.0f, 144.0f, 204.0f, 20.0f}, "X", nullptr,
               &settings.wallSize.x, 1.0f, 12.0f);
  GuiSliderBar({100.0f, 172.0f, 204.0f, 20.0f}, "Y", nullptr,
               &settings.wallSize.y, 1.0f, 8.0f);
  GuiSliderBar({100.0f, 200.0f, 204.0f, 20.0f}, "Z", nullptr,
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
  GuiSliderBar({100.0f, 236.0f, 204.0f, 20.0f}, nullptr, nullptr,
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

  if (editor.getSelection().hasLight()) {
    std::vector<Lighting::PointLight> &lights = level.getMutableLights();
    int index = editor.getSelection().getLightIndex();

    if (index >= 0 && index < static_cast<int>(lights.size())) {
      Lighting::PointLight &light = lights[index];

      DrawText("Selected Light", 28, 352, 18, GREEN);
      GuiSliderBar({100.0f, 380.0f, 204.0f, 20.0f}, "Strength", nullptr,
                   &light.intensity, 0.0f, 8.0f);
      GuiSliderBar({100.0f, 408.0f, 204.0f, 20.0f}, "Radius", nullptr,
                   &light.radius, 0.5f, 24.0f);
      GuiCheckBox({28.0f, 436.0f, 20.0f, 20.0f}, "Enabled",
                  &light.enabled);

      light.intensity = Lighting::clampIntensity(light.intensity);
      light.radius = Lighting::clampRadius(light.radius);
    }
  }

  Lighting::SceneLighting &lighting = level.getMutableLighting();
  DrawText("Sun", 28, 472, 18, GREEN);

  GuiSliderBar({100.0f, 500.0f, 204.0f, 20.0f}, "Ambient", nullptr,
               &lighting.ambientIntensity, 0.0f, 1.0f);
  GuiSliderBar({100.0f, 528.0f, 204.0f, 20.0f}, "Power", nullptr,
               &lighting.sun.intensity, 0.0f, 2.0f);
  GuiSliderBar({100.0f, 556.0f, 204.0f, 20.0f}, "Dir X", nullptr,
               &lighting.sun.direction.x, -1.0f, 1.0f);
  GuiSliderBar({100.0f, 584.0f, 204.0f, 20.0f}, "Dir Y", nullptr,
               &lighting.sun.direction.y, -1.0f, 1.0f);
  GuiSliderBar({100.0f, 612.0f, 204.0f, 20.0f}, "Dir Z", nullptr,
               &lighting.sun.direction.z, -1.0f, 1.0f);

  if (Vector3Length(lighting.sun.direction) <= 0.001f) {
    lighting.sun.direction = {-0.45f, 0.90f, -0.30f};
  }

  lighting.ambientIntensity =
      std::clamp(lighting.ambientIntensity, 0.0f, 1.0f);
  lighting.sun.intensity =
      std::clamp(lighting.sun.intensity, 0.0f, 2.0f);

  if (GuiButton({28.0f, 648.0f, 132.0f, 28.0f}, "Reload")) {
    level.loadFromFile(LEVEL_PATH);
  }

  if (GuiButton({172.0f, 648.0f, 132.0f, 28.0f}, "Save")) {
    level.saveToFile(LEVEL_PATH);
  }

  int toolIndex = toolToIndex(settings.tool);
  if (GuiDropdownBox({28.0f, 76.0f, 276.0f, 28.0f},
                     "Select;Wall;Enemy;Player;Light", &toolIndex,
                     settings.toolDropdownOpen)) {
    settings.toolDropdownOpen = !settings.toolDropdownOpen;
    settings.tool = indexToTool(toolIndex);
  }
}
} // namespace EditorUI
