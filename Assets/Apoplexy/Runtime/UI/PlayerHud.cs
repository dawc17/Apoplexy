using Apoplexy.Core;
using Apoplexy.Player;
using Apoplexy.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Apoplexy.UI
{
    public sealed class PlayerHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private GameSession gameSession;
        [SerializeField] private Text weaponText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text statusText;
        [SerializeField] private Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private Text awarenessText;
        [SerializeField] private Graphic awarenessPanel;
        [SerializeField] private Graphic damageVignette;
        [SerializeField] private Graphic crouchVignette;
        [SerializeField] private GameObject stateOverlay;
        [SerializeField] private Text stateTitleText;
        [SerializeField] private Text statePromptText;
        [SerializeField] private Graphic crosshair;

        [Header("Formatting")]
        [SerializeField] private string ammoFormat = "{0:000} / {1:000}";
        [SerializeField] private string weaponFormat = "{0}";
        [SerializeField] private string healthFormat = "{0:000}";
        [SerializeField] private string reloadText = "RELOADING";
        [SerializeField] private string deadTitle = "SIGNAL LOST";
        [SerializeField] private string winTitle = "you! !!won";
        [SerializeField] private string restartPrompt = "r to reboot or sum";

        [Header("Colors")]
        [SerializeField] private Color normalColor = new(0.92f, 0.92f, 0.88f, 1f);
        [SerializeField] private Color dangerColor = new(0.82f, 0.09f, 0.11f, 1f);
        [SerializeField] private Color damageVignetteColor = new(0.38f, 0f, 0f, 0.42f);
        [SerializeField] private Color crouchVignetteColor = new(0f, 0f, 0f, 0.14f);

        private void Awake()
        {
            if (weaponController == null)
            {
                weaponController = FindAnyObjectByType<PlayerWeaponController>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }

            if (playerController == null)
            {
                playerController = FindAnyObjectByType<FirstPersonController>();
            }

            if (gameSession == null)
            {
                gameSession = FindAnyObjectByType<GameSession>();
            }
        }

        private void OnEnable()
        {
            if (weaponController != null)
            {
                weaponController.AmmunitionChanged += Refresh;
            }

            if (playerHealth != null)
            {
                playerHealth.HealthChanged += OnHealthChanged;
            }

            Refresh();
            RefreshHealth();
        }

        private void OnDisable()
        {
            if (weaponController != null)
            {
                weaponController.AmmunitionChanged -= Refresh;
            }

            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
            }
        }

        private void Update()
        {
            BindMissingReferences();
            RefreshStatus();
            RefreshSessionUi();
        }

        private void BindMissingReferences()
        {
            if (weaponController == null)
            {
                weaponController = FindAnyObjectByType<PlayerWeaponController>();

                if (weaponController != null)
                {
                    weaponController.AmmunitionChanged += Refresh;
                    Refresh();
                }
            }

            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.HealthChanged += OnHealthChanged;
                    RefreshHealth();
                }
            }

            if (playerController == null)
            {
                playerController = FindAnyObjectByType<FirstPersonController>();
            }

            if (gameSession == null)
            {
                gameSession = FindAnyObjectByType<GameSession>();
            }
        }

        private void Refresh()
        {
            if (weaponController == null || weaponController.Weapon == null)
            {
                SetText(weaponText, string.Empty);
                SetText(ammoText, string.Empty);

                if (crosshair != null)
                {
                    crosshair.enabled = false;
                }
                return;
            }

            SetText(weaponText, string.Format(weaponFormat, weaponController.Weapon.DisplayName));

            SetText(ammoText, string.Format(ammoFormat, weaponController.Ammunition, weaponController.ReserveAmmunition));

            if (crosshair != null)
            {
                crosshair.enabled = true;
            }

            RefreshStatus();
        }

        private void RefreshHealth()
        {
            if (playerHealth == null)
            {
                SetText(healthText, string.Empty);
                SetFill(healthFill, 0f);
                return;
            }

            SetText(healthText, string.Format(healthFormat, playerHealth.Health));

            float healthPercent = playerHealth.MaxHealth > 0
                ? Mathf.Clamp01((float)playerHealth.Health / playerHealth.MaxHealth)
                : 0f;

            SetFill(healthFill, healthPercent);

            if (healthFill != null)
            {
                healthFill.color = playerHealth.Health <= 30 ? dangerColor : normalColor;
            }
        }

        private void OnHealthChanged(int currentHealth, int maxHealth)
        {
            RefreshHealth();
        }

        private void RefreshStatus()
        {
            if (weaponController == null)
            {
                SetText(statusText, string.Empty);
                return;
            }

            if (weaponController.IsReloading)
            {
                SetText(statusText, reloadText);
                return;
            }

            SetText(statusText, string.Empty);
        }

        private void RefreshSessionUi()
        {
            if (gameSession == null)
            {
                SetAlpha(damageVignette, 0f);
                SetActive(stateOverlay, false);
                return;
            }

            SetAlpha(damageVignette, gameSession.DamageVignetteAmount * damageVignetteColor.a, damageVignetteColor);
            SetAlpha(
                crouchVignette,
                playerHealth != null
                    && !playerHealth.IsDead
                    && playerController != null
                    && playerController.IsCrouching
                    ? crouchVignetteColor.a
                    : 0f,
                crouchVignetteColor);

            bool hasAwareness = !string.IsNullOrEmpty(gameSession.AwarenessText);
            SetText(awarenessText, gameSession.AwarenessText);
            SetActive(awarenessText != null ? awarenessText.gameObject : null, hasAwareness);

            if (awarenessText != null)
            {
                awarenessText.color = gameSession.AwarenessColor;
            }

            if (awarenessPanel != null)
            {
                awarenessPanel.gameObject.SetActive(hasAwareness);
                awarenessPanel.color = new Color(
                    gameSession.AwarenessColor.r,
                    gameSession.AwarenessColor.g,
                    gameSession.AwarenessColor.b,
                    awarenessPanel.color.a);
            }

            bool showState = gameSession.State is GameState.Dead or GameState.Win;
            SetActive(stateOverlay, showState);

            if (showState)
            {
                bool dead = gameSession.State == GameState.Dead;
                SetText(stateTitleText, dead ? deadTitle : winTitle);
                SetText(statePromptText, restartPrompt);
            }
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetFill(Image image, float value)
        {
            if (image != null)
            {
                image.fillAmount = value;
            }
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private static void SetAlpha(Graphic graphic, float alpha, Color baseColor)
        {
            if (graphic == null)
            {
                return;
            }

            baseColor.a = alpha;
            graphic.color = baseColor;
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }
    }
}
