#include "game.hpp"

#include "../editor/editorui.hpp"
#include "../render/renderer.hpp"
#include "../ui/ui.hpp"
#include "../viewmodel/viewmodeldebug.hpp"

#include "audio/audiosystem.hpp"
#include "gamestate.hpp"
#include "raylib.h"
#include "raymath.h"
#include "viewmodel/proceduralweaponanimationcatalog.hpp"
#include "weapon/weaponcatalog.hpp"
#include "weapon/weapondata.hpp"
#include "weapon/weaponinventory.hpp"

namespace {
constexpr int PSX_RENDER_WIDTH = 640;
constexpr int PSX_RENDER_HEIGHT = 320;
} // namespace

Game::Game() {
  assets.load();
  audio.load();
  audio.playMusic();
  sceneTarget = LoadRenderTexture(PSX_RENDER_WIDTH, PSX_RENDER_HEIGHT);
  SetTextureFilter(sceneTarget.texture, TEXTURE_FILTER_POINT);
  weapons.addWeapon(WeaponCatalog::Pistol,
                    ProceduralWeaponAnimationCatalog::Pistol);
  weapons.addWeapon(WeaponCatalog::Shotgun,
                    ProceduralWeaponAnimationCatalog::Pistol);
  reset();
}

Game::~Game() {
  UnloadRenderTexture(sceneTarget);
  audio.unload();
  assets.unload();
}

void Game::reset() {
  state = GameState::Playing;
  damageVignetteTimer = 0.0f;
  footstepStopGraceTimer = 0.0f;
  footstepNoiseTimer = 0.0f;
  audio.stop(AudioId::PistolReloadStart);
  audio.stop(AudioId::PlayerFootstep);

  if (!level.loadFromFile("levels/test_arena.json")) {
    level.loadTestArena();
  }

  player.reset(level.getPlayerSpawn());

  weapons.reset();

  enemies.clear();

  for (Vector3 spawn : level.getEnemySpawns()) {
    enemies.emplace_back(spawn);
  }

  camera = player.getCamera();
}

void Game::update(float dt) {
  audio.update();

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
  levelEditor.update(level, dt);

  if (levelEditor.isEnabled()) {
    camera = levelEditor.getCamera();
    audio.stop(AudioId::PistolReloadStart);
    audio.stop(AudioId::PlayerFootstep);
    return;
  }

  if (IsKeyPressed(KEY_F2)) {
    ViewmodelDebug::panelOpen = !ViewmodelDebug::panelOpen;

    if (ViewmodelDebug::panelOpen) {
      EnableCursor();
    } else {
      DisableCursor();
    }
  }

  if (ViewmodelDebug::panelOpen) {
    camera = player.getCamera();
    audio.stop(AudioId::PistolReloadStart);
    audio.stop(AudioId::PlayerFootstep);
    return;
  }

  if (IsKeyPressed(KEY_X)) {
    enemiesFrozen = !enemiesFrozen;
  }

  player.update(dt, level);
  camera = player.getCamera();

  audio.setListener({
      camera.position,
      Vector3Normalize(Vector3Subtract(camera.target, camera.position)),
  });
  updateFootsteps(dt);

  weapons.update(dt, player, enemies, level, camera, particles, audio);

  // tune fire shake here
  if (weapons.getActiveWeapon().consumeShotFired()) {
    const WeaponData &weapon = weapons.getActiveWeapon().getData();
    float noiseRadius =
        weapon.modelId == WeaponModelId::Shotgun ? 28.0f : 7.0f;
    
    notifyEnemiesOfNoise(camera.position, noiseRadius);
    startCameraShake(0.15f, 0.12f);
  }

  particles.update(dt);

  if (!enemiesFrozen) {
    for (Enemy &enemy : enemies) {
      enemy.update(dt, player, level);
    }

    constexpr int separationPasses = 3;
    for (int pass = 0; pass < separationPasses; ++pass) {
      for (int i = 0; i < static_cast<int>(enemies.size()); ++i) {
        for (int j = i + 1; j < static_cast<int>(enemies.size()); ++j) {
          enemies[i].resolveOverlap(enemies[j]);
        }
      }
    }
  }

  if (player.consumeDamageTaken()) {
    damageVignetteTimer = damageVignetteDuration;
    audio.play(AudioId::PlayerHurt);
    startCameraShake(0.22f, 0.18f);
  }

  damageVignetteTimer =
      Clamp(damageVignetteTimer - dt, 0.0f, damageVignetteDuration);

  updateCameraShake(dt);

  if (player.isDead()) {
    audio.stop(AudioId::PistolReloadStart);
    audio.stop(AudioId::PlayerFootstep);
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
    audio.stop(AudioId::PistolReloadStart);
    audio.stop(AudioId::PlayerFootstep);
    state = GameState::Win;
  }
}

