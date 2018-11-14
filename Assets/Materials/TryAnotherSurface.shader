Shader "Custom/TryAnotherSurface" {
	Properties {
		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			float4 screenPos;
		};


		sampler2D_float _LastCameraDepthTexture;

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
		}


		void surf (Input IN, inout SurfaceOutputStandard o) {
			fixed4 c;
			float rawZ = 2.0 * SAMPLE_DEPTH_TEXTURE_PROJ(_LastCameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
			if (rawZ > 0.0) {
				c.r = rawZ;
				c.g = 1.0 - rawZ;
				c.b = 0.; //rawZ * rawZ;
			}
			else {
				c.b = 1.0;
			}
			o.Albedo = c.rgb;
			o.Alpha = 1.;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
