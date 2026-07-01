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
        [SerializeField] private TMP_Text healthLabelText;
        [SerializeField] private Graphic damageVignette;
        [SerializeField] private Graphic crouchVignette;
        [SerializeField] private GameObject stateOverlay;
        [SerializeField] private TMP_Text stateTitleText;
        [SerializeField] private TMP_Text statePromptText;
        [SerializeField] private WeaponHud weaponHud;
        [SerializeField] private HealthHud healthHud;
        [SerializeField] private AwarenessHud awarenessHud;

        [Header("Typography")]
        [SerializeField] private Font terminalSourceFont;
        [SerializeField] private Font japaneseSourceFont;

        [Header("Formatting")]
        [SerializeField] private string healthLabel = "生命";
        [SerializeField] private string deadTitle = "SIGNAL LOST";
        [SerializeField] private string winTitle = "you! !!won";
        [SerializeField] private string restartPrompt = "r to reboot or sum";

        [Header("Colors")]
        [SerializeField] private Color textColor = new(0.92f, 0.92f, 0.88f, 1f);
        [SerializeField] private Color dimTextColor = new(0.52f, 0.52f, 0.49f, 1f);
        [SerializeField] private Color dangerColor = new(0.82f, 0.09f, 0.11f, 1f);
        [SerializeField] private Color damageVignetteColor = new(0.38f, 0f, 0f, 0.42f);
        [SerializeField] private Color crouchVignetteColor = new(0f, 0f, 0f, 0.14f);

        private const string TerminalFontResourcePath = "Fonts/AdwaitaMono-Regular";
        private const string JapaneseFontResourcePath = "Fonts/NotoSansJP-Regular";

        private TMP_FontAsset terminalFontAsset;
        private TMP_FontAsset japaneseFontAsset;
        private bool typographyApplied;

        private void Awake()
        {
            ApplyTypography();
            BindMissingReferences();
        }

        private void OnEnable()
        {
            ApplyTypography();
            BindMissingReferences();
            BindChildHuds();
            RefreshSessionUi();
        }

        private void OnDisable()
        {
            UnbindChildHuds();
        }

        private void Update()
        {
            BindMissingReferences();
            BindChildHuds();
            RefreshSessionUi();
        }

        private void BindChildHuds()
        {
            if (weaponHud != null)
            {
                weaponHud.Bind(weaponController);
            }
            if (awarenessHud != null)
            {
                awarenessHud.Bind(gameSession);
            }
            if (healthHud != null)
            {
                healthHud.Bind(playerHealth);
            }
        }

        private void UnbindChildHuds()
        {
            if (weaponHud != null)
            {
                weaponHud.Bind(null);
            }

            if (awarenessHud != null)
            {
                awarenessHud.Bind(null);
            }

            if (healthHud != null)
            {
                healthHud.Bind(null);
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

        private void RefreshSessionUi()
        {
            if (gameSession == null)
            {
                SetAlpha(damageVignette, 0f);
                SetAlpha(crouchVignette, 0f);
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

            SetFont(stateTitleText, terminalFontAsset);
            SetFont(statePromptText, terminalFontAsset);
            SetFont(healthLabelText, japaneseFontAsset != null ? japaneseFontAsset : terminalFontAsset);

            if (awarenessHud != null)
            {
                awarenessHud.ApplyTypography(terminalFontAsset, japaneseFontAsset != null ? japaneseFontAsset : terminalFontAsset);
            }

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
