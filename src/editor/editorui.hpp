#pragma once

class LevelEditor;
class Level;

#include <string_view>

namespace EditorUI {
void draw(LevelEditor &editor, Level &level, std::string_view levelPath);
}
