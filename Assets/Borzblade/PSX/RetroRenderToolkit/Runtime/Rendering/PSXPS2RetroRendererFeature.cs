using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Compatibility wrapper. This file keeps the original MonoScript GUID used by
// existing renderer feature sub-assets while the implementation lives in the
// Borzblade namespace.
[DisallowMultipleRendererFeature]
public sealed class PSXPS2RetroRendererFeature : Borzblade.RetroRenderToolkit.RetroRendererFeature
{
}

namespace Borzblade.RetroRenderToolkit
{
    public enum PSXPS2PixelationMode
    {
        Scale = 0,
        FixedVerticalResolution = 1
    }

    public enum PSXPS2DitherPatternMode
    {
        Procedural = 0,
        Texture = 1
    }

    [Serializable]
    public sealed class PSXPS2RetroGlobalSettings
    {
        public bool enabled = true;
        [Range(0f, 1f)] public float intensity = 0.65f;
        [Range(1f, 12f)] public float pixelScale = 1f;
        public PSXPS2PixelationMode pixelationMode = PSXPS2PixelationMode.Scale;
        [Range(64f, 1080f)] public float fixedVerticalResolution = 240f;
        [Range(2f, 64f)] public float colorSteps = 28f;
        [Range(0f, 1f)] public float ditherStrength = 0.12f;
        public PSXPS2DitherPatternMode ditherPatternMode = PSXPS2DitherPatternMode.Procedural;
        public Texture2D ditherPatternTexture;
        [Range(0.25f, 16f)] public float ditherPatternScale = 1f;
        [Range(0f, 1f)] public float ditherThreshold = 0.5f;
        [Range(0f, 1f)] public float scanlineStrength = 0.08f;
        [Range(0f, 1f)] public float vignetteStrength = 0.12f;
        [Range(0f, 2f)] public float saturation = 1.05f;
        [Range(0f, 2f)] public float contrast = 1.05f;
        [Range(0f, 1f)] public float bleed = 0.08f;
        public Color colorTint = Color.white;
        [Range(0.2f, 3f)] public float gamma = 1f;
        [Range(0f, 1f)] public float blackLevel = 0f;
        [Range(0.25f, 8f)] public float ditherScale = 1f;
        [Range(0f, 1f)] public float crtMaskStrength = 0f;
        [Range(0f, 1f)] public float chromaticOffset = 0f;
        [Range(0f, 1f)] public float noiseStrength = 0f;
        [Range(0f, 1f)] public float horizontalJitter = 0f;
        [Range(0f, 1f)] public float curvature = 0f;
        public bool globalFogEnabled = false;
        public Color globalFogColor = new Color(0.42f, 0.46f, 0.50f, 1f);
        [Range(0f, 1f)] public float globalFogIntensity = 0f;
    }

    [DisallowMultipleRendererFeature]
    public class RetroRendererFeature : FullScreenPassRendererFeature
    {
        public const string ShaderName = "Hidden/Borzblade/Retro Render Toolkit/PSX PS2 Global Pass";

        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
        private static readonly int PixelScaleId = Shader.PropertyToID("_PixelScale");
        private static readonly int PixelationModeId = Shader.PropertyToID("_PixelationMode");
        private static readonly int FixedVerticalResolutionId = Shader.PropertyToID("_FixedVerticalResolution");
        private static readonly int ColorStepsId = Shader.PropertyToID("_ColorSteps");
        private static readonly int DitherStrengthId = Shader.PropertyToID("_DitherStrength");
        private static readonly int DitherPatternModeId = Shader.PropertyToID("_DitherPatternMode");
        private static readonly int DitherPatternTextureId = Shader.PropertyToID("_DitherPatternTexture");
        private static readonly int DitherPatternScaleId = Shader.PropertyToID("_DitherPatternScale");
        private static readonly int DitherThresholdId = Shader.PropertyToID("_DitherThreshold");
        private static readonly int ScanlineStrengthId = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int VignetteStrengthId = Shader.PropertyToID("_VignetteStrength");
        private static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        private static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        private static readonly int BleedId = Shader.PropertyToID("_Bleed");
        private static readonly int ColorTintId = Shader.PropertyToID("_ColorTint");
        private static readonly int GammaId = Shader.PropertyToID("_Gamma");
        private static readonly int BlackLevelId = Shader.PropertyToID("_BlackLevel");
        private static readonly int DitherScaleId = Shader.PropertyToID("_DitherScale");
        private static readonly int CrtMaskStrengthId = Shader.PropertyToID("_CrtMaskStrength");
        private static readonly int ChromaticOffsetId = Shader.PropertyToID("_ChromaticOffset");
        private static readonly int NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        private static readonly int HorizontalJitterId = Shader.PropertyToID("_HorizontalJitter");
        private static readonly int CurvatureId = Shader.PropertyToID("_Curvature");
        private static readonly int GlobalFogEnabledId = Shader.PropertyToID("_GlobalFogEnabled");
        private static readonly int GlobalFogColorId = Shader.PropertyToID("_GlobalFogColor");
        private static readonly int GlobalFogIntensityId = Shader.PropertyToID("_GlobalFogIntensity");

