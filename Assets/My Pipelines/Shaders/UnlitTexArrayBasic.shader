Shader "My Pipeline/UnlitTexArrayBasic" {
	
	Properties {
			 _MainTexA ("TARray", 2Darray) = "" {}
	}

	SubShader {
		
		Pass {
				Cull Off // two sided, to see projection through screen. Or could use Back
			HLSLPROGRAM
			
			#pragma target 3.5
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma vertex UnlitTexArrayBasicPassVertex
			#pragma fragment UnlitTexArrayBasicPassFragment
			
			#include "../ShaderLibrary/UnlitTexArrayBasic.hlsl"
			
			ENDHLSL
		}
	}
}