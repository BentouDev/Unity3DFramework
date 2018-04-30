using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    public sealed class GradientFogComponent : PostProcessEffectRenderer<GradientFogModel>
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

            internal static readonly int _Density = Shader.PropertyToID("_Density");
            internal static readonly int _Start = Shader.PropertyToID("_Start");
            internal static readonly int _End = Shader.PropertyToID("_End");
            internal static readonly int _TempRT = Shader.PropertyToID("_TempRT");
        }

        const string k_ShaderString = "Hidden/Post FX/GradientFog";

//        public override string GetName()
//        {
//            return "GradientFog";
//        }
//
//        public override DepthTextureMode GetCameraFlags()
//        {
//            return DepthTextureMode.Depth;
//        }
//
//        public override CameraEvent GetCameraEvent()
//        {
//            return CameraEvent.AfterImageEffectsOpaque;
//        }

        public override void Render(PostProcessRenderContext context)
        {
//            var cmd = context.command;
//            var settings = model.settings;
//
//            var material = context.materialFactory.Get(k_ShaderString);
//            material.shaderKeywords = null;
//            var fogColor = GraphicsUtils.isLinearColorSpace ? RenderSettings.fogColor.linear : RenderSettings.fogColor;
//
//            material.SetColor(Uniforms._FogColor, fogColor);
//            material.SetColor(Uniforms._TopColor, model.settings.TopColor);
//            material.SetColor(Uniforms._MidColor, model.settings.MidColor);
//            material.SetColor(Uniforms._BottomColor, model.settings.BottomColor);
//
//            material.SetFloat(Uniforms._BlendFactor, model.settings.Blend);
//            material.SetFloat(Uniforms._Min, model.settings.Minimum);
//            material.SetFloat(Uniforms._Max, model.settings.Maximum);
//            material.SetFloat(Uniforms._Density, RenderSettings.fogDensity);
//            material.SetFloat(Uniforms._Start, RenderSettings.fogStartDistance);
//            material.SetFloat(Uniforms._End, RenderSettings.fogEndDistance);
//
//            material.SetVector(Uniforms._CamDir, context.camera.transform.forward.normalized);
//
//            switch (RenderSettings.fogMode)
//            {
//                case FogMode.Linear:
//                    material.EnableKeyword("FOG_LINEAR");
//                    break;
//                case FogMode.Exponential:
//                    material.EnableKeyword("FOG_EXP");
//                    break;
//                case FogMode.ExponentialSquared:
//                    material.EnableKeyword("FOG_EXP2");
//                    break;
//            }
//
//            var fbFormat = context.isHdr
//                ? RenderTextureFormat.DefaultHDR
//                : RenderTextureFormat.Default;
//
//            cb.GetTemporaryRT(Uniforms._TempRT, context.width, context.height, 24, FilterMode.Bilinear, fbFormat);
//            cb.Blit(BuiltinRenderTextureType.CameraTarget, Uniforms._TempRT);
//            cb.Blit(Uniforms._TempRT, BuiltinRenderTextureType.CameraTarget, material, settings.excludeSkybox ? 1 : 0);
//            cb.ReleaseTemporaryRT(Uniforms._TempRT);
        }
    }
}