#pragma once

#include "raylib.h"

#include <vector>

class Player;
class Enemy;

class Weapon {
public:
  Weapon();

  void reset();

  void update(float dt, const Player &player, std::vector<Enemy> &enemies,
              const Camera3D camera);

  void drawViewModel() const;

private:
  void tryShoot(const Player &player, std::vector<Enemy> &enemies,
                const Camera3D &camera);
  Ray makeShootRay(const Camera3D &camera) const;

private:
  int damage = 15;
  float range = 100.0f;
  float fireRate = 6.0f;
  float cooldown = 0.0f;

  float recoil = 0.0f;
  float muzzleFlashTimer = 0.0f;
};
