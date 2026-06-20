#include "ui.hpp"
#include "hud.hpp"

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
// int aliveEnemyCount(const Game &game) {
//   int count = 0;
//
//   for (const Enemy &enemy : game.getEnemies()) {
//     if (enemy.isAlive()) {
//       ++count;
//     }
//   }
//
//   return count;
// }

float pulse(float speed) {
  return 0.5f + 0.5f * std::sinf(static_cast<float>(GetTime()) * speed);
}

void drawScanlines(int screenWidth, int screenHeight) {
  for (int y = 0; y < screenHeight; y += 3) {
    DrawRectangle(0, y, screenWidth, 1, Fade(BLACK, 0.26f));
  }

  int sweepY = static_cast<int>(
      std::fmod(GetTime() * 46.0, static_cast<double>(screenHeight)));
  DrawRectangle(0, sweepY, screenWidth, 2, Fade(WHITE, 0.08f));
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

void drawAwarenessIndicator(const Game &game, const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;
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
  Vector2 textSize = MeasureTextEx(ctx.font, label, fontSize, 1.0f);
  Vector2 accentSize =
      MeasureTextEx(ctx.japaneseFont, accent, accentFontSize, 1.0f);
  float contentWidth = accentSize.x + textGap + textSize.x;
  float horizontalPadding = 36.0f;
  Rectangle panel = Hud::anchoredRect(
      ctx.screen, Hud::Anchor::TopLeft,
      {contentWidth + horizontalPadding * 2.0f, 78.0f},
      {ctx.screen.width * 0.5f - contentWidth * 0.5f - horizontalPadding,
       24.0f});
  DrawRectangleRec({panel.x, panel.y, panel.width, panel.height},
                   Fade(style.shadow, 0.82f));

  Hud::panel(ctx, panel, color);
  float contentX = panel.x + panel.width / 2.0f - contentWidth / 2.0f;
  float labelY = panel.y + 16.0f;
  float accentY = labelY + textSize.y / 2.0f - accentSize.y / 2.0f;
  Hud::text(ctx, ctx.japaneseFont, accent, {contentX, accentY}, accentFontSize,
            Fade(color, 0.72f));
  Hud::text(ctx, ctx.font, label, {contentX + accentSize.x + textGap, labelY},
            fontSize, color);
}

void drawWatermark(const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;
  float fontSize = 25.0f;
  float lineHeight = 28.0f;
  float right = ctx.screen.width - 24.0f;
  float y = 45.0f;
  Color textColor = Fade(style.text, 0.50f);

  auto drawRightAligned = [&](const char *text, float lineY, Color color) {
    Vector2 size = MeasureTextEx(ctx.font, text, fontSize, 1.0f);
    Hud::text(ctx, ctx.font, text, {right - size.x, lineY}, fontSize, color);
  };

  drawRightAligned("APOPLEXY [PRE-PRE-ALPHA] v.0.7", y, textColor);
  drawRightAligned("USER : OPERATOR_01 (000000000)", y + lineHeight, textColor);
  drawRightAligned("MADE BY CZAPLA", y + lineHeight * 2.0f, textColor);
}

#ifdef DEBUG
void drawViewmodelDebugPanel(const Game &game) {
  if (!ViewmodelDebug::panelOpen) {
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

void drawHealthBar(const Game &game, const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;

  int health = game.getPlayer().getHealth();
  float healthPercent = static_cast<float>(health) / 100.0f;

  if (healthPercent < 0.0f) {
    healthPercent = 0.0f;
  } else if (healthPercent > 1.0f) {
    healthPercent = 1.0f;
  }

  float width = 620.0f;
  float height = 13.0f;
  Rectangle bar = Hud::anchoredRect(ctx.screen, Hud::Anchor::BottomCenter,
                                    {width, height}, {0.0f, 94.0f});
  float x = bar.x;
  float y = bar.y;
  float tick = 12.0f;
  float labelY = y - 35.0f;

  Color frame = Fade(style.text, 0.78);
  Color fill = health <= 30 ? Fade(style.danger, 0.72f + pulse(32.0f) * 0.28f)
                            : Fade(style.text, 0.88f);
  Color backing = Fade(style.shadow, 0.42f);
  DrawRectangleRec({x, y, width, height}, backing);
  DrawRectangleRec(
      {x + 3.0f, y + 3.0f, (width - 6.0f) * healthPercent, height - 6.0f},
      fill);

  DrawRectangleLinesEx({x, y, width, height}, 2.0f, frame);

  DrawLineEx({x - tick, y}, {x - 2.0f, y}, 2.0f, frame);
  DrawLineEx({x - tick, y + height}, {x - 2.0f, y + height}, 2.0f, frame);
  DrawLineEx({x + width + 2.0f, y}, {x + width + tick, y}, 2.0f, frame);
  DrawLineEx({x + width + 2.0f, y + height}, {x + width + tick, y + height},
             2.0f, frame);

  DrawLineEx({x + width * 0.25f, y + 2.0f},
             {x + width * 0.25f, y + height - 2.0f}, 1.0f,
             Fade(style.shadow, 0.52f));
  DrawLineEx({x + width * 0.5f, y + 2.0f},
             {x + width * 0.5f, y + height - 2.0f}, 1.0f,
             Fade(style.shadow, 0.52f));
  DrawLineEx({x + width * 0.75f, y + 2.0f},
             {x + width * 0.75f, y + height - 2.0f}, 1.0f,
             Fade(style.shadow, 0.52f));

  Hud::text(ctx, ctx.font, "VITAL", {x, labelY}, 24.0f,
            Fade(style.text, 0.84f));

  const char *healthText = TextFormat("%03d", health);
  Vector2 healthTextSize = MeasureTextEx(ctx.font, healthText, 21.0f, 1.0f);
  Hud::text(ctx, ctx.font, healthText,
            {x + width - healthTextSize.x, labelY + 2.0f}, 21.0f, fill);
}

void drawCrosshair(const Game &game, const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;
  const Weapon &weapon = game.getWeapon();

  float spread = weapon.getCurrentSpreadDegrees(game.getPlayer());
  int gap = 5 + static_cast<int>(spread * 4.0f);
  int length = 10;
  int thickness = 2;
  int centerX = static_cast<int>(ctx.screen.x + ctx.screen.width * 0.5f);
  int centerY = static_cast<int>(ctx.screen.y + ctx.screen.height * 0.5f);
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

void drawWeaponStatus(const Game &game, const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;
  const Weapon &weapon = game.getWeapon();
  const WeaponInventory &inventory = game.getWeaponInventory();
  Rectangle bounds = Hud::anchoredRect(ctx.screen, Hud::Anchor::BottomRight,
                                       {459.0f, 201.0f}, {22.0f, 24.0f});
  Hud::Panel panel{bounds, bounds.y + 72.0f};
  Color neutralColor = Fade(style.dimText, 0.82f);
  Color panelColor = weapon.isReloading() ? style.danger : neutralColor;

  panel.draw(ctx, panelColor);
  panel.header(ctx, "WEAPON I/F", "弾薬", panelColor);
  panel.labelValue(ctx, "MODEL", weapon.getData().name, neutralColor);
  panel.labelValue(ctx, "MAG",
                   TextFormat("%02d / %03d", weapon.getAmmoInMagazine(),
                              weapon.getReserveAmmo()),
                   panelColor);

  if (weapon.isReloading()) {
    panel.progress(ctx, weapon.getReloadProgress(), style.danger);
  } else {
    panel.cursorY = bounds.y + 165.0f;
    panel.message(ctx,
                  TextFormat("SLOT %d/%d", inventory.getActiveWeaponIndex() + 1,
                             inventory.getWeaponCount()),
                  24.0f, style.dimText);
  }
}

#ifdef DEBUG
void drawDebugOverlay(const Game &game, const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;
  Rectangle bounds = Hud::anchoredRect(ctx.screen, Hud::Anchor::TopLeft,
                                       {381.0f, 177.0f}, {18.0f, 18.0f});
  Hud::Panel panel{bounds, bounds.y + 72.0f};
  Vector3 position = game.getPlayer().getPosition();

  panel.draw(ctx, Fade(style.dimText, 0.82f));
  panel.header(ctx, "DEBUG", "診断", style.dimText);

  panel.message(ctx, TextFormat("X  % .3f", position.x), 24.0f, style.dimText);
  panel.message(ctx, TextFormat("Z  % .3f", position.z), 24.0f, style.dimText);
  panel.message(ctx, TextFormat("Y  % .3f", position.y), 24.0f, style.dimText);

  if (game.areEnemiesFrozen()) {
    Hud::text(ctx, ctx.font, "ENEMIES FROZEN",
              {bounds.x + 190.0f, bounds.y + 72.0f}, 20.0f, style.status);
  }

  if (Weapon::debugRaysEnabled) {
    Hud::text(ctx, ctx.font, "SHOT RAYS F4",
              {bounds.x + 190.0f, bounds.y + 106.0f}, 20.0f, style.status);
  }

  drawViewmodelDebugPanel(game);
}
#endif

void drawStateOverlay(const Game &game, const Hud::Context &ctx) {
  const Hud::Style &style = ctx.style;

  if (game.getState() != GameState::Dead && game.getState() != GameState::Win) {
    return;
  }

  bool dead = game.getState() == GameState::Dead;
  const char *title = dead ? "SIGNAL LOST" : "you! !!won";
  const char *accent = dead ? "断線" : "完了";
  Color color = dead ? style.danger : style.text;
  Rectangle panel = Hud::anchoredRect(ctx.screen, Hud::Anchor::Center,
                                      {646.0f, 224.0f}, {0.0f, 0.0f});
  float overlayDim = dead ? 0.52f : 0.76f;

  DrawRectangleRec(ctx.screen, Fade(BLACK, overlayDim));
  Hud::panel(ctx, panel, color);
  Hud::systemHeader(ctx, panel, "futuristic text here", accent, color);
  Hud::text(ctx, ctx.font, title, {panel.x + 48.0f, panel.y + 80.0f}, 58.0f,
            color);
  Hud::text(ctx, ctx.font, "r to reboot or sum",
            {panel.x + 48.0f, panel.y + 158.0f}, 31.0f, style.text);
}
} // namespace

namespace UI {
void draw(const Game &game) {
  if (game.isEditorEnabled()) {
    return;
  }

  int width = Hud::REFERENCE_WIDTH;
  int height = Hud::REFERENCE_HEIGHT;
  const Font &font = game.getAssets().getTerminalFont();
  const Font &japaneseFont = game.getAssets().getJapaneseFont();
  Hud::Context ctx{
      font,
      japaneseFont,
      Hud::style(),
      {0.0f, 0.0f, static_cast<float>(width), static_cast<float>(height)}};

  Hud::beginScale();

  drawHealthBar(game, ctx);
  drawWatermark(ctx);
  drawAwarenessIndicator(game, ctx);
  drawCrosshair(game, ctx);
  drawWeaponStatus(game, ctx);

#ifdef DEBUG
  drawDebugOverlay(game, ctx);
#endif
  drawStateOverlay(game, ctx);
  drawScanlines(width, height);

  Hud::endScale();
}
} // namespace UI
