using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    public sealed class GradientFogRenderer : PostProcessEffectRenderer<GradientFog>
    {
        static class Uniforms
        {
            internal static readonly int _FogColor = Shader.PropertyToID("_FogColor");
            internal static readonly int _TopColor = Shader.PropertyToID("_ColorTop");
            internal static readonly int _MidColor = Shader.PropertyToID("_ColorMid");
            internal static readonly int _BottomColor = Shader.PropertyToID("_ColorBot");
            internal static readonly int _BlendFactor = Shader.PropertyToID("_Blend");
            internal static readonly int _Min = Shader.PropertyToID("_Minimum");
            internal static readonly int _Max = Shader.PropertyToID("_Maximum");
            internal static readonly int _CamDir = Shader.PropertyToID("_CamDir");
            internal static readonly int _FogParams = Shader.PropertyToID("_FogParams");

            internal static readonly int _Density = Shader.PropertyToID("_Density");
            internal static readonly int _Start = Shader.PropertyToID("_Start");
            internal static readonly int _End = Shader.PropertyToID("_End");
            internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
        }

        private const string k_ShaderString = "Hidden/PostProcessing/Framework-GradientFog";

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("GradientFog");
            
            var sheet = context.propertySheets.Get(Shader.Find(k_ShaderString));
                sheet.ClearKeywords();
        
            var fogColor = RuntimeUtilities.isLinearColorSpace ? RenderSettings.fogColor.linear : RenderSettings.fogColor;
        
            sheet.properties.SetColor(Uniforms._FogColor, fogColor);
            sheet.properties.SetColor(Uniforms._TopColor, settings.TopColor);
            sheet.properties.SetColor(Uniforms._MidColor, settings.MidColor);
            sheet.properties.SetColor(Uniforms._BottomColor, settings.BottomColor);
        
            sheet.properties.SetFloat(Uniforms._BlendFactor, settings.Blend);
            sheet.properties.SetFloat(Uniforms._Min, settings.GradientSize.value.x);
            sheet.properties.SetFloat(Uniforms._Max, settings.GradientSize.value.y);
            sheet.properties.SetFloat(Uniforms._Density, RenderSettings.fogDensity);
            sheet.properties.SetFloat(Uniforms._Start, RenderSettings.fogStartDistance);
            sheet.properties.SetFloat(Uniforms._End, RenderSettings.fogEndDistance);
        
            sheet.properties.SetVector(Uniforms._CamDir, context.camera.transform.forward.normalized);
            sheet.properties.SetVector(Uniforms._FogParams, new Vector3(RenderSettings.fogDensity, RenderSettings.fogStartDistance, RenderSettings.fogEndDistance));

            switch (RenderSettings.fogMode)
            {
                case FogMode.Linear:
                    sheet.EnableKeyword("FOG_LINEAR");
                    break;
                case FogMode.Exponential:
                    sheet.EnableKeyword("FOG_EXP");
                    break;
                case FogMode.ExponentialSquared:
                    sheet.EnableKeyword("FOG_EXP2");
                    break;
            }

            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, settings.ExcludeSkybox ? 1 : 0);
            
            cmd.EndSample("GradientFog");
        }
    }
}