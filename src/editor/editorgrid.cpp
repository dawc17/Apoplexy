#include "editorgrid.hpp"

#include "raylib.h"

namespace EditorGrid {
void draw(float snapSize, float halfExtent) {
  if (snapSize <= 0.0f) {
    return;
  }

  Color minor = Fade(DARKGRAY, 0.45f);
  Color major = Fade(GREEN, 0.35f);

  int lineCount = static_cast<int>((halfExtent * 2.0f) / snapSize);

  for (int i = -lineCount / 2; i <= lineCount / 2; ++i) {
    float value = static_cast<float>(i) * snapSize;
    bool isMajor = i % 4 == 0;
    Color color = isMajor ? major : minor;

    DrawLine3D({-halfExtent, 0.02f, value}, {halfExtent, 0.02f, value}, color);
    DrawLine3D({value, 0.02f, -halfExtent}, {value, 0.02f, halfExtent}, color);
  }
}
} // namespace EditorGrid