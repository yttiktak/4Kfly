Shader "My Pipeline/LitTexArray" {
	
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_MainTexA("TextureArray", 2DArray) = "white" {}
		_Slice("Slice",int) = 0
		_emissivity("Emissivity",float) = 1
	}
	
	SubShader {
		
		Pass {
			HLSLPROGRAM
			
			#pragma target 3.5
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma multi_compile _ _SHADOWS_HARD
			#pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma vertex LitTexArrayPassVertex
			#pragma fragment LitTexArrayPassFragment
			
			#include "../ShaderLibrary/LitTexArray.hlsl"
			
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