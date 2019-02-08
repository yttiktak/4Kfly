Shader "My Pipeline/LitTexGeom" {
	
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
		_emissivity("Emissivity",float) = 1
	}
	
	SubShader {
		
		Pass {
			HLSLPROGRAM
			
			#pragma target 5.0
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma multi_compile _ _SHADOWS_HARD
			#pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma vertex LitTexGeomPassVertex
			#pragma geometry LitTexGeomPassGeometry
			#pragma fragment LitTexGeomPassFragment
			
			#include "../ShaderLibrary/LitTexGeom.hlsl"
			
			ENDHLSL
		}
		
		Pass {
			Tags {
				"LightMode" = "ShadowCaster"
			}
			
			HLSLPROGRAM
			
			#pragma target 3.5
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			
			#include "../ShaderLibrary/ShadowCaster.hlsl"
			
			ENDHLSL
		}
	}
}