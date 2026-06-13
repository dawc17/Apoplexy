#include "weapon.hpp"

#include "../collision/collision.hpp"
#include "../effects/particles.hpp"
#include "../enemy/enemy.hpp"
#include "../level/level.hpp"
#include "../player/player.hpp"

#include "audio/audiosystem.hpp"
#include "raylib.h"
#include "raymath.h"

#include <algorithm>
#include <cfloat>
#include <vector>

bool Weapon::debugRaysEnabled = false;

namespace {
BoundingBox expandedBox(BoundingBox box, float amount) {
  box.min = {box.min.x - amount, box.min.y - amount, box.min.z - amount};
  box.max = {box.max.x + amount, box.max.y + amount, box.max.z + amount};
  return box;
}
} // namespace

Weapon::Weapon(const WeaponData &weaponData,
               const ProceduralWeaponAnimationData &weaponProceduralAnimation)
    : data(&weaponData), proceduralAnimation(&weaponProceduralAnimation) {
  reset();
}

void Weapon::reset() {
  ammoInMagazine = data->ammo.magazineSize;
  reserveAmmo = data->ammo.maxReserveAmmo;

  cooldown = 0.0f;
  reloadTimer = 0.0f;
  muzzleFlashTimer = 0.0f;
  muzzleFlashRotation = 0.0f;
  spreadBloom = 0.0f;
  meleeTimer = 0.0f;
  meleeDuration = 0.0f;

  reloading = false;
  reloadSpinHasHit = false;
  meleeHasHit = false;
  shotFired = false;

  viewmodel.reset();
}

void Weapon::cancelReload(AudioSystem &audio) {
  if (!reloading) {
    return;
  }

  reloading = false;
  reloadTimer = 0.0f;
  reloadSpinHasHit = false;
  audio.stop(AudioId::PistolReloadStart);
}

void Weapon::cancelMelee() {
  meleeTimer = 0.0f;
  meleeDuration = 0.0f;
  meleeHasHit = false;
  viewmodel.clearMelee();
}

void Weapon::update(float dt, const Player &player, std::vector<Enemy> &enemies,
                    const Level &level, const Camera3D camera,
                    ParticleSystem &particles, AudioSystem &audio) {
  if (IsKeyPressed(KEY_F4)) {
    debugRaysEnabled = !debugRaysEnabled;
  }

  cooldown = std::max(0.0f, cooldown - dt);
  muzzleFlashTimer = std::max(0.0f, muzzleFlashTimer - dt);
  spreadBloom =
      std::max(0.0f, spreadBloom - data->fire.spreadRecoverySpeed * dt);

  for (DebugShotRay &ray : debugRays) {
    ray.timer = std::max(0.0f, ray.timer - dt);
  }

  debugRays.erase(
      std::remove_if(debugRays.begin(), debugRays.end(),
                     [](const DebugShotRay &ray) { return ray.timer <= 0.0f; }),
      debugRays.end());

  bool wantsToFire = data->fire.automatic
                         ? IsMouseButtonDown(MOUSE_BUTTON_LEFT)
                         : IsMouseButtonPressed(MOUSE_BUTTON_LEFT);

  bool meleePressed = IsKeyPressed(KEY_V);
  bool wasMeleeing = isMeleeing();

  if (meleePressed && !wasMeleeing) {
    tryMelee(audio);
    wasMeleeing = true;
  }

  if (isMeleeing()) {
    updateMelee(dt, camera, enemies, level, particles, audio);
  }

  if (wasMeleeing || isMeleeing()) {
    viewmodel.update(dt, player.isSprinting(), reloading, true,
                     getMeleeProgress(),
                     *proceduralAnimation);
    return;
  }

  if (reloading && data->ammo.reloadOneRoundAtATime && wantsToFire &&
      ammoInMagazine > 0 && cooldown <= 0.0f) {
    cancelReload(audio);
    tryShoot(player, enemies, level, camera, particles, audio);
    return;
  }

  if (reloading) {
    audio.playLooping(AudioId::PistolReloadStart);

    float previousReloadElapsed = data->ammo.reloadDuration - reloadTimer;
    reloadTimer = std::max(0.0f, reloadTimer - dt);
    float currentReloadElapsed = data->ammo.reloadDuration - reloadTimer;

    updateReloadSpinHit(previousReloadElapsed, currentReloadElapsed, camera,
                        enemies, level, particles, audio);

    if (reloadTimer <= 0.0f) {
      finishReload(audio);
    }
  }

  viewmodel.update(dt, player.isSprinting(), reloading, false, 1.0f,
                   *proceduralAnimation);

  if (IsKeyPressed(KEY_R)) {
    startReload();
  }

  if (wantsToFire && cooldown <= 0.0f) {
    tryShoot(player, enemies, level, camera, particles, audio);
  }

  if (!reloading && data->ammo.autoReloadWhenEmpty && ammoInMagazine <= 0 &&
      reserveAmmo > 0) {
    startReload();
  }
}

