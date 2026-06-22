using UnityEngine;

namespace Apoplexy.Levels
{
    public enum SpawnPointType
    {
        Player,
        Enemy
    }

    [DisallowMultipleComponent]
    public sealed class SpawnPoint : MonoBehaviour
    {
        [field: SerializeField]
        public SpawnPointType Type { get; private set; }

        public void Configure(SpawnPointType type)
        {
            Type = type;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Type == SpawnPointType.Player
                ? Color.cyan
                : Color.red;

            Gizmos.DrawWireSphere(transform.position, 0.35f);
            Gizmos.DrawLine(
                transform.position,
                transform.position + transform.forward);
        }
    }
}
