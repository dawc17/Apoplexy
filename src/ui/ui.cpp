#include "ui.hpp"

#include "../core/game.hpp"
#include "../core/gamestate.hpp"
#include "../enemy/enemy.hpp"
#ifdef DEBUG
#include "../viewmodel/viewmodeldebug.hpp"
#endif
#include "../weapon/weapon.hpp"
#include "../weapon/weaponinventory.hpp"

#ifdef DEBUG
#include "raygui/raygui.h"
#endif
#include "raylib.h"

#include <cmath>

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

struct HudStyle {
  Color text;
  Color dimText;
  Color panel;
  Color panelBorder;
  Color terminal;
  Color warning;
  Color danger;
  Color reticle;
  Color status;
  Color shadow;
};

const HudStyle &hudStyle() {
  static const HudStyle style{
      {214, 255, 205, 255}, {102, 154, 114, 255}, {2, 10, 6, 178},
      {78, 222, 116, 190},  {84, 255, 128, 255},  {230, 42, 42, 255},
      {185, 255, 205, 220}, {100, 255, 135, 255}, {0, 0, 0, 210}};

  return style;
}

float pulse(float speed) {
  return 0.5f + 0.5f * std::sinf(static_cast<float>(GetTime()) * speed);
}

void drawHudText(const Font &font, const char *text, Vector2 position,
                 float fontSize, Color color) {
  const HudStyle &style = hudStyle();
  DrawTextEx(font, text, {position.x + 1.0f, position.y + 1.0f}, fontSize, 1.0f,
             style.shadow);
  DrawTextEx(font, text, position, fontSize, 1.0f, color);
}

void drawHudPanel(Rectangle bounds, Color borderColor) {
  const HudStyle &style = hudStyle();
  constexpr float tick = 12.0f;

  DrawRectangleRec(bounds, style.panel);
  DrawRectangleLinesEx(bounds, 1.0f, Fade(borderColor, 0.48f));

  DrawLineEx({bounds.x, bounds.y}, {bounds.x + tick, bounds.y}, 2.0f,
             borderColor);
  DrawLineEx({bounds.x, bounds.y}, {bounds.x, bounds.y + tick}, 2.0f,
             borderColor);
  DrawLineEx({bounds.x + bounds.width - tick, bounds.y},
             {bounds.x + bounds.width, bounds.y}, 2.0f, borderColor);
  DrawLineEx({bounds.x + bounds.width, bounds.y},
             {bounds.x + bounds.width, bounds.y + tick}, 2.0f, borderColor);
  DrawLineEx({bounds.x, bounds.y + bounds.height - tick},
             {bounds.x, bounds.y + bounds.height}, 2.0f, borderColor);
  DrawLineEx({bounds.x, bounds.y + bounds.height},
             {bounds.x + tick, bounds.y + bounds.height}, 2.0f, borderColor);
  DrawLineEx({bounds.x + bounds.width - tick, bounds.y + bounds.height},
             {bounds.x + bounds.width, bounds.y + bounds.height}, 2.0f,
             borderColor);
  DrawLineEx({bounds.x + bounds.width, bounds.y + bounds.height - tick},
             {bounds.x + bounds.width, bounds.y + bounds.height}, 2.0f,
             borderColor);
}

void drawLabelValue(const Font &font, const char *label, const char *value,
                    Vector2 position, float valueX, Color valueColor) {
  const HudStyle &style = hudStyle();
  drawHudText(font, label, position, 16.0f, style.dimText);
  drawHudText(font, value, {valueX, position.y - 2.0f}, 22.0f, valueColor);
}

void drawScanlines(int screenWidth, int screenHeight) {
  for (int y = 0; y < screenHeight; y += 4) {
    DrawRectangle(0, y, screenWidth, 1, Fade(BLACK, 0.18f));
  }

  int sweepY = static_cast<int>(
      std::fmod(GetTime() * 46.0, static_cast<double>(screenHeight)));
  DrawRectangle(0, sweepY, screenWidth, 2, Fade(hudStyle().terminal, 0.08f));
}

void drawSystemHeader(const Font &font, Rectangle bounds, const char *title,
                      const char *accent, Color color) {
  const HudStyle &style = hudStyle();
  drawHudText(font, title, {bounds.x + 14.0f, bounds.y + 8.0f}, 14.0f, color);

  Vector2 accentSize = MeasureTextEx(font, accent, 14.0f, 1.0f);
  drawHudText(font, accent,
              {bounds.x + bounds.width - accentSize.x - 14.0f, bounds.y + 8.0f},
              14.0f, Fade(style.dimText, 0.88f));

  DrawLineEx({bounds.x + 12.0f, bounds.y + 30.0f},
             {bounds.x + bounds.width - 12.0f, bounds.y + 30.0f}, 1.0f,
             Fade(color, 0.35f));
}

