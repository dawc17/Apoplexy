using System;
using UnityEngine;

namespace Apoplexy.Weapons
{
    [Serializable]
    public sealed class ProceduralWeaponAnimationSettings
    {
        [Header("Mouse Sway")]
        public float swayPositionAmount = 0.0008f;
        public float swayRotationAmount = 0.045f;
        public Vector2 swayPositionClamp = new(0.035f, 0.025f);
        public Vector2 swayRotationClamp = new(4f, 3f);
        public float swayRollAmount = -0.35f;
        public float swayFollowSpeed = 16f;
        public float swaySettleSpeed = 10f;

        [Header("Walk Bob")]
        public float walkBobSpeed = 8f;
        public float walkBobX = 0.035f;
        public float walkBobY = 0.018f;
        public float walkBobEaseSpeed = 8f;

        [Header("Sprint")]
        public float sprintBobSpeedScale = 0.75f;
        public float sprintBobX = 0.018f;
        public float sprintBobY = 0.020f;
        public Vector3 sprintOffset = new(0.025f, -0.09f, -0.02f);
        public Vector3 sprintRotation = new(35f, 10f, 0f);
        public float sprintEaseSpeed = 10f;

        [Header("Recoil")]
        public float recoilDuration = 0.36f;
        public Vector3 recoilKickOffset = new(0f, -0.030f, -0.06f);
        public Vector3 recoilFollowThroughOffset = new(-0.012f, 0.010f, 0.020f);
        public Vector3 recoilKickRotation = new(-20f, 0f, 2.4f);
        public Vector3 recoilFollowThroughRotation = new(2.5f, -1.6f, -1.2f);

        [Header("Reload")]
        public Vector3 reloadSpinDegreesPerSecond = new(1080f, 0f, 0f);
        public float reloadSpinEaseInSpeed = 18f;
        public float reloadSpinEaseOutSpeed = 12f;

        [Header("Idle")]
        public float idleBobSpeed = 1.65f;
        public float idleBobY = 0.006f;
        public float idleBobEaseSpeed = 3.5f;
    }
}
