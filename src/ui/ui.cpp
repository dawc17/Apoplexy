#include "ui.hpp"

#include "../core/game.hpp"
#include "../core/gamestate.hpp"
#include "../enemy/enemy.hpp"
#include "../viewmodel/viewmodeldebug.hpp"
#include "../weapon/weapon.hpp"
#include "../weapon/weaponinventory.hpp"

#include "raygui/raygui.h"
#include "raylib.h"

namespace {
int aliveEnemyCount(const Game &game) {
  int count = 0;

  for (const Enemy &enemy : game.getEnemies()) {
    if (enemy.isAlive()) {
      ++count;
    }
  }

  return count;
}

void drawViewmodelDebugPanel() {
  if (!ViewmodelDebug::panelOpen) {
    DrawText("F2 Viewmodel", 24, 238, 18, Fade(WHITE, 0.65f));
    return;
  }

  DrawRectangle(18, 232, 334, 286, Fade(BLACK, 0.78f));
  DrawRectangleLines(18, 232, 334, 286, Fade(WHITE, 0.28f));
  DrawText("Viewmodel Debug (F2)", 30, 244, 18, WHITE);

  GuiSliderBar({112.0f, 276.0f, 170.0f, 18.0f}, "Pos X", nullptr,
               &ViewmodelDebug::positionOffset.x, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", ViewmodelDebug::positionOffset.x), 292, 276, 16,
           WHITE);
  GuiSliderBar({112.0f, 302.0f, 170.0f, 18.0f}, "Pos Y", nullptr,
               &ViewmodelDebug::positionOffset.y, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", ViewmodelDebug::positionOffset.y), 292, 302, 16,
           WHITE);
  GuiSliderBar({112.0f, 328.0f, 170.0f, 18.0f}, "Pos Z", nullptr,
               &ViewmodelDebug::positionOffset.z, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", ViewmodelDebug::positionOffset.z), 292, 328, 16,
           WHITE);

  GuiSliderBar({112.0f, 364.0f, 170.0f, 18.0f}, "Rot X", nullptr,
               &ViewmodelDebug::rotationOffsetDegrees.x, -180.0f, 180.0f);
  DrawText(TextFormat("% .1f", ViewmodelDebug::rotationOffsetDegrees.x), 292,
           364, 16, WHITE);
  GuiSliderBar({112.0f, 390.0f, 170.0f, 18.0f}, "Rot Y", nullptr,
               &ViewmodelDebug::rotationOffsetDegrees.y, -180.0f, 180.0f);
  DrawText(TextFormat("% .1f", ViewmodelDebug::rotationOffsetDegrees.y), 292,
           390, 16, WHITE);
  GuiSliderBar({112.0f, 416.0f, 170.0f, 18.0f}, "Rot Z", nullptr,
               &ViewmodelDebug::rotationOffsetDegrees.z, -180.0f, 180.0f);
  DrawText(TextFormat("% .1f", ViewmodelDebug::rotationOffsetDegrees.z), 292,
           416, 16, WHITE);

  GuiSliderBar({112.0f, 452.0f, 170.0f, 18.0f}, "Scale", nullptr,
               &ViewmodelDebug::scaleMultiplier, 0.01f, 20.0f);
  DrawText(TextFormat("% .2f", ViewmodelDebug::scaleMultiplier), 292, 452, 16,
           WHITE);

  if (GuiButton({112.0f, 484.0f, 82.0f, 22.0f}, "Reset")) {
    ViewmodelDebug::reset();
  }
}
} // namespace

namespace UI {
void draw(const Game &game) {
  int width = GetScreenWidth();
  int height = GetScreenHeight();

  if (game.isEditorEnabled()) {
    return;
  }

  DrawLine(width / 2 - 10, height / 2, width / 2 - 4, height / 2, BLUE);
  DrawLine(width / 2 + 4, height / 2, width / 2 + 10, height / 2, BLUE);
  DrawLine(width / 2, height / 2 - 10, width / 2, height / 2 - 4, BLUE);
  DrawLine(width / 2, height / 2 + 4, width / 2, height / 2 + 10, BLUE);

  DrawText(TextFormat("HP: %d", game.getPlayer().getHealth()), 24, 24, 28, RED);

  DrawText(TextFormat("Enemies: %d", aliveEnemyCount(game)), 24, 58, 24, RED);

  const Weapon &weapon = game.getWeapon();

  DrawText(TextFormat("Ammo: %d / %d", weapon.getAmmoInMagazine(),
                      weapon.getReserveAmmo()),
           width - 220, height - 72, 28, RED);

  if (weapon.isReloading()) {
    DrawText(
        TextFormat("Reloading %.0f%%", weapon.getReloadProgress() * 100.0f),
        width - 220, height - 104, 22, GREEN);
  }

  const WeaponInventory &inventory = game.getWeaponInventory();

  DrawText(TextFormat("Weapon: %s [%d/%d]", weapon.getData().name,
                      inventory.getActiveWeaponIndex() + 1,
                      inventory.getWeaponCount()),
           width - 260, height - 136, 22, RED);

  DrawText(TextFormat("Current xPos: %f", game.getPlayer().getPosition().x), 24,
           100, 28, RED);
  DrawText(TextFormat("Current zPos: %f", game.getPlayer().getPosition().z), 24,
           138, 28, RED);
  DrawText(TextFormat("Current yPos: %f", game.getPlayer().getPosition().y), 24,
           170, 28, RED);

  if (game.areEnemiesFrozen()) {
    DrawText("Enemies frozen", 24, 208, 24, GREEN);
  }

  drawViewmodelDebugPanel();

  if (game.getState() == GameState::Dead) {
    DrawText("DEAD", width / 2 - 60, height / 2 - 40, 48, RED);
    DrawText("Press R to restart", width / 2 - 120, height / 2 + 18, 24, WHITE);
  }

  if (game.getState() == GameState::Win) {
    DrawText("CLEAR", width / 2 - 70, height / 2 - 40, 48, GREEN);
    DrawText("Press R to restart", width / 2 - 120, height / 2 + 18, 24, WHITE);
  }
}
} // namespace UI
