#include "audiosystem.hpp"

#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cmath>
#include <iostream>

namespace {
float clamp01(float value) { return std::clamp(value, 0.0f, 1.0f); }

float clampPitch(float value) { return std::clamp(value, 0.25f, 4.0f); }
} // namespace

AudioSystem::~AudioSystem() { unload(); }

void AudioSystem::load() {
  if (loaded) {
    return;
  }

  busVolumes.fill(1.0f);
  busVolumes[static_cast<int>(AudioBus::Bgm)] = 0.0f;

  loadClip(AudioId::PistolFire, "audio/weapons/pistol_fire.wav",
           AudioBus::Weapons, 0.85f);
  loadClip(AudioId::ShotgunFire, "audio/weapons/shotgun_fire.wav",
           AudioBus::Weapons, 1.0f);
  loadClip(AudioId::PistolDryFire, "audio/weapons/pistol_dry_fire.wav",
           AudioBus::Weapons, 0.75f);
  loadClip(AudioId::PistolReloadStart, "audio/weapons/pistol_reload_start.wav",
           AudioBus::Weapons, 1.0f);
  loadClip(AudioId::PistolReloadEnd, "audio/weapons/pistol_reload_end.wav",
           AudioBus::Weapons, 0.75f);
  loadClip(AudioId::WeaponSwitch, "audio/weapons/switch.wav", AudioBus::Weapons,
           1.0f);
  loadClip(AudioId::PlayerFootstep, "audio/footsteps.wav", AudioBus::Player,
           0.65f);
  loadClip(AudioId::PlayerHurt, "audio/player/hurt.wav", AudioBus::Player,
           0.8f);
  loadClip(AudioId::EnemyHit, "audio/enemies/hit.wav", AudioBus::Enemies, 1.0f);
  loadClip(AudioId::EnemyDeath, "audio/enemies/death.wav", AudioBus::Enemies,
           0.85f);

  if (FileExists("audio/music/music.mp3")) {
    music = LoadMusicStream("audio/music/music.mp3");
    musicLoaded = IsMusicValid(music);

    if (musicLoaded) {
      music.looping = true;
      SetMusicVolume(music, busVolumes[static_cast<int>(AudioBus::Master)] *
                                busVolumes[static_cast<int>(AudioBus::Bgm)]);
    } else {
      std::cout << "Failed to load music file: music.mp3" << std::endl;
    }
  } else {
    std::cout << "Missing music file" << std::endl;
  }

  loaded = true;
}

void AudioSystem::unload() {
  for (AudioClip &clip : clips) {
    if (clip.loaded) {
      UnloadSound(clip.sound);
      clip.loaded = false;
    }
  }

  if (musicLoaded) {
    StopMusicStream(music);
    UnloadMusicStream(music);
    musicLoaded = false;
    musicPlaying = false;
  }

  loaded = false;
}

void AudioSystem::update() {
  if (!musicLoaded) {
    return;
  }

  SetMusicVolume(music, busVolumes[static_cast<int>(AudioBus::Master)] *
                            busVolumes[static_cast<int>(AudioBus::Bgm)]);
  UpdateMusicStream(music);
}

void AudioSystem::setBusVolume(AudioBus bus, float volume) {
  busVolumes[static_cast<int>(bus)] = clamp01(volume);
}

float AudioSystem::getBusVolume(AudioBus bus) const {
  return busVolumes[static_cast<int>(bus)];
}

void AudioSystem::setListener(const AudioListener &newListener) {
  listener = newListener;

  if (Vector3LengthSqr(listener.forward) <= 0.0001f) {
    listener.forward = {0.0f, 0.0f, 1.0f};
    return;
  }

  listener.forward = Vector3Normalize(listener.forward);
}

void AudioSystem::playMusic() {
  if (!musicLoaded || musicPlaying) {
    return;
  }

  PlayMusicStream(music);
  musicPlaying = true;
}

void AudioSystem::stopMusic() {
  if (!musicLoaded || !musicPlaying) {
    return;
  }

  StopMusicStream(music);
  musicPlaying = false;
}

void AudioSystem::play(AudioId id, AudioPlayback playback) {
  AudioClip *clip = getClip(id);

  if (!canPlay(clip)) {
    return;
  }

  SetSoundVolume(clip->sound, resolveVolume(*clip, playback.volume));
  SetSoundPitch(clip->sound, clampPitch(playback.pitch));
  SetSoundPan(clip->sound, std::clamp(playback.pan, -1.0f, 1.0f));
  PlaySound(clip->sound);
}

