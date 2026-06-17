#pragma once

#include "gamestate.hpp"

#include "../assets/assetmanager.hpp"
#include "../audio/audiosystem.hpp"
#include "../editor/leveleditor.hpp"
#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"
#include "../viewmodel/proceduralweaponanimationcatalog.hpp"
#include "../weapon/weapon.hpp"
#include "../weapon/weaponcatalog.hpp"
#include "../weapon/weaponinventory.hpp"

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
  Level &getMutableLevel();
  const Player &getPlayer() const;
  const Weapon &getWeapon() const;
  const WeaponInventory &getWeaponInventory() const;
  float getWeaponSwitchAmount() const;
  const std::vector<Enemy> &getEnemies() const;
  const Camera3D &getCamera() const;
  const AssetManager &getAssets() const;
  AudioSystem &getAudio();
  const AudioSystem &getAudio() const;
  const ParticleSystem &getParticles() const;
  void drawEditorTestDebug() const;
  const LevelEditor &getLevelEditor() const;
  LevelEditor &getMutableLevelEditor();
  bool areEnemiesFrozen() const;
  bool isEditorEnabled() const;

private:
  void reset();
  void updatePlaying(float dt);
  void updateWinSequence(float dt);
  void drawPlaying();
  void drawWinSequenceDim() const;
  void updateFootsteps(float dt);
  void updateEnemies(float dt);
  void updateEditorTest(float dt);
  void notifyEnemiesOfNoise(Vector3 position, float radius);
  void startCameraShake(float strength, float duration);
  void updateCameraShake(float dt);
  void drawDamageVignette() const;
  void drawCrouchVignette() const;
  void drawShaderPresetDebugPanel();

private:
  GameState state = GameState::Menu;

  AssetManager assets;
  AudioSystem audio;
  RenderTexture2D sceneTarget{};

  Level level;
  LevelEditor levelEditor;
  Player player;
  WeaponInventory weapons;
  ParticleSystem particles;

  std::vector<Enemy> enemies;

  float cameraShakeTimer = 0.0f;
  float cameraShakeDuration = 0.0f;
  float cameraShakeStrength = 0.0f;
  float footstepStopGraceTimer = 0.0f;
  float footstepNoiseTimer = 0.0f;
  float damageVignetteTimer = 0.0f;
  float damageVignetteDuration = 0.45f;
  float winSequenceTimer = 0.0f;
  float winSequenceDuration = 4.35f;
  int psxShaderPresetIndex = 2;
  bool shaderPresetPanelOpen = false;
  bool enemiesFrozen = false;

  Camera3D camera{};
};
