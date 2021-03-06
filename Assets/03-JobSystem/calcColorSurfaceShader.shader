﻿Shader "Custom/calcColorSurfaceShader" {
	Properties {
		_Hue("Hue", Float) = 0.1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Standard fullforwardshadows
		#pragma target 3.0

		struct Input {
			float3 worldPos;
		};
		
		UNITY_INSTANCING_BUFFER_START(Props)
			half _Hue;
		UNITY_INSTANCING_BUFFER_END(Props)
		
		half3 Hue2RGB(half h)
		{
			h = frac(h) * 6 - 2;
			half3 rgb = saturate(half3(abs(h - 1) - 1, 2 - abs(h), 2 - abs(h - 2)));
			return rgb;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{
			half3 c = Hue2RGB(UNITY_ACCESS_INSTANCED_PROP(Props, _Hue));
			o.Albedo = c.rgb;
			
			o.Metallic = 0;
			o.Smoothness = 0;
			o.Alpha = 1;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
