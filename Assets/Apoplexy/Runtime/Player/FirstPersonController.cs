using Apoplexy.AI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Apoplexy.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(AudioSource))]
    public sealed class FirstPersonController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera viewCamera;
        [SerializeField] private InputActionAsset inputActions;

        [Header("Look")]
        [SerializeField] private float mouseSensitivity = 0.14f;
        [SerializeField] private float minimumPitch = -85f;
        [SerializeField] private float maximumPitch = 85f;

        [Header("Movement")]
        [SerializeField] private float moveSpeed = 4f;
        [SerializeField] private float sprintMultiplier = 1.45f;
        [SerializeField] private float crouchMultiplier = 0.42f;
        [SerializeField] private float movementAcceleration = 32f;
        [SerializeField] private float stoppingAcceleration = 42f;
        [SerializeField] private float gravity = 24f;
        [SerializeField] private float groundedForce = 2f;

        [Header("Collider")]
        [SerializeField] private float playerHeight = 1.8f;
        [SerializeField] private float playerRadius = 0.35f;

        [Header("Camera")]
        [SerializeField] private float standingEyeHeight = 1.53f;
        [SerializeField] private float crouchingEyeHeight = 0.9f;
        [SerializeField] private float baseFieldOfView = 90f;
        [SerializeField] private float sprintFieldOfViewIncrease = 10f;
        [SerializeField] private float sprintFovEaseSpeed = 7f;
        [SerializeField] private float crouchEaseSpeed = 12f;

        [Header("Head Bob")]
        [SerializeField] private float bobRadiansPerUnit = 3.1f;
        [SerializeField] private float verticalBobDistance = 0.06f;
        [SerializeField] private float horizontalBobDistance = 0.032f;
        [SerializeField] private float headBobEaseSpeed = 10f;
        [SerializeField] private float crouchBobScale = 0.35f;
        [SerializeField] private float sprintBobBoost = 0.18f;

        [Header("Footstep Audio")]
        [SerializeField] private AudioClip[] footstepSounds = new AudioClip[0];
        [SerializeField, Range(0f, 1f)] private float walkFootstepVolume = 0.68f;
        [SerializeField, Range(0f, 1f)] private float sprintFootstepVolume = 0.85f;
        [SerializeField, Range(0f, 1f)] private float crouchFootstepVolume = 0.32f;
        [SerializeField] private Vector2 footstepPitchJitter = new(-0.04f, 0.04f);
        [SerializeField] private Vector2 footstepPanJitter = new(-0.1f, 0.1f);

        [Header("Footstep Noise")]
        [SerializeField, Min(0f)] private float minimumFootstepSpeed = 1.2f;
        [SerializeField, Min(0.01f)] private float walkStepDistance = 1.72f;
        [SerializeField, Min(0.01f)] private float sprintStepDistance = 1.92f;
        [SerializeField, Min(0.01f)] private float crouchStepDistance = 1.42f;
        [SerializeField, Min(0f)] private float walkNoiseRadius = 4.5f;
        [SerializeField, Min(0f)] private float sprintNoiseRadius = 8f;
        [SerializeField, Min(0f)] private float crouchNoiseRadius = 1.35f;

        private CharacterController controller;
        private AudioSource footstepAudioSource;
        private InputActionMap playerActions;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction sprintAction;
        private InputAction crouchAction;
        private InputAction attackAction;

        private Vector3 horizontalVelocity;
        private Vector3 cameraBaseLocalPosition;

        private float verticalVelocity;
        private float yaw;
        private float pitch;
        private float crouchAmount;
        private float sprintFovAmount;
        private float headBobTimer;
        private float footstepDistance;
        private float headBobAmount;
        private int lastFootstepIndex = -1;

        private bool sprintBlockedByAttack;

        public float HorizontalSpeed => new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude;

        public bool IsGrounded { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsCrouching { get; private set; }

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            footstepAudioSource = GetComponent<AudioSource>();
            footstepAudioSource.playOnAwake = false;
            footstepAudioSource.loop = false;
            footstepAudioSource.spatialBlend = 0f;

            if (viewCamera == null)
            {
                viewCamera = GetComponentInChildren<Camera>();
            }

            if (viewCamera == null || inputActions == null)
            {
                Debug.LogError("FirstPersonController requires a camera and InputActionAsset.", this);
                enabled = false;
                return;
            }

            ConfigureController();

            cameraBaseLocalPosition = viewCamera.transform.localPosition;
            cameraBaseLocalPosition.y = standingEyeHeight;

            yaw = transform.eulerAngles.y;
            pitch = NormalizeAngle(viewCamera.transform.localEulerAngles.x);

            FindInputActions();
        }

        private void OnEnable()
        {
            playerActions?.Enable();
        }

        private void Start()
        {
            SetCursorLocked(true);
        }

        private void OnDisable()
        {
            playerActions?.Disable();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;

            UpdateCursor();
            UpdateLook();
            UpdateMovement(deltaTime);
            UpdatePresentation(deltaTime);
        }

        private void LateUpdate()
        {
            UpdateCameraTransform();
        }

        private void ConfigureController()
        {
            controller.height = playerHeight;
            controller.radius = playerRadius;
            controller.center = Vector3.up * (playerHeight * 0.5f);
            controller.stepOffset = 0.3f;
            controller.slopeLimit = 50f;
            controller.skinWidth = 0.03f;
            controller.minMoveDistance = 0f;
        }

        private void FindInputActions()
        {
            playerActions = inputActions.FindActionMap("Player", true);
            moveAction = playerActions.FindAction("Move", true);
            lookAction = playerActions.FindAction("Look", true);
            sprintAction = playerActions.FindAction("Sprint", true);
            crouchAction = playerActions.FindAction("Crouch", true);
            attackAction = playerActions.FindAction("Attack", true);
        }

        private void UpdateCursor()
        {
            if (Keyboard.current?.escapeKey.wasPressedThisFrame == true)
            {
                SetCursorLocked(Cursor.lockState != CursorLockMode.Locked);
            }

            if (Mouse.current?.leftButton.wasPressedThisFrame == true && Cursor.lockState != CursorLockMode.Locked)
            {
                SetCursorLocked(true);
            }
        }

        private void UpdateLook()
        {
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                return;
            }

            Vector2 lookDelta = lookAction.ReadValue<Vector2>();

            yaw += lookDelta.x * mouseSensitivity;
            pitch -= lookDelta.y * mouseSensitivity;
            pitch = Mathf.Clamp(pitch, minimumPitch, maximumPitch);

            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private void UpdateMovement(float deltaTime)
        {
            IsGrounded = controller.isGrounded;

            Vector2 moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

            bool wantsCrouch = crouchAction.IsPressed() && IsGrounded;
            bool wantsSprint = sprintAction.IsPressed();
            bool attacking = attackAction.IsPressed();

            IsCrouching = wantsCrouch;

            if (!wantsSprint)
            {
                sprintBlockedByAttack = false;
            }
            else if (attacking)
            {
                sprintBlockedByAttack = true;
            }

            IsSprinting = wantsSprint && !IsCrouching && !sprintBlockedByAttack && moveInput.sqrMagnitude > 0.001f;

            float targetSpeed = moveSpeed;

            if (IsSprinting)
            {
                targetSpeed *= sprintMultiplier;
            }
            else if (IsCrouching)
            {
                targetSpeed *= crouchMultiplier;
            }

            Vector3 movementDirection = transform.forward * moveInput.y + transform.right * moveInput.x;

            Vector3 targetVelocity = movementDirection * targetSpeed;

            float acceleration = moveInput.sqrMagnitude > 0.001f ? movementAcceleration : stoppingAcceleration;

            float movementEase = 1f - Mathf.Exp(-acceleration * deltaTime);

            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, movementEase);

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -groundedForce;
            }
            else
            {
                verticalVelocity -= gravity * deltaTime;
            }

            Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;

            CollisionFlags collision = controller.Move(velocity * deltaTime);

            IsGrounded = controller.isGrounded || (collision & CollisionFlags.Below) != 0;

            if ((collision & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            {
                verticalVelocity = 0f;
            }

            UpdateFootstepNoise(deltaTime);
        }

        private void UpdatePresentation(float deltaTime)
        {
            float sprintTarget = IsSprinting ? 1f : 0f;
            float sprintEase = 1f - Mathf.Exp(-sprintFovEaseSpeed * deltaTime);

            sprintFovAmount = Mathf.Lerp(sprintFovAmount, sprintTarget, sprintEase);

            float crouchTarget = IsCrouching ? 1f : 0f;
            float crouchEase = 1f - Mathf.Exp(-crouchEaseSpeed * deltaTime);

            crouchAmount = Mathf.Lerp(crouchAmount, crouchTarget, crouchEase);

            float horizontalSpeed = HorizontalSpeed;
            float currentMaximumSpeed = moveSpeed * (IsSprinting ? sprintMultiplier : IsCrouching ? crouchMultiplier : 1f);

            float speedAmount = Mathf.Clamp01(horizontalSpeed / Mathf.Max(currentMaximumSpeed, 0.001f));

            if (IsGrounded && horizontalSpeed > 0.1f)
            {
                headBobTimer += deltaTime * horizontalSpeed * bobRadiansPerUnit;
            }

            float sprintAmount = Mathf.Clamp01(horizontalSpeed / (moveSpeed * sprintMultiplier));

            float bobBoost = IsSprinting ? 1f + sprintBobBoost * sprintAmount : 1f;

            float targetBobAmount = speedAmount * bobBoost;

            if (IsCrouching)
            {
                targetBobAmount *= crouchBobScale;
            }

            if (!IsGrounded)
            {
                targetBobAmount = 0f;
            }

            float bobEase = 1f - Mathf.Exp(-headBobEaseSpeed * deltaTime);

            headBobAmount = Mathf.Lerp(headBobAmount, targetBobAmount, bobEase);
        }

        private void UpdateCameraTransform()
        {
            float bobY = Mathf.Sin(headBobTimer) * verticalBobDistance * headBobAmount;
            float bobX = Mathf.Cos(headBobTimer * 0.5f) * horizontalBobDistance * headBobAmount;

            Vector3 cameraPosition = cameraBaseLocalPosition;
            cameraPosition.x += bobX;
            cameraPosition.y = Mathf.Lerp(standingEyeHeight, crouchingEyeHeight, crouchAmount) + bobY;

            viewCamera.transform.localPosition = cameraPosition;
            viewCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

            viewCamera.fieldOfView = baseFieldOfView + sprintFieldOfViewIncrease * sprintFovAmount;
        }

        private void UpdateFootstepNoise(float deltaTime)
        {
            float speed = HorizontalSpeed;

            if (!IsGrounded || speed < minimumFootstepSpeed)
            {
                footstepDistance = 0f;
                return;
            }

            float stepDistance = walkStepDistance;
            float noiseRadius = walkNoiseRadius;
            float pitch = 1f;
            float volume = walkFootstepVolume;

            if (IsSprinting)
            {
                stepDistance = sprintStepDistance;
                noiseRadius = sprintNoiseRadius;
                pitch = 1.08f;
                volume = sprintFootstepVolume;
            }
            else if (IsCrouching)
            {
                stepDistance = crouchStepDistance;
                noiseRadius = crouchNoiseRadius;
                pitch = 0.92f;
                volume = crouchFootstepVolume;
            }

            footstepDistance += speed * deltaTime;

            while (footstepDistance >= stepDistance)
            {
                footstepDistance -= stepDistance;
                PlayFootstep(volume, pitch);
                NoiseSystem.Emit(transform.position, noiseRadius, gameObject);
            }
        }

        private void PlayFootstep(float volume, float pitch)
        {
            if (footstepAudioSource == null || footstepSounds == null || footstepSounds.Length == 0)
            {
                return;
            }

            int footstepIndex = Random.Range(0, footstepSounds.Length);

            if (footstepSounds.Length > 1)
            {
                int guard = 0;

                while (footstepIndex == lastFootstepIndex && guard < 8)
                {
                    footstepIndex = Random.Range(0, footstepSounds.Length);
                    guard++;
                }
            }

            AudioClip footstep = footstepSounds[footstepIndex];

            if (footstep == null)
            {
                return;
            }

            lastFootstepIndex = footstepIndex;

            footstepAudioSource.pitch = pitch + Random.Range(footstepPitchJitter.x, footstepPitchJitter.y);
            footstepAudioSource.panStereo = Random.Range(footstepPanJitter.x, footstepPanJitter.y);
            footstepAudioSource.PlayOneShot(footstep, volume);
        }

        private static void SetCursorLocked(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        private static float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}
