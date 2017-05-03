// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hidden/Post FX/GradientFog"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Blend("Blend Factor", Range(0.001, 0.999)) = 1
		_Minimum("Minimum", Range(0.001, 0.999)) = 1
		_Maximum("Maximum", Range(0.001, 0.999)) = 1
		_CamDir("Camera Direction", Vector) = (0,0,0,0)
	}

	CGINCLUDE

	#pragma multi_compile __ FOG_LINEAR FOG_EXP FOG_EXP2
	#include "UnityCG.cginc"
	#include "../../../../PostProcessing/Resources/Shaders/Common.cginc"

	#define SKYBOX_THREASHOLD_VALUE 0.9999

	struct Varyings
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};

	Varyings VertFog(AttributesDefault v)
	{
		Varyings o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = UnityStereoScreenSpaceUVAdjust(v.texcoord, _MainTex_ST);
		return o;
	}

	sampler2D _CameraDepthTexture;

	half4 _FogColor;

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
	float _CamFov;

	half ComputeFog(float z)
	{
		half fog = 0.0;
#if FOG_LINEAR
		fog = (_End - z) / (_End - _Start);
#elif FOG_EXP
		fog = exp2(-_Density * z);
#else // FOG_EXP2
		fog = _Density * z;
		fog = exp2(-fog * fog);
#endif
		return saturate(fog);
	}

	float ComputeDistance(float depth)
	{
		float dist = depth * _ProjectionParams.z;
		dist -= _ProjectionParams.y;
		return dist;
	}

	fixed4 ComputeFogColor(float pos)
	{
		float length = _Maximum - _Minimum;

		pos = min(pos, 0.9f);
		
		float center    = _Minimum + 0.5f * length;
		float halfBlend = _Blend * 0.5f;

		float bottomStrength = min(max(pos - _Minimum + halfBlend, 0) / _Blend, 1);
		float topStrength    = min(max(pos - _Maximum + halfBlend, 0) / _Blend, 1);

		fixed4 color  = lerp(_ColorBot, _ColorMid, bottomStrength) * (1 - bottomStrength);
		       color += lerp(_ColorMid, _ColorTop, topStrength)    * (topStrength);

		color.a = 1;

		return (color);
	}

	half4 FragFog(Varyings i) : SV_Target
	{
		half4 color = tex2D(_MainTex, i.uv);

		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
		depth = Linear01Depth(depth);
		float dist = ComputeDistance(depth);
		half fog = 1.0 - ComputeFog(dist);

		float coeff = sin(60);
		float pos   = ((_CamDir.y + 1) * 0.33f) + (i.uv.y) * -coeff;

		fixed4 c = ComputeFogColor(pos);

		return lerp(color, c, fog);
	}

	half4 FragFogExcludeSkybox(Varyings i) : SV_Target
	{
		half4 color = tex2D(_MainTex, i.uv);

		float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
		depth = Linear01Depth(depth);
		float skybox = depth < SKYBOX_THREASHOLD_VALUE;
		float dist = ComputeDistance(depth);
		half fog = 1.0 - ComputeFog(dist);

		float coeff = sin(60);
		float pos   = ((_CamDir.y + 1) * 0.33f) + (i.uv.y) * -coeff;

		fixed4 c = ComputeFogColor(pos);

		return lerp(color, c, fog * skybox);
	}

	ENDCG

	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM

			#pragma vertex VertFog
			#pragma fragment FragFog

			ENDCG
		}

		Pass
		{
			CGPROGRAM

			#pragma vertex VertFog
			#pragma fragment FragFogExcludeSkybox

			ENDCG
		}
	}
}