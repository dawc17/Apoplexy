using Apoplexy.Weapons;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Apoplexy.UI
{
    public sealed class WeaponHud : MonoBehaviour
    {
        [SerializeField] private TMP_Text weaponModelText;
        [SerializeField] private TMP_Text ammoText;
        [SerializeField] private TMP_Text weaponStatusText;
        [SerializeField] private Image reloadProgressFill;

        [SerializeField] private string ammoFormat = "{0:00} / {1:000}";
        [SerializeField] private string reloadText = "RELOADING";
        [SerializeField] private string weaponReadyText = "SLOT {0}/{1}";

        private PlayerWeaponController weaponController;

        public void Bind(PlayerWeaponController controller)
        {
            if (weaponController == controller)
            {
                return;
            }

            if (weaponController != null)
            {
                weaponController.AmmunitionChanged -= Refresh;
            }

            weaponController = controller;

            if (weaponController != null)
            {
                weaponController.AmmunitionChanged += Refresh;
                Refresh();
            }
            else
            {
                Clear();
            }
        }

        private void OnDisable()
        {
            if (weaponController != null)
            {
                weaponController.AmmunitionChanged -= Refresh;
                weaponController = null;
            }
        }

        private void Update()
        {
            RefreshStatus();
        }

        private void Refresh()
        {
            if (weaponController == null || weaponController.Weapon == null)
            {
                Clear();
                return;
            }

            SetText(weaponModelText, weaponController.Weapon.DisplayName);
            SetText(ammoText, string.Format(
                  ammoFormat,
                  weaponController.Ammunition,
                  weaponController.ReserveAmmunition
                  ));
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
                  weaponReadyText, Mathf.Max(1, weaponController.ActiveWeaponIndex + 1),
                  Mathf.Max(1, weaponController.WeaponCount)
                  ));
        }

        private void Clear()
        {
            SetText(weaponModelText, string.Empty);
            SetText(ammoText, string.Empty);
            SetText(weaponStatusText, string.Empty);
            SetFill(reloadProgressFill, 0f);
        }

        private static void SetText(TMP_Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetFill(Image image, float value)
        {
            if (image == null)
            {
                return;
            }

            float fill = Mathf.Clamp01(value);
            image.fillAmount = fill;

            RectTransform rectTransform = image.rectTransform;
            Vector3 scale = rectTransform.localScale;
            scale.x = fill;
            rectTransform.localScale = scale;
        }
    }
}
