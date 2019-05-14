Shader "My Pipeline/UnlitTexArray" {
	
	Properties {
			 _MainTexA ("TARray", 2Darray) = "" {}
        	 _AuxTexA ("TARray 1", 2Darray) = "" {}
        	 _mag ("mag",Range(0.07,0.25)) = 0.19
        	 _k2 ("Aberation k2",Range(-0.2,0.2)) = 0.0
        	 _kem ("Aberation emulation",Range(-0.2,0.2)) = 0.0
        	 _magOn ("Magnify On",Int) = 0
        	 _calibrate ("Calibrate",Int) = 0
        	 _bandSize("Calibration Band Size",float) = 1.0
        	 _viewer ("Viewer Distance Inverse",Range(-0.5,0.5)) = -0.1
        	 _centripital ("Centripital Adjustment",Range(-0.5,0.5)) = -0.1
        	 _untilt ("if flycam tilts in, shift this much to offset that.",Range(-0.5,0.5)) = 0.0
	}

	SubShader {
		
		Pass {
				Cull Off // two sided, to see projection through screen. Or could use Back
			HLSLPROGRAM
			
			#pragma target 3.5
			
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling
			
			#pragma vertex UnlitTexArrayPassVertex
			#pragma fragment UnlitTexArrayPassFragment
			
			#include "../ShaderLibrary/UnlitTexArray.hlsl"
			
			ENDHLSL
		}
	}
}