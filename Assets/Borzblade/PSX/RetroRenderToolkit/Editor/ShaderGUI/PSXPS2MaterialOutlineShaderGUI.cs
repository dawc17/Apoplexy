using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public sealed class PSXPS2MaterialOutlineShaderGUI : ShaderGUI
    {
        private enum Preset
        {
            Black,
            Warm,
            Crunchy
        }

        private static bool outlineFoldout = true;
        private static bool retroFoldout = true;
        private static bool advancedFoldout;

        private MaterialEditor editor;
        private MaterialProperty[] properties;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] materialProperties)
        {
            editor = materialEditor;
            properties = materialProperties;

            DrawPresets();
            EditorGUI.BeginChangeCheck();
            DrawOutline();
            DrawRetro();
            DrawAdvanced();

            if (EditorGUI.EndChangeCheck())
            {
                ForEachMaterial(SetupMaterial, "Edit Retro Material Outline");
            }
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            base.AssignNewShaderToMaterial(material, oldShader, newShader);
            ApplyBlack(material);
            SetupMaterial(material);
        }

        private void DrawPresets()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Material Outline Presets", EditorStyles.boldLabel);
                using (new EditorGUILayout.HorizontalScope())
                {
                    PresetButton("Black", Preset.Black);
                    PresetButton("Warm", Preset.Warm);
                    PresetButton("Crunchy", Preset.Crunchy);
                }
            }
        }

        private void PresetButton(string label, Preset preset)
        {
            if (!GUILayout.Button(label))
            {
                return;
            }

            ForEachMaterial(material =>
            {
                switch (preset)
                {
                    case Preset.Warm:
                        ApplyWarm(material);
                        break;
                    case Preset.Crunchy:
                        ApplyCrunchy(material);
                        break;
                    default:
                        ApplyBlack(material);
                        break;
                }
                SetupMaterial(material);
            }, $"Apply {label} Outline Preset");
        }

        private void DrawOutline()
        {
            outlineFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(outlineFoldout, "Outline");
            if (outlineFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_OutlineColor", "Color");
                    RetroShaderGUIUtility.Draw(editor, properties, "_OutlineThickness", "Thickness");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DistanceFadeStart", "Fade Start");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DistanceFadeEnd", "Fade End");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DistanceFadeStrength", "Fade Strength");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawRetro()
        {
            retroFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(retroFoldout, "Retro Controls");
            if (retroFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapEnabled", "Vertex Snap");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapStrength", "Snap Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapResolution", "Snap Resolution");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapDistanceFade", "Snap Distance Fade");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapFadeStart", "Snap Fade Start");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapFadeEnd", "Snap Fade End");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSeamReduction", "Snap Seam Reduction");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexSnapSpace", "Snap Space");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceEnabled", "Vertex Draw Distance");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistance", "Draw Distance");
                    RetroShaderGUIUtility.Draw(editor, properties, "_VertexDrawDistanceFade", "Draw Distance Fade");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherEnabled", "Ordered Dither");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherStrength", "Dither Strength");
                    RetroShaderGUIUtility.Draw(editor, properties, "_DitherScale", "Dither Scale");
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawAdvanced()
        {
            advancedFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(advancedFoldout, "Advanced");
            if (advancedFoldout)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    RetroShaderGUIUtility.Draw(editor, properties, "_QueueOffset", "Queue Offset");
                    editor.EnableInstancingField();
                }
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void ForEachMaterial(System.Action<Material> action, string undoName)
        {
            Undo.RecordObjects(editor.targets, undoName);
            foreach (Object target in editor.targets)
            {
                if (target is Material material)
                {
                    action(material);
                    EditorUtility.SetDirty(material);
                }
            }
        }

        public static void ApplyBlack(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetColor(material, "_OutlineColor", new Color(0.02f, 0.018f, 0.015f, 1f));
        }

        public static void ApplyWarm(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetColor(material, "_OutlineColor", new Color(0.12f, 0.07f, 0.035f, 1f));
            RetroShaderGUIUtility.SetFloat(material, "_OutlineThickness", 0.018f);
        }

        public static void ApplyCrunchy(Material material)
        {
            ApplyBase(material);
            RetroShaderGUIUtility.SetFloat(material, "_OutlineThickness", 0.035f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.18f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 1f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.16f);
        }

        public static void SetupMaterial(Material material)
        {
            if (material == null)
            {
                return;
            }

            material.SetOverrideTag("RenderType", "Transparent");
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
            material.SetFloat("_ZWrite", 0f);
            material.renderQueue = (int)RenderQueue.Transparent + Mathf.Clamp((int)RetroShaderGUIUtility.GetFloat(material, "_QueueOffset", -1f), -50, 50);

            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_VERTEX_SNAP", RetroShaderGUIUtility.GetFloat(material, "_VertexSnapEnabled") > 0.5f);
            RetroShaderGUIUtility.SetKeyword(material, "_RETRO_DITHER", RetroShaderGUIUtility.GetFloat(material, "_DitherEnabled") > 0.5f);
        }

        private static void ApplyBase(Material material)
        {
            RetroShaderGUIUtility.SetFloat(material, "_OutlineThickness", 0.025f);
            RetroShaderGUIUtility.SetFloat(material, "_DistanceFadeStart", 8f);
            RetroShaderGUIUtility.SetFloat(material, "_DistanceFadeEnd", 55f);
            RetroShaderGUIUtility.SetFloat(material, "_DistanceFadeStrength", 0.25f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapStrength", 0.1f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapResolution", 240f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapDistanceFade", 0.35f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapFadeStart", 5f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapFadeEnd", 45f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSeamReduction", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexSnapSpace", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistance", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_VertexDrawDistanceFade", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherEnabled", 0f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherStrength", 0.08f);
            RetroShaderGUIUtility.SetFloat(material, "_DitherScale", 1f);
        }
    }
}
