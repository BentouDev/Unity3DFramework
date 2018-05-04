Shader "Hidden/PostProcessing/Framework-ColorShift"
{
    HLSLINCLUDE

        #include "PostProcessing/Shaders/StdLib.hlsl"
        
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        
        // Parameters
	    float2 _RedShift;
	    float2 _GreenShift;
	    float2 _BlueShift;
	    
        float4 FragBroken(VaryingsDefault i) : SV_Target
        {
            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
                  depth = Linear01Depth(depth);
                  
            half4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo.xy);
            
            float2 pls_uv = i.texcoordStereo.xy;
            float2 min_uv = i.texcoordStereo.xy;

            if (min_uv.x > 0.5f)
            {
                min_uv *= _GreenShift;
            }
            else
            {
                min_uv.x += 0.5f;
                min_uv *= _GreenShift - float2(0.5f,0);
            }
            
            float red_minus = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, min_uv).r;
            
            //float red_plus_a =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (pls_uv - _RedShift) / (1 - _RedShift)).r;
            //float red_plus_b =  SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, (pls_uv + _RedShift) * (1 - _RedShift)).r;
            
            return source + float4(red_minus, 0, 0, 0);
        }
        
        float4 Frag(VaryingsDefault i) : SV_Target
        {
            half4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo.xy);
            
            float red   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo + _RedShift).r;
            float green = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo + _GreenShift).g;
            float blue  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo + _BlueShift).b;
            
            return source * 0.5f + float4(red, green, blue, 1);
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
    }
}