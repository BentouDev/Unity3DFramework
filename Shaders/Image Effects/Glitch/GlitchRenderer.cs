using UnityEngine;
using System;
using UnityEngine.Rendering.PostProcessing;

namespace UnityEngine.Rendering.PostProcessing
{
    public class GlitchRenderer : PostProcessEffectRenderer<Glitch>
    {
        static class Uniforms
        {
            internal static readonly int _Displacement  = Shader.PropertyToID("_DispTex");
            internal static readonly int _Strength      = Shader.PropertyToID("_Intensity");
            internal static readonly int _ColorBleed    = Shader.PropertyToID("_ColorIntensity");
            internal static readonly int _NoiseDistance = Shader.PropertyToID("_NoiseDistance");
        }
        
        private const string k_ShaderString = "Hidden/PostProcessing/Framework-Glitch";

        float glitchup, glitchdown, flicker,
            glitchupTime = 0.05f, glitchdownTime = 0.05f, flickerTime = 0.5f;
        
        public override DepthTextureMode GetCameraFlags()
        {
            return DepthTextureMode.Depth;
        }
        
        public override void Render(PostProcessRenderContext context)
        {
            var cmd = context.command;
            cmd.BeginSample("Glitch");
            
            var sheet = context.propertySheets.Get(Shader.Find(k_ShaderString));
            sheet.ClearKeywords();
            
            if (settings.Displacement.value)
                sheet.properties.SetTexture(Uniforms._Displacement, settings.Displacement);
            
            sheet.properties.SetFloat(Uniforms._NoiseDistance, settings.NoiseDistance);
            sheet.properties.SetFloat(Uniforms._ColorBleed,    settings.ColorBleed);
            sheet.properties.SetFloat(Uniforms._Strength,      settings.Strength);

            {
                flicker += Time.deltaTime * settings.ColorBleed;
                
                if (flicker > flickerTime)
                {
                    sheet.properties.SetFloat("filterRadius", Random.Range(-3f, 3f) * settings.ColorBleed);
                    sheet.properties.SetVector("direction", Quaternion.AngleAxis(Random.Range(0, 360) * settings.ColorBleed, Vector3.forward) * Vector4.one);
                    
                    flicker = 0;
                    flickerTime = Random.value;
                }

                if (Math.Abs(settings.ColorBleed.value) < Mathf.Epsilon)
                    sheet.properties.SetFloat("filterRadius", 0);
        
                glitchup += Time.deltaTime * settings.FlipIntensity;
                
                if (glitchup > glitchupTime)
                {
                    if (Random.value < 0.1f * settings.FlipIntensity)
                        sheet.properties.SetFloat("flip_up", Random.Range(0, 1f) * settings.FlipIntensity);
                    else
                        sheet.properties.SetFloat("flip_up", 0);
			
                    glitchup = 0;
                    glitchupTime = Random.value/10f;
                }

                if (Math.Abs(settings.FlipIntensity.value) < Mathf.Epsilon)
                    sheet.properties.SetFloat("flip_up", 0);

                glitchdown += Time.deltaTime * settings.FlipIntensity;
                
                if (glitchdown > glitchdownTime)
                {
                    if (Random.value < 0.1f * settings.FlipIntensity)
                        sheet.properties.SetFloat("flip_down", 1 - Random.Range(0, 1f) * settings.FlipIntensity);
                    else
                        sheet.properties.SetFloat("flip_down", 1);
			
                    glitchdown = 0;
                    glitchdownTime = Random.value/10f;
                }

                if (Math.Abs(settings.FlipIntensity.value) < Mathf.Epsilon)
                    sheet.properties.SetFloat("flip_down", 1);

                if (Random.value < 0.05 * settings.Strength)
                {
                    sheet.properties.SetFloat("displace", Random.value * settings.Strength);
                    sheet.properties.SetFloat("scale", 1 - Random.value * settings.Strength);
                }
                else
                    sheet.properties.SetFloat("displace", 0);
            }

            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
            
            cmd.EndSample("Glitch");
        }
    }
}