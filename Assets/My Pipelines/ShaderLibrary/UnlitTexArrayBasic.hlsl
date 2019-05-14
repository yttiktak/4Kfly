#ifndef MYRP_UNLITYEXARRAYBASIC_INCLUDED
#define MYRP_UNLITYEXARRAYBASIC_INCLUDED

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
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	float3 uv : TEXCOORD0;
    float3 uv2 : TEXCOORD1;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput UnlitTexArrayBasicPassVertex (VertexInput input) {
	VertexOutput output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);
	output.uv.xy = input.uv.xy;
	output.uv.z = input.uv2.x;                              // expecting slice number in uv2. Frag will take it in z
	return output;
}


Texture2DArray _MainTexA;
SamplerState sampler_MainTexA;


float4 UnlitTexArrayBasicPassFragment (VertexOutput input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);

	    float3 texcolor;

		texcolor = _MainTexA.Sample(sampler_MainTexA, input.uv);


	return float4( texcolor, 1);
}

#endif // MYRP_UNLITYEXARRAYBASIC_INCLUDED
