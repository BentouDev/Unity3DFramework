Shader "Hidden/PostProcessing/Framework-Glitch"
{
    HLSLINCLUDE

        #include "PostProcessing/Shaders/StdLib.hlsl"
        
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
        TEXTURE2D_SAMPLER2D(_CameraDepthTexture, sampler_CameraDepthTexture);
        
        // Parameters
	    sampler2D _DispTex;
	    float _Intensity; // Range(0.1, 1.0)
        float _ColorIntensity; // Range(0.1, 1.0)
        float _NoiseDistance;
        
        // Variables
		float4 direction;

		float filterRadius;
		float flip_up, flip_down;
		float displace;
        float scale;

        float rand3(float3 co)
        {
            return frac(sin( dot(co.xyz ,float3(12.9898,78.233,45.5432) )) * 43758.5453);
        }
        
        float rand2(float2 co)
        {
            return frac(sin( dot(co.xy ,float2(12.9898,78.233) )) * 43758.5453);
        }

        float4 Frag(VaryingsDefault i) : SV_Target
        {
            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, sampler_CameraDepthTexture, i.texcoordStereo);
                  depth = Linear01Depth(depth);
            
            half4 normal = tex2D (_DispTex, i.texcoordStereo.xy * scale);
            half4 source = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo.xy);
			
			i.texcoordStereo.y  -= (1 - (i.texcoordStereo.y + flip_up)) * step(i.texcoordStereo.y, flip_up) + (1 - (i.texcoordStereo.y - flip_down)) * step(flip_down, i.texcoordStereo.y);
			i.texcoordStereo.xy += (normal.xy - 0.5) * displace * _Intensity;
			
			half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo.xy);
			half4 redcolor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo.xy + direction.xy * 0.01 * filterRadius * _ColorIntensity);
			half4 greencolor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoordStereo.xy - direction.xy * 0.01 * filterRadius * _ColorIntensity);

			color += float4(redcolor.r, redcolor.b, redcolor.g, 1) * step(filterRadius, -0.001);
			color *= 1 - 0.5 * step(filterRadius, -0.001);

			color += float4(greencolor.g, greencolor.b, greencolor.r, 1) * step(0.001, filterRadius);
			color *= 1 - 0.5 * step(0.001, filterRadius);
			
            float noiseStrength = max(0, depth - _NoiseDistance) / (1 - _NoiseDistance);
            return lerp(source, color, depth);
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