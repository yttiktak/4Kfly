Shader "Custom/ReplacementShader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
	    _Metallic("Metallic", Range(0,1)) = 0.0
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
// add vertex:vert to get control of vertex and z-warp them
		#pragma surface surf Standard  vertex:vert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#pragma debug

		sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
			fixed zed;
			INTERNAL_DATA
		};


		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _ZOpt; // these two are changed via ShaderGlobals
		float _ZMag; // because as a replacement shader, I do not get the 'material' interface.

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			/****
			Shader warning in 'Custom/ReplacementShader': Use of UNITY_MATRIX_MV is detected. 
			To transform a vertex into view space, consider using UnityObjectToViewPos for better performance.'
			handy info at:
			https://gamedev.stackexchange.com/questions/131978/shader-reconstructing-position-from-depth-in-vr-through-projection-matrix/140924#140924

			**/
			float4 hpos = mul(UNITY_MATRIX_MV, v.vertex);
			if (hpos.z != 0) {
				o.zed = (1.0 + _ZMag / _ZOpt) / (1.0 - _ZMag / hpos.z);
			}
			else {
				o.zed = 1.0;
			}
			hpos.xy *= o.zed;
			v.vertex = mul(hpos, UNITY_MATRIX_IT_MV);
		}



		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Alpha = c.a;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
	
			// o.Alpha = 1.0;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
