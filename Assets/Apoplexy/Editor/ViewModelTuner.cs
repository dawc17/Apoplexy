using Apoplexy.Weapons;
using UnityEditor;
using UnityEngine;

namespace Apoplexy.Editor
{
    public sealed class ViewModelTuner : EditorWindow
    {
        private WeaponDefinition weapon;
        private PlayerWeaponController runtimeController;

        private SerializedObject serializedWeapon;
        private SerializedProperty holdPosition;
        private SerializedProperty holdRotation;
        private SerializedProperty viewModelScale;

        [MenuItem("Window/Apoplexy/Viewmodel Tuner")]
        private static void Open()
        {
            GetWindow<ViewModelTuner>("Viewmodel Tuner");
        }

        private void OnEnable()
        {
            EditorApplication.update += Repaint;

            if (Selection.activeObject is WeaponDefinition selectedWeapon)
            {
                SetWeapon(selectedWeapon);
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= Repaint;
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
            EditorGUILayout.LabelField("Viewmodel Pose", EditorStyles.boldLabel);

            WeaponDefinition selectedWeapon = (WeaponDefinition)EditorGUILayout.ObjectField("Weapon", weapon, typeof(WeaponDefinition), false);

            if (selectedWeapon != weapon)
            {
                SetWeapon(selectedWeapon);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                runtimeController = (PlayerWeaponController)EditorGUILayout.ObjectField(
                    "Runtime Controller", runtimeController, typeof(PlayerWeaponController), true);

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
                EditorGUILayout.HelpBox("The runtime controller is using a different weapon.", MessageType.Warning);
            }

            EditorGUILayout.Space();

            serializedWeapon.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(holdPosition, new GUIContent("Position"));

            EditorGUILayout.PropertyField(holdRotation, new GUIContent("Rotation"));

            EditorGUILayout.PropertyField(viewModelScale, new GUIContent("Scale"));

            if (EditorGUI.EndChangeCheck())
            {
                serializedWeapon.ApplyModifiedProperties();
                EditorUtility.SetDirty(weapon);
                ApplyPreview();
            }

            EditorGUILayout.Space();

            using (new EditorGUI.DisabledScope(runtimeController == null))
            {
                if (GUILayout.Button("Apply to runtime viewmodel"))
                {
                    ApplyPreview();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox(
                    "Enter play mode for a live preview. " + "Changes are written directly to the weapon asset " + "and persist after play mode.", MessageType.Info
                    );
            }
        }

        private void SetWeapon(WeaponDefinition selectedWeapon)
        {
            weapon = selectedWeapon;

            if (weapon == null)
            {
                serializedWeapon = null;
                return;
            }

            serializedWeapon = new SerializedObject(weapon);

            holdPosition = serializedWeapon.FindProperty("holdPosition");

            holdRotation = serializedWeapon.FindProperty("holdRotation");

            viewModelScale = serializedWeapon.FindProperty("viewModelScale");

            if (runtimeController == null)
            {
                FindRuntimeController();
            }
        }

        private void FindRuntimeController()
        {
            runtimeController = Object.FindFirstObjectByType<PlayerWeaponController>();

            if (runtimeController != null && weapon == null)
            {
                SetWeapon(runtimeController.Weapon);
            }
        }

        private void ApplyPreview()
        {
            if (runtimeController == null)
            {
                FindRuntimeController();
            }

            if (runtimeController != null && runtimeController.Weapon == weapon)
            {
                runtimeController.ApplyViewModelPose();
            }
        }
    }
}
