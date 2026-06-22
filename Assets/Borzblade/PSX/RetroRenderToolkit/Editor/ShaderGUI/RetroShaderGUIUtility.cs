using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Borzblade.RetroRenderToolkit.Editor
{
    internal static class RetroShaderGUIUtility
    {
        public static MaterialProperty Find(MaterialProperty[] properties, string propertyName)
        {
            if (properties == null)
            {
                return null;
            }

            foreach (MaterialProperty property in properties)
            {
                if (property != null && property.name == propertyName)
                {
                    return property;
                }
            }

            return null;
        }

        public static void Draw(MaterialEditor editor, MaterialProperty[] properties, string propertyName, string label)
        {
            Draw(editor, properties, propertyName, label, ObjectNames.NicifyVariableName(propertyName));
        }

        public static void Draw(MaterialEditor editor, MaterialProperty[] properties, string propertyName, string label, string tooltip)
        {
            MaterialProperty property = Find(properties, propertyName);
            if (property != null)
            {
                editor.ShaderProperty(property, new GUIContent(label, tooltip));
            }
        }

        public static void DrawTextureWithColor(MaterialEditor editor, MaterialProperty[] properties, string textureName, string colorName, string label)
        {
            MaterialProperty texture = Find(properties, textureName);
            MaterialProperty color = Find(properties, colorName);
            if (texture != null && color != null)
            {
                editor.TexturePropertySingleLine(new GUIContent(label), texture, color);
                editor.TextureScaleOffsetProperty(texture);
                DrawPointFilterWarning(texture);
            }
        }

        public static void DrawTextureWithScalar(MaterialEditor editor, MaterialProperty[] properties, string textureName, string scalarName, string label)
        {
            MaterialProperty texture = Find(properties, textureName);
            MaterialProperty scalar = Find(properties, scalarName);
            if (texture != null && scalar != null)
            {
                editor.TexturePropertySingleLine(new GUIContent(label), texture, scalar);
                DrawPointFilterWarning(texture);
            }
        }

        public static void DrawPointFilterWarning(MaterialProperty textureProperty)
        {
            Texture texture = textureProperty != null ? textureProperty.textureValue : null;
            if (texture == null || texture.filterMode == FilterMode.Point)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.HelpBox("PSX-style materials look best with point-filtered textures. This texture is not set to Point.", MessageType.Warning);
                if (GUILayout.Button("Set Texture Filter Mode To Point"))
                {
                    SetTexturePointFiltered(texture);
                }
            }
        }

        public static void SetTexturePointFiltered(Texture texture)
        {
            if (texture == null)
            {
                return;
            }

            string path = AssetDatabase.GetAssetPath(texture);
            TextureImporter importer = string.IsNullOrEmpty(path) ? null : AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }

            texture.filterMode = FilterMode.Point;
            EditorUtility.SetDirty(texture);
        }

        public static void DrawResetHeader(string label, System.Action resetAction)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
                if (GUILayout.Button("Reset", GUILayout.Width(72f)))
                {
                    resetAction?.Invoke();
                }
            }
        }

        public static void SetupCutoutRenderState(Material material, bool unlit = false)
        {
            if (material == null)
            {
                return;
            }

            SetFloat(material, "_Surface", 1f);
            SetFloat(material, "_Blend", 0f);
            SetFloat(material, "_AlphaClip", 1f);
            bool twoSided = GetFloat(material, "_TwoSidedEnabled", 1f) > 0.5f;
            float cull = twoSided ? (float)CullMode.Off : GetFloat(material, "_CullMode", (float)CullMode.Back);
            SetFloat(material, "_Cull", cull);

            SetFloat(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            SetFloat(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            SetFloat(material, "_SrcBlendAlpha", (float)UnityEngine.Rendering.BlendMode.One);
            SetFloat(material, "_DstBlendAlpha", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            SetFloat(material, "_ZWrite", 0f);
            SetFloat(material, "_AlphaToMask", 0f);

            material.SetOverrideTag("RenderType", "Transparent");
            int queueOffset = Mathf.Clamp((int)GetFloat(material, "_QueueOffset"), -50, 50);
            material.renderQueue = (int)RenderQueue.Transparent + queueOffset;

            // Kept for depth and shadow passes; the forward pass blends alpha instead of clipping it.
            SetKeyword(material, "_ALPHATEST_ON", true);
            SetKeyword(material, "_SURFACE_TYPE_TRANSPARENT", !unlit);
            SetKeyword(material, "_NORMALMAP", !unlit && material.HasProperty("_BumpMap") && material.GetTexture("_BumpMap") != null);
            SetKeyword(material, "_EMISSION", HasEmission(material));
            SetKeyword(material, "_RECEIVE_SHADOWS_OFF", !unlit && GetFloat(material, "_ReceiveShadows", 1f) < 0.5f);

            SetKeyword(material, "_RETRO_VERTEX_SNAP", !unlit && GetFloat(material, "_VertexSnapEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_SNAP_ANCHORS", !unlit && GetFloat(material, "_VertexSnapUseAnchors") > 0.5f);
            SetKeyword(material, "_RETRO_VERTEX_WOBBLE", !unlit && GetFloat(material, "_VertexWobbleEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_UV_PIXEL", GetFloat(material, "_UvPixelEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_AFFINE", !unlit && GetFloat(material, "_AffineEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_POSTERIZE", GetFloat(material, "_PosterizeEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_DITHER", GetFloat(material, "_DitherEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_RIM", !unlit && GetFloat(material, "_RimEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_DITHER_FADE", GetFloat(material, "_DitherCutoutFadeEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_DISTANCE_FADE", GetFloat(material, "_DistanceFadeEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_CAMERA_FADE", GetFloat(material, "_CameraFadeEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_BACKFACE_TINT", !unlit && GetFloat(material, "_BackfaceTintEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_FOG", GetFloat(material, "_RetroFogEnabled") > 0.5f);

            SetKeyword(material, "_RETRO_FOLIAGE_WIND", GetFloat(material, "_WindEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_FOLIAGE_GRADIENT", GetFloat(material, "_FoliageGradientEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_FOLIAGE_VARIATION", GetFloat(material, "_FoliageVariationEnabled") > 0.5f);
            SetKeyword(material, "_RETRO_FOLIAGE_MOBILE", GetFloat(material, "_FoliageMobileLighting") > 0.5f);

            material.SetShaderPassEnabled("ShadowCaster", true);
        }

        public static bool HasEmission(Material material)
        {
            Color emission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : Color.black;
            return emission.maxColorComponent > 0.0001f || (material.HasProperty("_EmissionMap") && material.GetTexture("_EmissionMap") != null);
        }

        public static void SetKeyword(Material material, string keyword, bool enabled)
        {
            if (enabled)
            {
                material.EnableKeyword(keyword);
            }
            else
            {
                material.DisableKeyword(keyword);
            }
        }

        public static float GetFloat(Material material, string propertyName, float fallback = 0f)
        {
            return material != null && material.HasProperty(propertyName) ? material.GetFloat(propertyName) : fallback;
        }

        public static void SetFloat(Material material, string propertyName, float value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetFloat(propertyName, value);
            }
        }

        public static void SetColor(Material material, string propertyName, Color value)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetColor(propertyName, value);
            }
        }

        public static void SetTexture(Material material, string propertyName, Texture texture)
        {
            if (material != null && material.HasProperty(propertyName))
            {
                material.SetTexture(propertyName, texture);
            }
        }
    }
}
