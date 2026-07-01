using Apoplexy.Core;
using Apoplexy.Player;
using Apoplexy.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Apoplexy.UI
{
    public sealed class PlayerHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private FirstPersonController playerController;
        [SerializeField] private GameSession gameSession;
        [SerializeField] private WeaponHud weaponHud;
        [SerializeField] private HealthHud healthHud;
        [SerializeField] private AwarenessHud awarenessHud;
        [SerializeField] private SessionStateHud sessionStateHud;

        [Header("Typography")]
        [SerializeField] private Font terminalSourceFont;
        [SerializeField] private Font japaneseSourceFont;

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
        }

        private void OnDisable()
        {
            UnbindChildHuds();
        }

        private void Update()
        {
            BindMissingReferences();
            BindChildHuds();
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
            if (sessionStateHud != null)
            {
                sessionStateHud.Bind(gameSession, playerHealth, playerController);
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

            if (sessionStateHud != null)
            {
                sessionStateHud.Bind(null, null, null);
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

            if (awarenessHud != null)
            {
                awarenessHud.ApplyTypography(terminalFontAsset, japaneseFontAsset != null ? japaneseFontAsset : terminalFontAsset);
            }

            if (sessionStateHud != null)
            {
                sessionStateHud.ApplyTypography(terminalFontAsset);
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
    }
}
