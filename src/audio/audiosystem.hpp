#pragma once

#include "raylib.h"

#include <array>
#include <string>
#include <string_view>

enum class AudioId {
  PistolFire,
  PistolDryFire,
  PistolReloadStart,
  PistolReloadEnd,
  PlayerFootstep,
  PlayerHurt,
  EnemyHit,
  EnemyDeath,

  Count
};

enum class AudioBus {
  Master,
  Sfx,
  Weapons,
  Player,
  Enemies,

  Count
};

struct AudioListener {
  Vector3 position{};
  Vector3 forward{0.0f, 0.0f, 1.0f};
};

struct AudioPlayback {
  float volume = 1.0f;
  float pitch = 1.0f;
  float pan = 0.0f;
};

struct PositionalAudioPlayback {
  float volume = 1.0f;
  float pitch = 1.0f;
  float minDistance = 1.0f;
  float maxDistance = 28.0f;
};

class AudioSystem {
public:
  AudioSystem() = default;
  ~AudioSystem();

  AudioSystem(const AudioSystem &) = delete;
  AudioSystem &operator=(const AudioSystem &) = delete;

  void load();
  void unload();

  void setBusVolume(AudioBus bus, float volume);
  float getBusVolume(AudioBus bus) const;

  void setListener(const AudioListener &listener);

  void play(AudioId id, AudioPlayback playback = {});
  void playLooping(AudioId id, AudioPlayback playback = {});
  void playAt(AudioId id, Vector3 position,
              PositionalAudioPlayback playback = {});
  void stop(AudioId id);

  bool isLoaded(AudioId id) const;

private:
  struct AudioClip {
    Sound sound{};
    std::string path;
    AudioBus bus = AudioBus::Sfx;
    float baseVolume = 1.0f;
    bool loaded = false;
    bool warnedOnPlayback = false;
  };

  static constexpr int audioIdCount = static_cast<int>(AudioId::Count);
  static constexpr int audioBusCount = static_cast<int>(AudioBus::Count);

  void loadClip(AudioId id, std::string_view path, AudioBus bus,
                float baseVolume = 1.0f);
  bool canPlay(AudioClip *clip);
  float resolveVolume(const AudioClip &clip, float playbackVolume) const;
  float resolvePan(Vector3 position) const;
  float resolveDistanceVolume(Vector3 position, float minDistance,
                              float maxDistance) const;
  AudioClip *getClip(AudioId id);
  const AudioClip *getClip(AudioId id) const;

private:
  std::array<AudioClip, audioIdCount> clips{};
  std::array<float, audioBusCount> busVolumes{};
  AudioListener listener{};
  bool loaded = false;
};
