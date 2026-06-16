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
  static const HudStyle style{{235, 235, 225, 255}, {132, 132, 126, 255},
                              {0, 0, 0, 196},       {218, 218, 206, 220},
                              {235, 235, 225, 255}, {245, 245, 235, 255},
                              {210, 24, 28, 255},   {235, 235, 225, 220},
                              {210, 24, 28, 255},   {0, 0, 0, 230}};

  return style;
}

float pulse(float speed) {
  return 0.5f + 0.5f * std::sinf(static_cast<float>(GetTime()) * speed);
}

constexpr float HUD_SCALE = 1.7f;
constexpr float HUD_PAD = 24.0f;
constexpr float HUD_HEADER_HEIGHT = 34.0f;
constexpr float HUD_HEADER_FONT_SIZE = 21.0f;
constexpr float HUD_LABEL_FONT_SIZE = 22.0f;
constexpr float HUD_VALUE_FONT_SIZE = 31.0f;

void drawHudText(const Font &font, const char *text, Vector2 position,
                 float fontSize, Color color) {
  const HudStyle &style = hudStyle();
  DrawTextEx(font, text, {position.x + 2.0f, position.y + 2.0f}, fontSize, 1.0f,
             Fade(style.shadow, 0.85f));
  DrawTextEx(font, text, position, fontSize, 1.0f, color);
}

void drawHudPanel(Rectangle bounds, Color borderColor) {
  const HudStyle &style = hudStyle();
  constexpr float tick = 24.0f;

  DrawRectangleRec(bounds, style.panel);
  DrawRectangleLinesEx(bounds, 2.0f, Fade(borderColor, 0.72f));
  DrawRectangleRec({bounds.x, bounds.y, bounds.width, 5.0f},
                   Fade(borderColor, 0.92f));
  DrawRectangleRec(
      {bounds.x, bounds.y + bounds.height - 5.0f, bounds.width, 5.0f},
      Fade(borderColor, 0.58f));

  DrawLineEx({bounds.x, bounds.y}, {bounds.x + tick, bounds.y}, 3.0f,
             borderColor);
  DrawLineEx({bounds.x, bounds.y}, {bounds.x, bounds.y + tick}, 3.0f,
             borderColor);
  DrawLineEx({bounds.x + bounds.width - tick, bounds.y},
             {bounds.x + bounds.width, bounds.y}, 3.0f, borderColor);
  DrawLineEx({bounds.x + bounds.width, bounds.y},
             {bounds.x + bounds.width, bounds.y + tick}, 3.0f, borderColor);
  DrawLineEx({bounds.x, bounds.y + bounds.height - tick},
             {bounds.x, bounds.y + bounds.height}, 3.0f, borderColor);
  DrawLineEx({bounds.x, bounds.y + bounds.height},
             {bounds.x + tick, bounds.y + bounds.height}, 3.0f, borderColor);
  DrawLineEx({bounds.x + bounds.width - tick, bounds.y + bounds.height},
             {bounds.x + bounds.width, bounds.y + bounds.height}, 3.0f,
             borderColor);
  DrawLineEx({bounds.x + bounds.width, bounds.y + bounds.height - tick},
             {bounds.x + bounds.width, bounds.y + bounds.height}, 3.0f,
             borderColor);
}

void drawLabelValue(const Font &font, const char *label, const char *value,
                    Vector2 position, float valueX, Color valueColor) {
  const HudStyle &style = hudStyle();
  drawHudText(font, label, position, HUD_LABEL_FONT_SIZE, style.dimText);
  drawHudText(font, value, {valueX, position.y - 4.0f}, HUD_VALUE_FONT_SIZE,
              valueColor);
}

void drawScanlines(int screenWidth, int screenHeight) {
  for (int y = 0; y < screenHeight; y += 3) {
    DrawRectangle(0, y, screenWidth, 1, Fade(BLACK, 0.26f));
  }

  int sweepY = static_cast<int>(
      std::fmod(GetTime() * 46.0, static_cast<double>(screenHeight)));
  DrawRectangle(0, sweepY, screenWidth, 2, Fade(WHITE, 0.08f));
}

