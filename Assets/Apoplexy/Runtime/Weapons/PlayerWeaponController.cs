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

        public event Action AmmunitionChanged;

        public int Ammunition => ammunition;
        public int ReserveAmmunition => reserveAmmunition;
        public bool IsReloading => reloading;
        public WeaponDefinition Weapon => weapon;

        public float ReloadProgress => reloading ? 1f - reloadTimer / weapon.ReloadDuration : 0f;

        public void ApplyViewModelPose()
        {
            if (viewModelRoot == null || viewModelInstance == null)
            {
                return;
            }

            viewModelRoot.localPosition = weapon.HoldPotision;
            viewModelRoot.localRotation = Quaternion.Euler(weapon.HoldRotation);

            viewModelInstance.transform.localScale = weapon.ViewModelScale;
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
                TryFire();
            }

            if (!reloading && weapon.AutoReloadWhenEmpty && ammunition <= 0 && reserveAmmunition > 0)
            {
                StartReload();
            }
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
            int loaded = Mathf.Min(needed, reserveAmmunition);

            ammunition += loaded;
            reserveAmmunition -= loaded;
            reloading = false;

            AmmunitionChanged?.Invoke();
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

            viewModelRoot.localPosition = weapon.HoldPotision;
            viewModelRoot.localRotation = Quaternion.Euler(weapon.HoldRotation);

            viewModelInstance = Instantiate(weapon.ViewModelPrefab, viewModelRoot);

            GameObject instance = viewModelInstance;

            int viewModelLayer = LayerMask.NameToLayer("ViewModel");

            if (viewModelLayer < 0)
            {
                Debug.LogError("The ViewModel layer does not exist.", this);
            }
            else
            {
                SetLayerRecursively(instance, viewModelLayer);
            }

            instance.name = weapon.ViewModelPrefab.name;
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            instance.transform.localScale = weapon.ViewModelScale;

            if (weapon.ViewModelMaterial != null)
            {
                foreach (Renderer modelRenderer in instance.GetComponentsInChildren<Renderer>())
                {
                    modelRenderer.sharedMaterial = weapon.ViewModelMaterial;
                }
            }
        }
    }
}
