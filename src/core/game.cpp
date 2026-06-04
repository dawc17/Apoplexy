#include "game.hpp"

#include "../render/renderer.hpp"
#include "../ui/ui.hpp"

#include "gamestate.hpp"
#include "raylib.h"
#include "raymath.h"

namespace {
constexpr int PSX_RENDER_WIDTH = 640;
constexpr int PSX_RENDER_HEIGHT = 320;
} // namespace

Game::Game() {
  assets.load();
  sceneTarget = LoadRenderTexture(PSX_RENDER_WIDTH, PSX_RENDER_HEIGHT);
  SetTextureFilter(sceneTarget.texture, TEXTURE_FILTER_POINT);
  reset();
}

Game::~Game() {
  UnloadRenderTexture(sceneTarget);
  assets.unload();
}

void Game::reset() {
  state = GameState::Playing;
  damageVignetteTimer = 0.0f;

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
  if (IsKeyPressed(KEY_X)) {
    enemiesFrozen = !enemiesFrozen;
  }

  player.update(dt, level);
  camera = player.getCamera();

  weapon.update(dt, player, enemies, level, camera, particles);

  // tune fire shake here
  if (weapon.consumeShotFired()) {
    startCameraShake(0.15f, 0.12f);
  }

  particles.update(dt);

  if (!enemiesFrozen) {
    for (Enemy &enemy : enemies) {
      enemy.update(dt, player, level);
    }
  }

  if (player.consumeDamageTaken()) {
    damageVignetteTimer = damageVignetteDuration;
    startCameraShake(0.22f, 0.18f);
  }

  damageVignetteTimer = Clamp(damageVignetteTimer - dt, 0.0f,
                              damageVignetteDuration);

  updateCameraShake(dt);

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

  Shader monoShader = assets.getMonoShader();
  Vector2 virtualResolution = {static_cast<float>(PSX_RENDER_WIDTH),
                               static_cast<float>(PSX_RENDER_HEIGHT)};
  float colorLevels = 25.0f;
  float ditherStrength = 0.18f;

  SetShaderValue(monoShader, GetShaderLocation(monoShader, "virtualResolution"),
                 &virtualResolution, SHADER_UNIFORM_VEC2);
  SetShaderValue(monoShader, GetShaderLocation(monoShader, "colorLevels"),
                 &colorLevels, SHADER_UNIFORM_FLOAT);
  SetShaderValue(monoShader, GetShaderLocation(monoShader, "ditherStrength"),
                 &ditherStrength, SHADER_UNIFORM_FLOAT);

  BeginShaderMode(monoShader);
  Renderer::drawWorld(*this);
  EndShaderMode();

  EndTextureMode();

  DrawTexturePro(sceneTarget.texture,
                 {0.0f, 0.0f, static_cast<float>(sceneTarget.texture.width),
                  -static_cast<float>(sceneTarget.texture.height)},
                 {0.0f, 0.0f, static_cast<float>(GetScreenWidth()),
                  static_cast<float>(GetScreenHeight())},
                 {0.0f, 0.0f}, 0.0f, WHITE);

  drawDamageVignette();

  UI::draw(*this);
}

void Game::drawDamageVignette() const {
  if (damageVignetteTimer <= 0.0f) {
    return;
  }

  float t = Clamp(damageVignetteTimer / damageVignetteDuration, 0.0f, 1.0f);
  int width = GetScreenWidth();
  int height = GetScreenHeight();

  Color bloodRed{95, 0, 0, 255};
  DrawRectangle(0, 0, width, height, Fade(bloodRed, 0.12f * t));

  int maxThickness = static_cast<int>(static_cast<float>(height) * 0.26f);
  constexpr int rings = 10;

  for (int i = 0; i < rings; ++i) {
    float ringT = static_cast<float>(i + 1) / static_cast<float>(rings);
    float alpha = t * ringT * ringT * 0.28f;
    float thickness =
        static_cast<float>(maxThickness) / static_cast<float>(rings);

    Rectangle edge{
        thickness * static_cast<float>(i) * 0.5f,
        thickness * static_cast<float>(i) * 0.5f,
        static_cast<float>(width) - thickness * static_cast<float>(i),
        static_cast<float>(height) - thickness * static_cast<float>(i),
    };

    DrawRectangleLinesEx(edge, thickness, Fade(bloodRed, alpha));
  }
}

void Game::startCameraShake(float strength, float duration) {
  cameraShakeStrength = strength;
  cameraShakeDuration = duration;
  cameraShakeTimer = duration;
}

void Game::updateCameraShake(float dt) {
  if (cameraShakeTimer <= 0.0f) {
    return;
  }

  cameraShakeTimer -= dt;

  float t = cameraShakeTimer / cameraShakeDuration;
  float strength = cameraShakeStrength * t;

  Vector3 forward =
      Vector3Normalize(Vector3Subtract(camera.target, camera.position));
  Vector3 right = Vector3Normalize(Vector3CrossProduct(forward, camera.up));
  Vector3 up = camera.up;

  float x = static_cast<float>(GetRandomValue(-100, 100)) / 100.0f;
  float y = static_cast<float>(GetRandomValue(-100, 100)) / 100.0f;

  Vector3 offset = Vector3Add(Vector3Scale(right, x * strength),
                              Vector3Scale(up, y * strength));
  camera.position = Vector3Add(camera.position, offset);
  camera.target = Vector3Add(camera.target, offset);
}

GameState Game::getState() const { return state; }
const Level &Game::getLevel() const { return level; }
const Player &Game::getPlayer() const { return player; }
const Weapon &Game::getWeapon() const { return weapon; }
const std::vector<Enemy> &Game::getEnemies() const { return enemies; }
const Camera3D &Game::getCamera() const { return camera; }
const AssetManager &Game::getAssets() const { return assets; }
const ParticleSystem &Game::getParticles() const { return particles; }
bool Game::areEnemiesFrozen() const { return enemiesFrozen; }
