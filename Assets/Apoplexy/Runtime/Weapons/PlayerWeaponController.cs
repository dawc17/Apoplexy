using System;
using Apoplexy.AI;
using Apoplexy.Combat;
using Apoplexy.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.InputSystem;

namespace Apoplexy.Weapons
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PlayerWeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private WeaponDefinition weapon;
        [SerializeField] private Transform viewModelRoot;

        [Header("Inventory")]
        [SerializeField] private WeaponDefinition[] weapons = new WeaponDefinition[0];
        [SerializeField, Min(0)] private int startingWeaponIndex;
        [SerializeField, Min(0.01f)] private float weaponSwitchDuration = 0.34f;
        [SerializeField] private AudioClip weaponSwitchSound;
        [SerializeField] private Vector3 weaponSwitchOffset = new(0.10f, -0.42f, -0.12f);
        [SerializeField] private Vector3 weaponSwitchRotation = new(18f, 22f, -12f);

        [Header("Collision")]
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private bool drawDebugShots = true;

        [Header("Noise")]
        [SerializeField, Min(0f)] private float quietShotNoiseRadius = 7f;
        [SerializeField, Min(0f)] private float loudShotNoiseRadius = 28f;

        [Header("Hit Feedback")]
        [SerializeField] private AudioClip enemyHitSound;

        private static readonly Key[] WeaponNumberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };

        private InputAction attackAction;
        private InputAction reloadAction;
        private AudioSource audioSource;
        private AudioSource reloadAudioSource;
        private GameObject viewModelInstance;

        private int ammunition;
        private int reserveAmmunition;
        private float cooldown;
        private float reloadTimer;
        private float spreadBloom;
        private bool reloading;

        private int activeWeaponIndex;
        private int pendingWeaponIndex;
        private int[] weaponAmmunition;
        private int[] weaponReserveAmmunition;
        private float weaponSwitchTimer;
        private bool weaponSwitchCommited;

        private Vector2 swayPosition;
        private Vector2 swayRotation;
        private float idleBobTimer;
        private float idleBobAmount;
        private float walkBobTimer;
        private float walkBobAmount;
        private float sprintAmount;
        private float recoilTimer;
        private float recoilAmount;
        private float reloadAmount;
        private Vector3 reloadSpinRotation;
        // private float meleeTimer;
        // private float meleeDuration;
        // private bool meleeHasHit;
        // private bool reloadSpinHasHit;

        private GameObject viewModelModel;
        private Transform muzzleSocket;
        private Transform muzzleFlashTransform;
        private SpriteRenderer muzzleFlashRenderer;

        private float muzzleFlashTimer;
        private float muzzleFlashRotation;
        private bool muzzleFlashPreviewVisible;

        public event Action AmmunitionChanged;
        public event Action<WeaponDefinition> ShotFired;

        public int Ammunition => ammunition;
        public int ReserveAmmunition => reserveAmmunition;
        public bool IsReloading => reloading;
        public bool IsSwitching => weaponSwitchTimer > 0f;
        public WeaponDefinition Weapon => weapon;
        public int ActiveWeaponIndex => activeWeaponIndex;
        public int WeaponCount => weapons != null ? weapons.Length : 0;
        public Transform MuzzleSocket => muzzleSocket;
        public float CurrentSpreadDegrees => weapon != null ? CalculateSpread() : 0f;
        // public bool IsMeleeing => meleeTimer > 0f;

        public float ReloadProgress => reloading ? 1f - reloadTimer / weapon.ReloadDuration : 0f;

        public void ApplyViewModelPose()
        {
            if (viewModelRoot == null || viewModelInstance == null)
            {
                return;
            }

            viewModelRoot.localPosition = weapon.HoldPosition;
            viewModelRoot.localRotation = Quaternion.Euler(weapon.HoldRotation);

            if (viewModelModel != null)
            {
                viewModelModel.transform.localScale = weapon.ViewModelScale;
            }

            if (muzzleSocket != null)
            {
                muzzleSocket.localPosition = weapon.MuzzlePosition;
            }
        }

        public void ApplyMuzzleFlashSettings()
        {
            if (muzzleSocket == null)
            {
                return;
            }

            muzzleSocket.localPosition = weapon.MuzzlePosition;

            if (muzzleFlashRenderer == null)
            {
                return;
            }

            muzzleFlashRenderer.sprite = weapon.MuzzleFlashSprite;
            muzzleFlashRenderer.color = weapon.MuzzleFlashColor;
            SetMuzzleFlashScale(1f);
            UpdateMuzzleFlash();
        }

        public void SetMuzzleFlashPreviewVisible(bool visible)
        {
            muzzleFlashPreviewVisible = visible;
            UpdateMuzzleFlash();
        }

        public void PlayMuzzleFlashPreview()
        {
            muzzleFlashTimer = weapon.MuzzleFlashDuration;
            muzzleFlashRotation = UnityEngine.Random.Range(-25f, 25f);
            UpdateMuzzleFlash();
        }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            reloadAudioSource = gameObject.AddComponent<AudioSource>();
            reloadAudioSource.playOnAwake = false;
            reloadAudioSource.spatialBlend = 0f;

            if (viewCamera == null)
            {
                viewCamera = GetComponentInParent<Camera>();
            }

            if (player == null)
            {
                player = GetComponentInParent<FirstPersonController>();
            }

            InitializeWeaponInventory();

            if (viewCamera == null || player == null || inputActions == null || weapon == null)
            {
                Debug.LogError("PlayerWeaponController has missing references.", this);

                enabled = false;
                return;
            }

            InputActionMap playerActions = inputActions.FindActionMap("Player", true);

            attackAction = playerActions.FindAction("Attack", true);
            reloadAction = playerActions.FindAction("Reload", false);

            playerActions.Enable();

            ammunition = weaponAmmunition[activeWeaponIndex];
            reserveAmmunition = weaponReserveAmmunition[activeWeaponIndex];

            CreateViewModel();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            muzzleFlashTimer = Mathf.Max(0f, muzzleFlashTimer - deltaTime);

            cooldown = Mathf.Max(0f, cooldown - deltaTime);
            spreadBloom = Mathf.Max(0f, spreadBloom - weapon.BloomRecovery * deltaTime);

            UpdateWeaponSwitch(deltaTime);
            UpdateReload(deltaTime);
            // UpdateMelee(deltaTime);

            HandleWeaponSwitchInput();

            if (IsSwitching)
            {
                UpdateViewModelMotion(deltaTime);
                UpdateMuzzleFlash();
                return;
            }

            // if ((Keyboard.current?.vKey.wasPressedThisFrame == true) && !IsMeleeing)
            // {
            //     StartMelee();
            // }
            //
            // if (IsMeleeing)
            // {
            //     UpdateViewModelMotion(deltaTime);
            //     UpdateMuzzleFlash();
            //     return;
            // }

            bool reloadPressed = reloadAction?.WasPressedThisFrame() ?? (Keyboard.current?.rKey.wasPressedThisFrame == true);

            if (reloadPressed)
            {
                StartReload();
            }

            bool wantsToFire = weapon.Automatic ? attackAction.IsPressed() : attackAction.WasPressedThisFrame();

            if (wantsToFire && cooldown <= 0f)
            {
                if (reloading && weapon.ReloadOneAtATime && ammunition > 0)
                {
                    CancelReload();
                }

                TryFire();
            }

            if (!reloading && weapon.AutoReloadWhenEmpty && ammunition <= 0 && reserveAmmunition > 0)
            {
                StartReload();
            }

            UpdateViewModelMotion(deltaTime);
            UpdateMuzzleFlash();
        }

        private void OnDisable()
        {
            StopReloadSound();
        }

        private void UpdateWeaponSwitch(float deltaTime)
        {
            if (weaponSwitchTimer <= 0f)
            {
                return;
            }

            weaponSwitchTimer = Mathf.Max(0f, weaponSwitchTimer - deltaTime);

            if (!weaponSwitchCommited && weaponSwitchTimer <= weaponSwitchDuration * 0.5f)
            {
                CommitWeaponSwitch();
            }
        }

        private void CancelReload()
        {
            reloading = false;
            reloadTimer = 0f;
            // reloadSpinHasHit = false;
            StopReloadSound();
        }

        private void DestroyViewModel()
        {
            if (viewModelInstance != null)
            {
                Destroy(viewModelInstance);
            }

            viewModelInstance = null;
            viewModelModel = null;
            muzzleSocket = null;
            muzzleFlashTransform = null;
            muzzleFlashRenderer = null;
            muzzleFlashTimer = 0f;

            if (viewModelRoot != null)
            {
                viewModelRoot.localPosition = Vector3.zero;
                viewModelRoot.localRotation = Quaternion.identity;
            }
        }

        private void InitializeWeaponInventory()
        {
            if ((weapons == null || weapons.Length == 0) && weapon != null)
            {
                weapons = new[] { weapon };
            }

            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            activeWeaponIndex = Mathf.Clamp(startingWeaponIndex, 0, weapons.Length - 1);

            if (weapon != null)
            {
                int configuredWeaponIndex = Array.IndexOf(weapons, weapon);

                if (configuredWeaponIndex >= 0)
                {
                    activeWeaponIndex = configuredWeaponIndex;
                }
            }

            if (weapons[activeWeaponIndex] == null)
            {
                activeWeaponIndex = FindFirstValidWeaponIdx();
            }

            if (activeWeaponIndex < 0)
            {
                weapon = null;
                return;
            }

            weapon = weapons[activeWeaponIndex];
            pendingWeaponIndex = activeWeaponIndex;
            weaponAmmunition = new int[weapons.Length];
            weaponReserveAmmunition = new int[weapons.Length];

            for (int i = 0; i < weapons.Length; i++)
            {
                WeaponDefinition inventoryWeapon = weapons[i];

                if (inventoryWeapon == null)
                {
                    continue;
                }

                weaponAmmunition[i] = inventoryWeapon.MagazineSize;
                weaponReserveAmmunition[i] = inventoryWeapon.ReserveAmmo;
            }
        }

        private int FindFirstValidWeaponIdx()
        {
            if (weapons == null)
            {
                return -1;
            }

            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    return i;
                }
            }

            return -1;
        }

        private void HandleWeaponSwitchInput()
        {
            if (IsSwitching || weapons == null || weapons.Length <= 1 || Keyboard.current == null)
            {
                return;
            }

            for (int i = 0; i < weapons.Length && i < WeaponNumberKeys.Length; i++)
            {
                if (Keyboard.current[WeaponNumberKeys[i]].wasPressedThisFrame)
                {
                    RequestWeaponSwitch(i);
                    return;
                }
            }

            if (Mouse.current == null)
            {
                return;
            }

            float scrollY = Mouse.current.scroll.ReadValue().y;

            if (scrollY > 0f)
            {
                SwitchWeaponOffset(-1);
            }
            else if (scrollY < 0f)
            {
                SwitchWeaponOffset(1);
            }
        }

        private void SwitchWeaponOffset(int offset)
        {
            if (weapons == null || weapons.Length == 0)
            {
                return;
            }

            for (int step = 1; step <= weapons.Length; step++)
            {
                int index = (activeWeaponIndex + offset * step + weapons.Length) % weapons.Length;

                if (weapons[index] != null)
                {
                    RequestWeaponSwitch(index);
                    return;
                }
            }
        }

        private void RequestWeaponSwitch(int index)
        {
            if (weapons == null
                || index < 0
                || index >= weapons.Length
                || weapons[index] == null
                || index == activeWeaponIndex)
            {
                return;
            }

            SaveActiveWeaponState();
            CancelReload();
            pendingWeaponIndex = index;
            weaponSwitchTimer = weaponSwitchDuration;
            weaponSwitchCommited = false;
            muzzleFlashTimer = 0f;
            muzzleFlashPreviewVisible = false;

            PlaySound(weaponSwitchSound);
        }

        private void CommitWeaponSwitch()
        {
            if (weapons == null
                || pendingWeaponIndex < 0
                || pendingWeaponIndex >= weapons.Length
                || weapons[pendingWeaponIndex] == null
                )
            {
                weaponSwitchCommited = true;
                return;
            }

            DestroyViewModel();

            activeWeaponIndex = pendingWeaponIndex;
            weapon = weapons[activeWeaponIndex];
            ammunition = weaponAmmunition[activeWeaponIndex];
            reserveAmmunition = weaponReserveAmmunition[activeWeaponIndex];

            cooldown = 0f;
            reloadTimer = 0f;
            spreadBloom = 0f;
            recoilTimer = weapon.Animation.recoilDuration;
            recoilAmount = 0f;
            reloadAmount = 0f;
            reloadSpinRotation = Vector3.zero;
            // meleeTimer = 0f;
            // meleeDuration = 0f;
            // meleeHasHit = false;
            // reloadSpinHasHit = false;

            CreateViewModel();
            weaponSwitchCommited = true;
            AmmunitionChanged?.Invoke();

        }

        private void SaveActiveWeaponState()
        {
            if (weaponAmmunition == null
                || weaponReserveAmmunition == null
                || activeWeaponIndex < 0
                || activeWeaponIndex >= weaponAmmunition.Length)
            {
                return;
            }

            weaponAmmunition[activeWeaponIndex] = ammunition;
            weaponReserveAmmunition[activeWeaponIndex] = reserveAmmunition;
        }

        private void TryFire()
        {
            if (reloading)
            {
                return;
            }

            if (ammunition <= 0)
            {
                StartReload();
                return;
            }

            ammunition--;
            cooldown = 1f / weapon.FireRate;
            recoilTimer = 0f;
            recoilAmount = 1f;

            muzzleFlashTimer = weapon.MuzzleFlashDuration;
            muzzleFlashRotation = UnityEngine.Random.Range(-25f, 25f);

            float spread = CalculateSpread();

            for (int pellet = 0; pellet < weapon.PelletCount; pellet++)
            {
                FireRay(CreateSpreadDirection(spread));
            }

            spreadBloom = Mathf.Min(weapon.MaximumBloom, spreadBloom + weapon.BloomPerShot);

            PlaySound(weapon.FireSound);
            NoiseSystem.Emit(
                viewCamera.transform.position,
                weapon.PelletCount > 1 ? loudShotNoiseRadius : quietShotNoiseRadius,
                gameObject);
            ShotFired?.Invoke(weapon);
            AmmunitionChanged?.Invoke();
        }

        private void FireRay(Vector3 direction)
        {
            Vector3 origin = viewCamera.transform.position;

            bool hitSomething = Physics.Raycast(origin, direction, out RaycastHit hit, weapon.Range, hitMask, QueryTriggerInteraction.Ignore);

            Vector3 end = hitSomething ? hit.point : origin + direction * weapon.Range;

            if (hitSomething)
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();

                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageInfo(weapon.Damage, hit.point, direction, gameObject));
                    PlaySound(enemyHitSound);
                }
            }

            if (drawDebugShots)
            {
                Debug.DrawLine(origin, end, hitSomething ? Color.green : Color.red, 0.8f);
            }
        }

        private Vector3 CreateSpreadDirection(float spreadDegrees)
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * Mathf.Tan(spreadDegrees * Mathf.Deg2Rad);

            Transform cameraTransform = viewCamera.transform;

            return (cameraTransform.forward + cameraTransform.right * offset.x + cameraTransform.up * offset.y).normalized;
        }

        private float CalculateSpread()
        {
            float spread = weapon.BaseSpread + spreadBloom;

            if (player.IsSprinting)
            {
                spread += weapon.SprintSpread;
            }
            else if (player.HorizontalSpeed > 1f)
            {
                spread += weapon.MovingSpread;
            }

            return spread;
        }

        private void StartReload()
        {
            if (reloading || ammunition >= weapon.MagazineSize || reserveAmmunition <= 0)
            {
                return;
            }
            reloading = true;
            reloadTimer = weapon.ReloadDuration;
            // reloadSpinHasHit = false;

            PlayReloadSound();
        }

        private void UpdateReload(float deltaTime)
        {
            if (!reloading)
            {
                return;
            }

            float previousElapsed = weapon.ReloadDuration - reloadTimer;
            reloadTimer = Mathf.Max(0f, reloadTimer - deltaTime);
            float currentElapsed = weapon.ReloadDuration - reloadTimer;

            // UpdateReloadSpinHit(previousElapsed, currentElapsed);

            if (reloadTimer > 0f)
            {
                return;
            }

            int needed = weapon.MagazineSize - ammunition;
            int loaded = weapon.ReloadOneAtATime ? Mathf.Min(1, reserveAmmunition) : Mathf.Min(needed, reserveAmmunition);

            ammunition += loaded;
            reserveAmmunition -= loaded;

            AmmunitionChanged?.Invoke();

            bool shouldContinueReloading = weapon.ReloadOneAtATime && ammunition < weapon.MagazineSize && reserveAmmunition > 0;

            if (shouldContinueReloading)
            {
                reloadTimer = weapon.ReloadDuration;
                // reloadSpinHasHit = false;
                PlayReloadSound();
                return;
            }

            reloading = false;
            // reloadSpinHasHit = false;
            StopReloadSound();
        }

        // private void StartMelee()
        // {
        //     CancelReload();
        //
        //     meleeDuration = weapon.MeleeWindupDuration + weapon.MeleeActiveDuration + weapon.MeleeRecoveryDuration;
        //     meleeTimer = meleeDuration;
        //     meleeHasHit = false;
        // }
        //
        // private void UpdateMelee(float deltaTime)
        // {
        //     if (!IsMeleeing)
        //     {
        //         return;
        //     }
        //
        //     float previousElapsed = meleeDuration - meleeTimer;
        //     meleeTimer = Mathf.Max(0f, meleeTimer - deltaTime);
        //     float elapsed = meleeDuration - meleeTimer;
        //     float activeStart = weapon.MeleeWindupDuration;
        //     float activeEnd = activeStart + weapon.MeleeActiveDuration;
        //
        //     if (!meleeHasHit && previousElapsed <= activeEnd && elapsed >= activeStart)
        //     {
        //         PerformMeleeHit(1f);
        //         meleeHasHit = true;
        //     }
        //
        //     if (meleeTimer <= 0f)
        //     {
        //         meleeDuration = 0f;
        //         meleeHasHit = false;
        //     }
        // }
        //
        // private void UpdateReloadSpinHit(float previousElapsed, float currentElapsed)
        // {
        //     if (!weapon.ReloadOneAtATime || reloadSpinHasHit)
        //     {
        //         return;
        //     }
        //
        //     float activeStart = weapon.MeleeWindupDuration;
        //     float activeEnd = activeStart + weapon.MeleeActiveDuration;
        //
        //     if (previousElapsed > activeEnd || currentElapsed < activeStart)
        //     {
        //         return;
        //     }
        //
        //     PerformMeleeHit(weapon.ReloadSpinKnockbackMultiplier);
        //     reloadSpinHasHit = true;
        // }
        //
        // private void PerformMeleeHit(float knockbackMultiplier)
        // {
        //     Vector3 origin = viewCamera.transform.position;
        //     Vector3 direction = viewCamera.transform.forward;
        //
        //     bool hitSomething = Physics.SphereCast(
        //         origin,
        //         weapon.MeleeRadius,
        //         direction,
        //         out RaycastHit hit,
        //         weapon.MeleeRange,
        //         hitMask,
        //         QueryTriggerInteraction.Ignore);
        //
        //     if (!hitSomething)
        //     {
        //         if (drawDebugShots)
        //         {
        //             Debug.DrawLine(origin, origin + direction * weapon.MeleeRange, Color.red, 0.35f);
        //         }
        //
        //         return;
        //     }
        //
        //     IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
        //
        //     if (damageable == null)
        //     {
        //         return;
        //     }
        //
        //     damageable.TakeDamage(new DamageInfo(weapon.MeleeDamage, hit.point, direction, gameObject));
        //
        //     EnemyController enemy = hit.collider.GetComponentInParent<EnemyController>();
        //
        //     if (enemy != null)
        //     {
        //         Vector3 knockbackDirection = enemy.transform.position - origin;
        //         enemy.ApplyKnockback(
        //             knockbackDirection,
        //             weapon.MeleeKnockbackImpulse * knockbackMultiplier,
        //             weapon.MeleeKnockbackLift);
        //     }
        //
        //     PlaySound(enemyHitSound);
        //
        //     if (drawDebugShots)
        //     {
        //         Debug.DrawLine(origin, hit.point, Color.green, 0.35f);
        //     }
        // }
        //
        private void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            audioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);

            audioSource.PlayOneShot(clip);
        }

        private void PlayReloadSound()
        {
            AudioClip clip = weapon.ReloadSound;

            if (clip == null)
            {
                return;
            }

            reloadAudioSource.Stop();
            reloadAudioSource.clip = clip;
            reloadAudioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
            reloadAudioSource.Play();
        }

        private void StopReloadSound()
        {
            if (reloadAudioSource == null)
            {
                return;
            }

            reloadAudioSource.Stop();
            reloadAudioSource.clip = null;
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;

            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private void UpdateViewModelMotion(float deltaTime)
        {
            if (viewModelRoot == null || viewModelInstance == null)
            {
                return;
            }

            ProceduralWeaponAnimationSettings motion = weapon.Animation;

            UpdateSway(motion, deltaTime);
            UpdateBob(motion, deltaTime);
            UpdateReloadMotion(motion, deltaTime);

            recoilTimer = Mathf.Min(motion.recoilDuration, recoilTimer + deltaTime);

            float recoilProgress = motion.recoilDuration > 0f
                ? recoilTimer / motion.recoilDuration
                : 1f;

            float recoilKick = RecoilKickCurve(recoilProgress) * recoilAmount;
            float recoilFollowThrough = RecoilFollowThroughCurve(recoilProgress) * recoilAmount;

            float bobX = Mathf.Sin(walkBobTimer) * motion.walkBobX * walkBobAmount;
            float bobY = Mathf.Abs(Mathf.Cos(walkBobTimer)) * motion.walkBobY * walkBobAmount;
            float idleBobY = Mathf.Sin(idleBobTimer) * motion.idleBobY * idleBobAmount;

            bobX += Mathf.Sin(walkBobTimer * motion.sprintBobSpeedScale)
                * motion.sprintBobX * sprintAmount;
            bobY += Mathf.Abs(Mathf.Cos(walkBobTimer * motion.sprintBobSpeedScale))
                * motion.sprintBobY * sprintAmount;

            // float meleeProgress = meleeDuration > 0f ? Mathf.Clamp01(1f - meleeTimer / meleeDuration) : 1f;
            // float meleePose = IsMeleeing ? Mathf.Sin(meleeProgress * Mathf.PI) : 0f;
            float switchPose = EaseInOutCubic(GetWeaponSwitchAmount());
            Vector3 switchOffset = weaponSwitchOffset * switchPose;
            Vector3 switchRotation = weaponSwitchRotation * switchPose;

            Vector3 position = weapon.HoldPosition + motion.sprintOffset * sprintAmount;
            position += switchOffset;
            // position += new Vector3(0.04f, -0.08f, -0.18f) * meleePose;
            position.x += swayPosition.x + bobX;
            position.y += swayPosition.y - bobY - idleBobY;
            position.x += -recoilKick * motion.recoilKickOffset.x
                + recoilFollowThrough * motion.recoilFollowThroughOffset.x;
            position.y += -recoilKick * motion.recoilKickOffset.y
                + recoilFollowThrough * motion.recoilFollowThroughOffset.y;
            position.z += recoilKick * motion.recoilKickOffset.z
                + recoilFollowThrough * motion.recoilFollowThroughOffset.z;

            Vector3 rotation = weapon.HoldRotation;
            rotation += new Vector3(
                swayRotation.y,
                swayRotation.x,
                swayRotation.x * motion.swayRollAmount);
            rotation += motion.recoilKickRotation * recoilKick
                + motion.recoilFollowThroughRotation * recoilFollowThrough;
            rotation += switchRotation;
            // rotation += new Vector3(-26f, 18f, -10f) * meleePose;
            rotation += reloadSpinRotation * EaseInOutCubic(reloadAmount);
            rotation += motion.sprintRotation * sprintAmount;

            viewModelRoot.localPosition = position;
            viewModelRoot.localRotation = Quaternion.Euler(rotation);
        }

        private float GetWeaponSwitchAmount()
        {
            if (weaponSwitchTimer <= 0f || weaponSwitchDuration <= 0f)
            {
                return 0f;
            }

            float time = 1f - weaponSwitchTimer / weaponSwitchDuration;

            if (time < 0.5f)
            {
                return time / 0.5f;
            }

            return 1f - (time - 0.5f) / 0.5f;
        }

        private void UpdateSway(ProceduralWeaponAnimationSettings motion, float deltaTime)
        {
            Vector2 mouseDelta = Vector2.zero;

            if (Cursor.lockState == CursorLockMode.Locked && Mouse.current != null)
            {
                mouseDelta = Mouse.current.delta.ReadValue();
            }

            Vector2 targetPosition = new(
                Mathf.Clamp(
                    -mouseDelta.x * motion.swayPositionAmount,
                    -motion.swayPositionClamp.x,
                    motion.swayPositionClamp.x),
                Mathf.Clamp(
                    -mouseDelta.y * motion.swayPositionAmount,
                    -motion.swayPositionClamp.y,
                    motion.swayPositionClamp.y));

            Vector2 targetRotation = new(
                Mathf.Clamp(
                    mouseDelta.x * motion.swayRotationAmount,
                    -motion.swayRotationClamp.x,
                    motion.swayRotationClamp.x),
                Mathf.Clamp(
                    -mouseDelta.y * motion.swayRotationAmount,
                    -motion.swayRotationClamp.y,
                    motion.swayRotationClamp.y));

            float follow = Mathf.Min(1f, deltaTime * motion.swayFollowSpeed);
            float settle = Mathf.Min(1f, deltaTime * motion.swaySettleSpeed);

            swayPosition = Vector2.Lerp(swayPosition, targetPosition, follow);
            swayRotation = Vector2.Lerp(swayRotation, targetRotation, follow);
            swayPosition = Vector2.Lerp(swayPosition, Vector2.zero, settle);
            swayRotation = Vector2.Lerp(swayRotation, Vector2.zero, settle);
        }

        private void UpdateBob(ProceduralWeaponAnimationSettings motion, float deltaTime)
        {
            const float referenceWalkSpeed = 4f;
            const float referenceSprintSpeed = referenceWalkSpeed * 1.45f;
            const float weaponBobRadiansPerUnit = 0.85f;

            float movementAmount = Mathf.Clamp01(player.HorizontalSpeed / referenceWalkSpeed);
            float sprintMotionAmount = player.IsSprinting
                ? Mathf.Clamp01(player.HorizontalSpeed / referenceSprintSpeed)
                : 0f;

            idleBobTimer += deltaTime * motion.idleBobSpeed;

            bool moving = player.HorizontalSpeed > 0.08f;
            bool idle = !moving && !player.IsSprinting && !reloading;
            float idleTarget = idle ? 1f : 0f;
            float idleEase = 1f - Mathf.Exp(-motion.idleBobEaseSpeed * deltaTime);
            idleBobAmount = Mathf.Lerp(idleBobAmount, idleTarget, idleEase);

            if (moving)
            {
                walkBobTimer += deltaTime * motion.walkBobSpeed * weaponBobRadiansPerUnit;
            }

            float walkEase = 1f - Mathf.Exp(-motion.walkBobEaseSpeed * deltaTime);
            walkBobAmount = Mathf.Lerp(
                walkBobAmount,
                moving ? movementAmount : 0f,
                walkEase);

            float sprintEase = 1f - Mathf.Exp(-motion.sprintEaseSpeed * deltaTime);
            sprintAmount = Mathf.Lerp(sprintAmount, sprintMotionAmount, sprintEase);
        }

        private void UpdateReloadMotion(ProceduralWeaponAnimationSettings motion, float deltaTime)
        {
            float targetAmount = reloading ? 1f : 0f;
            float easeSpeed = reloading
                ? motion.reloadSpinEaseInSpeed
                : motion.reloadSpinEaseOutSpeed;
            float ease = 1f - Mathf.Exp(-easeSpeed * deltaTime);

            reloadAmount = Mathf.Lerp(reloadAmount, targetAmount, ease);

            if (reloading)
            {
                float spinAmount = reloadAmount * reloadAmount;
                reloadSpinRotation += motion.reloadSpinDegreesPerSecond * (deltaTime * spinAmount);
                reloadSpinRotation.x = Mathf.Repeat(reloadSpinRotation.x, 360f);
                reloadSpinRotation.y = Mathf.Repeat(reloadSpinRotation.y, 360f);
                reloadSpinRotation.z = Mathf.Repeat(reloadSpinRotation.z, 360f);
            }
            else if (reloadAmount < 0.001f)
            {
                reloadAmount = 0f;
                reloadSpinRotation = Vector3.zero;
            }
        }

        private static float RecoilKickCurve(float time)
        {
            time = Mathf.Clamp01(time);

            if (time < 0.10f)
            {
                return EaseOutCubic(time / 0.10f) * 1.18f;
            }

            if (time < 0.58f)
            {
                float recovery = EaseInOutSine((time - 0.10f) / 0.48f);
                return Mathf.Lerp(1.18f, -0.16f, recovery);
            }

            float settle = EaseInOutSine((time - 0.58f) / 0.42f);
            return Mathf.Lerp(-0.16f, 0f, settle);
        }

        private static float RecoilFollowThroughCurve(float time)
        {
            time = Mathf.Clamp01(time);

            if (time < 0.08f)
            {
                return 0f;
            }

            if (time < 0.46f)
            {
                return EaseOutCubic((time - 0.08f) / 0.38f);
            }

            float recovery = EaseInOutSine((time - 0.46f) / 0.54f);
            return 1f - recovery;
        }

        private static float EaseOutCubic(float time)
        {
            time = Mathf.Clamp01(time);
            float inverse = 1f - time;
            return 1f - inverse * inverse * inverse;
        }

        private static float EaseInOutCubic(float time)
        {
            time = Mathf.Clamp01(time);

            if (time < 0.5f)
            {
                return 4f * time * time * time;
            }

            float value = -2f * time + 2f;
            return 1f - value * value * value * 0.5f;
        }

        private static float EaseInOutSine(float time)
        {
            time = Mathf.Clamp01(time);
            return -(Mathf.Cos(Mathf.PI * time) - 1f) * 0.5f;
        }

        private void CreateMuzzleFlash(GameObject weaponContainer)
        {
            muzzleSocket = new GameObject("MuzzleSocket").transform;
            muzzleSocket.SetParent(weaponContainer.transform, false);
            muzzleSocket.localPosition = weapon.MuzzlePosition;

            if (weapon.MuzzleFlashSprite == null)
            {
                return;
            }

            GameObject flash = new("MuzzleFlash");
            flash.transform.SetParent(muzzleSocket, false);

            muzzleFlashTransform = flash.transform;
            muzzleFlashRenderer = flash.AddComponent<SpriteRenderer>();
            muzzleFlashRenderer.sprite = weapon.MuzzleFlashSprite;
            muzzleFlashRenderer.color = new Color(weapon.MuzzleFlashColor.r, weapon.MuzzleFlashColor.g, weapon.MuzzleFlashColor.b, 0f);
            muzzleFlashRenderer.sortingOrder = 100;
            muzzleFlashRenderer.enabled = false;

            int viewModelLayer = LayerMask.NameToLayer("ViewModel");

            if (viewModelLayer >= 0)
            {
                flash.layer = viewModelLayer;
                muzzleSocket.gameObject.layer = viewModelLayer;
            }

            Vector2 spriteSize = weapon.MuzzleFlashSprite.bounds.size;

            muzzleFlashTransform.localScale = new Vector3(
                weapon.MuzzleFlashSize.x / Mathf.Max(spriteSize.x, 0.001f),
                weapon.MuzzleFlashSize.y / Mathf.Max(spriteSize.y, 0.001f), 1f);
        }

        private void UpdateMuzzleFlash()
        {
            if (muzzleFlashRenderer == null)
            {
                return;
            }

            bool timedFlashVisible = muzzleFlashTimer > 0f;
            bool visible = muzzleFlashPreviewVisible || timedFlashVisible;
            muzzleFlashRenderer.enabled = visible;

            if (!visible)
            {
                return;
            }

            float progress = muzzleFlashPreviewVisible
                ? 1f
                : Mathf.Clamp01(muzzleFlashTimer / weapon.MuzzleFlashDuration);

            float rotation = muzzleFlashPreviewVisible ? 0f : muzzleFlashRotation;
            muzzleFlashTransform.rotation = viewCamera.transform.rotation * Quaternion.Euler(0f, 0f, rotation);

            Color color = weapon.MuzzleFlashColor;
            color.a *= muzzleFlashPreviewVisible ? 0.65f : 0.95f * progress;
            muzzleFlashRenderer.color = color;

            SetMuzzleFlashScale(progress);
        }

        private void SetMuzzleFlashScale(float progress)
        {
            if (muzzleFlashTransform == null || weapon.MuzzleFlashSprite == null)
            {
                return;
            }

            Vector2 spriteSize = weapon.MuzzleFlashSprite.bounds.size;

            muzzleFlashTransform.localScale = new Vector3(
                weapon.MuzzleFlashSize.x / Mathf.Max(spriteSize.x, 0.001f)
                * progress,
                weapon.MuzzleFlashSize.y / Mathf.Max(spriteSize.y, 0.001f)
                * progress, 1f
                );
        }

        private void CreateViewModel()
        {
            if (weapon.ViewModelPrefab == null)
            {
                return;
            }

            if (viewModelRoot == null)
            {
                GameObject root = new("ViewModel");
                viewModelRoot = root.transform;
                viewModelRoot.SetParent(viewCamera.transform, false);
            }

            viewModelRoot.localPosition = weapon.HoldPosition;
            viewModelRoot.localRotation = Quaternion.Euler(weapon.HoldRotation);

            viewModelInstance = new GameObject(weapon.ViewModelPrefab.name);
            viewModelInstance.transform.SetParent(viewModelRoot, false);

            viewModelModel = Instantiate(weapon.ViewModelPrefab, viewModelInstance.transform);

            viewModelModel.name = "Model";
            viewModelModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            viewModelModel.transform.localScale = weapon.ViewModelScale;

            GameObject instance = viewModelInstance;

            CreateMuzzleFlash(instance);

            int viewModelLayer = LayerMask.NameToLayer("ViewModel");

            if (viewModelLayer < 0)
            {
                Debug.LogError("The ViewModel layer does not exist.", this);
            }
            else
            {
                SetLayerRecursively(instance, viewModelLayer);
            }

            if (weapon.ViewModelMaterial != null)
            {
                foreach (Renderer modelRenderer in viewModelModel.GetComponentsInChildren<Renderer>())
                {
                    modelRenderer.sharedMaterial = weapon.ViewModelMaterial;
                }
            }

            recoilTimer = weapon.Animation.recoilDuration;
        }
    }
}
