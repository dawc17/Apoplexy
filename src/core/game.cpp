#include "game.hpp"

#include "../editor/editorui.hpp"
#include "../render/renderer.hpp"
#include "../ui/ui.hpp"
#ifdef DEBUG
#include "../viewmodel/viewmodeldebug.hpp"
#endif

#include "audio/audiosystem.hpp"
#include "gamestate.hpp"
#include "raylib.h"
#include "raymath.h"
#include "viewmodel/proceduralweaponanimationcatalog.hpp"
#include "weapon/weaponcatalog.hpp"
#include "weapon/weapondata.hpp"
#include "weapon/weaponinventory.hpp"

#include <cmath>

namespace {
constexpr int PSX_RENDER_WIDTH = 640;
constexpr int PSX_RENDER_HEIGHT = 320;
constexpr float WIN_SEQUENCE_SLOWMO_SCALE = 0.18f;
constexpr float WIN_SEQUENCE_MAX_DIM = 0.76f;

void drawRadialVignette(Color color, float opacity, float radiusScale) {
  int width = GetScreenWidth();
  int height = GetScreenHeight();
  float halfWidth = static_cast<float>(width) * 0.5f;
  float halfHeight = static_cast<float>(height) * 0.5f;
  float cornerRadius =
      std::sqrtf(halfWidth * halfWidth + halfHeight * halfHeight);
  float radius = cornerRadius * radiusScale;

  DrawCircleGradient({halfWidth, halfHeight}, radius, Fade(color, 0.0f),
                     Fade(color, opacity));
}

void setShaderFloat(Shader shader, const char *name, float value) {
  SetShaderValue(shader, GetShaderLocation(shader, name), &value,
                 SHADER_UNIFORM_FLOAT);
}

void setShaderVec2(Shader shader, const char *name, Vector2 value) {
  SetShaderValue(shader, GetShaderLocation(shader, name), &value,
                 SHADER_UNIFORM_VEC2);
}

void setShaderVec3(Shader shader, const char *name, Vector3 value) {
  SetShaderValue(shader, GetShaderLocation(shader, name), &value,
                 SHADER_UNIFORM_VEC3);
}

void configurePsxGlobalShader(Shader shader, Vector2 virtualResolution) {
  Vector2 screenSize{static_cast<float>(GetScreenWidth()),
                     static_cast<float>(GetScreenHeight())};
  Vector3 colorTint{1.0f, 0.98f, 0.94f};
  Vector3 fogColor{0.42f, 0.46f, 0.50f};

  setShaderVec2(shader, "virtualResolution", virtualResolution);
  setShaderVec2(shader, "screenSize", screenSize);
  setShaderVec3(shader, "colorTint", colorTint);
  setShaderVec3(shader, "fogColor", fogColor);

  setShaderFloat(shader, "time", static_cast<float>(GetTime()));
  setShaderFloat(shader, "intensity", 0.82f);
  setShaderFloat(shader, "pixelScale", 1.0f);
  setShaderFloat(shader, "fixedVerticalResolution", 240.0f);
  setShaderFloat(shader, "useFixedVerticalResolution", 0.0f);
  setShaderFloat(shader, "colorSteps", 24.0f);
  setShaderFloat(shader, "ditherStrength", 0.18f);
  setShaderFloat(shader, "ditherScale", 1.0f);
  setShaderFloat(shader, "scanlineStrength", 0.10f);
  setShaderFloat(shader, "vignetteStrength", 0.16f);
  setShaderFloat(shader, "saturation", 1.08f);
  setShaderFloat(shader, "contrast", 1.10f);
  setShaderFloat(shader, "colorBleed", 0.10f);
  setShaderFloat(shader, "gammaValue", 1.0f);
  setShaderFloat(shader, "blackLevel", 0.02f);
  setShaderFloat(shader, "chromaticOffset", 0.16f);
  setShaderFloat(shader, "noiseStrength", 0.025f);
  setShaderFloat(shader, "horizontalJitter", 0.02f);
  setShaderFloat(shader, "curvature", 0.0f);
  setShaderFloat(shader, "fogAmount", 0.0f);
  setShaderFloat(shader, "vertexSnapStrength", 1.0f);
}
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
  winSequenceTimer = 0.0f;
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

  case GameState::WinSequence:
    updateWinSequence(dt);
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

    if (levelEditor.isTestMode()) {
      updateEditorTest(dt);
    }

    return;
  }

#ifdef DEBUG
  if (IsKeyPressed(KEY_F2)) {
    ViewmodelDebug::panelOpen = !ViewmodelDebug::panelOpen;
  }

  if (ViewmodelDebug::panelOpen) {
    camera = player.getCamera();
    audio.stop(AudioId::PistolReloadStart);
    audio.stop(AudioId::PlayerFootstep);

    if (IsCursorHidden()) {
      EnableCursor();
    }

    return;
  } else if (!IsCursorHidden()) {
    DisableCursor();
  }

  if (IsKeyPressed(KEY_X)) {
    enemiesFrozen = !enemiesFrozen;
  }
#endif

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
    float noiseRadius = weapon.modelId == WeaponModelId::Shotgun ? 28.0f : 7.0f;

    notifyEnemiesOfNoise(camera.position, noiseRadius);
    startCameraShake(0.15f, 0.12f);
  }

  particles.update(dt);

  if (!enemiesFrozen) {
    updateEnemies(dt);
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
    state = GameState::WinSequence;
    winSequenceTimer = 0.0f;
  }
}

