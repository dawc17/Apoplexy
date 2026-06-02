#include "game.hpp"

#include "../render/renderer.hpp"
#include "../ui/ui.hpp"

#include "gamestate.hpp"
#include "raylib.h"

Game::Game() {
  assets.load();
  sceneTarget = LoadRenderTexture(GetScreenWidth(), GetScreenHeight());
  reset();
}

Game::~Game() {
  UnloadRenderTexture(sceneTarget);
  assets.unload();
}

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

  weapon.update(dt, player, enemies, level, camera);

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
  BeginTextureMode(sceneTarget);
  ClearBackground(BLACK);

  Renderer::drawWorld(*this);

  EndTextureMode();

  Shader monoShader = assets.getMonoShader();
  Vector2 resolution = {static_cast<float>(sceneTarget.texture.width),
                        static_cast<float>(sceneTarget.texture.height)};
  float pixelSize = 2.0f;
  float threshold = 0.48f;

  SetShaderValue(monoShader, GetShaderLocation(monoShader, "resolution"),
                 &resolution, SHADER_UNIFORM_VEC2);
  SetShaderValue(monoShader, GetShaderLocation(monoShader, "pixelSize"),
                 &pixelSize, SHADER_UNIFORM_FLOAT);
  SetShaderValue(monoShader, GetShaderLocation(monoShader, "threshold"),
                 &threshold, SHADER_UNIFORM_FLOAT);

  BeginShaderMode(monoShader);
  DrawTexturePro(sceneTarget.texture,
                 {0.0f, 0.0f, static_cast<float>(sceneTarget.texture.width),
                  -static_cast<float>(sceneTarget.texture.height)},
                 {0.0f, 0.0f, static_cast<float>(GetScreenWidth()),
                  static_cast<float>(GetScreenHeight())},
                 {0.0f, 0.0f}, 0.0f, WHITE);
  EndShaderMode();

  UI::draw(*this);
}

GameState Game::getState() const { return state; }
const Level &Game::getLevel() const { return level; }
const Player &Game::getPlayer() const { return player; }
const Weapon &Game::getWeapon() const { return weapon; }
const std::vector<Enemy> &Game::getEnemies() const { return enemies; }
const Camera3D &Game::getCamera() const { return camera; }
const AssetManager &Game::getAssets() const { return assets; }
