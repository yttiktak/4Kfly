#ifndef MYRP_UNLITYEXARRAY_INCLUDED
#define MYRP_UNLITYEXARRAY_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"


struct VertexInput {
	float4 pos : POSITION;
	float2 uv : TEXCOORD0; // traditional UV paints the hex cell from texture
	float2 uv2 : TEXCOORD1; // use uv2 to get which slice to use (it is a textureArray)
	float2 uv3 : TEXCOORD2; // use uv3 to indicate actual position in the lens array
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	float3 uv : TEXCOORD0;
    float3 uv2 : TEXCOORD1;
    float3 uv3 : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput UnlitTexArrayPassVertex (VertexInput input) {
	VertexOutput output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);
	output.uv.xy = input.uv.xy;
	output.uv.z = input.uv2.x;                              // expecting slice number in uv2. Frag will take it in z
	output.uv3.xy = input.uv3.xy;
	return output;
}

			int _magOn;
			int _calibrate;
			float _bandSize;
			float _mag;
			float _k2;
			float _kem;
			float _viewer;
			float _centripital;
			float _untilt;

		Texture2DArray _MainTexA;
		Texture2DArray _AuxTexA;
        SamplerState sampler_MainTexA;

/**
 radius of uv, hex cell tip is at Rm = 0.5
 distance to flat of a hex cell is...
 sqrt of Rm^2 - (Rm/2)^2 is sqrt3 / 4 = 0.43
**/

float4 UnlitTexArrayPassFragment (VertexOutput input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);

		float2 rv;	// radial position in this cell
    	float r;
    	float rc;   // calibration radius
    	float2 arv; // radial position on lens
    	float me; // mag effective
    	float cei;	// inverse of centripital or viewer distance;
    	float k2e;
    	if (_magOn < 1)  { // 0, flag I am making mosaic for behind a lens
    		me = 1.0 - 0.5 * _k2;
    		k2e = _k2;
    		cei = _centripital + _untilt;
    	} else {			// 1, flag I am pretending to have a lens array in front of mosaic
    		k2e = _kem;
    		me = _mag - 0.5 * k2e;
    		cei = _viewer + _centripital + _untilt;	// it still has the centripital tho. so add it in.
    	}

    	rv.x = input.uv.x - 0.5; // radial position, relative to center at (.5 .5)
    	rv.y = input.uv.y - 0.5; //
    	r = length(rv);

    	arv.x = input.uv3.x - 0.5;
    	arv.y = input.uv3.y - 0.5;

    	input.uv.x = 0.5 + (rv.x) * me + r * rv.x * k2e +  arv.x * cei;
    	input.uv.y = 0.5 + (rv.y) * me + r * rv.y * k2e +  arv.y * cei;

	    float3 texcolor;

		if (_calibrate < 1) {
		    if (input.uv.z < 2048){
				texcolor = _MainTexA.Sample(sampler_MainTexA, input.uv);
			} else {
			    input.uv.z = input.uv.z - 2048;
				texcolor = _AuxTexA.Sample(sampler_MainTexA, input.uv);
			}
		} else {
			rc = _bandSize * 4.0 *  length(rv + arv * _viewer);
			texcolor.r = 0.2 * round(5.0*(1.0 - (rc %2)*0.5));
			texcolor.g = 0.2 * round(5.0*(((rc )%4)*0.25));
			texcolor.b = 0.2 * round(5.0*(((rc )%8)*0.125));
		}

        if (abs(rv.x) > 0.426) {
            texcolor = 0;
        }
        if ( 0.89 - abs(rv.y)*1.89 < abs(rv.x)  ) {
            texcolor = 0;
        }

	return float4( texcolor, 1);
}

#endif // MYRP_UNLITYEXARRAY_INCLUDED
