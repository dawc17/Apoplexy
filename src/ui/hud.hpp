#pragma once

#include "raylib.h"
#include "rlgl.h"

#include <algorithm>

namespace Hud {
constexpr float PAD = 24.0f;
constexpr float HEADER_HEIGHT = 34.0f;
constexpr float HEADER_FONT_SIZE = 21.0f;
constexpr float LABEL_FONT_SIZE = 22.0f;
constexpr float VALUE_FONT_SIZE = 31.0f;
constexpr int REFERENCE_WIDTH = 2560;
constexpr int REFERENCE_HEIGHT = 1440;

struct Style {
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

inline const Style &style() {
  static const Style value{{235, 235, 225, 255}, {132, 132, 126, 255},
                           {0, 0, 0, 196},       {218, 218, 206, 220},
                           {235, 235, 225, 255}, {245, 245, 235, 255},
                           {210, 24, 28, 255},   {235, 235, 225, 220},
                           {210, 24, 28, 255},   {0, 0, 0, 230}};
  return value;
}

struct Context {
  const Font &font;
  const Font &japaneseFont;
  const Style &style;
  Rectangle screen;
};

enum class Anchor {
  TopLeft,
  TopRight,
  BottomLeft,
  BottomRight,
  Center,
  BottomCenter,
};

inline float scale() {
  float scaleX = static_cast<float>(GetScreenWidth()) /
                 static_cast<float>(REFERENCE_WIDTH);
  float scaleY = static_cast<float>(GetScreenHeight()) /
                 static_cast<float>(REFERENCE_HEIGHT);
  return std::min(scaleX, scaleY);
}

inline void beginScale() {
  float value = scale();
  float offsetX =
      (static_cast<float>(GetScreenWidth()) - REFERENCE_WIDTH * value) * 0.5f;
  float offsetY =
      (static_cast<float>(GetScreenHeight()) - REFERENCE_HEIGHT * value) * 0.5f;

  rlPushMatrix();
  rlTranslatef(offsetX, offsetY, 0.0f);
  rlScalef(value, value, 1.0f);
}

inline void endScale() { rlPopMatrix(); }

inline Rectangle anchoredRect(Rectangle screen, Anchor anchor, Vector2 size,
                              Vector2 margin) {
  Rectangle result{screen.x, screen.y, size.x, size.y};

  switch (anchor) {
  case Anchor::TopLeft:
    result.x = screen.x + margin.x;
    result.y = screen.y + margin.y;
    break;
  case Anchor::TopRight:
    result.x = screen.x + screen.width - size.x - margin.x;
    result.y = screen.y + margin.y;
    break;
  case Anchor::BottomLeft:
    result.x = screen.x + margin.x;
    result.y = screen.y + screen.height - size.y - margin.y;
    break;
  case Anchor::BottomRight:
    result.x = screen.x + screen.width - size.x - margin.x;
    result.y = screen.y + screen.height - size.y - margin.y;
    break;
  case Anchor::Center:
    result.x = screen.x + screen.width * 0.5f - size.x * 0.5f;
    result.y = screen.y + screen.height * 0.5f - size.y * 0.5f;
    break;
  case Anchor::BottomCenter:
    result.x = screen.x + screen.width * 0.5f - size.x * 0.5f;
    result.y = screen.y + screen.height - size.y - margin.y;
    break;
  }

  return result;
}

inline void text(const Context &ctx, const Font &font, const char *value,
                 Vector2 position, float fontSize, Color color) {
  DrawTextEx(font, value, {position.x + 2.0f, position.y + 2.0f}, fontSize,
             1.0f, Fade(ctx.style.shadow, 0.85f));
  DrawTextEx(font, value, position, fontSize, 1.0f, color);
}

inline void panel(const Context &ctx, Rectangle bounds, Color borderColor) {
  constexpr float tick = 24.0f;

  DrawRectangleRec(bounds, ctx.style.panel);
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

inline void systemHeader(const Context &ctx, Rectangle bounds,
                         const char *title, const char *accent, Color color) {
  DrawRectangleRec(
      {bounds.x + 12.0f, bounds.y + 12.0f, bounds.width - 24.0f, HEADER_HEIGHT},
      Fade(ctx.style.text, 0.12f));
  DrawLineEx({bounds.x + 12.0f, bounds.y + 12.0f + HEADER_HEIGHT + 2.0f},
             {bounds.x + bounds.width - 12.0f,
              bounds.y + 12.0f + HEADER_HEIGHT + 2.0f},
             1.0f, Fade(ctx.style.text, 0.62f));

  text(ctx, ctx.font, title, {bounds.x + 24.0f, bounds.y + 17.0f},
       HEADER_FONT_SIZE, color);

  Vector2 accentSize =
      MeasureTextEx(ctx.japaneseFont, accent, HEADER_FONT_SIZE, 1.0f);
  text(ctx, ctx.japaneseFont, accent,
       {bounds.x + bounds.width - accentSize.x - 24.0f, bounds.y + 17.0f},
       HEADER_FONT_SIZE, Fade(ctx.style.dimText, 0.88f));
}

struct Panel {
  Rectangle bounds;
  float cursorY;

  void draw(const Context &ctx, Color color) const {
    panel(ctx, bounds, color);
  }

  void header(const Context &ctx, const char *title, const char *accent,
              Color color) const {
    systemHeader(ctx, bounds, title, accent, color);
  }

  void labelValue(const Context &ctx, const char *label, const char *value,
                  Color valueColor, float valueX = 190.0f) {
    text(ctx, ctx.font, label, {bounds.x + PAD, cursorY}, LABEL_FONT_SIZE,
         ctx.style.dimText);
    text(ctx, ctx.font, value, {bounds.x + valueX, cursorY - 4.0f},
         VALUE_FONT_SIZE, valueColor);
    cursorY += 50.0f;
  }

  void message(const Context &ctx, const char *value, float fontSize,
               Color color) {
    text(ctx, ctx.font, value, {bounds.x + PAD, cursorY}, fontSize, color);
    cursorY += fontSize + 10.0f;
  }

  void progress(const Context &ctx, float value, Color color) const {
    Rectangle track{bounds.x + PAD, bounds.y + 170.0f,
                    bounds.width - PAD * 2.0f, 7.0f};
    DrawRectangleRec(track, Fade(ctx.style.dimText, 0.35f));
    DrawRectangleRec({track.x, track.y, track.width * value, track.height},
                     color);
  }
};
} // namespace Hud
