using Apoplexy.Weapons;
using UnityEditor;
using UnityEngine;

namespace Apoplexy.Editor
{
    public sealed class MuzzleFlashTuner : EditorWindow
    {
        private WeaponDefinition weapon;
        private PlayerWeaponController runtimeController;
        private SerializedObject serializedWeapon;
        private SerializedProperty muzzleFlashSprite;
        private SerializedProperty muzzlePosition;
        private SerializedProperty muzzleFlashSize;
        private SerializedProperty muzzleFlashDuration;
        private SerializedProperty muzzleFlashColor;
        private bool previewFlash = true;

        [MenuItem("Window/Apoplexy/Muzzle Flash Tuner")]
        private static void Open()
        {
            GetWindow<MuzzleFlashTuner>("Muzzle Flash Tuner");
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;
            SceneView.duringSceneGui += DrawMuzzleSocket;

            if (Selection.activeObject is WeaponDefinition selectedWeapon)
            {
                SetWeapon(selectedWeapon);
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
            SceneView.duringSceneGui -= DrawMuzzleSocket;

            if (runtimeController != null)
            {
                runtimeController.SetMuzzleFlashPreviewVisible(false);
            }
        }

        private void OnSelectionChange()
        {
            if (Selection.activeObject is WeaponDefinition selectedWeapon)
            {
                SetWeapon(selectedWeapon);
                Repaint();
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Muzzle Flash", EditorStyles.boldLabel);

            WeaponDefinition selectedWeapon = (WeaponDefinition)EditorGUILayout.ObjectField(
                "Weapon",
                weapon,
                typeof(WeaponDefinition),
                false);

            if (selectedWeapon != weapon)
            {
                SetWeapon(selectedWeapon);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                runtimeController = (PlayerWeaponController)EditorGUILayout.ObjectField(
                    "Runtime Controller",
                    runtimeController,
                    typeof(PlayerWeaponController),
                    true);

                if (GUILayout.Button("Find", GUILayout.Width(50f)))
                {
                    FindRuntimeController();
                }
            }

            if (weapon == null || serializedWeapon == null)
            {
                EditorGUILayout.HelpBox("Select a WeaponDefinition asset.", MessageType.Info);
                return;
            }

            if (Application.isPlaying && runtimeController != null && runtimeController.Weapon != weapon)
            {
                EditorGUILayout.HelpBox(
                    "The runtime controller is using a different weapon.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space();
            serializedWeapon.Update();
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(muzzleFlashSprite, new GUIContent("Sprite"));
            EditorGUILayout.PropertyField(muzzlePosition, new GUIContent("Socket Position"));
            EditorGUILayout.PropertyField(muzzleFlashSize, new GUIContent("Flash Size"));
            EditorGUILayout.PropertyField(muzzleFlashDuration, new GUIContent("Duration"));
            EditorGUILayout.PropertyField(muzzleFlashColor, new GUIContent("Color"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedWeapon.ApplyModifiedProperties();
                EditorUtility.SetDirty(weapon);
                ApplyPreview();
            }

            EditorGUILayout.Space();

            bool newPreviewFlash = EditorGUILayout.Toggle("Show Flash Preview", previewFlash);

            if (newPreviewFlash != previewFlash)
            {
                previewFlash = newPreviewFlash;
                ApplyPreview();
            }

            using (new EditorGUI.DisabledScope(runtimeController == null))
            {
                if (GUILayout.Button("Flash Once"))
                {
                    runtimeController.PlayMuzzleFlashPreview();
                }

                if (GUILayout.Button("Select Runtime Socket") && runtimeController.MuzzleSocket != null)
                {
                    Selection.activeGameObject = runtimeController.MuzzleSocket.gameObject;
                    SceneView.lastActiveSceneView?.FrameSelected();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter play mode for the generated socket and live flash preview. "
                    + "Changes are written directly to the weapon asset.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "The cyan Scene-view handle marks the exact socket center. "
                    + "The Game view shows the flash continuously while preview is enabled.",
                    MessageType.None);
            }
        }

        private void SetWeapon(WeaponDefinition selectedWeapon)
        {
            if (runtimeController != null)
            {
                runtimeController.SetMuzzleFlashPreviewVisible(false);
            }

            weapon = selectedWeapon;

            if (weapon == null)
            {
                serializedWeapon = null;
                return;
            }

            serializedWeapon = new SerializedObject(weapon);
            muzzleFlashSprite = serializedWeapon.FindProperty("muzzleFlashSprite");
            muzzlePosition = serializedWeapon.FindProperty("muzzlePosition");
            muzzleFlashSize = serializedWeapon.FindProperty("muzzleFlashSize");
            muzzleFlashDuration = serializedWeapon.FindProperty("muzzleFlashDuration");
            muzzleFlashColor = serializedWeapon.FindProperty("muzzleFlashColor");

            if (runtimeController == null)
            {
                FindRuntimeController();
            }

            ApplyPreview();
        }

        private void FindRuntimeController()
        {
            runtimeController = Object.FindFirstObjectByType<PlayerWeaponController>();

            if (runtimeController == null)
            {
                return;
            }

            if (weapon == null)
            {
                SetWeapon(runtimeController.Weapon);
                return;
            }

            ApplyPreview();
        }

        private void ApplyPreview()
        {
            if (runtimeController == null)
            {
                FindRuntimeController();
            }

            if (runtimeController != null && runtimeController.Weapon == weapon)
            {
                runtimeController.ApplyMuzzleFlashSettings();
                runtimeController.SetMuzzleFlashPreviewVisible(previewFlash);
                SceneView.RepaintAll();
            }
        }

        private void DrawMuzzleSocket(SceneView sceneView)
        {
            if (!Application.isPlaying
                || runtimeController == null
                || runtimeController.MuzzleSocket == null
                || runtimeController.Weapon != weapon)
            {
                return;
            }

            Transform socket = runtimeController.MuzzleSocket;
            Vector3 position = socket.position;
            float size = HandleUtility.GetHandleSize(position) * 0.045f;

            Handles.color = Color.cyan;
            Handles.SphereHandleCap(0, position, Quaternion.identity, size, EventType.Repaint);
            Handles.DrawLine(position - socket.right * size * 2f, position + socket.right * size * 2f);
            Handles.DrawLine(position - socket.up * size * 2f, position + socket.up * size * 2f);
            Handles.Label(position + socket.up * size * 2.5f, "Muzzle Socket");

            EditorGUI.BeginChangeCheck();
            Vector3 movedPosition = Handles.PositionHandle(position, socket.rotation);

            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(weapon, "Move Muzzle Socket");
            Vector3 localPosition = socket.parent.InverseTransformPoint(movedPosition);

            serializedWeapon.Update();
            muzzlePosition.vector3Value = localPosition;
            serializedWeapon.ApplyModifiedProperties();
            EditorUtility.SetDirty(weapon);
            ApplyPreview();
        }
    }
}