void drawSystemHeader(const Font &font, const Font &japaneseFont,
                      Rectangle bounds, const char *title, const char *accent,
                      Color color) {
  const HudStyle &style = hudStyle();
  DrawRectangleRec(
      {bounds.x + 12.0f, bounds.y + 12.0f, bounds.width - 24.0f,
       HUD_HEADER_HEIGHT},
      Fade(style.text, 0.12f));
  DrawLineEx({bounds.x + 12.0f, bounds.y + 12.0f + HUD_HEADER_HEIGHT + 2.0f},
             {bounds.x + bounds.width - 12.0f,
              bounds.y + 12.0f + HUD_HEADER_HEIGHT + 2.0f},
             1.0f,
             Fade(style.text, 0.62f));

  drawHudText(font, title, {bounds.x + 24.0f, bounds.y + 17.0f},
              HUD_HEADER_FONT_SIZE, color);

  Vector2 accentSize =
      MeasureTextEx(japaneseFont, accent, HUD_HEADER_FONT_SIZE, 1.0f);
  drawHudText(
      japaneseFont, accent,
      {bounds.x + bounds.width - accentSize.x - 24.0f, bounds.y + 17.0f},
      HUD_HEADER_FONT_SIZE,
      Fade(style.dimText, 0.88f));
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

void drawAwarenessIndicator(const Game &game, const Font &font,
                            const Font &japaneseFont, int screenWidth) {
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

  float fontSize = 46.0f;
  float accentFontSize = 24.0f;
  float textGap = 34.0f;
  const char *accent = awarenessAccent(label);
  Vector2 textSize = MeasureTextEx(font, label, fontSize, 1.0f);
  Vector2 accentSize =
      MeasureTextEx(japaneseFont, accent, accentFontSize, 1.0f);
  float contentWidth = accentSize.x + textGap + textSize.x;
  float horizontalPadding = 36.0f;
  Rectangle panel{screenWidth / 2.0f - contentWidth / 2.0f -
                      horizontalPadding,
                  24.0f, contentWidth + horizontalPadding * 2.0f, 78.0f};
  DrawRectangleRec({panel.x, panel.y, panel.width, panel.height},
                   Fade(style.shadow, 0.82f));

  drawHudPanel(panel, color);
  float contentX = panel.x + panel.width / 2.0f - contentWidth / 2.0f;
  float labelY = panel.y + 16.0f;
  float accentY = labelY + textSize.y / 2.0f - accentSize.y / 2.0f;
  drawHudText(japaneseFont, accent, {contentX, accentY}, accentFontSize,
              Fade(color, 0.72f));
  drawHudText(font, label,
              {contentX + accentSize.x + textGap, labelY}, fontSize, color);
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

void drawVitals(const Game &game, const Font &font, const Font &japaneseFont) {
  const HudStyle &style = hudStyle();

  Rectangle panel{18.0f, 18.0f, 381.0f, 177.0f};
  int health = game.getPlayer().getHealth();
  Color neutralColor = Fade(style.dimText, 0.82f);
  Color healthColor = health <= 30
                          ? Fade(style.danger, 0.62f + pulse(16.0f) * 0.38f)
                          : neutralColor;

  drawHudPanel(panel, healthColor);
  drawSystemHeader(font, japaneseFont, panel, "BIO-LINK", "状態", healthColor);
  drawLabelValue(font, "VITAL", TextFormat("%03d", health),
                 {panel.x + HUD_PAD, panel.y + 72.0f}, panel.x + 218.0f,
                 healthColor);
  drawLabelValue(font, "TARGETS", TextFormat("%02d", aliveEnemyCount(game)),
                 {panel.x + HUD_PAD, panel.y + 122.0f}, panel.x + 218.0f,
                 aliveEnemyCount(game) > 0 ? style.danger : style.dimText);
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
  Color reticle = Fade(style.reticle, 0.62f + pulse(7.0f) * 0.18f);

  DrawRectangle(centerX - gap - length, centerY - thickness / 2, length,
                thickness, reticle);
  DrawRectangle(centerX + gap, centerY - thickness / 2, length, thickness,
                reticle);
  DrawRectangle(centerX - thickness / 2, centerY - gap - length, thickness,
                length, reticle);
  DrawRectangle(centerX - thickness / 2, centerY + gap, thickness, length,
                reticle);
  DrawRectangle(centerX - 1, centerY - 1, 2, 2, Fade(style.dimText, 0.7f));
}

void drawWeaponStatus(const Game &game, const Font &font,
                      const Font &japaneseFont, int screenWidth,
                      int screenHeight) {
  const HudStyle &style = hudStyle();
  const Weapon &weapon = game.getWeapon();
  const WeaponInventory &inventory = game.getWeaponInventory();
  Rectangle panel{screenWidth - 481.0f, screenHeight - 225.0f, 459.0f,
                  201.0f};
  Color neutralColor = Fade(style.dimText, 0.82f);
  Color panelColor = weapon.isReloading() ? style.danger : neutralColor;

  drawHudPanel(panel, panelColor);
  drawSystemHeader(font, japaneseFont, panel, "WEAPON I/F", "弾薬",
                   panelColor);
  drawLabelValue(font, "MODEL", weapon.getData().name,
                 {panel.x + HUD_PAD, panel.y + 72.0f}, panel.x + 190.0f,
                 neutralColor);
  drawLabelValue(font, "MAG",
                 TextFormat("%02d / %03d", weapon.getAmmoInMagazine(),
                            weapon.getReserveAmmo()),
                 {panel.x + HUD_PAD, panel.y + 122.0f}, panel.x + 190.0f,
                 panelColor);

  if (weapon.isReloading()) {
    float progress = weapon.getReloadProgress();
    Rectangle bar{panel.x + HUD_PAD, panel.y + 170.0f,
                  (panel.width - HUD_PAD * 2.0f) * progress, 7.0f};
    DrawRectangleRec(
        {panel.x + HUD_PAD, panel.y + 170.0f, panel.width - HUD_PAD * 2.0f,
         7.0f},
        Fade(style.dimText, 0.35f));
    DrawRectangleRec(bar, style.danger);
  } else {
    drawHudText(font,
                TextFormat("SLOT %d/%d", inventory.getActiveWeaponIndex() + 1,
                           inventory.getWeaponCount()),
                {panel.x + HUD_PAD, panel.y + 165.0f}, 24.0f, style.dimText);
  }
}

void drawPlayerStatusIndicators(const Game &game, const Font &font) {
  const HudStyle &style = hudStyle();

  if (game.getPlayer().isCrouching()) {
    Rectangle panel{18.0f, 212.0f, 262.0f, 65.0f};
    Color neutralColor = Fade(style.dimText, 0.82f);
    drawHudPanel(panel, neutralColor);
    drawHudText(font, "LOW PROFILE", {panel.x + 24.0f, panel.y + 18.0f},
                27.0f,
                neutralColor);
  }
}

#ifdef DEBUG
void drawDebugOverlay(const Game &game, const Font &font,
                      const Font &japaneseFont, int screenHeight) {
  const HudStyle &style = hudStyle();
  float panelY =
      screenHeight > 340 ? static_cast<float>(screenHeight) - 225.0f : 178.0f;
  Rectangle panel{18.0f, panelY, 510.0f, 201.0f};
  Vector3 position = game.getPlayer().getPosition();

  drawHudPanel(panel, Fade(style.dimText, 0.82f));
  drawSystemHeader(font, japaneseFont, panel, "DEBUG", "診断", style.dimText);

  drawHudText(font, TextFormat("X  % .3f", position.x),
              {panel.x + HUD_PAD, panel.y + 72.0f}, 27.0f, style.dimText);
  drawHudText(font, TextFormat("Z  % .3f", position.z),
              {panel.x + HUD_PAD, panel.y + 109.0f}, 27.0f, style.dimText);
  drawHudText(font, TextFormat("Y  % .3f", position.y),
              {panel.x + HUD_PAD, panel.y + 146.0f}, 27.0f, style.dimText);

  if (game.areEnemiesFrozen()) {
    drawHudText(font, "ENEMIES FROZEN", {panel.x + 255.0f, panel.y + 72.0f},
                24.0f, style.status);
  }

  if (Weapon::debugRaysEnabled) {
    drawHudText(font, "SHOT RAYS F4", {panel.x + 255.0f, panel.y + 109.0f},
                24.0f, style.status);
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
  Color color = dead ? style.danger : style.text;
  const Font &font = game.getAssets().getTerminalFont();
  const Font &japaneseFont = game.getAssets().getJapaneseFont();
  Rectangle panel{screenWidth / 2.0f - 323.0f, screenHeight / 2.0f - 112.0f,
                  646.0f, 224.0f};

  DrawRectangle(0, 0, screenWidth, screenHeight, Fade(BLACK, 0.52f));
  drawHudPanel(panel, color);
  drawSystemHeader(font, japaneseFont, panel, "OPERATOR LINK", accent, color);
  drawHudText(font, title, {panel.x + 48.0f, panel.y + 80.0f}, 58.0f, color);
  drawHudText(font, "PRESS R TO REBOOT", {panel.x + 48.0f, panel.y + 158.0f},
              31.0f, style.text);
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
  const Font &japaneseFont = game.getAssets().getJapaneseFont();

  drawVitals(game, font, japaneseFont);
  drawAwarenessIndicator(game, font, japaneseFont, width);
  drawCrosshair(game, width, height);
  drawWeaponStatus(game, font, japaneseFont, width, height);
  drawPlayerStatusIndicators(game, font);

#ifdef DEBUG
  drawDebugOverlay(game, font, japaneseFont, height);
#endif
  drawStateOverlay(game, width, height);
  drawScanlines(width, height);
}
} // namespace UI
