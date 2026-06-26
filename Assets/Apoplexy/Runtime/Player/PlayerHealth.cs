using System;
using Apoplexy.Combat;
using Apoplexy.Weapons;
using UnityEngine;

namespace Apoplexy.Player
{
    public sealed class PlayerHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int maxHealth = 100;
        [SerializeField] private bool disableControlsOnDeath = true;

        private int health;
        private bool dead;

        public event Action<int, int> HealthChanged;
        public event Action Died;

        public int Health => health;
        public int MaxHealth => maxHealth;
        public bool IsDead => dead;

        private void Awake()
        {
            health = maxHealth;
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (dead)
            {
                return;
            }

            health = Mathf.Max(0, health - damageInfo.Damage);
            HealthChanged?.Invoke(health, maxHealth);

            if (health <= 0)
            {
                Die();
            }
        }

        public void ResetHealth()
        {
            dead = false;
            health = maxHealth;
            HealthChanged?.Invoke(health, maxHealth);

            if (!disableControlsOnDeath)
            {
                return;
            }

            foreach (FirstPersonController controller in GetComponentsInChildren<FirstPersonController>())
            {
                controller.enabled = true;
            }

            foreach (PlayerWeaponController weaponController in GetComponentsInChildren<PlayerWeaponController>())
            {
                weaponController.enabled = true;
            }
        }

        private void Die()
        {
            dead = true;
            Died?.Invoke();

            if (!disableControlsOnDeath)
            {
                return;
            }

            foreach (FirstPersonController controller in GetComponentsInChildren<FirstPersonController>())
            {
                controller.enabled = false;
            }

            foreach (PlayerWeaponController weaponController in GetComponentsInChildren<PlayerWeaponController>())
            {
                weaponController.enabled = false;
            }
        }
    }
}