        public PSXPS2RetroGlobalSettings settings = new PSXPS2RetroGlobalSettings();

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
            // Camera stacks invoke renderer features for each camera. Apply the
            // full-screen retro effect only after the final camera has composed
            // the stack, otherwise its color processing is applied repeatedly.
            if (!renderingData.cameraData.resolveFinalTarget)
            {
                return;
            }

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

            passMaterial.SetFloat(IntensityId, settings.intensity);
            passMaterial.SetFloat(PixelScaleId, settings.pixelScale);
            passMaterial.SetFloat(PixelationModeId, (float)settings.pixelationMode);
            passMaterial.SetFloat(FixedVerticalResolutionId, settings.fixedVerticalResolution);
            passMaterial.SetFloat(ColorStepsId, settings.colorSteps);
            passMaterial.SetFloat(DitherStrengthId, settings.ditherStrength);
            passMaterial.SetFloat(DitherPatternModeId, settings.ditherPatternMode == PSXPS2DitherPatternMode.Texture && settings.ditherPatternTexture != null ? 1f : 0f);
            if (settings.ditherPatternTexture != null)
            {
                passMaterial.SetTexture(DitherPatternTextureId, settings.ditherPatternTexture);
            }
            passMaterial.SetFloat(DitherPatternScaleId, settings.ditherPatternScale);
            passMaterial.SetFloat(DitherThresholdId, settings.ditherThreshold);
            passMaterial.SetFloat(ScanlineStrengthId, settings.scanlineStrength);
            passMaterial.SetFloat(VignetteStrengthId, settings.vignetteStrength);
            passMaterial.SetFloat(SaturationId, settings.saturation);
            passMaterial.SetFloat(ContrastId, settings.contrast);
            passMaterial.SetFloat(BleedId, settings.bleed);
            passMaterial.SetColor(ColorTintId, settings.colorTint);
            passMaterial.SetFloat(GammaId, settings.gamma);
            passMaterial.SetFloat(BlackLevelId, settings.blackLevel);
            passMaterial.SetFloat(DitherScaleId, settings.ditherScale);
            passMaterial.SetFloat(CrtMaskStrengthId, settings.crtMaskStrength);
            passMaterial.SetFloat(ChromaticOffsetId, settings.chromaticOffset);
            passMaterial.SetFloat(NoiseStrengthId, settings.noiseStrength);
            passMaterial.SetFloat(HorizontalJitterId, settings.horizontalJitter);
            passMaterial.SetFloat(CurvatureId, settings.curvature);
            passMaterial.SetFloat(GlobalFogEnabledId, settings.globalFogEnabled ? 1f : 0f);
            passMaterial.SetColor(GlobalFogColorId, settings.globalFogColor);
            passMaterial.SetFloat(GlobalFogIntensityId, settings.globalFogIntensity);
        }

        private void ConfigureBaseFeature()
        {
            injectionPoint = FullScreenPassRendererFeature.InjectionPoint.AfterRenderingPostProcessing;
            fetchColorBuffer = true;
            requirements = ScriptableRenderPassInput.None;
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
            runtimeMaterial.name = "PSX PS2 Retro Global Pass (Runtime)";
            runtimeMaterial.hideFlags = HideFlags.HideAndDontSave;
            passMaterial = runtimeMaterial;
        }
    }
}
