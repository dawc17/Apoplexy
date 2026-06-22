using UnityEngine;

namespace Apoplexy.Combat
{
    public sealed class DamageableTarget : MonoBehaviour, IDamageable
    {
        [SerializeField, Min(1)] private int health = 30;

        public void TakeDamage(DamageInfo damageInfo)
        {
            health -= damageInfo.Damage;

            Debug.Log(
                $"{name} took {damageInfo.Damage} damage. " +
                $"{Mathf.Max(health, 0)} health remains.",
                this);

            if (health <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
