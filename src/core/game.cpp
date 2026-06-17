#include "game.hpp"

#include "../editor/editorui.hpp"
#include "../render/renderer.hpp"
#include "../ui/ui.hpp"
#ifdef DEBUG
#include "raygui/raygui.h"
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

#include <algorithm>
#include <cmath>

namespace {
constexpr int PSX_RENDER_WIDTH = 640;
constexpr int PSX_RENDER_HEIGHT = 320;
constexpr float WIN_SEQUENCE_SLOWMO_SCALE = 0.18f;
constexpr float WIN_SEQUENCE_MAX_DIM = 0.76f;

struct PsxShaderPreset {
  const char *name;
  float intensity;
  float pixelScale;
  float fixedVerticalResolution;
  float useFixedVerticalResolution;
  float colorSteps;
  float ditherStrength;
  float ditherScale;
  float scanlineStrength;
  float vignetteStrength;
  float saturation;
  float contrast;
  float colorBleed;
  Vector3 colorTint;
  float gammaValue;
  float blackLevel;
  float chromaticOffset;
  float noiseStrength;
  float horizontalJitter;
  float curvature;
  Vector3 fogColor;
  float fogAmount;
  float vertexSnapStrength;
};

constexpr PsxShaderPreset PSX_SHADER_PRESETS[] = {
    {
        "Off/Neutral",
        0.0f, 1.0f, 240.0f, 0.0f, 32.0f, 0.0f, 1.0f, 0.0f, 0.0f, 1.0f,
        1.0f, 0.0f, {1.0f, 1.0f, 1.0f}, 1.0f, 0.0f, 0.0f, 0.0f, 0.0f,
        0.0f, {0.42f, 0.46f, 0.50f}, 0.0f, 0.0f,
    },
    {
        "Clean PS2",
        0.55f, 1.0f, 360.0f, 0.0f, 42.0f, 0.05f, 1.0f, 0.035f, 0.05f,
        1.05f, 1.04f, 0.03f, {1.0f, 1.0f, 1.0f}, 1.0f, 0.0f, 0.04f,
        0.005f, 0.0f, 0.0f, {0.42f, 0.46f, 0.50f}, 0.0f, 0.35f,
    },
    {
        "Crunchy PSX",
        0.82f, 1.0f, 240.0f, 0.0f, 24.0f, 0.18f, 1.0f, 0.10f, 0.16f,
        1.08f, 1.10f, 0.10f, {1.0f, 0.98f, 0.94f}, 1.0f, 0.02f, 0.16f,
        0.025f, 0.02f, 0.0f, {0.42f, 0.46f, 0.50f}, 0.0f, 1.0f,
    },
    {
        "Horror PSX",
        0.90f, 1.0f, 220.0f, 0.0f, 18.0f, 0.24f, 1.2f, 0.12f, 0.28f,
        0.78f, 1.22f, 0.14f, {0.92f, 0.95f, 1.0f}, 1.08f, 0.08f, 0.18f,
        0.045f, 0.035f, 0.03f, {0.08f, 0.10f, 0.12f}, 0.16f, 1.0f,
    },
    {
        "Dark Fantasy",
        0.82f, 1.0f, 260.0f, 0.0f, 22.0f, 0.18f, 1.0f, 0.08f, 0.22f,
        0.86f, 1.18f, 0.08f, {1.0f, 0.92f, 0.82f}, 1.02f, 0.06f, 0.10f,
        0.02f, 0.015f, 0.0f, {0.20f, 0.16f, 0.22f}, 0.12f, 0.85f,
    },
    {
        "VHS/CRT",
        0.88f, 1.0f, 240.0f, 0.0f, 28.0f, 0.13f, 1.0f, 0.18f, 0.18f,
        1.12f, 1.08f, 0.22f, {1.0f, 0.96f, 0.92f}, 0.98f, 0.025f, 0.28f,
        0.05f, 0.06f, 0.18f, {0.42f, 0.46f, 0.50f}, 0.0f, 0.7f,
    },
    {
        "Mobile Fast",
        0.48f, 1.0f, 320.0f, 0.0f, 32.0f, 0.04f, 1.0f, 0.025f, 0.04f,
        1.0f, 1.03f, 0.0f, {1.0f, 1.0f, 1.0f}, 1.0f, 0.0f, 0.0f, 0.0f,
        0.0f, 0.0f, {0.42f, 0.46f, 0.50f}, 0.0f, 0.25f,
    },
};

constexpr int PSX_SHADER_PRESET_COUNT =
    static_cast<int>(sizeof(PSX_SHADER_PRESETS) / sizeof(PSX_SHADER_PRESETS[0]));

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

void configurePsxGlobalShader(Shader shader, Vector2 virtualResolution,
                              const PsxShaderPreset &preset) {
  Vector2 screenSize{static_cast<float>(GetScreenWidth()),
                     static_cast<float>(GetScreenHeight())};

  setShaderVec2(shader, "virtualResolution", virtualResolution);
  setShaderVec2(shader, "screenSize", screenSize);
  setShaderVec3(shader, "colorTint", preset.colorTint);
  setShaderVec3(shader, "fogColor", preset.fogColor);

  setShaderFloat(shader, "time", static_cast<float>(GetTime()));
  setShaderFloat(shader, "intensity", preset.intensity);
  setShaderFloat(shader, "pixelScale", preset.pixelScale);
  setShaderFloat(shader, "fixedVerticalResolution",
                 preset.fixedVerticalResolution);
  setShaderFloat(shader, "useFixedVerticalResolution",
                 preset.useFixedVerticalResolution);
  setShaderFloat(shader, "colorSteps", preset.colorSteps);
  setShaderFloat(shader, "ditherStrength", preset.ditherStrength);
  setShaderFloat(shader, "ditherScale", preset.ditherScale);
  setShaderFloat(shader, "scanlineStrength", preset.scanlineStrength);
  setShaderFloat(shader, "vignetteStrength", preset.vignetteStrength);
  setShaderFloat(shader, "saturation", preset.saturation);
  setShaderFloat(shader, "contrast", preset.contrast);
  setShaderFloat(shader, "colorBleed", preset.colorBleed);
  setShaderFloat(shader, "gammaValue", preset.gammaValue);
  setShaderFloat(shader, "blackLevel", preset.blackLevel);
  setShaderFloat(shader, "chromaticOffset", preset.chromaticOffset);
  setShaderFloat(shader, "noiseStrength", preset.noiseStrength);
  setShaderFloat(shader, "horizontalJitter", preset.horizontalJitter);
  setShaderFloat(shader, "curvature", preset.curvature);
  setShaderFloat(shader, "fogAmount", preset.fogAmount);
  setShaderFloat(shader, "vertexSnapStrength", preset.vertexSnapStrength);
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

  if (IsKeyPressed(KEY_F3)) {
    shaderPresetPanelOpen = !shaderPresetPanelOpen;
  }

  if (ViewmodelDebug::panelOpen || shaderPresetPanelOpen) {
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

  int presetIndex = std::clamp(psxShaderPresetIndex, 0, PSX_SHADER_PRESET_COUNT - 1);
  configurePsxGlobalShader(psxShader, virtualResolution,
                           PSX_SHADER_PRESETS[presetIndex]);

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

#ifdef DEBUG
  drawShaderPresetDebugPanel();
#endif
}

void Game::drawShaderPresetDebugPanel() {
#ifdef DEBUG
  if (!shaderPresetPanelOpen) {
    DrawText("F3 PSX Presets", GetScreenWidth() - 172, 24, 18,
             Fade(WHITE, 0.65f));
    return;
  }

  Rectangle panel{static_cast<float>(GetScreenWidth()) - 302.0f, 62.0f,
                  278.0f, 300.0f};
  DrawRectangleRec(panel, Fade(BLACK, 0.82f));
  DrawRectangleLinesEx(panel, 1.0f, Fade(WHITE, 0.35f));
  DrawText("PSX SHADER PRESET (F3)", static_cast<int>(panel.x + 14.0f),
           static_cast<int>(panel.y + 12.0f), 18, WHITE);

  for (int i = 0; i < PSX_SHADER_PRESET_COUNT; ++i) {
    Rectangle button{panel.x + 14.0f, panel.y + 44.0f + i * 32.0f,
                     panel.width - 28.0f, 25.0f};
    bool selected = i == psxShaderPresetIndex;
    const char *label =
        TextFormat("%s%s", selected ? "> " : "  ", PSX_SHADER_PRESETS[i].name);

    if (selected) {
      DrawRectangleRec(button, Fade(WHITE, 0.16f));
    }

    if (GuiButton(button, label)) {
      psxShaderPresetIndex = i;
    }
  }

  DrawText("Unity-inspired approximation",
           static_cast<int>(panel.x + 14.0f),
           static_cast<int>(panel.y + panel.height - 28.0f), 14,
           Fade(WHITE, 0.62f));
#else
  (void)shaderPresetPanelOpen;
#endif
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
