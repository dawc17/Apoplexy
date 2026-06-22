using UnityEngine;

namespace Apoplexy.Weapons
{
    [CreateAssetMenu(
        fileName = "WeaponDefinition",
        menuName = "Apoplexy/Weapon Definition"
        )]
    public sealed class WeaponDefinition : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string displayName = "Weapon";

        [Header("Firing")]
        [SerializeField, Min(1)] private int damage = 15;
        [SerializeField, Min(0.1f)] private float range = 100f;
        [SerializeField, Min(0.01f)] private float fireRate = 6f;
        [SerializeField] private bool automatic;
        [SerializeField, Min(1)] private int pelletCount = 1;

        [Header("Spread")]
        [SerializeField, Min(0f)] private float baseSpread = 0.15f;
        [SerializeField, Min(0f)] private float movingSpread = 0.45f;
        [SerializeField, Min(0f)] private float sprintSpread = 0.95f;
        [SerializeField, Min(0f)] private float bloomPerShot = 0.25f;
        [SerializeField, Min(0f)] private float maximumBloom = 1.25f;
        [SerializeField, Min(0f)] private float bloomRecovery = 8f;

        [Header("Ammunition")]
        [SerializeField, Min(1)] private int magazineSize = 12;
        [SerializeField, Min(0)] private int reserveAmmo = 48;
        [SerializeField, Min(0.01f)] private float reloadDuration = 1.8f;
        [SerializeField] private bool autoReloadWhenEmpty = true;

        [Header("Presentation")]
        [SerializeField] private GameObject viewModelPrefab;
        [SerializeField] private Material viewModelMaterial;
        [SerializeField] private Vector3 holdPosition = new(0.247f, 0.147f, -0.381f);

        [SerializeField] private Vector3 holdRotation = new(2.8f, 7.4f, 1.9f);

        [SerializeField] private Vector3 viewModelScale = Vector3.one * 0.104f;
        [SerializeField] private ProceduralWeaponAnimationSettings animation = new();

        [SerializeField] private AudioClip fireSound;
        [SerializeField] private AudioClip reloadSound;

        public string DisplayName => displayName;
        public int Damage => damage;
        public float Range => range;
        public float FireRate => fireRate;
        public bool Automatic => automatic;
        public int PelletCount => pelletCount;
        public float BaseSpread => baseSpread;
        public float MovingSpread => movingSpread;
        public float SprintSpread => sprintSpread;
        public float BloomPerShot => bloomPerShot;
        public float MaximumBloom => maximumBloom;
        public float BloomRecovery => bloomRecovery;
        public int MagazineSize => magazineSize;
        public int ReserveAmmo => reserveAmmo;
        public float ReloadDuration => reloadDuration;
        public bool AutoReloadWhenEmpty => autoReloadWhenEmpty;
        public GameObject ViewModelPrefab => viewModelPrefab;
        public Material ViewModelMaterial => viewModelMaterial;
        public Vector3 HoldPosition => holdPosition;
        public Vector3 HoldRotation => holdRotation;
        public Vector3 ViewModelScale => viewModelScale;
        public ProceduralWeaponAnimationSettings Animation => animation;
        public AudioClip FireSound => fireSound;
        public AudioClip ReloadSound => reloadSound;
    }
}