void AudioSystem::playLooping(AudioId id, AudioPlayback playback) {
  AudioClip *clip = getClip(id);

  if (!canPlay(clip)) {
    return;
  }

  SetSoundVolume(clip->sound, resolveVolume(*clip, playback.volume));
  SetSoundPitch(clip->sound, clampPitch(playback.pitch));
  SetSoundPan(clip->sound, std::clamp(playback.pan, -1.0f, 1.0f));

  if (!IsSoundPlaying(clip->sound)) {
    PlaySound(clip->sound);
  }
}

void AudioSystem::playAt(AudioId id, Vector3 position,
                         PositionalAudioPlayback playback) {
  AudioClip *clip = getClip(id);

  if (!canPlay(clip)) {
    return;
  }

  float distanceVolume = resolveDistanceVolume(position, playback.minDistance,
                                               playback.maxDistance);
  SetSoundVolume(clip->sound,
                 resolveVolume(*clip, playback.volume * distanceVolume));
  SetSoundPitch(clip->sound, clampPitch(playback.pitch));
  SetSoundPan(clip->sound, resolvePan(position));
  PlaySound(clip->sound);
}

void AudioSystem::stop(AudioId id) {
  AudioClip *clip = getClip(id);

  if (clip == nullptr || !clip->loaded) {
    return;
  }

  StopSound(clip->sound);
}

bool AudioSystem::isLoaded(AudioId id) const {
  const AudioClip *clip = getClip(id);
  return clip != nullptr && clip->loaded;
}

void AudioSystem::loadClip(AudioId id, std::string_view path, AudioBus bus,
                           float baseVolume) {
  AudioClip &clip = clips[static_cast<int>(id)];

  clip.path = std::string(path);
  clip.bus = bus;
  clip.baseVolume = clamp01(baseVolume);
  clip.warnedOnPlayback = false;

  if (!FileExists(clip.path.c_str())) {
    std::cout << "Missing audio file: " << clip.path << std::endl;
    return;
  }

  clip.sound = LoadSound(clip.path.c_str());
  clip.loaded = IsSoundValid(clip.sound);

  if (!clip.loaded) {
    std::cout << "Failed to load audio file: " << clip.path << std::endl;
  }
}

bool AudioSystem::canPlay(AudioClip *clip) {
  if (clip == nullptr) {
    return false;
  }

  if (!clip->loaded) {
    if (!clip->warnedOnPlayback && !clip->path.empty()) {
      std::cout << "Skipping unloaded audio file: " << clip->path << std::endl;
      clip->warnedOnPlayback = true;
    }

    return false;
  }

  return true;
}

float AudioSystem::resolveVolume(const AudioClip &clip,
                                 float playbackVolume) const {
  return clamp01(busVolumes[static_cast<int>(AudioBus::Master)] *
                 busVolumes[static_cast<int>(clip.bus)] * clip.baseVolume *
                 playbackVolume);
}

float AudioSystem::resolvePan(Vector3 position) const {
  Vector3 toSource = Vector3Subtract(position, listener.position);

  if (Vector3LengthSqr(toSource) <= 0.0001f) {
    return 0.0f;
  }
  Vector3 direction = Vector3Normalize(toSource);
  Vector3 right = Vector3Normalize(
      Vector3CrossProduct(listener.forward, {0.0f, 1.0f, 0.0f}));

  if (Vector3LengthSqr(right) <= 0.0001f) {
    return 0.0f;
  }

  return std::clamp(Vector3DotProduct(direction, right), -1.0f, 1.0f);
}

float AudioSystem::resolveDistanceVolume(Vector3 position, float minDistance,
                                         float maxDistance) const {
  minDistance = std::max(0.0f, minDistance);
  maxDistance = std::max(minDistance + 0.01f, maxDistance);

  float distance = Vector3Distance(listener.position, position);

  if (distance <= minDistance) {
    return 1.0f;
  }

  if (distance >= maxDistance) {
    return 0.0f;
  }

  float t = (distance - minDistance) / (maxDistance - minDistance);
  return 1.0f - t;
}

AudioSystem::AudioClip *AudioSystem::getClip(AudioId id) {
  int index = static_cast<int>(id);

  if (index < 0 || index >= audioIdCount) {
    return nullptr;
  }

  return &clips[index];
}

const AudioSystem::AudioClip *AudioSystem::getClip(AudioId id) const {
  int index = static_cast<int>(id);

  if (index < 0 || index >= audioIdCount) {
    return nullptr;
  }

  return &clips[index];
}
