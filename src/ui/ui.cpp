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

void drawViewmodelDebugPanel(const Game &game) {
  if (!ViewmodelDebug::panelOpen) {
    DrawText("F2 Viewmodel", 24, 238, 18, Fade(WHITE, 0.65f));
    return;
  }

  const WeaponData &weapon = game.getWeapon().getData();
  ViewmodelDebug::syncFromWeapon(weapon);
  ViewmodelDebug::Entry &entry = ViewmodelDebug::entryFor(weapon.modelId);

  DrawRectangle(18, 232, 334, 442, Fade(BLACK, 0.78f));
  DrawRectangleLines(18, 232, 334, 442, Fade(WHITE, 0.28f));
  DrawText(TextFormat("Viewmodel Debug: %s (F2)", weapon.name), 30, 244, 18,
           WHITE);

  GuiSliderBar({112.0f, 276.0f, 170.0f, 18.0f}, "Pos X", nullptr,
               &entry.position.x, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", entry.position.x), 292, 276, 16, WHITE);
  GuiSliderBar({112.0f, 302.0f, 170.0f, 18.0f}, "Pos Y", nullptr,
               &entry.position.y, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", entry.position.y), 292, 302, 16, WHITE);
  GuiSliderBar({112.0f, 328.0f, 170.0f, 18.0f}, "Pos Z", nullptr,
               &entry.position.z, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", entry.position.z), 292, 328, 16, WHITE);

  GuiSliderBar({112.0f, 364.0f, 170.0f, 18.0f}, "Rot X", nullptr,
               &entry.rotationDegrees.x, -180.0f, 180.0f);
  DrawText(TextFormat("% .1f", entry.rotationDegrees.x), 292, 364, 16, WHITE);
  GuiSliderBar({112.0f, 390.0f, 170.0f, 18.0f}, "Rot Y", nullptr,
               &entry.rotationDegrees.y, -180.0f, 180.0f);
  DrawText(TextFormat("% .1f", entry.rotationDegrees.y), 292, 390, 16, WHITE);
  GuiSliderBar({112.0f, 416.0f, 170.0f, 18.0f}, "Rot Z", nullptr,
               &entry.rotationDegrees.z, -180.0f, 180.0f);
  DrawText(TextFormat("% .1f", entry.rotationDegrees.z), 292, 416, 16, WHITE);

  GuiSliderBar({112.0f, 452.0f, 170.0f, 18.0f}, "Scale", nullptr,
               &entry.scale, 0.001f, 1.0f);
  DrawText(TextFormat("% .4f", entry.scale), 292, 452, 16, WHITE);

  GuiSliderBar({112.0f, 488.0f, 170.0f, 18.0f}, "Muz X", nullptr,
               &entry.muzzlePoint.x, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", entry.muzzlePoint.x), 292, 488, 16, WHITE);
  GuiSliderBar({112.0f, 514.0f, 170.0f, 18.0f}, "Muz Y", nullptr,
               &entry.muzzlePoint.y, -1.0f, 1.0f);
  DrawText(TextFormat("% .3f", entry.muzzlePoint.y), 292, 514, 16, WHITE);
  GuiSliderBar({112.0f, 540.0f, 170.0f, 18.0f}, "Muz Z", nullptr,
               &entry.muzzlePoint.z, -1.0f, 2.0f);
  DrawText(TextFormat("% .3f", entry.muzzlePoint.z), 292, 540, 16, WHITE);

  GuiSliderBar({112.0f, 576.0f, 170.0f, 18.0f}, "Flash W", nullptr,
               &entry.muzzleFlashWidth, 0.05f, 4.0f);
  DrawText(TextFormat("% .3f", entry.muzzleFlashWidth), 292, 576, 16, WHITE);
  GuiSliderBar({112.0f, 602.0f, 170.0f, 18.0f}, "Flash H", nullptr,
               &entry.muzzleFlashHeight, 0.05f, 4.0f);
  DrawText(TextFormat("% .3f", entry.muzzleFlashHeight), 292, 602, 16, WHITE);

  if (GuiButton({112.0f, 640.0f, 82.0f, 22.0f}, "Reset")) {
    ViewmodelDebug::reset(weapon);
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

  drawViewmodelDebugPanel(game);

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
