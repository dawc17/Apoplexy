using Apoplexy.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apoplexy.UI
{
    public sealed class AwarenessHud : MonoBehaviour
    {
        [SerializeField] private Graphic awarenessPanel;
        [SerializeField] private TMP_Text awarenessAccentText;
        [SerializeField] private TMP_Text awarenessText;

        [SerializeField] private float horizontalPadding = 36f;
        [SerializeField] private float textGap = 34f;
        [SerializeField] private float minWidth = 220f;

        [SerializeField] private Color panelColor = new(0f, 0f, 0f, 0.77f);
        [SerializeField] private Color dangerColor = new(0.82f, 0.09f, 0.11f, 1f);

        private GameSession gameSession;
        private TMP_FontAsset textFont;
        private TMP_FontAsset accentFont;

        public void ApplyTypography(TMP_FontAsset textFontAsset, TMP_FontAsset accentFontAsset)
        {
            textFont = textFontAsset;
            accentFont = accentFontAsset;
            SetFont(awarenessText, textFont);
            SetFont(awarenessAccentText, accentFont);
        }

        public void Bind(GameSession session)
        {
            gameSession = session;
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (gameSession == null || string.IsNullOrEmpty(gameSession.AwarenessText))
            {
                SetActive(awarenessPanel != null ? awarenessPanel.gameObject : null, false);

                return;
            }

            SetActive(awarenessPanel != null ? awarenessPanel.gameObject : null, true);

            Color awarenessColor = gameSession.AwarenessText == "DISCOVERED"
                ? Color.Lerp(
                    new Color(dangerColor.r, dangerColor.g, dangerColor.b, 0.7f),
                    dangerColor,
                    Mathf.PingPong(Time.unscaledTime * 12f, 1f))
                : gameSession.AwarenessColor;

            SetText(awarenessText, gameSession.AwarenessText);
            SetText(awarenessAccentText, AwarenessAccent(gameSession.AwarenessText));
            SetFont(awarenessText, textFont);
            SetFont(awarenessAccentText, accentFont);
            SetTextColor(awarenessText, awarenessColor);
            SetTextColor(awarenessAccentText, new Color(awarenessColor.r, awarenessColor.g, awarenessColor.b, 0.72f));
            ResizePanel();

            if (awarenessPanel != null)
            {
                awarenessPanel.color = panelColor;
            }
        }

        private void ResizePanel()
        {
            if (awarenessPanel == null || awarenessAccentText == null || awarenessText == null)
            {
                return;
            }

            RectTransform panelRect = awarenessPanel.rectTransform;
            RectTransform accentRect = awarenessAccentText.rectTransform;
            RectTransform textRect = awarenessText.rectTransform;

            awarenessAccentText.ForceMeshUpdate(true, true);
            awarenessText.ForceMeshUpdate(true, true);

            float accentWidth = Mathf.Ceil(awarenessAccentText.GetPreferredValues(awarenessAccentText.text, Mathf.Infinity, Mathf.Infinity).x);

            float textWidth = Mathf.Ceil(awarenessText.GetPreferredValues(awarenessText.text, Mathf.Infinity, Mathf.Infinity).x);
            float contentWidth = accentWidth + textGap + textWidth;
            float panelWidth = Mathf.Max(minWidth, contentWidth + horizontalPadding * 2f);

            Vector2 panelSize = panelRect.sizeDelta;
            panelSize.x = panelWidth;
            panelRect.sizeDelta = panelSize;

            float contentStart = -contentWidth * 0.5f;

            accentRect.anchorMin = new Vector2(0.5f, 0.5f);
            accentRect.anchorMax = new Vector2(0.5f, 0.5f);
            accentRect.pivot = new Vector2(0f, 0.5f);
            accentRect.anchoredPosition = new Vector2(contentStart, 0f);
            accentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, accentWidth);

            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.pivot = new Vector2(0f, 0.5f);
            textRect.anchoredPosition = new Vector2(contentStart + accentWidth + textGap, 0f);
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

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null) text.text = value;
        }

        private static void SetTextColor(TMP_Text text, Color color)
        {
            if (text != null) text.color = color;
        }

        private static void SetFont(TMP_Text text, TMP_FontAsset fontAsset)
        {
            if (text != null && fontAsset != null && text.font != fontAsset)
            {
                text.font = fontAsset;
            }
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active) target.SetActive(active);
        }
    }
}