void Weapon::drawViewModel(const Camera3D &camera, const AssetManager &assets,
                           const Lighting::SceneLighting &lighting,
                           Vector3 pointLightContribution,
                           float switchAmount) const {
  viewmodel.draw(camera, *data, *proceduralAnimation, assets, muzzleFlashTimer,
                 muzzleFlashRotation, switchAmount, lighting,
                 pointLightContribution);
}

void Weapon::drawDebugRays() const {
  if (!debugRaysEnabled) {
    return;
  }

  for (const DebugShotRay &ray : debugRays) {
    float alpha = ray.timer / 0.35f;
    Color color = ray.hit ? GREEN : RED;
    DrawLine3D(ray.start, ray.end, Fade(color, alpha));
    DrawSphere(ray.end, 0.045f, color);
  }
}

const WeaponData &Weapon::getData() const { return *data; }

int Weapon::getAmmoInMagazine() const { return ammoInMagazine; }

int Weapon::getReserveAmmo() const { return reserveAmmo; }

int Weapon::getMagazineSize() const { return data->ammo.magazineSize; }

bool Weapon::isReloading() const { return reloading; }

bool Weapon::isMeleeing() const { return meleeTimer > 0.0f; }

float Weapon::getMeleeProgress() const {
  if (meleeDuration <= 0.0f) {
    return 1.0f;
  }

  return std::clamp(1.0f - meleeTimer / meleeDuration, 0.0f, 1.0f);
}

float Weapon::getReloadProgress() const {
  if (!reloading || data->ammo.reloadDuration <= 0.0f) {
    return 0.0f;
  }

  return 1.0f - reloadTimer / data->ammo.reloadDuration;
}

float Weapon::getCurrentSpreadDegrees(const Player &player) const {
  float spread = data->fire.spreadDegrees + spreadBloom;

  if (player.isSprinting()) {
    spread += data->fire.sprintSpreadDegrees;
  } else if (player.getHorizontalSpeed() > 1.0f) {
    spread += data->fire.movingSpreadDegrees;
  }

  return std::min(spread, data->fire.spreadDegrees +
                              data->fire.maxSpreadBloomDegrees +
                              data->fire.sprintSpreadDegrees);
}

void Weapon::tryShoot(const Player &player, std::vector<Enemy> &enemies,
                      const Level &level, const Camera3D &camera,
                      ParticleSystem &particles, AudioSystem &audio) {
  if (reloading || isMeleeing()) {
    return;
  }

  if (ammoInMagazine <= 0) {
    audio.play(AudioId::PistolDryFire,
               {0.75f,
                0.96f + static_cast<float>(GetRandomValue(0, 8)) / 100.0f,
                0.0f});
    startReload();
    return;
  }

  --ammoInMagazine;

  cooldown = 1.0f / data->fire.fireRate;
  muzzleFlashTimer = 0.025f;
  muzzleFlashRotation = static_cast<float>(GetRandomValue(-25, 25));
  shotFired = true;
  viewmodel.addRecoil(1.0f);

  AudioId fireSound = data->modelId == WeaponModelId::Shotgun
                          ? AudioId::ShotgunFire
                          : AudioId::PistolFire;

  audio.play(
      fireSound,
      {1.0f, 0.96f + static_cast<float>(GetRandomValue(0, 8)) / 100.0f, 0.0f});

  float spreadDegrees = getCurrentSpreadDegrees(player);

  for (int pellet = 0; pellet < data->fire.pelletCount; ++pellet) {
    Ray ray = makeShootRay(camera, spreadDegrees);
    firePelletRay(ray, enemies, level, particles, audio);
  }

  spreadBloom = std::min(data->fire.maxSpreadBloomDegrees,
                         spreadBloom + data->fire.spreadBloomPerShot);
}

bool Weapon::startReload() {
  if (reloading || isMeleeing()) {
    return false;
  }

  if (ammoInMagazine >= data->ammo.magazineSize) {
    return false;
  }

  if (reserveAmmo <= 0) {
    return false;
  }

  reloading = true;
  reloadTimer = data->ammo.reloadDuration;
  reloadSpinHasHit = false;
  return true;
}

