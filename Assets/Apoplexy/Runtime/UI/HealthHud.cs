using Apoplexy.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apoplexy.UI
{
    public sealed class HealthHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;
        [SerializeField] private Image currentFill;
        [SerializeField] private Image damageTrailFill;

        [SerializeField] private string healthFormat = "{0:000}";
        [SerializeField] private Color currentFillColor = new(0.92f, 0.92f, 0.88f, 0.88f);
        [SerializeField] private Color damageTrailColor = new(0.82f, 0.09f, 0.11f, 0.92f);

        [SerializeField, Min(0f)] private float trailHoldSeconds = 0.5f;
        [SerializeField, Min(0.01f)] private float trailRetreatSpeed = 1.35f;
        [SerializeField, Min(0f)] private float minimumVisibleTrail = 0.002f;

        private PlayerHealth playerHealth;
        private bool initialized;
        private float currentPercent;
        private float trailPercent;
        private float trailHoldUntil;

        public void Bind(PlayerHealth health)
        {
            if (playerHealth == health)
            {
                return;
            }

            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
            }

            playerHealth = health;

            if (playerHealth != null)
            {
                playerHealth.HealthChanged += OnHealthChanged;
                initialized = false;
                OnHealthChanged(playerHealth.Health, playerHealth.MaxHealth);
            }
            else
            {
                Clear();
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.HealthChanged -= OnHealthChanged;
                playerHealth = null;
            }
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (Time.unscaledTime >= trailHoldUntil)
            {
                trailPercent = Mathf.MoveTowards(trailPercent, currentPercent, trailRetreatSpeed * Time.unscaledDeltaTime);
            }

            ApplyFills();
        }

        private void OnHealthChanged(int health, int maxHealth)
        {
            float nextPercent = maxHealth > 0
                ? Mathf.Clamp01((float)health / maxHealth)
                : 0f;

            if (!initialized)
            {
                currentPercent = nextPercent;
                trailPercent = nextPercent;
                initialized = true;
            }

            if (nextPercent < currentPercent)
            {
                trailPercent = Mathf.Max(trailPercent, currentPercent);
                trailHoldUntil = Time.unscaledTime + trailHoldSeconds;
            }
            else if (nextPercent > currentPercent)
            {
                trailPercent = nextPercent;
            }

            currentPercent = nextPercent;

            if (healthText != null)
            {
                healthText.text = string.Format(healthFormat, health);
            }

            ApplyFills();
        }

        private void Clear()
        {
            initialized = false;
            currentPercent = 0f;
            trailPercent = 0f;

            if (healthText != null)
            {
                healthText.text = string.Empty;
            }

            SetFill(currentFill, 0f);
            SetFill(damageTrailFill, 0f);
        }

        private void ApplyFills()
        {
            SetFill(damageTrailFill, trailPercent);
            SetFill(currentFill, currentPercent);

            if (currentFill != null)
            {
                currentFill.color = currentFillColor;
            }

            if (damageTrailFill != null)
            {
                damageTrailFill.color = damageTrailColor;
                damageTrailFill.gameObject.SetActive(trailPercent > currentPercent + minimumVisibleTrail);
            }
        }

        private static void SetFill(Image image, float value)
        {
            if (image != null)
            {
                float fill = Mathf.Clamp01(value);
                RectTransform rectTransform = image.rectTransform;
                Vector3 scale = rectTransform.localScale;
                scale.x = fill;
                rectTransform.localScale = scale;
            }
        }
    }
}
