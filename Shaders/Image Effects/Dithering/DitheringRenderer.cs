using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    public class DitheringRenderer : PostProcessEffectRenderer<FullscreenDithering>
    {
        static class Uniforms
        {
            internal static readonly int Pattern = Shader.PropertyToID("_Pattern");
            internal static readonly int DitherSize = Shader.PropertyToID("_DitherSize");
            internal static readonly int First = Shader.PropertyToID("_First");
            internal static readonly int Second = Shader.PropertyToID("_Second");
            internal static readonly int Third = Shader.PropertyToID("_Third");
            internal static readonly int TextureScale = Shader.PropertyToID("_TextureScale");
        }

        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }

        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Dithering");
            {
                var sheet = context.propertySheets.Get(Shader.Find("Hidden/Framework/PostProcessing/Dithering"));
            
                if (settings.Pattern.value)
                    sheet.properties.SetTexture(Uniforms.Pattern, settings.Pattern);
            
                sheet.properties.SetFloat(Uniforms.DitherSize, settings.DitherSize);
                sheet.properties.SetFloat(Uniforms.First, settings.OriginalContribution);
                sheet.properties.SetFloat(Uniforms.Second, settings.DitherContribution);
                sheet.properties.SetFloat(Uniforms.Third, settings.NominalValue);

                var scale = settings.TextureScale.GetValue<Vector2>();
                
                sheet.properties.SetVector(Uniforms.TextureScale, new Vector4(scale.x, scale.y, 0, 0));

                cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);                
            }
            cmd.EndSample("Dithering");
        }
    }
}