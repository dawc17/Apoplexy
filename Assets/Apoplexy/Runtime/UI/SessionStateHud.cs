using Apoplexy.Core;
using Apoplexy.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apoplexy.UI
{
    public sealed class SessionStateHud : MonoBehaviour
    {
        [SerializeField] private Graphic damageVignette;
        [SerializeField] private Graphic crouchVignette;
        [SerializeField] private GameObject stateOverlay;
        [SerializeField] private TMP_Text stateTitleText;
        [SerializeField] private TMP_Text statePromptText;

        [SerializeField] private string deadTitle = "SIGNAL LOST";
        [SerializeField] private string winTitle = "MISSION COMPLETE";
        [SerializeField] private string restartPrompt = "R TO REBOOT";

        [SerializeField] private Color textColor = new(0.92f, 0.92f, 0.88f, 1f);
        [SerializeField] private Color dangerColor = new(0.82f, 0.09f, 0.11f, 1f);
        [SerializeField] private Color damageVignetteColor = new(0.38f, 0f, 0f, 0.42f);
        [SerializeField] private Color crouchVignetteColor = new(0f, 0f, 0f, 0.14f);

        private GameSession gameSession;
        private PlayerHealth playerHealth;
        private FirstPersonController playerController;

        public void Bind(GameSession session, PlayerHealth health, FirstPersonController controller)
        {
            gameSession = session;
            playerHealth = health;
            playerController = controller;
            Refresh();
        }

        public void ApplyTypography(TMP_FontAsset fontAsset)
        {
            SetFont(stateTitleText, fontAsset);
            SetFont(statePromptText, fontAsset);
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (gameSession == null)
            {
                SetAlpha(damageVignette, 0f);
                SetAlpha(crouchVignette, 0f);
                SetActive(stateOverlay, false);
                return;
            }

            SetAlpha(damageVignette, gameSession.DamageVignetteAmount * damageVignetteColor.a, damageVignetteColor);

            SetAlpha(crouchVignette, playerHealth != null
                                        && !playerHealth.IsDead
                                        && playerController != null
                                        && playerController.IsCrouching
                                        ? crouchVignetteColor.a
                                        : 0f,
                                        crouchVignetteColor);

            bool showState = gameSession.State is GameState.Dead or GameState.Win;
            SetActive(stateOverlay, showState);

            if (!showState)
            {
                return;
            }

            bool dead = gameSession.State == GameState.Dead;
            SetText(stateTitleText, dead ? deadTitle : winTitle);
            SetText(statePromptText, restartPrompt);
            SetTextColor(stateTitleText, dead ? dangerColor : textColor);
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