const char *awarenessAccent(const char *label) {
  if (TextIsEqual(label, "DISCOVERED")) {
    return "警告";
  }
  if (TextIsEqual(label, "SPOTTED")) {
    return "視認";
  }
  if (TextIsEqual(label, "SEEN?")) {
    return "検知";
  }

  return "聴音";
}

void drawAwarenessIndicator(const Game &game, const Font font,
                            int screenWidth) {
  const HudStyle &style = hudStyle();
  const char *label = nullptr;
  Color color = style.text;

  for (const Enemy &enemy : game.getEnemies()) {
    if (!enemy.isAlive()) {
      continue;
    }

    EnemyState state = enemy.getState();
    if (state == EnemyState::Chase || state == EnemyState::AttackWindup ||
        state == EnemyState::AttackRecovery) {
      label = "DISCOVERED";
      color = Fade(style.danger, 0.7f + pulse(12.0f) * 0.3f);
      break;
    }

    if (state == EnemyState::Alert) {
      label = "SPOTTED";
      color = style.warning;
    } else if (state == EnemyState::Suspicious && label == nullptr) {
      label = "SEEN?";
      color = style.warning;
    } else if (state == EnemyState::Search && label == nullptr) {
      label = "HEARD";
      color = style.warning;
    }
  }

  if (label == nullptr) {
    return;
  }

  float fontSize = 28.0f;
  Vector2 textSize = MeasureTextEx(font, label, fontSize, 1.0f);
  Rectangle panel{screenWidth / 2.0f - textSize.x / 2.0f - 34.0f, 20.0f,
                  textSize.x + 68.0f, 54.0f};

  drawHudPanel(panel, color);
  drawHudText(font, awarenessAccent(label), {panel.x + 12.0f, panel.y + 8.0f},
              14.0f, Fade(color, 0.72f));
  drawHudText(font, label, {panel.x + 34.0f, panel.y + 20.0f}, fontSize, color);
}

#ifdef DEBUG
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

  GuiSliderBar({112.0f, 452.0f, 170.0f, 18.0f}, "Scale", nullptr, &entry.scale,
               0.001f, 1.0f);
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
#endif

void drawVitals(const Game &game, const Font &font) {
  const HudStyle &style = hudStyle();

  Rectangle panel{18.0f, 18.0f, 224.0f, 104.0f};
  int health = game.getPlayer().getHealth();
  Color healthColor = health <= 30
                          ? Fade(style.danger, 0.62f + pulse(16.0f) * 0.38f)
                          : style.terminal;

  drawHudPanel(panel, healthColor);
  drawSystemHeader(font, panel, "BIO-LINK", "状態", healthColor);
  drawLabelValue(font, "VITAL", TextFormat("%03d", health),
                 {panel.x + 14.0f, panel.y + 42.0f}, panel.x + 128.0f,
                 healthColor);
  drawLabelValue(font, "TARGETS", TextFormat("%02d", aliveEnemyCount(game)),
                 {panel.x + 14.0f, panel.y + 72.0f}, panel.x + 128.0f,
                 style.danger);
}

void drawCrosshair(const Game &game, int screenWidth, int screenHeight) {
  const HudStyle &style = hudStyle();
  const Weapon &weapon = game.getWeapon();

  float spread = weapon.getCurrentSpreadDegrees(game.getPlayer());
  int gap = 5 + static_cast<int>(spread * 4.0f);
  int length = 10;
  int thickness = 2;
  int centerX = screenWidth / 2;
  int centerY = screenHeight / 2;
  Color reticle = Fade(style.reticle, 0.72f + pulse(7.0f) * 0.18f);

  DrawRectangle(centerX - gap - length, centerY - thickness / 2, length,
                thickness, reticle);
  DrawRectangle(centerX + gap, centerY - thickness / 2, length, thickness,
                reticle);
  DrawRectangle(centerX - thickness / 2, centerY - gap - length, thickness,
                length, reticle);
  DrawRectangle(centerX - thickness / 2, centerY + gap, thickness, length,
                reticle);
  DrawRectangle(centerX - 1, centerY - 1, 2, 2, Fade(style.terminal, 0.7f));
}

