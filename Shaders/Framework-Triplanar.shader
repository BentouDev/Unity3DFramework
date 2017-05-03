Shader "Framework/Triplanar" {
	Properties {
		_SideColor("Side Color", Color) = (1,1,1,1)
		_Side("Side", 2D) = "white" {}
		_TopColor("Top Color", Color) = (1,1,1,1)
		_Top("Top", 2D) = "white" {}
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_Bottom("Bottom", 2D) = "white" {}

		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_Alpha ("Alpha", Range(0,1)) = 0.0
	}
	SubShader { 
		Tags {
			"Queue" = "Geometry"
			"IgnoreProjector" = "False"
			"RenderType" = "Opaque"
		}
	
		Cull Back
		ZWrite On
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows
		#pragma exclude_renderers flash

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		fixed4 _SideColor, _TopColor, _BottomColor;
		sampler2D _Side, _Top, _Bottom;
		float4  _Side_ST, _Top_ST, _Bottom_ST;
		half2 _SideScale, _TopScale, _BottomScale;

		struct Input {
			float3 worldPos;
			float3 worldNormal;
		};

		half _Glossiness;
		half _Metallic;
		half _Alpha;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float3 projNormal = saturate(pow(IN.worldNormal * 1.4, 4));

			// SIDE X
			float3 x = tex2D(_Side, (_Side_ST.xy * IN.worldPos.zy + _Side_ST.zw)) * abs(IN.worldNormal.x) * _SideColor;

			// TOP / BOTTOM
			float3 y = 0;
			if (IN.worldNormal.y > 0) {
				y = tex2D(_Top, (_Top_ST.xy * IN.worldPos.zx + _Top_ST.zw)) * abs(IN.worldNormal.y) * _TopColor;
			}
			else {
				y = tex2D(_Bottom, (_Bottom_ST.xy * IN.worldPos.zx + _Bottom_ST.zw)) * abs(IN.worldNormal.y) * _BottomColor;
			}

			// SIDE Z	
			float3 z = tex2D(_Side, (_Side_ST.xy * IN.worldPos.xy + _Side_ST.zw)) * abs(IN.worldNormal.z) * _SideColor;

			o.Albedo = z;
			o.Albedo = lerp(o.Albedo, x, projNormal.x);
			o.Albedo = lerp(o.Albedo, y, projNormal.y);

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = _Alpha;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
