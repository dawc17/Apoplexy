using Apoplexy.Core;
using Apoplexy.Player;
using Apoplexy.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
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
        [SerializeField] private TMP_Text healthLabelText;
        [SerializeField] private TMP_Text awarenessAccentText;
        [SerializeField] private TMP_Text awarenessText;
        [SerializeField] private Graphic awarenessPanel;
        [SerializeField] private Graphic damageVignette;
        [SerializeField] private Graphic crouchVignette;
        [SerializeField] private GameObject stateOverlay;
        [SerializeField] private TMP_Text stateTitleText;
        [SerializeField] private TMP_Text statePromptText;
        [SerializeField] private HealthHud healthHud;

        [Header("Typography")]
        [SerializeField] private Font terminalSourceFont;
        [SerializeField] private Font japaneseSourceFont;

        [Header("Formatting")]
        [SerializeField] private string ammoFormat = "{0:00} / {1:000}";
        [SerializeField] private string healthLabel = "生命";
        [SerializeField] private string reloadText = "RELOADING";
        [SerializeField] private string weaponReadyText = "SLOT {0}/{1}";
        [SerializeField] private string deadTitle = "SIGNAL LOST";
        [SerializeField] private string winTitle = "you! !!won";
        [SerializeField] private string restartPrompt = "r to reboot or sum";

        [Header("Layout")]
        [SerializeField] private float awarenessHorizontalPadding = 36f;
        [SerializeField] private float awarenessTextGap = 34f;
        [SerializeField] private float awarenessMinWidth = 220f;

        [Header("Colors")]
        [SerializeField] private Color textColor = new(0.92f, 0.92f, 0.88f, 1f);
        [SerializeField] private Color dimTextColor = new(0.52f, 0.52f, 0.49f, 1f);
        [SerializeField] private Color panelColor = new(0f, 0f, 0f, 0.77f);
        [SerializeField] private Color dangerColor = new(0.82f, 0.09f, 0.11f, 1f);
        [SerializeField] private Color damageVignetteColor = new(0.38f, 0f, 0f, 0.42f);
        [SerializeField] private Color crouchVignetteColor = new(0f, 0f, 0f, 0.14f);

        private const string TerminalFontResourcePath = "Fonts/AdwaitaMono-Regular";
        private const string JapaneseFontResourcePath = "Fonts/NotoSansJP-Regular";

        private TMP_FontAsset terminalFontAsset;
        private TMP_FontAsset japaneseFontAsset;
        private bool typographyApplied;
        private bool subscribedToWeapon;

        private void Awake()
        {
            ApplyTypography();
            BindMissingReferences();
        }

        private void OnEnable()
        {
            ApplyTypography();
            BindMissingReferences();
            Subscribe();
            Refresh();
            BindChildHuds();
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
            RefreshStatus();
            RefreshSessionUi();
        }

        private void BindChildHuds()
        {
            if (healthHud != null)
            {
                healthHud.Bind(playerHealth);
            }
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
        }

        private void Unsubscribe()
        {
            if (subscribedToWeapon && weaponController != null)
            {
                weaponController.AmmunitionChanged -= Refresh;
            }

            subscribedToWeapon = false;
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
                ResizeAwarenessPanel();

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

        private void ResizeAwarenessPanel()
        {
            if (awarenessPanel == null || awarenessAccentText == null || awarenessText == null)
            {
                return;
            }

            RectTransform panelRect = awarenessPanel.rectTransform;
            RectTransform accentRect = awarenessAccentText.rectTransform;
            RectTransform textRect = awarenessText.rectTransform;

            awarenessAccentText.ForceMeshUpdate();
            awarenessText.ForceMeshUpdate();

            float accentWidth = Mathf.Ceil(awarenessAccentText.GetPreferredValues().x);
            float textWidth = Mathf.Ceil(awarenessText.GetPreferredValues().x);
            float contentWidth = accentWidth + awarenessTextGap + textWidth;
            float panelWidth = Mathf.Max(awarenessMinWidth, contentWidth + awarenessHorizontalPadding * 2f);

            Vector2 panelSize = panelRect.sizeDelta;
            panelSize.x = panelWidth;
            panelRect.sizeDelta = panelSize;

            accentRect.anchorMin = new Vector2(0f, 0.5f);
            accentRect.anchorMax = new Vector2(0f, 0.5f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.anchoredPosition = new Vector2(awarenessHorizontalPadding, 0f);
            accentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, accentWidth);

            textRect.anchorMin = new Vector2(0f, 0.5f);
            textRect.anchorMax = new Vector2(0f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = new Vector2(awarenessHorizontalPadding + accentWidth + awarenessTextGap, 0f);
            textRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
        }

        private static string AwarenessAccent(string label)
        {
            return label switch
            {
                "DISCOVERED" => "警告",
                "SPOTTED" => "視認",
                "SEEN?" => "検知",
                _ => "聴音",
            };
        }

        private void ApplyTypography()
        {
            if (typographyApplied)
            {
                return;
            }

            terminalSourceFont ??= Resources.Load<Font>(TerminalFontResourcePath);
            japaneseSourceFont ??= Resources.Load<Font>(JapaneseFontResourcePath);

            terminalFontAsset ??= CreateRuntimeFontAsset(terminalSourceFont);
            japaneseFontAsset ??= CreateRuntimeFontAsset(japaneseSourceFont);

            if (terminalFontAsset == null && japaneseFontAsset == null)
            {
                return;
            }

            foreach (TMP_Text text in GetComponentsInChildren<TMP_Text>(true))
            {
                TMP_FontAsset targetFont = ContainsCjk(text.text) && japaneseFontAsset != null
                    ? japaneseFontAsset
                    : terminalFontAsset;

                SetFont(text, targetFont);
            }

            SetFont(weaponModelText, terminalFontAsset);
            SetFont(ammoText, terminalFontAsset);
            SetFont(weaponStatusText, terminalFontAsset);
            SetFont(awarenessText, terminalFontAsset);
            SetFont(stateTitleText, terminalFontAsset);
            SetFont(statePromptText, terminalFontAsset);
            SetFont(healthLabelText, japaneseFontAsset != null ? japaneseFontAsset : terminalFontAsset);
            SetFont(awarenessAccentText, japaneseFontAsset != null ? japaneseFontAsset : terminalFontAsset);

            if (japaneseFontAsset != null)
            {
                SetText(healthLabelText, healthLabel);
            }

            typographyApplied = true;
        }

        private static TMP_FontAsset CreateRuntimeFontAsset(Font sourceFont)
        {
            if (sourceFont == null)
            {
                return null;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                sourceFont,
                96,
                9,
                GlyphRenderMode.SDFAA,
                2048,
                2048,
                AtlasPopulationMode.Dynamic,
                true);

            fontAsset.name = sourceFont.name + " TMP Runtime";
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            fontAsset.hideFlags = HideFlags.DontSave;
            return fontAsset;
        }

        private static bool ContainsCjk(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return false;
            }

            foreach (char character in value)
            {
                if (character is >= '\u3040' and <= '\u30ff'
                    or >= '\u3400' and <= '\u9fff')
                {
                    return true;
                }
            }

            return false;
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetFont(TMP_Text text, TMP_FontAsset fontAsset)
        {
            if (text != null && fontAsset != null && text.font != fontAsset)
            {
                text.font = fontAsset;
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
