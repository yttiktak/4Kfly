Shader "Unlit/TomoView"
{
	Properties
	{
		_tex3d ("Texture", 2DArray) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 viewd : TEXCOORD1;
				float4 vertex : SV_POSITION;
			};

			// sampler3D _tex3d;
			UNITY_DECLARE_TEX2DARRAY(_tex3d);
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.viewd = normalize(ObjSpaceViewDir (v.vertex));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
			fixed4 samco;
			fixed4 col;
			float3 uvang;
			for (int si = 0; si < 476; si++) {
				uvang.z = 476-si;
				uvang.xy = i.uv.xy - (i.viewd.xy * 0.002f) * (si * 0.1f);
				samco = UNITY_SAMPLE_TEX2DARRAY(_tex3d, uvang);
				if (samco.r  + samco.g  + samco.b < 0.9) {
					 
				}
				else {
					col = col * 0.2f + samco * 0.8f;
				}
			}
				return col;
			}
			ENDCG
		}
	}
}
