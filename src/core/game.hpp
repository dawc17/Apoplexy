#pragma once

#include "gamestate.hpp"

#include "../assets/assetmanager.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"
#include "../weapon/weapon.hpp"

#include <vector>

class Game {
public:
  Game();
  ~Game();

  void update(float dt);
  void draw();

private:
  void reset();
  void updatePlaying(float dt);
  void drawPlaying();

private:
  GameState state = GameState::Menu;

  AssetManager assets;

  Level level;
  Player player;
  Weapon weapon;

  std::vector<Enemy> enemies;

  Camera3D camera{};
};
