using System.Collections;
using System.IO;
using Apoplexy.AI;
using Apoplexy.Player;
using Apoplexy.Weapons;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Apoplexy.Core
{
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class GameSession : MonoBehaviour
    {
        private const float WinSequenceDuration = 4.35f;
        private const float WinSequenceSlowMotion = 0.18f;
        private const float DamageVignetteDuration = 0.45f;

        private static readonly Color TerminalRed = new(0.82f, 0.09f, 0.11f, 1f);

        private FirstPersonController playerController;
        private PlayerHealth playerHealth;
        private PlayerWeaponController weaponController;
        private Camera playerCamera;
        private AudioSource audioSource;
        private AudioClip musicClip;
        private AudioClip hurtClip;

        private GameState state = GameState.Playing;
        private Vector3 cameraShakeOffset;
        private float cameraShakeTimer;
        private float cameraShakeDuration;
        private float cameraShakeStrength;
        private float damageVignetteTimer;
        private float winSequenceTimer;
        private float enemyScanTimer;
        private int initialEnemyCount;
        private bool hasSeenEnemies;
        private string awarenessText = string.Empty;
        private Color awarenessColor = Color.white;

        public GameState State => state;
        public float DamageVignetteAmount => Mathf.Clamp01(damageVignetteTimer / DamageVignetteDuration);
        public float WinSequenceAmount => Mathf.Clamp01(winSequenceTimer / WinSequenceDuration);
        public string AwarenessText => awarenessText;
        public Color AwarenessColor => awarenessColor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (FindAnyObjectByType<GameSession>() != null)
            {
                return;
            }

            GameObject session = new("GameSession");
            DontDestroyOnLoad(session);
            session.AddComponent<GameSession>();
        }

        private void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Start()
        {
            BindScene();
            StartCoroutine(LoadAudioClip("Apoplexy/Audio/music/music.mp3", AudioType.MPEG, clip =>
            {
                musicClip = clip;
                PlayMusic();
            }));
            StartCoroutine(LoadAudioClip("Apoplexy/Audio/player/hurt.wav", AudioType.WAV, clip => hurtClip = clip));
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            UnbindPlayerHealth();
            UnbindWeapon();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            BindScene();
            PlayMusic();
        }

        private void Update()
        {
            if (state is GameState.Dead or GameState.Win)
            {
                if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }

                return;
            }

            damageVignetteTimer = Mathf.Max(0f, damageVignetteTimer - Time.unscaledDeltaTime);
            enemyScanTimer -= Time.unscaledDeltaTime;

            if (enemyScanTimer <= 0f)
            {
                enemyScanTimer = 0.25f;
                UpdateEnemyWinCondition();
            }

            if (state == GameState.WinSequence)
            {
                Time.timeScale = WinSequenceSlowMotion;
                winSequenceTimer += Time.unscaledDeltaTime;

                if (winSequenceTimer >= WinSequenceDuration)
                {
                    Time.timeScale = 1f;
                    state = GameState.Win;
                    SetControlsEnabled(false);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }

        private void LateUpdate()
        {
            if (playerCamera == null)
            {
                return;
            }

            if (cameraShakeOffset != Vector3.zero)
            {
                playerCamera.transform.localPosition -= cameraShakeOffset;
                cameraShakeOffset = Vector3.zero;
            }

            if (cameraShakeTimer <= 0f)
            {
                return;
            }

            cameraShakeTimer = Mathf.Max(0f, cameraShakeTimer - Time.unscaledDeltaTime);
            float amount = cameraShakeDuration > 0f ? cameraShakeTimer / cameraShakeDuration : 0f;
            float strength = cameraShakeStrength * amount;

            cameraShakeOffset = new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength),
                0f);

            playerCamera.transform.localPosition += cameraShakeOffset;
        }

        private void OnGUI()
        {
            DrawCrosshair();
        }

        private void BindScene()
        {
            Time.timeScale = 1f;
            state = GameState.Playing;
            winSequenceTimer = 0f;
            damageVignetteTimer = 0f;
            enemyScanTimer = 0.5f;
            initialEnemyCount = 0;
            hasSeenEnemies = false;

            UnbindPlayerHealth();
            UnbindWeapon();

            playerController = FindAnyObjectByType<FirstPersonController>();
            playerHealth = FindAnyObjectByType<PlayerHealth>();
            weaponController = FindAnyObjectByType<PlayerWeaponController>();
            playerCamera = Camera.main != null ? Camera.main : FindAnyObjectByType<Camera>();

            if (playerHealth != null)
            {
                playerHealth.Damaged += OnPlayerDamaged;
                playerHealth.Died += OnPlayerDied;
            }

            if (weaponController != null)
            {
                weaponController.ShotFired += OnShotFired;
            }
        }

        private void UnbindPlayerHealth()
        {
            if (playerHealth == null)
            {
                return;
            }

            playerHealth.Damaged -= OnPlayerDamaged;
            playerHealth.Died -= OnPlayerDied;
        }

        private void UnbindWeapon()
        {
            if (weaponController == null)
            {
                return;
            }

            weaponController.ShotFired -= OnShotFired;
        }

        private void OnPlayerDamaged(int currentHealth, int maxHealth)
        {
            damageVignetteTimer = DamageVignetteDuration;
            StartCameraShake(0.22f, 0.18f);

            if (hurtClip != null)
            {
                AudioSource.PlayClipAtPoint(hurtClip, playerController != null ? playerController.transform.position : Vector3.zero, 0.8f);
            }
        }

        private void OnPlayerDied()
        {
            state = GameState.Dead;
            Time.timeScale = 1f;
            SetControlsEnabled(false);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnShotFired(WeaponDefinition weapon)
        {
            StartCameraShake(0.15f, 0.12f);
        }

        private void UpdateEnemyWinCondition()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Exclude);

            if (!hasSeenEnemies && enemies.Length > 0)
            {
                hasSeenEnemies = true;
                initialEnemyCount = enemies.Length;
            }

            UpdateAwareness(enemies);

            if (!hasSeenEnemies || state != GameState.Playing)
            {
                return;
            }

            int alive = 0;

            foreach (EnemyController enemy in enemies)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    alive++;
                }
            }

            if (initialEnemyCount > 0 && alive == 0)
            {
                state = GameState.WinSequence;
                winSequenceTimer = 0f;
            }
        }

        private void UpdateAwareness(EnemyController[] enemies)
        {
            awarenessText = string.Empty;
            awarenessColor = Color.white;

            foreach (EnemyController enemy in enemies)
            {
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                if (enemy.State is EnemyState.Chase or EnemyState.AttackWindup or EnemyState.AttackRecovery)
                {
                    awarenessText = "DISCOVERED";
                    awarenessColor = TerminalRed;
                    return;
                }

                if (enemy.State == EnemyState.Alert)
                {
                    awarenessText = "SPOTTED";
                    awarenessColor = new Color(1f, 0.82f, 0.25f, 1f);
                }
                else if (enemy.State == EnemyState.Suspicious && string.IsNullOrEmpty(awarenessText))
                {
                    awarenessText = "SEEN?";
                    awarenessColor = new Color(1f, 0.82f, 0.25f, 1f);
                }
                else if (enemy.State == EnemyState.Search && string.IsNullOrEmpty(awarenessText))
                {
                    awarenessText = "HEARD";
                    awarenessColor = new Color(1f, 0.82f, 0.25f, 1f);
                }
            }
        }

        private void SetControlsEnabled(bool enabled)
        {
            if (playerController != null)
            {
                playerController.enabled = enabled;
            }

            if (weaponController != null)
            {
                weaponController.enabled = enabled;
            }
        }

        private void StartCameraShake(float strength, float duration)
        {
            cameraShakeStrength = strength;
            cameraShakeDuration = duration;
            cameraShakeTimer = duration;
        }

        private IEnumerator LoadAudioClip(string assetRelativePath, AudioType type, System.Action<AudioClip> onLoaded)
        {
            string path = Path.Combine(Application.dataPath, assetRelativePath);

            if (!File.Exists(path))
            {
                yield break;
            }

            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip("file://" + path, type);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
            onLoaded?.Invoke(clip);
        }

        private void PlayMusic()
        {
            if (audioSource == null || musicClip == null)
            {
                return;
            }

            if (audioSource.isPlaying && audioSource.clip == musicClip)
            {
                return;
            }

            audioSource.clip = musicClip;
            audioSource.volume = 0.0f;
            audioSource.Play();
        }

        private void DrawCrosshair()
        {
            if (state != GameState.Playing || weaponController == null)
            {
                return;
            }

            float spread = weaponController.CurrentSpreadDegrees;
            float gap = 5f + spread * 4f;
            float length = 10f;
            float thickness = 2f;
            float x = Screen.width * 0.5f;
            float y = Screen.height * 0.5f;
            Color color = new(0.92f, 0.92f, 0.88f, 0.72f + Mathf.PingPong(Time.unscaledTime * 3.5f, 0.18f));

            DrawRect(new Rect(x - gap - length, y - thickness * 0.5f, length, thickness), color);
            DrawRect(new Rect(x + gap, y - thickness * 0.5f, length, thickness), color);
            DrawRect(new Rect(x - thickness * 0.5f, y - gap - length, thickness, length), color);
            DrawRect(new Rect(x - thickness * 0.5f, y + gap, thickness, length), color);
        }

        private static void DrawRect(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
