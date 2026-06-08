#include "ui.hpp"

#include "../core/game.hpp"
#include "../core/gamestate.hpp"
#include "../enemy/enemy.hpp"
#include "../weapon/weapon.hpp"
#include "../weapon/weaponinventory.hpp"

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
