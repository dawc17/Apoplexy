#pragma once

#include "gamestate.hpp"

#include "../assets/assetmanager.hpp"
#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"
#include "../weapon/weapon.hpp"

#include <raylib.h>
#include <vector>

class Game {
public:
  Game();
  ~Game();

  void update(float dt);
  void draw();
  GameState getState() const;
  const Level &getLevel() const;
  const Player &getPlayer() const;
  const Weapon &getWeapon() const;
  const std::vector<Enemy> &getEnemies() const;
  const Camera3D &getCamera() const;
  const AssetManager &getAssets() const;
  const ParticleSystem &getParticles() const;

private:
  void reset();
  void updatePlaying(float dt);
  void drawPlaying();

private:
  GameState state = GameState::Menu;

  AssetManager assets;
  RenderTexture2D sceneTarget{};

  Level level;
  Player player;
  Weapon weapon;
  ParticleSystem particles;

  std::vector<Enemy> enemies;

  Camera3D camera{};
};
