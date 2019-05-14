Shader "My Pipeline/LitVertexColors" {
	
	Properties {
		_Color ("Color", Color) = (1, 1, 1, 1)
		_hydrophobic_color("Hydrophobicity",Vector) = (1,0,-1,0)
        _neutral_color("Neutral",Color) = (0.5,0.5,0.5,0)
       	_watery_spot("Watery spot",Vector) = (0,0,0,0)
        _reactive("Reactivity",Float) = 0
	}
	
	SubShader {
		
		Pass {
			HLSLPROGRAM
			
			#pragma target 3.5
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma multi_compile _ _SHADOWS_HARD
			#pragma multi_compile _ _SHADOWS_SOFT
			
			#pragma vertex LitVertexColorsPassVertex
			#pragma fragment LitVertexColorsPassFragment
			
			#include "../ShaderLibrary/LitVertexColors.hlsl"
			
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