void Weapon::finishReload(AudioSystem &audio) {
  int neededAmmo = data->ammo.magazineSize - ammoInMagazine;
  int ammoToLoad = data->ammo.reloadOneRoundAtATime
                       ? std::min(1, reserveAmmo)
                       : std::min(neededAmmo, reserveAmmo);

  ammoInMagazine += ammoToLoad;
  reserveAmmo -= ammoToLoad;

  if (data->ammo.reloadOneRoundAtATime &&
      ammoInMagazine < data->ammo.magazineSize && reserveAmmo > 0) {
    reloadTimer = data->ammo.reloadDuration;
    reloadSpinHasHit = false;
    return;
  }

  reloading = false;
  reloadTimer = 0.0f;
  reloadSpinHasHit = false;
  audio.stop(AudioId::PistolReloadStart);
  audio.play(AudioId::PistolReloadEnd);
}

void Weapon::updateReloadSpinHit(float previousReloadElapsed,
                                 float currentReloadElapsed,
                                 const Camera3D &camera,
                                 std::vector<Enemy> &enemies,
                                 const Level &level, ParticleSystem &particles,
                                 AudioSystem &audio) {
  if (data->modelId != WeaponModelId::Shotgun) {
    return;
  }

  if (!data->ammo.reloadOneRoundAtATime || reloadSpinHasHit) {
    return;
  }

  float activeStart = data->melee.windupDuration;
  float activeEnd = activeStart + data->melee.activeDuration;
  bool crossedActiveWindow =
      previousReloadElapsed <= activeEnd && currentReloadElapsed >= activeStart;

  if (!crossedActiveWindow) {
    return;
  }

  performMeleeHit(camera, enemies, level, particles, audio,
                  data->melee.reloadSpinKnockbackMultiplier);
  reloadSpinHasHit = true;
}

void Weapon::tryMelee(AudioSystem &audio) {
  if (isMeleeing()) {
    return;
  }

  cancelReload(audio);

  meleeDuration = data->melee.windupDuration + data->melee.activeDuration +
                  data->melee.recoveryDuration;
  meleeTimer = meleeDuration;
  meleeHasHit = false;
}

void Weapon::updateMelee(float dt, const Camera3D &camera,
                         std::vector<Enemy> &enemies, const Level &level,
                         ParticleSystem &particles, AudioSystem &audio) {
  if (!isMeleeing()) {
    return;
  }

  float previousElapsed = meleeDuration - meleeTimer;
  meleeTimer = std::max(0.0f, meleeTimer - dt);
  float elapsed = meleeDuration - meleeTimer;
  float activeStart = data->melee.windupDuration;
  float activeEnd = activeStart + data->melee.activeDuration;

  if (!meleeHasHit && previousElapsed <= activeEnd && elapsed >= activeStart) {
    performMeleeHit(camera, enemies, level, particles, audio);
    meleeHasHit = true;
  }

  if (meleeTimer <= 0.0f) {
    meleeDuration = 0.0f;
    meleeHasHit = false;
  }
}

void Weapon::performMeleeHit(const Camera3D &camera,
                             std::vector<Enemy> &enemies, const Level &level,
                             ParticleSystem &particles, AudioSystem &audio,
                             float knockbackMultiplier) {
  Vector3 forward =
      Vector3Normalize(Vector3Subtract(camera.target, camera.position));

  Ray ray{};
  ray.position = camera.position;
  ray.direction = forward;

  int hitEnemyIndex = -1;
  Vector3 enemyHitPoint{};
  float enemyHitDistance = FLT_MAX;

  for (int i = 0; i < static_cast<int>(enemies.size()); ++i) {
    if (!enemies[i].isAlive()) {
      continue;
    }

    BoundingBox hitbox =
        expandedBox(enemies[i].getHitbox(), data->melee.radius);
    RayCollision hit = GetRayCollisionBox(ray, hitbox);

    if (!hit.hit || hit.distance < 0.0f || hit.distance > data->melee.range) {
      continue;
    }

    if (hit.distance >= enemyHitDistance) {
      continue;
    }

    hitEnemyIndex = i;
    enemyHitPoint = hit.point;
    enemyHitDistance = hit.distance;
  }

  Vector3 levelHitPoint{};
  float levelHitDistance = FLT_MAX;
  bool hitLevel =
      Collision::rayLevel(ray, level, levelHitPoint, levelHitDistance);

  if (hitEnemyIndex < 0) {
    float debugDistance =
        hitLevel ? std::min(levelHitDistance, data->melee.range)
                 : data->melee.range;
    addDebugRay(ray, debugDistance, hitLevel);
    return;
  }

  if (hitLevel && levelHitDistance < enemyHitDistance) {
    addDebugRay(ray, levelHitDistance, true);
    return;
  }

  addDebugRay(ray, enemyHitDistance, true);

  Vector3 hitNormal = Vector3Normalize(
      Vector3Subtract(enemyHitPoint, enemies[hitEnemyIndex].getPosition()));

  particles.spawnEnemyHit(enemyHitPoint, hitNormal,
                          enemies[hitEnemyIndex].getVelocity());

  audio.play(
      AudioId::EnemyHit,
      {1.0f, 0.96f + static_cast<float>(GetRandomValue(0, 8)) / 100.0f, 0.0f});

  Vector3 enemyPosition = enemies[hitEnemyIndex].getPosition();
  Vector3 enemyVelocity = enemies[hitEnemyIndex].getVelocity();
  Vector3 knockbackDirection =
      Vector3Subtract(enemyPosition, camera.position);

  enemies[hitEnemyIndex].applyKnockback(
      knockbackDirection, data->melee.knockbackImpulse * knockbackMultiplier,
      data->melee.knockbackLift);

  if (enemies[hitEnemyIndex].applyDamage(data->melee.damage)) {
    particles.spawnEnemyDeath(
        {enemyPosition.x, enemyPosition.y + 0.75f, enemyPosition.z},
        enemyVelocity);
  }
}

