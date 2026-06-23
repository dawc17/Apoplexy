using System;
using Apoplexy.Combat;
using Apoplexy.Player;
using UnityEngine;
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

        [Header("Collision")]
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private bool drawDebugShots = true;

        private InputAction attackAction;
        private InputAction reloadAction;
        private AudioSource audioSource;
        private GameObject viewModelInstance;

        private int ammunition;
        private int reserveAmmunition;
        private float cooldown;
        private float reloadTimer;
        private float spreadBloom;
        private bool reloading;

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

        private GameObject viewModelModel;
        private Transform muzzleSocket;
        private Transform muzzleFlashTransform;
        private SpriteRenderer muzzleFlashRenderer;

        private float muzzleFlashTimer;
        private float muzzleFlashRotation;
        private bool muzzleFlashPreviewVisible;

        public event Action AmmunitionChanged;

        public int Ammunition => ammunition;
        public int ReserveAmmunition => reserveAmmunition;
        public bool IsReloading => reloading;
        public WeaponDefinition Weapon => weapon;
        public Transform MuzzleSocket => muzzleSocket;

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

            if (viewCamera == null)
            {
                viewCamera = GetComponentInParent<Camera>();
            }

            if (player == null)
            {
                player = GetComponentInParent<FirstPersonController>();
            }

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

            ammunition = weapon.MagazineSize;
            reserveAmmunition = weapon.ReserveAmmo;

            CreateViewModel();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            muzzleFlashTimer = Mathf.Max(0f, muzzleFlashTimer - deltaTime);

            cooldown = Mathf.Max(0f, cooldown - deltaTime);
            spreadBloom = Mathf.Max(0f, spreadBloom - weapon.BloomRecovery * deltaTime);

            UpdateReload(deltaTime);

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

        private void CancelReload()
        {
            reloading = false;
            reloadTimer = 0f;
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

                damageable?.TakeDamage(new DamageInfo(weapon.Damage, hit.point, direction, gameObject));
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

            PlaySound(weapon.ReloadSound);
        }

        private void UpdateReload(float deltaTime)
        {
            if (!reloading)
            {
                return;
            }

            reloadTimer = Mathf.Max(0f, reloadTimer - deltaTime);

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
                PlaySound(weapon.ReloadSound);
                return;
            }

            reloading = false;
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            audioSource.pitch = UnityEngine.Random.Range(0.96f, 1.04f);

            audioSource.PlayOneShot(clip);
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

            Vector3 position = weapon.HoldPosition + motion.sprintOffset * sprintAmount;
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
            rotation += reloadSpinRotation * EaseInOutCubic(reloadAmount);
            rotation += motion.sprintRotation * sprintAmount;

            viewModelRoot.localPosition = position;
            viewModelRoot.localRotation = Quaternion.Euler(rotation);
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
