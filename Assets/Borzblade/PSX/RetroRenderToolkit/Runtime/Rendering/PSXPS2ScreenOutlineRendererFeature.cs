using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Borzblade.RetroRenderToolkit
{
    [Serializable]
    public sealed class PSXPS2ScreenOutlineSettings
    {
        public bool enabled = true;
        public FullScreenPassRendererFeature.InjectionPoint passEvent = FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
        public Color outlineColor = new Color(0.015f, 0.012f, 0.01f, 1f);
        [Range(0f, 1f)] public float intensity = 0.85f;
        [Range(0.25f, 6f)] public float thickness = 1.25f;
        [Range(0f, 64f)] public float depthSensitivity = 18f;
        [Range(0f, 16f)] public float normalSensitivity = 4f;
        public float distanceFadeStart = 12f;
        public float distanceFadeEnd = 85f;
        [Range(0f, 1f)] public float blend = 0.9f;
    }

    [DisallowMultipleRendererFeature]
    public sealed class PSXPS2ScreenOutlineRendererFeature : FullScreenPassRendererFeature
    {
        public const string ShaderName = "Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Screen Outline";

        private static readonly int OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int ThicknessId = Shader.PropertyToID("_Thickness");
        private static readonly int DepthSensitivityId = Shader.PropertyToID("_DepthSensitivity");
        private static readonly int NormalSensitivityId = Shader.PropertyToID("_NormalSensitivity");
        private static readonly int DistanceFadeStartId = Shader.PropertyToID("_DistanceFadeStart");
        private static readonly int DistanceFadeEndId = Shader.PropertyToID("_DistanceFadeEnd");
        private static readonly int BlendId = Shader.PropertyToID("_Blend");

        public PSXPS2ScreenOutlineSettings settings = new PSXPS2ScreenOutlineSettings();

        [NonSerialized] private Material runtimeMaterial;

        public override void Create()
        {
            EnsureMaterial();
            ConfigureBaseFeature();
            ApplySettingsToMaterial();
            base.Create();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (settings == null || !settings.enabled || settings.intensity <= 0f || settings.thickness <= 0f)
            {
                return;
            }

            EnsureMaterial();
            if (passMaterial == null)
            {
                return;
            }

            ConfigureBaseFeature();
            ApplySettingsToMaterial();
            base.AddRenderPasses(renderer, ref renderingData);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            CoreUtils.Destroy(runtimeMaterial);
            runtimeMaterial = null;
        }

        public void ApplySettingsToMaterial()
        {
            if (passMaterial == null || settings == null)
            {
                return;
            }

            passMaterial.SetColor(OutlineColorId, settings.outlineColor);
            passMaterial.SetFloat(IntensityId, settings.intensity);
            passMaterial.SetFloat(ThicknessId, settings.thickness);
            passMaterial.SetFloat(DepthSensitivityId, settings.depthSensitivity);
            passMaterial.SetFloat(NormalSensitivityId, settings.normalSensitivity);
            passMaterial.SetFloat(DistanceFadeStartId, settings.distanceFadeStart);
            passMaterial.SetFloat(DistanceFadeEndId, settings.distanceFadeEnd);
            passMaterial.SetFloat(BlendId, settings.blend);
        }

        private void ConfigureBaseFeature()
        {
            injectionPoint = settings != null ? settings.passEvent : FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
            fetchColorBuffer = true;
            requirements = ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal;
            passIndex = 0;
            bindDepthStencilAttachment = false;
        }

        private void EnsureMaterial()
        {
            if (passMaterial != null)
            {
                return;
            }

            Shader shader = Shader.Find(ShaderName);
            if (shader == null)
            {
                return;
            }

            runtimeMaterial = CoreUtils.CreateEngineMaterial(shader);
            runtimeMaterial.name = "PSX PS2 Screen Outline (Runtime)";
            runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            passMaterial = runtimeMaterial;
        }
    }
}
