// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Hidden/Post FX/GradientFog"
{
	Properties
	{
		_MainTex("Main Texture", 2D) = "white" {}
		_ColorTop("Top Color", Color) = (1,1,1,1)
		_ColorMid("Mid Color", Color) = (1,1,1,1)
		_ColorBot("Bot Color", Color) = (1,1,1,1)
		_Middle("Middle", Range(0.001, 0.999)) = 1
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

	float _Middle;
	float _Minimum;
	float _Maximum;

	float _Density;
	float _Start;
	float _End;

	float4 _CamDir;

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
		
		// fixed4 color = _ColorTop * pos + _ColorBot * (1 - pos);

		float center  = _Minimum + 0.5f * length;
		float halfMid = _Middle * 0.5f;

		

		float bottomStrength = 1 - (max(pos - _Minimum, 0) / 0.5f * length);
		float topStrength = min(1, (pos - center) / 0.5f * length);

		// This is good enought, but better blending would be handy

		// fixed4 color = fixed4(min(max(pos - _Maximum + halfMid, 0) / _Middle, 1), 0, 0, 1);

		fixed4 color = lerp(_ColorBot, _ColorMid, min(max(pos - _Minimum + halfMid, 0) / _Middle, 1)) * (1-min(max(pos - _Minimum + halfMid, 0) / _Middle, 1));// step(pos, center);// _Minimum + 0.5f * _Middle);
		       color += lerp(_ColorMid, _ColorTop, min(max(pos - _Maximum + halfMid, 0) / _Middle, 1)) * (min(max(pos - _Maximum + halfMid, 0) / _Middle, 1)); // step(center, pos);// * step(_Maximum - 0.5f * _Middle, pos);

		//fixed4 c  = lerp(_ColorBot, _ColorMid, i.texcoord.y / _Middle) * step(i.texcoord.y, _Middle);
		//	     c += lerp(_ColorMid, _ColorTop, (i.texcoord.y - _Middle) / (1 - _Middle)) * step(_Middle, i.texcoord.y);

		color.a = 1;
		
		/*if (pos < _Minimum - length * 0.5f)
			color = _ColorBot;

		if (pos > _Minimum - length * 0.5f && pos < _Minimum + length * 0.5f)
			color = lerp(_ColorBot, _ColorMid, (pos - _Minimum - length * 0.5f) / (length));*/

			 //  color += lerp(_ColorTop, _ColorMid, (_Maximum - pos) / _Middle) * step(pos, _Maximum - _Middle);


		// This is something, regions are working but colors are broken
		/*fixed4	color = lerp(_ColorBot, _ColorMid, (pos - _Minimum) / length) * step(pos, _Maximum);
				color += lerp(_ColorTop, _ColorMid, (pos - _Minimum) / length) * step(_Minimum, pos);*/

					 // + lerp(_Color)

		//fixed4 c = lerp(_ColorBot, _ColorMid, pos / length) * step(pos, _Maximum);
		//c += lerp(_ColorMid, _ColorTop, (pos - length) / (1 - length)) * step(_Minimum, pos);
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

		float pos = ((_CamDir.y + 1) * 0.33f) + (i.uv.y) * -coeff;

		// float3 pos = float3(_CamDir.xyz + float3(i.vertex.xy, 0));
		// float3 pos = _WorldSpaceCameraPos + float3(UNITY_MATRIX_IT_MV[2].xyz + float3(i.vertex.xy, 0));

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

		float pos = ((_CamDir.y + 1) * 0.33f) + (i.uv.y) * -coeff;

		// float3 pos = float3(_CamDir.xyz + float3(i.vertex.xy, 0));
		// float3 pos = _WorldSpaceCameraPos + float3(UNITY_MATRIX_IT_MV[2].xyz + float3(i.vertex.xy, 0));

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