void Game::updateWinSequence(float dt) {
  float slowDt = dt * WIN_SEQUENCE_SLOWMO_SCALE;

  winSequenceTimer += dt;
  camera = player.getCamera();
  weapons.updatePresentation(slowDt, player);
  particles.update(slowDt);
  updateCameraShake(slowDt);

  damageVignetteTimer =
      Clamp(damageVignetteTimer - slowDt, 0.0f, damageVignetteDuration);

  if (winSequenceTimer >= winSequenceDuration) {
    state = GameState::Win;
  }
}

void Game::draw() {
  if (levelEditor.isEnabled()) {
    ClearBackground(BLACK);

    Renderer::drawWorld(*this);

    UI::draw(*this);

    drawWinSequenceDim();

    EditorUI::draw(levelEditor, level);

    return;
  }

  BeginTextureMode(sceneTarget);
  ClearBackground(BLACK);

  Shader psxShader = assets.getPsxGlobalShader();
  Vector2 virtualResolution = {static_cast<float>(PSX_RENDER_WIDTH),
                               static_cast<float>(PSX_RENDER_HEIGHT)};

  Renderer::drawWorld(*this);

  EndTextureMode();

  configurePsxGlobalShader(psxShader, virtualResolution);

  BeginShaderMode(psxShader);
  DrawTexturePro(sceneTarget.texture,
                 {0.0f, 0.0f, static_cast<float>(sceneTarget.texture.width),
                  -static_cast<float>(sceneTarget.texture.height)},
                 {0.0f, 0.0f, static_cast<float>(GetScreenWidth()),
                  static_cast<float>(GetScreenHeight())},
                 {0.0f, 0.0f}, 0.0f, WHITE);
  EndShaderMode();

  drawCrouchVignette();
  drawDamageVignette();

  UI::draw(*this);
  drawWinSequenceDim();

  EditorUI::draw(levelEditor, level);
}

void Game::drawWinSequenceDim() const {
  if (state != GameState::WinSequence) {
    return;
  }

  float fadeT = Clamp(winSequenceTimer / winSequenceDuration, 0.0f, 1.0f);
  float smoothT =
      fadeT * fadeT * fadeT * (fadeT * (fadeT * 6.0f - 15.0f) + 10.0f);

  DrawRectangle(0, 0, GetScreenWidth(), GetScreenHeight(),
                Fade(BLACK, WIN_SEQUENCE_MAX_DIM * smoothT));
}

void Game::drawCrouchVignette() const {
  if (!player.isCrouching() || state != GameState::Playing) {
    return;
  }

  drawRadialVignette(BLACK, 0.42f, 1.28f);
}

void Game::drawDamageVignette() const {
  if (damageVignetteTimer <= 0.0f) {
    return;
  }

  float t = Clamp(damageVignetteTimer / damageVignetteDuration, 0.0f, 1.0f);
  drawRadialVignette({95, 0, 0, 255}, 0.58f * t, 1.24f);
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

  if (player.isCrouching()) {
    pitch = 0.92f;
    volume = 0.32f;
  }

  audio.playLooping(AudioId::PlayerFootstep, {volume, pitch, 0.0f});

  footstepNoiseTimer -= dt;

  if (footstepNoiseTimer <= 0.0f) {
    float noiseRadius = player.isSprinting() ? 8.0f : 4.5f;
    footstepNoiseTimer = player.isSprinting() ? 0.30f : 0.46f;

    if (player.isCrouching()) {
      noiseRadius = 1.35f;
      footstepNoiseTimer = 0.72f;
    }
    notifyEnemiesOfNoise(player.getPosition(), noiseRadius);
  }
}

void Game::updateEnemies(float dt) {
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

void Game::updateEditorTest(float dt) {
  LevelEditor::NoiseEvent noiseEvent{};

  if (levelEditor.consumeNoiseEvent(noiseEvent)) {
    notifyEnemiesOfNoise(noiseEvent.position, noiseEvent.radius);
  }

  particles.update(dt);

  if (enemiesFrozen) {
    return;
  }

  for (Enemy &enemy : enemies) {
    enemy.updateEditorTest(dt, level);
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
void Game::drawEditorTestDebug() const {
  if (!levelEditor.isTestMode()) {
    return;
  }

  if (levelEditor.hasLastTestNoise()) {
    Vector3 noisePosition = levelEditor.getLastTestNoisePosition();
    DrawSphere(noisePosition, 0.22f, SKYBLUE);
    DrawCircle3D(noisePosition, levelEditor.getLastTestNoiseRadius(),
                 {1.0f, 0.0f, 0.0f}, 90.0f, Fade(SKYBLUE, 0.45f));
  }

  for (const Enemy &enemy : enemies) {
    if (!enemy.isAlive()) {
      continue;
    }

    EnemyState enemyState = enemy.getState();
    if (enemyState != EnemyState::Suspicious &&
        enemyState != EnemyState::Alert && enemyState != EnemyState::Search &&
        enemyState != EnemyState::Chase) {
      continue;
    }

    Vector3 start = enemy.getPosition();
    Vector3 end = enemy.getInvestigationTarget();
    start.y += 0.08f;
    end.y += 0.08f;

    Color color = enemyState == EnemyState::Search ? ORANGE : RED;
    DrawLine3D(start, end, color);
    DrawSphere(end, 0.16f, color);
  }
}
const LevelEditor &Game::getLevelEditor() const { return levelEditor; }
LevelEditor &Game::getMutableLevelEditor() { return levelEditor; }
bool Game::areEnemiesFrozen() const { return enemiesFrozen; }
bool Game::isEditorEnabled() const { return levelEditor.isEnabled(); }
