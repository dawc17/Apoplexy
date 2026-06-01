#include "game.hpp"

#include "../render/renderer.hpp"
#include "../ui/ui.hpp"

#include "gamestate.hpp"
#include "raylib.h"

Game::Game() {
  assets.load();
  reset();
}

Game::~Game() { assets.unload(); }

void Game::reset() {
  state = GameState::Playing;

  level.loadTestArena();

  player.reset(level.getPlayerSpawn());

  weapon.reset();

  enemies.clear();

  for (Vector3 spawn : level.getEnemySpawns()) {
    enemies.emplace_back(spawn);
  }

  camera = player.getCamera();
}

void Game::update(float dt) {
  switch (state) {
  case GameState::Menu:
    if (IsKeyPressed(KEY_ENTER)) {
      reset();
    }
    break;

  case GameState::Playing:
    updatePlaying(dt);
    break;

  case GameState::Dead:
    if (IsKeyPressed(KEY_R)) {
      reset();
    }
    break;

  case GameState::Win:
    if (IsKeyPressed(KEY_R)) {
      reset();
    }
    break;
  }
}

void Game::updatePlaying(float dt) {
  player.update(dt, level);
  camera = player.getCamera();

  weapon.update(dt, player, enemies, camera);

  for (Enemy &enemy : enemies) {
    enemy.update(dt, player, level);
  }

  if (player.isDead()) {
    state = GameState::Dead;
  }

  bool allEnemiesDead = true;

  for (const Enemy &enemy : enemies) {
    if (enemy.isAlive()) {
      allEnemiesDead = false;
      break;
    }
  }

  if (allEnemiesDead) {
    state = GameState::Win;
  }
}

void Game::draw() {
  Renderer::drawWorld(*this);
  UI::draw(*this);
}
