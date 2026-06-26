using Apoplexy.Levels;
using Apoplexy.Player;
using UnityEngine;

namespace Apoplexy.AI
{
    public sealed class EnemySpawner : MonoBehaviour
    {
        private const float PlaceholderHeight = 1.88f;
        private const float PlaceholderRadius = 0.4f;

        private static bool hasSpawnedThisScene;

        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform player;
        [SerializeField] private bool spawnOnStart = true;
        [SerializeField] private bool destroySpawnMarkers;

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

                EnemyController enemy = SpawnEnemy(spawnPoint.transform.position, spawnPoint.transform.rotation);
                enemy.Configure(player);

                if (destroySpawnMarkers)
                {
                    Destroy(spawnPoint.gameObject);
                }
            }
        }

        private EnemyController SpawnEnemy(Vector3 position, Quaternion rotation)
        {
            if (enemyPrefab != null)
            {
                return Instantiate(enemyPrefab, position, rotation, transform);
            }

            GameObject enemyObject = new("Enemy");
            enemyObject.name = "Enemy";
            enemyObject.transform.SetPositionAndRotation(position, rotation);
            enemyObject.transform.SetParent(transform);

            CharacterController characterController = enemyObject.AddComponent<CharacterController>();
            characterController.height = PlaceholderHeight;
            characterController.radius = PlaceholderRadius;
            characterController.center = Vector3.up * (PlaceholderHeight * 0.5f);

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

            return enemyObject.AddComponent<EnemyController>();
        }

        private void FindPlayerIfNeeded()
        {
            if (player != null)
            {
                return;
            }

            FirstPersonController playerController = FindAnyObjectByType<FirstPersonController>();

            if (playerController != null)
            {
                player = playerController.transform;
            }
        }
    }
}
