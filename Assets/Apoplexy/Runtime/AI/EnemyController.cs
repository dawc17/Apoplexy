using Apoplexy.Combat;
using Apoplexy.Player;
using UnityEngine.AI;
using UnityEngine;

namespace Apoplexy.AI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CharacterController))]
    public sealed class EnemyController : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField] private Transform player;
        [SerializeField] private Renderer bodyRenderer;

        [Header("Body")]
        [SerializeField, Min(0.1f)] private float radius = 0.4f;
        [SerializeField, Min(0.5f)] private float height = 1.88f;
        [SerializeField] private Vector3 eyeOffset = new(0f, 1.45f, 0f);

        [Header("Grounding")]
        [SerializeField] private bool alignVisualBottomToGroundOnStart = true;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField, Min(0f)] private float groundLift = 0.03f;

        [Header("Movement")]
        [SerializeField, Min(0f)] private float speed = 2.5f;
        [SerializeField, Min(0f)] private float searchSpeed = 1.8f;
        [SerializeField, Min(0f)] private float patrolSpeed = 1.45f;
        [SerializeField, Min(0f)] private float acceleration = 14f;
        [SerializeField, Min(0f)] private float gravity = 24f;
        [SerializeField, Min(0f)] private float turnSpeed = 14f;
        [SerializeField, Min(0f)] private float stoppingDistance = 0.55f;

        [Header("Navigation")]
        [SerializeField] private bool useNavMeshPathing = true;
        [SerializeField, Min(0.05f)] private float navMeshSampleDistance = 1.2f;
        [SerializeField, Min(0.05f)] private float navPathRefreshInterval = 0.2f;
        [SerializeField, Min(0.05f)] private float navCornerReachDistance = 0.35f;
        [SerializeField, Min(0.1f)] private float stuckTimeout = 1.2f;
        [SerializeField, Min(0.001f)] private float stuckMoveThreshold = 0.03f;

        [Header("Patrol")]
        [SerializeField] private GameObject[] patrolPoints;
        [SerializeField] private bool startOnPatrol = true;
        [SerializeField] private bool randomizeInitialPatrolPoint = true;
        [SerializeField, Min(0f)] private float patrolWaitDuration = 0.65f;

        [Header("Group Behavior")]
        [SerializeField, Range(0f, 0.5f)] private float patrolSpeedVariance = 0.12f;
        [SerializeField, Range(0f, 0.75f)] private float patrolWaitVariance = 0.35f;
        [SerializeField, Min(0f)] private float investigationScatterRadius = 1.25f;

        [Header("Combat")]
        [SerializeField, Min(1)] private int maxHealth = 30;
        [SerializeField, Min(1)] private int attackDamage = 18;
        [SerializeField, Min(0.1f)] private float attackRange = 1.4f;
        [SerializeField, Min(0f)] private float attackWindupDuration = 0.65f;
        [SerializeField, Min(0f)] private float attackRecoveryDuration = 0.85f;
        [SerializeField, Min(0f)] private float knockbackDecay = 12f;

        [Header("Senses")]
        [SerializeField, Min(0f)] private float visionRange = 18f;
        [SerializeField, Range(1f, 179f)] private float visionHalfAngleDegrees = 55f;
        [SerializeField, Min(0f)] private float suspicionDuration = 0.65f;
        [SerializeField, Min(0f)] private float alertDuration = 0.22f;
        [SerializeField, Min(0f)] private float loseSightGrace = 0.45f;
        [SerializeField, Min(0f)] private float searchDuration = 2.2f;
        [SerializeField, Min(0f)] private float searchPointWaitDuration = 0.35f;
        [SerializeField, Min(0f)] private float searchSweepRadius = 2.2f;
        [SerializeField, Range(1, 8)] private int searchSweepPointCount = 4;
        [SerializeField] private LayerMask sightBlockers = ~0;
        [SerializeField] private LayerMask obstacleMask = ~0;

        [Header("Feedback")]
        [SerializeField] private Color idleColor = Color.white;
        [SerializeField] private Color searchColor = new(0.45f, 0.65f, 1f);
        [SerializeField] private Color suspiciousColor = new(1f, 0.82f, 0.25f);
        [SerializeField] private Color alertColor = new(1f, 0.32f, 0.22f);
        [SerializeField] private Color attackWindupColor = new(1f, 0f, 1f);
        [SerializeField] private Color attackRecoveryColor = new(1f, 0.55f, 0f);
        [SerializeField] private Color hitFlashColor = new(1f, 0.58f, 0.58f);
        [SerializeField] private Color deadColor = new(0.25f, 0.25f, 0.25f);
        [SerializeField, Min(0f)] private float hitFlashDuration = 0.1f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem hitParticlesPrefab;
        [SerializeField] private ParticleSystem deathParticlesPrefab;
        [SerializeField, Min(0f)] private float particleFallbackLifetime = 2f;

        private CharacterController controller;
        private Renderer[] bodyRenderers;
        private MaterialPropertyBlock materialProperties;
        private Vector3 horizontalVelocity;
        private Vector3 knockbackVelocity;
        private Vector3 lastKnownPlayerPosition;
        private Vector3 investigationTarget;
        private NavMeshPath navPath;
        private Vector3 navPathTarget;
        private Vector3 sampledNavTarget;
        private Vector3 lastStuckCheckPosition;
        private float navPathRefreshTimer;
        private float stuckTimer;
        private int patrolIndex;
        private int searchSweepIndex;
        private float pointWaitTimer;
        private float patrolSpeedMultiplier = 1f;
        private float patrolWaitMultiplier = 1f;
        private float verticalVelocity;
        private float stateTimer;
        private float timeSinceSeenPlayer = 999f;
        private float hitFlashTimer;
        private int health;

        public EnemyState State { get; private set; } = EnemyState.Idle;
        public bool IsAlive => State != EnemyState.Dead;

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
        }

        public void ConfigurePatrolRoute(GameObject[] routePoints)
        {
            patrolPoints = routePoints;
            patrolIndex = FindInitialPatrolPointIndex();
        }

        public void SnapToGround(float groundY)
        {
            CacheBodyRenderers();
            ConfigureController();
            AlignVisualBottomToY(groundY);
            verticalVelocity = -2f;
        }

        public void ApplyKnockback(Vector3 direction, float impulse, float lift)
        {
            if (!IsAlive)
            {
                return;
            }

            direction.y = 0f;

            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = -transform.forward;
            }

            direction.Normalize();
            knockbackVelocity += direction * impulse;
            verticalVelocity = Mathf.Max(verticalVelocity, lift);

            if (State == EnemyState.AttackWindup)
            {
                SetState(EnemyState.AttackRecovery, attackRecoveryDuration);
            }
        }

        public void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive)
            {
                return;
            }

            health -= damageInfo.Damage;

            Vector3 threatPosition = damageInfo.Source != null
                ? damageInfo.Source.transform.position
                : damageInfo.Point;

            if (health <= 0)
            {
                Die();
                return;
            }

            SpawnHitParticles(damageInfo);

            hitFlashTimer = hitFlashDuration;
            UpdateVisualState();
            ReactToThreat(threatPosition);
        }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            health = maxHealth;
            materialProperties = new MaterialPropertyBlock();
            navPath = new NavMeshPath();
            lastStuckCheckPosition = transform.position;
            InitializeIndividualVariation();

            CacheBodyRenderers();
            ConfigureController();
        }

        private void CacheBodyRenderers()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponentInChildren<Renderer>();
            }

            bodyRenderers = GetComponentsInChildren<Renderer>();

            if (bodyRenderers.Length == 0 && bodyRenderer != null)
            {
                bodyRenderers = new[] { bodyRenderer };
            }
        }

        private void OnEnable()
        {
            NoiseSystem.NoiseEmitted += OnNoiseEmitted;
        }

        private void Start()
        {
            FindPlayerIfNeeded();
            AlignVisualBottomToGround();
            UpdateVisualState();

            if (startOnPatrol && HasPatrolRoute())
            {
                patrolIndex = FindInitialPatrolPointIndex();
                SetState(EnemyState.Patrol);
            }
        }

        private void OnDisable()
        {
            NoiseSystem.NoiseEmitted -= OnNoiseEmitted;
        }

        private void Update()
        {
            if (!IsAlive)
            {
                return;
            }

            FindPlayerIfNeeded();

            float deltaTime = Time.deltaTime;
            float previousHitFlashTimer = hitFlashTimer;

            stateTimer = Mathf.Max(0f, stateTimer - deltaTime);
            hitFlashTimer = Mathf.Max(0f, hitFlashTimer - deltaTime);
            timeSinceSeenPlayer += deltaTime;

            if (previousHitFlashTimer > 0f && hitFlashTimer <= 0f)
            {
                UpdateVisualState();
            }

            bool seesPlayer = CanSeePlayer();

            if (seesPlayer)
            {
                lastKnownPlayerPosition = player.position;
                timeSinceSeenPlayer = 0f;
            }

            switch (State)
            {
                case EnemyState.Idle:
                    StopHorizontalMovement(deltaTime);

                    if (seesPlayer)
                    {
                        SetState(EnemyState.Suspicious, suspicionDuration);
                    }
                    else if (HasPatrolRoute())
                    {
                        SetState(EnemyState.Patrol);
                    }
                    break;

                case EnemyState.Patrol:
                    if (seesPlayer)
                    {
                        SetState(EnemyState.Suspicious, suspicionDuration);
                        break;
                    }

                    UpdatePatrol(deltaTime);
                    break;

                case EnemyState.Suspicious:
                    FaceTowards(lastKnownPlayerPosition, deltaTime);
                    StopHorizontalMovement(deltaTime);

                    if (!seesPlayer)
                    {
                        BeginSearch(lastKnownPlayerPosition);
                    }
                    else if (stateTimer <= 0f)
                    {
                        SetState(EnemyState.Alert, alertDuration);
                    }
                    break;

                case EnemyState.Alert:
                    FaceTowards(lastKnownPlayerPosition, deltaTime);
                    StopHorizontalMovement(deltaTime);

                    if (!seesPlayer)
                    {
                        BeginSearch(lastKnownPlayerPosition);
                    }
                    else if (stateTimer <= 0f)
                    {
                        SetState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Chase:
                    UpdateChase(deltaTime);
                    break;

                case EnemyState.Search:
                    UpdateSearch(seesPlayer, deltaTime);
                    break;

                case EnemyState.AttackWindup:
                    FaceTowards(player.position, deltaTime);
                    StopHorizontalMovement(deltaTime);

                    if (stateTimer <= 0f)
                    {
                        AttackPlayer();
                        SetState(EnemyState.AttackRecovery, attackRecoveryDuration);
                    }
                    break;

                case EnemyState.AttackRecovery:
                    StopHorizontalMovement(deltaTime);

                    if (stateTimer <= 0f)
                    {
                        if (timeSinceSeenPlayer <= loseSightGrace)
                        {
                            SetState(EnemyState.Chase);
                        }
                        else
                        {
                            BeginSearch(lastKnownPlayerPosition);
                        }
                    }
                    break;
            }

            ApplyGravityAndKnockback(deltaTime);
        }

        private void UpdateChase(float deltaTime)
        {
            if (player == null)
            {
                SetState(EnemyState.Idle);
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= attackRange && timeSinceSeenPlayer <= loseSightGrace)
            {
                SetState(EnemyState.AttackWindup, attackWindupDuration);
                return;
            }

            if (timeSinceSeenPlayer > loseSightGrace)
            {
                BeginSearch(lastKnownPlayerPosition);
                return;
            }

            MoveToward(player.position, speed, deltaTime);
        }

        private void UpdatePatrol(float deltaTime)
        {
            if (!HasPatrolRoute())
            {
                SetState(EnemyState.Idle);
                return;
            }

            Transform point = GetPatrolPoint(patrolIndex);

            if (point == null)
            {
                AdvancePatrolPoint();
                return;
            }

            if (pointWaitTimer > 0f)
            {
                pointWaitTimer = Mathf.Max(0f, pointWaitTimer - deltaTime);
                StopHorizontalMovement(deltaTime);
                FaceTowards(point.position, deltaTime);
                return;
            }

            MoveToward(point.position, patrolSpeed * patrolSpeedMultiplier, deltaTime);

            if (HasReached(point.position))
            {
                AdvancePatrolPoint();
                pointWaitTimer = GetPatrolWaitDuration();
            }
            else if (IsStuck(deltaTime))
            {
                AdvancePatrolPoint();
                pointWaitTimer = GetPatrolWaitDuration();
                ResetStuckCheck();
                ResetNavPath();
            }
        }

        private void UpdateSearch(bool seesPlayer, float deltaTime)
        {
            if (seesPlayer)
            {
                SetState(EnemyState.Suspicious, suspicionDuration);
                return;
            }

            Vector3 target = GetSearchTarget();

            if (pointWaitTimer > 0f)
            {
                pointWaitTimer = Mathf.Max(0f, pointWaitTimer - deltaTime);
                StopHorizontalMovement(deltaTime);
                FaceTowards(target, deltaTime);
                return;
            }

            MoveToward(target, searchSpeed, deltaTime);

            if (HasReached(target))
            {
                searchSweepIndex++;
                pointWaitTimer = searchPointWaitDuration;
            }
            else if (IsStuck(deltaTime))
            {
                searchSweepIndex++;
                pointWaitTimer = searchPointWaitDuration;
                ResetStuckCheck();
                ResetNavPath();
            }
        }

        private void BeginSearch(Vector3 target)
        {
            investigationTarget = ScatterInvestigationTarget(target);
            searchSweepIndex = 0;
            pointWaitTimer = 0f;
            SetState(EnemyState.Search, searchDuration);
        }

        private void InitializeIndividualVariation()
        {
            patrolSpeedMultiplier = Random.Range(1f - patrolSpeedVariance, 1f + patrolSpeedVariance);
            patrolWaitMultiplier = Random.Range(1f - patrolWaitVariance, 1f + patrolWaitVariance);
        }

        private float GetPatrolWaitDuration()
        {
            return patrolWaitDuration * patrolWaitMultiplier;
        }

        private Vector3 ScatterInvestigationTarget(Vector3 target)
        {
            if (investigationScatterRadius <= 0f)
            {
                return target;
            }

            Vector2 scatter = Random.insideUnitCircle * investigationScatterRadius;
            Vector3 scatteredTarget = target + new Vector3(scatter.x, 0f, scatter.y);

            if (!useNavMeshPathing)
            {
                return scatteredTarget;
            }

            return NavMesh.SamplePosition(scatteredTarget, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas)
                ? hit.position
                : target;
        }

        private bool HasReached(Vector3 target)
        {
            Vector3 reachableTarget = GetReachableTarget(target);
            Vector3 toTarget = reachableTarget - transform.position;
            toTarget.y = 0f;
            return toTarget.sqrMagnitude <= stoppingDistance * stoppingDistance;
        }

        private Vector3 GetReachableTarget(Vector3 target)
        {
            if (!useNavMeshPathing)
            {
                return target;
            }

            return NavMesh.SamplePosition(target, out NavMeshHit hit, navMeshSampleDistance, NavMesh.AllAreas)
                ? hit.position
                : target;
        }

        private Vector3 GetSearchTarget()
        {
            if (searchSweepIndex <= 0 || searchSweepRadius <= 0f || searchSweepPointCount <= 1)
            {
                return investigationTarget;
            }

            int sweepIndex = (searchSweepIndex - 1) % searchSweepPointCount;
            float angle = sweepIndex * Mathf.PI * 2f / searchSweepPointCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * searchSweepRadius;
            return investigationTarget + offset;
        }

        private bool HasPatrolRoute()
        {
            return patrolPoints != null && patrolPoints.Length > 0;
        }

        private int FindInitialPatrolPointIndex()
        {
            if (!HasPatrolRoute())
            {
                return 0;
            }

            if (randomizeInitialPatrolPoint && patrolPoints.Length > 1)
            {
                return Random.Range(0, patrolPoints.Length);
            }

            return FindClosestPatrolPointIndex();
        }

        private int FindClosestPatrolPointIndex()
        {
            int closestIndex = 0;
            float closestDistance = float.PositiveInfinity;

            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Transform point = GetPatrolPoint(i);

                if (point == null)
                {
                    continue;
                }

                float distance = (point.position - transform.position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private Transform GetPatrolPoint(int index)
        {
            if (patrolPoints == null || index < 0 || index >= patrolPoints.Length)
            {
                return null;
            }

            GameObject point = patrolPoints[index];
            return point != null ? point.transform : null;
        }

        private void AdvancePatrolPoint()
        {
            if (!HasPatrolRoute())
            {
                return;
            }

            patrolIndex = (patrolIndex + 1) % patrolPoints.Length;
        }

        private void MoveToward(Vector3 target, float targetSpeed, float deltaTime)
        {
            Vector3 movementTarget = ResolveMovementTarget(target, deltaTime);
            Vector3 toTarget = movementTarget - transform.position;
            toTarget.y = 0f;

            if (toTarget.magnitude <= stoppingDistance)
            {
                StopHorizontalMovement(deltaTime);
                return;
            }

            Vector3 desiredDirection = useNavMeshPathing
                ? toTarget.normalized
                : SteerAroundObstacles(toTarget.normalized);
            Vector3 desiredVelocity = desiredDirection * targetSpeed;
            float ease = 1f - Mathf.Exp(-acceleration * deltaTime);

            horizontalVelocity = Vector3.Lerp(horizontalVelocity, desiredVelocity, ease);
            FaceDirection(desiredDirection, deltaTime);
        }

        private Vector3 ResolveMovementTarget(Vector3 target, float deltaTime)
        {
            if (!useNavMeshPathing)
            {
                return target;
            }

            navPathRefreshTimer -= deltaTime;

            bool targetChanged = (target - navPathTarget).sqrMagnitude > 0.25f;

            if (navPathRefreshTimer <= 0f || targetChanged)
            {
                navPathRefreshTimer = navPathRefreshInterval;
                navPathTarget = target;
                RebuildNavPath(target);
            }

            if (navPath == null || navPath.status == NavMeshPathStatus.PathInvalid || navPath.corners.Length <= 1)
            {
                return sampledNavTarget;
            }

            for (int i = 1; i < navPath.corners.Length; i++)
            {
                Vector3 corner = navPath.corners[i];
                Vector3 toCorner = corner - transform.position;
                toCorner.y = 0f;

                float cornerReachDistance = Mathf.Max(navCornerReachDistance, radius + 0.2f);

                if (toCorner.sqrMagnitude > cornerReachDistance * cornerReachDistance)
                {
                    return corner;
                }
            }

            return sampledNavTarget;
        }

        private void RebuildNavPath(Vector3 target)
        {
            if (navPath == null)
            {
                navPath = new NavMeshPath();
            }

            bool hasStart = NavMesh.SamplePosition(transform.position,
                                                   out NavMeshHit startHit,
                                                   navMeshSampleDistance,
                                                   NavMesh.AllAreas);

            bool hasEnd = NavMesh.SamplePosition(
                target,
                out NavMeshHit endHit,
                navMeshSampleDistance,
                NavMesh.AllAreas);

            if (!hasStart || !hasEnd)
            {
                navPath.ClearCorners();
                sampledNavTarget = target;
                return;
            }

            sampledNavTarget = endHit.position;

            if (!NavMesh.CalculatePath(startHit.position, endHit.position, NavMesh.AllAreas, navPath))
            {
                navPath.ClearCorners();
            }
        }

        private void ResetNavPath()
        {
            navPath?.ClearCorners();
            navPathRefreshTimer = 0f;
            navPathTarget = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
            sampledNavTarget = transform.position;
        }

        private bool IsStuck(float deltaTime)
        {
            Vector3 moved = transform.position - lastStuckCheckPosition;
            moved.y = 0f;

            if (moved.magnitude > stuckMoveThreshold)
            {
                ResetStuckCheck();
                return false;
            }

            stuckTimer += deltaTime;
            return stuckTimer >= stuckTimeout;
        }

        private void ResetStuckCheck()
        {
            stuckTimer = 0f;
            lastStuckCheckPosition = transform.position;
        }

        private Vector3 SteerAroundObstacles(Vector3 desiredDirection)
        {
            Vector3 origin = transform.position + Vector3.up * 0.75f;

            if (!Physics.SphereCast(origin, radius * 0.65f, desiredDirection, out _, 1.1f, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                return desiredDirection;
            }

            Vector3 left = Quaternion.Euler(0f, -55f, 0f) * desiredDirection;
            Vector3 right = Quaternion.Euler(0f, 55f, 0f) * desiredDirection;

            bool leftBlocked = Physics.SphereCast(origin, radius * 0.65f, left, out _, 1.1f, obstacleMask, QueryTriggerInteraction.Ignore);
            bool rightBlocked = Physics.SphereCast(origin, radius * 0.65f, right, out _, 1.1f, obstacleMask, QueryTriggerInteraction.Ignore);

            if (!leftBlocked)
            {
                return left.normalized;
            }

            return rightBlocked ? -desiredDirection : right.normalized;
        }

        private void StopHorizontalMovement(float deltaTime)
        {
            float ease = 1f - Mathf.Exp(-acceleration * deltaTime);
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, ease);
        }

        private void ApplyGravityAndKnockback(float deltaTime)
        {
            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }
            else
            {
                verticalVelocity -= gravity * deltaTime;
            }

            knockbackVelocity = Vector3.MoveTowards(knockbackVelocity, Vector3.zero, knockbackDecay * deltaTime);

            Vector3 velocity = horizontalVelocity + knockbackVelocity + Vector3.up * verticalVelocity;
            controller.Move(velocity * deltaTime);
        }

        private bool CanSeePlayer()
        {
            if (player == null)
            {
                return false;
            }

            Vector3 eye = transform.position + eyeOffset;
            Vector3 target = player.position + Vector3.up * 1.2f;
            Vector3 toPlayer = target - eye;

            if (toPlayer.magnitude > visionRange)
            {
                return false;
            }

            Vector3 flatToPlayer = toPlayer;
            flatToPlayer.y = 0f;

            if (flatToPlayer.sqrMagnitude > 0.001f)
            {
                float angle = Vector3.Angle(transform.forward, flatToPlayer.normalized);

                if (angle > visionHalfAngleDegrees)
                {
                    return false;
                }
            }

            if (!Physics.Linecast(eye, target, out RaycastHit hit, sightBlockers, QueryTriggerInteraction.Ignore))
            {
                return true;
            }

            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        private void AttackPlayer()
        {
            if (player == null || Vector3.Distance(transform.position, player.position) > attackRange + 0.35f)
            {
                return;
            }

            IDamageable damageable = player.GetComponentInParent<IDamageable>();
            damageable?.TakeDamage(new DamageInfo(attackDamage, player.position, (player.position - transform.position).normalized, gameObject));
        }

        private void OnNoiseEmitted(NoiseEvent noiseEvent)
        {
            if (!IsAlive || noiseEvent.Source == gameObject)
            {
                return;
            }

            float distance = Vector3.Distance(transform.position, noiseEvent.Position);

            if (distance > noiseEvent.Radius)
            {
                return;
            }

            investigationTarget = noiseEvent.Position;
            lastKnownPlayerPosition = noiseEvent.Position;

            if (State is EnemyState.Idle or EnemyState.Search)
            {
                BeginSearch(noiseEvent.Position);
            }
            else if (State is EnemyState.Suspicious)
            {
                BeginSearch(noiseEvent.Position);
            }
        }

        private void ReactToThreat(Vector3 threatPosition)
        {
            investigationTarget = threatPosition;
            lastKnownPlayerPosition = threatPosition;
            SetState(EnemyState.Alert, alertDuration);
        }

        private void Die()
        {
            SpawnDeathParticles();
            Destroy(gameObject);
        }

        private void SpawnHitParticles(DamageInfo damageInfo)
        {
            if (hitParticlesPrefab == null)
            {
                return;
            }

            Quaternion rotation = RotationFromDamage(damageInfo);
            ParticleSystem particles = Instantiate(hitParticlesPrefab, damageInfo.Point, rotation);
            DestroyParticlesWhenFinished(particles);
        }

        private void SpawnDeathParticles()
        {
            if (deathParticlesPrefab == null)
            {
                return;
            }

            ParticleSystem particles = Instantiate(deathParticlesPrefab, transform.position + Vector3.up * (height * 0.5f), Quaternion.identity);

            DestroyParticlesWhenFinished(particles);
        }

        private static Quaternion RotationFromDamage(DamageInfo damageInfo)
        {
            if (damageInfo.Direction.sqrMagnitude <= 0.001f)
            {
                return Quaternion.identity;
            }

            return Quaternion.LookRotation(-damageInfo.Direction.normalized, Vector3.up);
        }

        private void DestroyParticlesWhenFinished(ParticleSystem particles)
        {
            if (particles == null)
            {
                return;
            }

            ParticleSystem.MainModule main = particles.main;
            float lifetime = main.duration + main.startLifetime.constantMax;

            Destroy(particles.gameObject, Mathf.Max(lifetime, particleFallbackLifetime));
        }

        private void SetState(EnemyState newState, float timer = 0f)
        {
            if (State == newState && Mathf.Approximately(stateTimer, timer))
            {
                return;
            }

            State = newState;
            stateTimer = timer;
            ResetStuckCheck();
            ResetNavPath();
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (bodyRenderers == null || bodyRenderers.Length == 0)
            {
                return;
            }

            Color color = hitFlashTimer > 0f ? hitFlashColor : State switch
            {
                EnemyState.Search => searchColor,
                EnemyState.Patrol => idleColor,
                EnemyState.Suspicious => suspiciousColor,
                EnemyState.AttackWindup => attackWindupColor,
                EnemyState.AttackRecovery => attackRecoveryColor,
                EnemyState.Alert or EnemyState.Chase => alertColor,
                EnemyState.Dead => deadColor,
                _ => idleColor,
            };

            foreach (Renderer renderer in bodyRenderers)
            {
                renderer.GetPropertyBlock(materialProperties);
                materialProperties.SetColor("_BaseColor", color);
                materialProperties.SetColor("_Color", color);
                renderer.SetPropertyBlock(materialProperties);
            }
        }

        private void FaceTowards(Vector3 target, float deltaTime)
        {
            Vector3 direction = target - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                FaceDirection(direction.normalized, deltaTime);
            }
        }

        private void FaceDirection(Vector3 direction, float deltaTime)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 1f - Mathf.Exp(-turnSpeed * deltaTime));
        }

        private void FindPlayerIfNeeded()
        {
            if (player != null)
            {
                return;
            }

            FirstPersonController controller = FindAnyObjectByType<FirstPersonController>();
            player = controller != null ? controller.transform : null;
        }

        private void AlignVisualBottomToGround()
        {
            if (!alignVisualBottomToGroundOnStart || bodyRenderers == null || bodyRenderers.Length == 0)
            {
                return;
            }

            if (!TryGetVisualBounds(out Bounds visualBounds))
            {
                return;
            }

            if (!TryFindGroundBelow(visualBounds, groundMask, out float groundY)
                && !TryFindGroundBelow(visualBounds, ~0, out groundY))
            {
                return;
            }

            AlignVisualBottomToY(groundY);
        }

        private void AlignVisualBottomToY(float groundY)
        {
            if (!TryGetVisualBounds(out Bounds visualBounds))
            {
                return;
            }

            float lift = groundY + groundLift - visualBounds.min.y;

            if (Mathf.Abs(lift) <= 0.001f)
            {
                return;
            }

            transform.position += Vector3.up * lift;
        }

        private bool TryGetVisualBounds(out Bounds visualBounds)
        {
            visualBounds = default;

            if (bodyRenderers == null || bodyRenderers.Length == 0)
            {
                return false;
            }

            bool found = false;

            foreach (Renderer renderer in bodyRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                if (!found)
                {
                    visualBounds = renderer.bounds;
                    found = true;
                    continue;
                }

                visualBounds.Encapsulate(renderer.bounds);
            }

            return found;
        }

        private bool TryFindGroundBelow(
            Bounds visualBounds,
            LayerMask mask,
            out float groundY)
        {
            Vector3 origin = new(
                visualBounds.center.x,
                visualBounds.max.y + 2f,
                visualBounds.center.z);

            RaycastHit[] hits = Physics.RaycastAll(
                origin,
                Vector3.down,
                visualBounds.size.y + 6f,
                mask,
                QueryTriggerInteraction.Ignore);

            groundY = 0f;
            float closestDistance = float.PositiveInfinity;
            bool found = false;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                {
                    continue;
                }

                if (hit.distance >= closestDistance)
                {
                    continue;
                }

                closestDistance = hit.distance;
                groundY = hit.point.y;
                found = true;
            }

            return found;
        }

        private void ConfigureController()
        {
            if (controller == null)
            {
                return;
            }

            if (TryGetBodyLocalBounds(out Bounds localBounds))
            {
                controller.center = localBounds.center;
                controller.height = Mathf.Max(0.1f, localBounds.size.y);
                controller.radius = Mathf.Max(0.05f, Mathf.Max(localBounds.extents.x, localBounds.extents.z));
            }
            else
            {
                controller.height = height;
                controller.radius = radius;
                controller.center = Vector3.up * (height * 0.5f);
            }

            controller.stepOffset = 0.25f;
            controller.slopeLimit = 50f;
            controller.skinWidth = 0.03f;
            controller.minMoveDistance = 0f;
        }

        private bool TryGetBodyLocalBounds(out Bounds localBounds)
        {
            localBounds = default;

            if (bodyRenderers == null || bodyRenderers.Length == 0)
            {
                return false;
            }

            bool hasBounds = false;

            foreach (Renderer renderer in bodyRenderers)
            {
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = true;

                Bounds worldBounds = renderer.bounds;
                Vector3 min = worldBounds.min;
                Vector3 max = worldBounds.max;

                EncapsulateLocalPoint(new Vector3(min.x, min.y, min.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(min.x, min.y, max.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(min.x, max.y, min.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(min.x, max.y, max.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(max.x, min.y, min.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(max.x, min.y, max.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(max.x, max.y, min.z), ref localBounds, ref hasBounds);
                EncapsulateLocalPoint(new Vector3(max.x, max.y, max.z), ref localBounds, ref hasBounds);
            }

            return hasBounds;
        }

        private void EncapsulateLocalPoint(
            Vector3 worldPoint,
            ref Bounds localBounds,
            ref bool hasBounds)
        {
            Vector3 localPoint = transform.InverseTransformPoint(worldPoint);

            if (!hasBounds)
            {
                localBounds = new Bounds(localPoint, Vector3.zero);
                hasBounds = true;
                return;
            }

            localBounds.Encapsulate(localPoint);
        }
    }
}