void Game::draw() {
  if (levelEditor.isEnabled()) {
    ClearBackground(BLACK);
    Renderer::drawWorld(*this);
    UI::draw(*this);
    EditorUI::draw(levelEditor, level);
    return;
  }

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

  Renderer::drawWorld(*this);

  EndTextureMode();

  BeginShaderMode(monoShader);
  DrawTexturePro(sceneTarget.texture,
                 {0.0f, 0.0f, static_cast<float>(sceneTarget.texture.width),
                  -static_cast<float>(sceneTarget.texture.height)},
                 {0.0f, 0.0f, static_cast<float>(GetScreenWidth()),
                  static_cast<float>(GetScreenHeight())},
                 {0.0f, 0.0f}, 0.0f, WHITE);
  EndShaderMode();

  drawDamageVignette();

  UI::draw(*this);

  EditorUI::draw(levelEditor, level);
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

void Game::updateFootsteps(float dt) {
  constexpr float startStepSpeed = 1.2f;
  constexpr float keepStepSpeed = 0.35f;
  constexpr float stopGraceDuration = 0.16f;

  float speed = player.getHorizontalSpeed();
  bool shouldStart = player.isGrounded() && speed >= startStepSpeed;
  bool shouldKeepPlaying = player.isGrounded() && speed >= keepStepSpeed;

  if (!shouldKeepPlaying) {
    footstepStopGraceTimer -= dt;

    if (footstepStopGraceTimer <= 0.0f) {
      audio.stop(AudioId::PlayerFootstep);
    }

    return;
  }

  if (!shouldStart && footstepStopGraceTimer <= 0.0f) {
    audio.stop(AudioId::PlayerFootstep);
    return;
  }

  footstepStopGraceTimer = stopGraceDuration;

  float pitch = player.isSprinting() ? 1.08f : 1.0f;
  float volume = player.isSprinting() ? 0.85f : 0.68f;

  audio.playLooping(AudioId::PlayerFootstep, {volume, pitch, 0.0f});

  footstepNoiseTimer -= dt;

  if (footstepNoiseTimer <= 0.0f) {
    float noiseRadius = player.isSprinting() ? 8.0f : 4.5f;
    footstepNoiseTimer = player.isSprinting() ? 0.30f : 0.46f;
    notifyEnemiesOfNoise(player.getPosition(), noiseRadius);
  }
}

void Game::notifyEnemiesOfNoise(Vector3 position, float radius) {
  if (enemiesFrozen) {
    return;
  }

  for (Enemy &enemy : enemies) {
    enemy.hearNoise(position, radius, level);
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
Level &Game::getMutableLevel() { return level; }
const Player &Game::getPlayer() const { return player; }
const Weapon &Game::getWeapon() const { return weapons.getActiveWeapon(); }
const WeaponInventory &Game::getWeaponInventory() const { return weapons; }
float Game::getWeaponSwitchAmount() const { return weapons.getSwitchAmount(); }
const std::vector<Enemy> &Game::getEnemies() const { return enemies; }
const Camera3D &Game::getCamera() const { return camera; }
const AssetManager &Game::getAssets() const { return assets; }
AudioSystem &Game::getAudio() { return audio; }
const AudioSystem &Game::getAudio() const { return audio; }
const ParticleSystem &Game::getParticles() const { return particles; }
const LevelEditor &Game::getLevelEditor() const { return levelEditor; }
LevelEditor &Game::getMutableLevelEditor() { return levelEditor; }
bool Game::areEnemiesFrozen() const { return enemiesFrozen; }
bool Game::isEditorEnabled() const { return levelEditor.isEnabled(); }
