﻿Shader "Custom/NewSurfaceShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_hydrophobic_color("Hydrophobicity",Vector) = (1,0,-1,0)
		_neutral_color("Neutral",Color) = (0.5,0.5,0.5,0)
		_watery_spot("Watery spot",Vector) = (0,0,0,0)
		_reactive("Reactivity",Float) = 0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
            fixed4 color;
            float3 worldPos;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float4 _hydrophobic_color;
		fixed4 _neutral_color;
		float4 _watery_spot;
		float _reactive;
        
        void vert(inout appdata_full v, out Input o  ) {
        	float factor = 1;
        	float d;
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.color = v.color;
            d = distance(v.vertex.xyz,_watery_spot.xyz);
            factor += _reactive / max(d,1);
            v.vertex.xyz += v.normal * factor * _hydrophobic_color.w * dot(v.color-_neutral_color,_hydrophobic_color);
        }
        
		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = IN.color * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
