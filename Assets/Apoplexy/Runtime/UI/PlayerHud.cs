using Apoplexy.Core;
using Apoplexy.Player;
using Apoplexy.Weapons;
using TMPro;
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
        [SerializeField] private TMP_Text weaponModelText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text weaponStatusText;
        [SerializeField] private Image reloadProgressFill;
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image healthFill;
        [SerializeField] private TMP_Text awarenessAccentText;
        [SerializeField] private TMP_Text awarenessText;
        [SerializeField] private Graphic awarenessPanel;
        [SerializeField] private Graphic damageVignette;
        [SerializeField] private Graphic crouchVignette;
        [SerializeField] private GameObject stateOverlay;
        [SerializeField] private TMP_Text stateTitleText;
        [SerializeField] private TMP_Text statePromptText;

        [Header("Formatting")]
        [SerializeField] private string ammoFormat = "{0:00} / {1:000}";
        [SerializeField] private string healthFormat = "{0:000}";
        [SerializeField] private string reloadText = "RELOADING";
        [SerializeField] private string weaponReadyText = "SLOT {0}/{1}";
        [SerializeField] private string deadTitle = "SIGNAL LOST";
        [SerializeField] private string winTitle = "you! !!won";
        [SerializeField] private string restartPrompt = "r to reboot or sum";

        [Header("Colors")]
        [SerializeField] private Color textColor = new(0.92f, 0.92f, 0.88f, 1f);
        [SerializeField] private Color dimTextColor = new(0.52f, 0.52f, 0.49f, 1f);
        [SerializeField] private Color panelColor = new(0f, 0f, 0f, 0.77f);
        [SerializeField] private Color dangerColor = new(0.82f, 0.09f, 0.11f, 1f);
        [SerializeField] private Color damageVignetteColor = new(0.38f, 0f, 0f, 0.42f);
        [SerializeField] private Color crouchVignetteColor = new(0f, 0f, 0f, 0.14f);

        private bool subscribedToWeapon;
        private bool subscribedToHealth;

        private void Awake()
        {
            BindMissingReferences();
        }

        private void OnEnable()
        {
            BindMissingReferences();
            Subscribe();
            Refresh();
            RefreshHealth();
            RefreshSessionUi();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            BindMissingReferences();
            Subscribe();
            RefreshHealth();
            RefreshStatus();
            RefreshSessionUi();
        }

        private void BindMissingReferences()
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

        private void Subscribe()
        {
            if (!subscribedToWeapon && weaponController != null)
            {
                weaponController.AmmunitionChanged += Refresh;
                subscribedToWeapon = true;
            }

            if (!subscribedToHealth && playerHealth != null)
            {
                playerHealth.HealthChanged += OnHealthChanged;
                subscribedToHealth = true;
            }
        }

        private void Unsubscribe()
        {
            if (subscribedToWeapon && weaponController != null)
            {
                weaponController.AmmunitionChanged -= Refresh;
            }

            if (subscribedToHealth && playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
            }

            subscribedToWeapon = false;
            subscribedToHealth = false;
        }

        private void Refresh()
        {
            if (weaponController == null || weaponController.Weapon == null)
            {
                SetText(weaponModelText, string.Empty);
                SetText(ammoText, string.Empty);
                SetText(weaponStatusText, string.Empty);
                SetFill(reloadProgressFill, 0f);
                return;
            }

            SetText(weaponModelText, weaponController.Weapon.DisplayName);
            SetText(ammoText, string.Format(ammoFormat, weaponController.Ammunition, weaponController.ReserveAmmunition));
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
                float pulse = Mathf.PingPong(Time.unscaledTime * 32f, 1f);
                healthFill.color = playerHealth.Health <= 30
                    ? Color.Lerp(new Color(dangerColor.r, dangerColor.g, dangerColor.b, 0.72f), dangerColor, pulse)
                    : new Color(textColor.r, textColor.g, textColor.b, 0.88f);
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
                SetText(weaponStatusText, string.Empty);
                SetFill(reloadProgressFill, 0f);
                return;
            }

            if (weaponController.IsReloading)
            {
                SetText(weaponStatusText, reloadText);
                SetFill(reloadProgressFill, weaponController.ReloadProgress);
                return;
            }

            SetFill(reloadProgressFill, 0f);
            SetText(weaponStatusText, string.Format(
                weaponReadyText,
                Mathf.Max(1, weaponController.ActiveWeaponIndex + 1),
                Mathf.Max(1, weaponController.WeaponCount)));
        }

        private void RefreshSessionUi()
        {
            if (gameSession == null)
            {
                SetAlpha(damageVignette, 0f);
                SetAlpha(crouchVignette, 0f);
                SetActive(awarenessPanel != null ? awarenessPanel.gameObject : null, false);
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
            SetActive(awarenessPanel != null ? awarenessPanel.gameObject : null, hasAwareness);

            if (hasAwareness)
            {
                Color awarenessColor = gameSession.AwarenessText == "DISCOVERED"
                    ? Color.Lerp(new Color(dangerColor.r, dangerColor.g, dangerColor.b, 0.7f), dangerColor, Mathf.PingPong(Time.unscaledTime * 12f, 1f))
                    : gameSession.AwarenessColor;

                SetText(awarenessText, gameSession.AwarenessText);
                SetText(awarenessAccentText, AwarenessAccent(gameSession.AwarenessText));
                SetTextColor(awarenessText, awarenessColor);
                SetTextColor(awarenessAccentText, new Color(awarenessColor.r, awarenessColor.g, awarenessColor.b, 0.72f));

                if (awarenessPanel != null)
                {
                    awarenessPanel.color = panelColor;
                }
            }

            bool showState = gameSession.State is GameState.Dead or GameState.Win;
            SetActive(stateOverlay, showState);

            if (showState)
            {
                bool dead = gameSession.State == GameState.Dead;
                SetText(stateTitleText, dead ? deadTitle : winTitle);
                SetText(statePromptText, restartPrompt);
                SetTextColor(stateTitleText, dead ? dangerColor : textColor);
            }
        }

        private static string AwarenessAccent(string label)
        {
            return label switch
            {
                "DISCOVERED" => "ALERT",
                "SPOTTED" => "SEEN",
                "SEEN?" => "TRACE",
                _ => "SOUND",
            };
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetTextColor(TMP_Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }

        private static void SetFill(Image image, float value)
        {
            if (image != null)
            {
                float fill = Mathf.Clamp01(value);
                image.fillAmount = fill;

                RectTransform rectTransform = image.rectTransform;
                Vector3 scale = rectTransform.localScale;
                scale.x = fill;
                rectTransform.localScale = scale;
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
