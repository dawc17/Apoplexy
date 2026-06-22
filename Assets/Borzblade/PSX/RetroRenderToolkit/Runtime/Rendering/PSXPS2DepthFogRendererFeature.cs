using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Borzblade.RetroRenderToolkit
{
    public enum PSXPS2DepthFogBlendMode
    {
        Linear = 0,
        Exponential = 1,
        ExponentialSquared = 2
    }

    [Serializable]
    public sealed class PSXPS2DepthFogSettings
    {
        public bool enabled = true;
        public FullScreenPassRendererFeature.InjectionPoint passEvent = FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
        public Color fogColor = new Color(0.42f, 0.46f, 0.50f, 1f);
        [Range(0f, 1f)] public float intensity = 0.45f;
        public float startDistance = 18f;
        public float endDistance = 85f;
        [Range(0.001f, 1f)] public float density = 0.035f;
        public PSXPS2DepthFogBlendMode blendMode = PSXPS2DepthFogBlendMode.Linear;
        [Range(0f, 32f)] public float steps = 8f;
        [Range(0f, 1f)] public float ditherStrength = 0.08f;
        [Range(0.25f, 8f)] public float ditherScale = 1f;
        public bool affectSky = false;
    }

    [DisallowMultipleRendererFeature]
    public sealed class PSXPS2DepthFogRendererFeature : FullScreenPassRendererFeature
    {
        public const string ShaderName = "Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Depth Fog";

        private static readonly int FogColorId = Shader.PropertyToID("_FogColor");
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int StartDistanceId = Shader.PropertyToID("_StartDistance");
        private static readonly int EndDistanceId = Shader.PropertyToID("_EndDistance");
        private static readonly int DensityId = Shader.PropertyToID("_Density");
        private static readonly int BlendModeId = Shader.PropertyToID("_BlendMode");
        private static readonly int StepsId = Shader.PropertyToID("_Steps");
        private static readonly int DitherStrengthId = Shader.PropertyToID("_DitherStrength");
        private static readonly int DitherScaleId = Shader.PropertyToID("_DitherScale");
        private static readonly int AffectSkyId = Shader.PropertyToID("_AffectSky");

        public PSXPS2DepthFogSettings settings = new PSXPS2DepthFogSettings();

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
            if (settings == null || !settings.enabled || settings.intensity <= 0f)
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

            passMaterial.SetColor(FogColorId, settings.fogColor);
            passMaterial.SetFloat(IntensityId, settings.intensity);
            passMaterial.SetFloat(StartDistanceId, settings.startDistance);
            passMaterial.SetFloat(EndDistanceId, settings.endDistance);
            passMaterial.SetFloat(DensityId, settings.density);
            passMaterial.SetFloat(BlendModeId, (float)settings.blendMode);
            passMaterial.SetFloat(StepsId, settings.steps);
            passMaterial.SetFloat(DitherStrengthId, settings.ditherStrength);
            passMaterial.SetFloat(DitherScaleId, settings.ditherScale);
            passMaterial.SetFloat(AffectSkyId, settings.affectSky ? 1f : 0f);
        }

        private void ConfigureBaseFeature()
        {
            injectionPoint = settings != null ? settings.passEvent : FullScreenPassRendererFeature.InjectionPoint.BeforeRenderingPostProcessing;
            fetchColorBuffer = true;
            requirements = ScriptableRenderPassInput.Depth;
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
            runtimeMaterial.name = "PSX PS2 Depth Fog (Runtime)";
            runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            passMaterial = runtimeMaterial;
        }
    }
}
