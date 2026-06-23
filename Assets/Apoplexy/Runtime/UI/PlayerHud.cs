using Apoplexy.Weapons;
using UnityEngine;
using UnityEngine.UI;

namespace Apoplexy.UI
{
    public sealed class PlayerHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerWeaponController weaponController;
        [SerializeField] private Text weaponText;
        [SerializeField] private Text ammoText;
        [SerializeField] private Text statusText;
        [SerializeField] private Graphic crosshair;

        [Header("Formatting")]
        [SerializeField] private string ammoFormat = "{0:000} / {1:000}";
        [SerializeField] private string weaponFormat = "{0}";
        [SerializeField] private string reloadText = "RELOADING";

        private void Awake()
        {
            if (weaponController == null)
            {
                weaponController = FindFirstObjectByType<PlayerWeaponController>();
            }
        }

        private void OnEnable()
        {
            if (weaponController == null)
            {
                return;
            }

            weaponController.AmmunitionChanged += Refresh;

            Refresh();
        }

        private void OnDisable()
        {
            if (weaponController == null)
            {
                return;
            }

            weaponController.AmmunitionChanged -= Refresh;
        }

        private void Update()
        {
            RefreshStatus();
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

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }
    }
}
