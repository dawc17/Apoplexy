using UnityEngine;

namespace Apoplexy.Combat
{
    public readonly struct DamageInfo
    {
        public DamageInfo(
            int damage,
            Vector3 point,
            Vector3 direction,
            GameObject source)
        {
            Damage = damage;
            Point = point;
            Direction = direction;
            Source = source;
        }

        public int Damage { get; }
        public Vector3 Point { get; }
        public Vector3 Direction { get; }
        public GameObject Source { get; }
    }

    public interface IDamageable
    {
        void TakeDamage(DamageInfo damageInfo);
    }
}
