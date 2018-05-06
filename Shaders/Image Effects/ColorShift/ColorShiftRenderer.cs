using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    public class ColorShiftRenderer : PostProcessEffectRenderer<ColorShift>
    {
        static class Uniforms
        {
            internal static readonly int _RedShift   = Shader.PropertyToID("_RedShift");
            internal static readonly int _GreenShift = Shader.PropertyToID("_GreenShift");
            internal static readonly int _BlueShift  = Shader.PropertyToID("_BlueShift");
        }
        
        private const string k_ShaderString = "Hidden/PostProcessing/Framework-ColorShift";
        
        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("ColorShift");
            
            var sheet = context.propertySheets.Get(Shader.Find(k_ShaderString));
            sheet.ClearKeywords();
            
            sheet.properties.SetVector(Uniforms._RedShift, new Vector4(
                settings.RedShift.value.x,
                settings.RedShift.value.y,
                0, 0
            ));
            
            sheet.properties.SetVector(Uniforms._GreenShift, new Vector4(
                settings.GreenShift.value.x, 
                settings.GreenShift.value.y, 
                0, 0
            ));
            
            sheet.properties.SetVector(Uniforms._BlueShift, new Vector4(
                settings.BlueShift.value.x,
                settings.BlueShift.value.y,
                0, 0
            ));

            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            
            cmd.EndSample("ColorShift");
        }
    }
}