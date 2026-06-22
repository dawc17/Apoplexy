using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;

namespace Borzblade.RetroRenderToolkit.Editor
{
    public static class RetroToolkitShaderDiagnostics
    {
        [MenuItem("Tools/Borzblade/Retro Render Toolkit/Run Shader Diagnostics")]
        public static void RunShaderDiagnostics()
        {
            Check(RetroRenderToolkitInstaller.HybridShaderName);
            Check(RetroRenderToolkitInstaller.TerrainShaderName);
            Check(RetroRenderToolkitInstaller.CutoutShaderName);
            Check(RetroRenderToolkitInstaller.FoliageShaderName);
            Check(RetroRenderToolkitInstaller.UnlitCutoutShaderName);
            Check(RetroRenderToolkitInstaller.WaterShaderName);
            Check(RetroRenderToolkitInstaller.MaterialOutlineShaderName);
            Check(RetroRenderToolkitInstaller.SpriteLitShaderName);
            Check(RetroRenderToolkitInstaller.SpriteUnlitShaderName);
            Check(RetroRendererFeature.ShaderName);
            Check(PSXPS2DepthFogRendererFeature.ShaderName);
            Check(PSXPS2ScreenOutlineRendererFeature.ShaderName);
        }

        private static void Check(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError($"[Borzblade Shader Diagnostics] Missing shader: {shaderName}");
                return;
            }

            Debug.Log($"[Borzblade Shader Diagnostics] {(shader.isSupported ? "OK" : "Unsupported")}: {shaderName}");
            foreach (ShaderMessage message in ShaderUtil.GetShaderMessages(shader))
            {
                string text = $"[Borzblade Shader Diagnostics] {shaderName}: {message.severity} line {message.line}: {message.message}";
                if (message.severity == ShaderCompilerMessageSeverity.Error)
                {
                    Debug.LogError(text);
                }
                else
                {
                    Debug.LogWarning(text);
                }
            }
        }
    }
}
