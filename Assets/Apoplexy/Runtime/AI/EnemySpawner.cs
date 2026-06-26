using Apoplexy.Levels;
using Apoplexy.Player;
using UnityEngine;

namespace Apoplexy.AI
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const float PlaceholderHeight = 1.88f;
        private const float PlaceholderRadius = 0.4f;
        private const float GroundProbeHeight = 3f;
        private const float GroundProbeDistance = 8f;
        private const float MinimumGroundNormalY = 0.55f;

        private static bool hasSpawnedThisScene;

        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool destroySpawnMarkers;
        [SerializeField] private LayerMask groundMask = ~0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSpawnState()
        {
            hasSpawnedThisScene = false;
        }

        private void Start()
        {
            if (spawnOnStart)
            {
                SpawnAll();
            }
        }

        public void SpawnAll()
        {
            if (hasSpawnedThisScene)
            {
                return;
            }

            hasSpawnedThisScene = true;
            FindPlayerIfNeeded();

            SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>();

            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.Type != SpawnPointType.Enemy)
                {
                    continue;
                }

                Vector3 groundPosition = ResolveGroundPosition(spawnPoint.transform.position);
                EnemyController enemy = SpawnEnemy(groundPosition, spawnPoint.transform.rotation);
                enemy.Configure(player);

                if (destroySpawnMarkers)
                {
                    Destroy(spawnPoint.gameObject);
                }
            }
        }

        private EnemyController SpawnEnemy(Vector3 groundPosition, Quaternion rotation)
        {
            GameObject enemyObject = enemyPrefab != null
                ? Instantiate(enemyPrefab, groundPosition, rotation)
                : CreatePlaceholderEnemy(groundPosition, rotation);

            EnemyController enemy = EnsureEnemyComponents(enemyObject);
            enemy.SnapToGround(groundPosition.y);

            return enemy;
        }

        private GameObject CreatePlaceholderEnemy(Vector3 position, Quaternion rotation)
        {
            GameObject enemyObject = new("Enemy");
            enemyObject.transform.SetPositionAndRotation(position, rotation);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Enemy Placeholder Visual";
            visual.transform.SetParent(enemyObject.transform, false);
            visual.transform.localPosition = Vector3.up * (PlaceholderHeight * 0.5f);
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = new Vector3(
                PlaceholderRadius * 2f,
                PlaceholderHeight,
                PlaceholderRadius * 2f);

            Collider visualCollider = visual.GetComponent<Collider>();

            if (visualCollider != null)
            {
                Destroy(visualCollider);
            }

            return enemyObject;
        }

        private static EnemyController EnsureEnemyComponents(GameObject enemyObject)
        {
            CharacterController characterController =
                enemyObject.GetComponent<CharacterController>();

            if (characterController == null)
            {
                characterController = enemyObject.AddComponent<CharacterController>();
                characterController.height = PlaceholderHeight;
                characterController.radius = PlaceholderRadius;
                characterController.center = Vector3.up * (PlaceholderHeight * 0.5f);
            }

            EnemyController enemy = enemyObject.GetComponent<EnemyController>();

            if (enemy == null)
            {
                enemy = enemyObject.AddComponent<EnemyController>();
            }

            return enemy;
        }

        private Vector3 ResolveGroundPosition(Vector3 markerPosition)
        {
            if (TryResolveGroundPosition(markerPosition, groundMask, out Vector3 groundPosition)
                || TryResolveGroundPosition(markerPosition, 1 << 0, out groundPosition)
                || TryResolveGroundPosition(markerPosition, ~0, out groundPosition))
            {
                return groundPosition;
            }

            return markerPosition;
        }

        private static bool TryResolveGroundPosition(
            Vector3 markerPosition,
            LayerMask mask,
            out Vector3 groundPosition)
        {
            if (mask.value == 0)
            {
                groundPosition = markerPosition;
                return false;
            }

            Vector3 rayOrigin = markerPosition + Vector3.up * GroundProbeHeight;
            RaycastHit[] hits = Physics.RaycastAll(
                rayOrigin,
                Vector3.down,
                GroundProbeHeight + GroundProbeDistance,
                mask,
                QueryTriggerInteraction.Ignore);

            groundPosition = markerPosition;
            float closestDistance = float.PositiveInfinity;
            bool found = false;

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider == null || hit.normal.y < MinimumGroundNormalY)
                {
                    continue;
                }

                if (hit.distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = hit.distance;
                groundPosition = hit.point;
                found = true;
            }

            return found;
        }

        private void FindPlayerIfNeeded()
        {
            if (player != null)
            {
                return;
            }

            FirstPersonController playerController =
                FindAnyObjectByType<FirstPersonController>();

            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
    }
}