void drawWeaponStatus(const Game &game, const Font &font, int screenWidth,
                      int screenHeight) {
  const HudStyle &style = hudStyle();
  const Weapon &weapon = game.getWeapon();
  const WeaponInventory &inventory = game.getWeaponInventory();
  Rectangle panel{screenWidth - 292.0f, screenHeight - 142.0f, 270.0f, 118.0f};
  Color panelColor = weapon.isReloading() ? style.warning : style.terminal;

  drawHudPanel(panel, panelColor);
  drawSystemHeader(font, panel, "WEAPON I/F", "弾薬", panelColor);
  drawLabelValue(font, "MODEL", weapon.getData().name,
                 {panel.x + 14.0f, panel.y + 42.0f}, panel.x + 112.0f,
                 style.text);
  drawLabelValue(font, "MAG",
                 TextFormat("%02d / %03d", weapon.getAmmoInMagazine(),
                            weapon.getReserveAmmo()),
                 {panel.x + 14.0f, panel.y + 72.0f}, panel.x + 112.0f,
                 panelColor);

  if (weapon.isReloading()) {
    float progress = weapon.getReloadProgress();
    Rectangle bar{panel.x + 14.0f, panel.y + 100.0f,
                  (panel.width - 28.0f) * progress, 4.0f};
    DrawRectangleRec(
        {panel.x + 14.0f, panel.y + 100.0f, panel.width - 28.0f, 4.0f},
        Fade(style.dimText, 0.35f));
    DrawRectangleRec(bar, style.warning);
  } else {
    drawHudText(font,
                TextFormat("SLOT %d/%d", inventory.getActiveWeaponIndex() + 1,
                           inventory.getWeaponCount()),
                {panel.x + 14.0f, panel.y + 96.0f}, 14.0f, style.dimText);
  }
}

void drawPlayerStatusIndicators(const Game &game, const Font &font) {
  const HudStyle &style = hudStyle();

  if (game.getPlayer().isCrouching()) {
    Rectangle panel{18.0f, 132.0f, 154.0f, 38.0f};
    drawHudPanel(panel, style.warning);
    drawHudText(font, "LOW PROFILE", {panel.x + 14.0f, panel.y + 11.0f}, 16.0f,
                style.warning);
  }
}

#ifdef DEBUG
void drawDebugOverlay(const Game &game, const Font &font, int screenHeight) {
  const HudStyle &style = hudStyle();
  float panelY =
      screenHeight > 220 ? static_cast<float>(screenHeight) - 142.0f : 178.0f;
  Rectangle panel{18.0f, panelY, 300.0f, 118.0f};
  Vector3 position = game.getPlayer().getPosition();

  drawHudPanel(panel, Fade(style.dimText, 0.82f));
  drawSystemHeader(font, panel, "DEBUG", "診断", style.dimText);

  drawHudText(font, TextFormat("X  % .3f", position.x),
              {panel.x + 14.0f, panel.y + 42.0f}, 16.0f, style.dimText);
  drawHudText(font, TextFormat("Z  % .3f", position.z),
              {panel.x + 14.0f, panel.y + 64.0f}, 16.0f, style.dimText);
  drawHudText(font, TextFormat("Y  % .3f", position.y),
              {panel.x + 14.0f, panel.y + 86.0f}, 16.0f, style.dimText);

  if (game.areEnemiesFrozen()) {
    drawHudText(font, "ENEMIES FROZEN", {panel.x + 150.0f, panel.y + 42.0f},
                14.0f, style.status);
  }

  if (Weapon::debugRaysEnabled) {
    drawHudText(font, "SHOT RAYS F4", {panel.x + 150.0f, panel.y + 64.0f},
                14.0f, style.status);
  }

  drawViewmodelDebugPanel(game);
}
#endif

void drawStateOverlay(const Game &game, int screenWidth, int screenHeight) {
  const HudStyle &style = hudStyle();

  if (game.getState() != GameState::Dead && game.getState() != GameState::Win) {
    return;
  }

  bool dead = game.getState() == GameState::Dead;
  const char *title = dead ? "SIGNAL LOST" : "AREA CLEAR";
  const char *accent = dead ? "断線" : "完了";
  Color color = dead ? style.danger : style.terminal;
  Font font = game.getAssets().getTerminalFont();
  Rectangle panel{screenWidth / 2.0f - 190.0f, screenHeight / 2.0f - 76.0f,
                  380.0f, 132.0f};

  DrawRectangle(0, 0, screenWidth, screenHeight, Fade(BLACK, 0.52f));
  drawHudPanel(panel, color);
  drawSystemHeader(font, panel, "OPERATOR LINK", accent, color);
  drawHudText(font, title, {panel.x + 28.0f, panel.y + 46.0f}, 34.0f, color);
  drawHudText(font, "PRESS R TO REBOOT", {panel.x + 28.0f, panel.y + 92.0f},
              18.0f, style.text);
}
} // namespace

namespace UI {
void draw(const Game &game) {
  int width = GetScreenWidth();
  int height = GetScreenHeight();

  if (game.isEditorEnabled()) {
    return;
  }

  const Font &font = game.getAssets().getTerminalFont();

  drawVitals(game, font);
  drawAwarenessIndicator(game, font, width);
  drawCrosshair(game, width, height);
  drawWeaponStatus(game, font, width, height);
  drawPlayerStatusIndicators(game, font);

#ifdef DEBUG
  drawDebugOverlay(game, font, height);
#endif
  drawStateOverlay(game, width, height);
  drawScanlines(width, height);
}
} // namespace UI