Ray Weapon::makeShootRay(const Camera3D &camera, float spreadDegrees) const {
  Vector3 forward =
      Vector3Normalize(Vector3Subtract(camera.target, camera.position));
  Vector3 right = Vector3Normalize(Vector3CrossProduct(forward, camera.up));
  Vector3 up = camera.up;

  float spreadRadians = spreadDegrees * DEG2RAD;
  float angle = static_cast<float>(GetRandomValue(0, 6283)) / 1000.0f;
  float radius =
      std::sqrtf(static_cast<float>(GetRandomValue(0, 10000)) / 10000.0f) *
      std::tanf(spreadRadians);

  Vector3 offset = Vector3Add(Vector3Scale(right, std::cosf(angle) * radius),
                              Vector3Scale(up, std::sinf(angle) * radius));
  Vector3 direction = Vector3Normalize(Vector3Add(forward, offset));

  Ray ray{};
  ray.position = camera.position;
  ray.direction = direction;
  return ray;
}

void Weapon::firePelletRay(Ray ray, std::vector<Enemy> &enemies,
                           const Level &level, ParticleSystem &particles,
                           AudioSystem &audio) {
  int hitEnemyIndex = -1;
  Vector3 enemyHitPoint{};
  Vector3 levelHitPoint{};
  float enemyHitDistance = FLT_MAX;
  float levelHitDistance = FLT_MAX;

  bool hitEnemy =
      Collision::rayEnemies(ray, enemies, hitEnemyIndex, enemyHitPoint);
  bool hitLevel =
      Collision::rayLevel(ray, level, levelHitPoint, levelHitDistance);

  if (!hitEnemy) {
    float debugDistance = hitLevel
                              ? std::min(levelHitDistance, data->fire.range)
                              : data->fire.range;
    addDebugRay(ray, debugDistance, hitLevel);
    return;
  }

  enemyHitDistance = Vector3Distance(ray.position, enemyHitPoint);
  if (enemyHitDistance > data->fire.range) {
    addDebugRay(ray, data->fire.range, false);
    return;
  }

  if (hitLevel && levelHitDistance < enemyHitDistance) {
    addDebugRay(ray, levelHitDistance, true);
    return;
  }

  addDebugRay(ray, enemyHitDistance, true);

  if (hitEnemyIndex < 0) {
    return;
  }

  Vector3 hitNormal = Vector3Normalize(
      Vector3Subtract(enemyHitPoint, enemies[hitEnemyIndex].getPosition()));

  particles.spawnEnemyHit(enemyHitPoint, hitNormal,
                          enemies[hitEnemyIndex].getVelocity());

  audio.play(
      AudioId::EnemyHit,
      {1.0f, 0.96f + static_cast<float>(GetRandomValue(0, 8)) / 100.0f, 0.0f});

  Vector3 enemyPosition = enemies[hitEnemyIndex].getPosition();
  Vector3 enemyVelocity = enemies[hitEnemyIndex].getVelocity();

  if (enemies[hitEnemyIndex].applyDamage(data->fire.damage)) {
    particles.spawnEnemyDeath(
        {enemyPosition.x, enemyPosition.y + 0.75f, enemyPosition.z},
        enemyVelocity);
  }
}

void Weapon::addDebugRay(Ray ray, float distance, bool hit) {
  if (!debugRaysEnabled) {
    return;
  }

  DebugShotRay debugRay{};
  debugRay.start = ray.position;
  debugRay.end =
      Vector3Add(ray.position, Vector3Scale(ray.direction, distance));
  debugRay.hit = hit;
  debugRay.timer = 0.35f;
  debugRays.push_back(debugRay);
}

bool Weapon::consumeShotFired() {
  bool fired = shotFired;
  shotFired = false;
  return fired;
}
