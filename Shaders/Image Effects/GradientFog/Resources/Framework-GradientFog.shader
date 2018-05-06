Shader "Hidden/PostProcessing/Framework-GradientFog"
{
    HLSLINCLUDE

        #pragma multi_compile __ FOG_LINEAR FOG_EXP FOG_EXP2
        #include "PostProcessing/Shaders/StdLib.hlsl"
        #include "PostProcessing/Shaders/Builtins/Fog.hlsl"

        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);

        #define SKYBOX_THREASHOLD_VALUE 0.9999
        
        half4 _ColorTop;
        half4 _ColorMid;
        half4 _ColorBot;
        
        float _Blend;
        float _Minimum;
        float _Maximum;
        
        float _Density;
        float _Start;
        float _End;
        
        float4 _CamDir;
        
        half4 ComputeGradientColor(float pos)
        {
            float length = _Maximum - _Minimum;
            float eval   = min(pos, 0.9f);
            
            float center = _Minimum + 0.5f * length;
            float halfBlend = _Blend * 0.5f;
                
            float bottomStrength  = min(max(pos - _Minimum + halfBlend, 0.0f) / _Blend, 1.0f);
            float topStrength     = min(max(pos - _Maximum + halfBlend, 0.0f) / _Blend, 1.0f);
            float bottomStrength2 = min(max(pos - _Minimum + _Blend, 0.0f) / _Blend, 1.0f);
            float topStrength2    = min(max(pos - _Maximum + _Blend, 0.0f) / _Blend, 1.0f);
            
            half4 color  = lerp(_ColorBot, _ColorMid, bottomStrength) * (1 - bottomStrength2);
                  color += lerp(_ColorMid, _ColorTop, topStrength)    * (topStrength2);
        
            color.a = 1;

            return color;
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
            depth = Linear01Depth(depth);
            float dist = ComputeFogDistance(depth);
            half fog = 1.0 - ComputeFog(dist);

            float coeff = sin(60);
            float pos   = ((_CamDir.y + 1) * 0.33f) + (i.texcoordStereo.y) * -coeff;
        
            half4 c = ComputeGradientColor(pos);

            return lerp(color, c, fog);
        }

        float4 FragExcludeSkybox(VaryingsDefault i) : SV_Target
        {
            half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo);

            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
            depth = Linear01Depth(depth);
            float skybox = depth < SKYBOX_THREASHOLD_VALUE;
            float dist = ComputeFogDistance(depth);
            half fog = 1.0 - ComputeFog(dist);
            
            float coeff = sin(60);
            float pos   = ((_CamDir.y + 1) * 0.33f) + (i.texcoordStereo.y) * -coeff;
        
            half4 c = ComputeGradientColor(pos);

            return lerp(color, c, fog * skybox);
        }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment Frag

            ENDHLSL
        }

        Pass
        {
            HLSLPROGRAM

                #pragma vertex VertDefault
                #pragma fragment FragExcludeSkybox

            ENDHLSL
        }
    }
}
