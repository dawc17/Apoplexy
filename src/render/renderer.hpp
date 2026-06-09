#pragma once

#include "lighting.hpp"

class Game;

namespace Renderer {
void submitDynamicPointLight(Lighting::PointLight light);
void clearDynamicPointLights();
void drawWorld(const Game &game);
}
