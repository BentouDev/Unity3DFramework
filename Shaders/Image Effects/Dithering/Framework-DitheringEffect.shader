Shader "Hidden/Framework/PostProcessing/Dithering"
{
    HLSLINCLUDE

        #include "PostProcessing/Shaders/StdLib.hlsl"
        #include "../../Framework-Dithering.cginc"
        
        TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);

		sampler2D _Pattern;
		float _DitherSize;
		float2 _TextureScale;
		float _First;
		float _Second;
		float _Third;
		
		struct _DitherVaryings
		{
		    float4 vertex : SV_POSITION;
		    float2 texcoord : TEXCOORD0;
		};
		
		_DitherVaryings Vert(AttributesDefault a)
		{
		    _DitherVaryings o;
            o.vertex = float4(a.vertex.xy, 0.0, 1.0);
            o.texcoord = TransformTriangleVertexToUV(a.vertex.xy);
        
        #if UNITY_UV_STARTS_AT_TOP
            o.texcoord = o.texcoord * float2(1.0, -1.0) + float2(0.0, 1.0);
        #endif
            
            return o;
		}
		
		#ifdef UNITY_COLORSPACE_GAMMA
		#define unity_ColorSpaceLuminance half4(0.22, 0.707, 0.071, 0.0) // Legacy: alpha is set to 0.0 to specify gamma mode
		#else
		#define unity_ColorSpaceLuminance half4(0.0396819152, 0.458021790, 0.00609653955, 1.0) // Legacy: alpha is set to 1.0 to specify linear mode
		#endif
		
        inline half Luminance(half3 rgb)
        {
            return dot(rgb, unity_ColorSpaceLuminance.rgb);
        }
		
		float4 Frag(_DitherVaryings i) : SV_Target
		{
            half4 src = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
            half4 dither = tex2D(_Pattern, (fmod(i.texcoord * _ScreenParams.xy * _TextureScale, _DitherSize) / _DitherSize) / _TextureScale);
            
            float  luminance = Luminance(src);
            float4 dryColor = float4(0,luminance,luminance,0);
            
            float4 finalColor = floor((src * _First) + (luminance * dither.r * _Second)) / _Third;
            
            return finalColor;
		}
    
    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

                #pragma vertex Vert
                #pragma fragment Frag

            ENDHLSL
        }
    }
}
