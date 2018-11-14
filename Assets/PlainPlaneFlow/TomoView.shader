Shader "Unlit/TomoView"
{
	Properties
	{
		_tex3d ("Texture", 2DArray) = "white" {}
	    _slices("slices",int)=476
		_ror("RiseRun",float) = 20
		_face("face",int) = 0
		_shrink("shrink",float) = 1.1
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
				float3 normal : NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float3 viewdir : TEXCOORD1;
				float3 norm : TEXCOORD2;
				float4 vertex : SV_POSITION;
			};

			// sampler3D _tex3d;
			UNITY_DECLARE_TEX2DARRAY(_tex3d);
			float4 _MainTex_ST;
			float _ror; //
			float _shrink; 
			int _face;
			int _slices;
			
			v2f vert (appdata v)
			{
				v2f o;
				float3 viewd;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				viewd = ObjSpaceViewDir(v.vertex);
				o.norm = normalize(v.normal);
				// viewd.y = -viewd.y;
				o.viewdir = normalize(viewd);
				// o.viewdir.y = -o.viewdir.y;
				// o.norm = v.normal;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 uv3;
				float3 uvs;
				uv3.xy = (i.uv-0.5)*_shrink + 0.5;
				fixed4 samco;
				fixed4 col = 0;
				float landingSlice;
				float stou = _ror / _slices;
				int faceInd = _face;
				if ((faceInd == 1) && (i.viewdir.x>0)) {
					faceInd = 101;
				}
				switch (faceInd)
				{
				case 0: { // front face
					/*** 
					uv3.xy point on face, face is first slice. Back slice is 476
					***/
						for (int si = _slices; si >=0; si--) {
							uvs.z = si;
							uvs.xy = uv3.xy + i.viewdir.xz * (si * stou) / i.viewdir.y;
							samco = UNITY_SAMPLE_TEX2DARRAY(_tex3d, uvs);
							if ((samco.r + samco.g + samco.b) > 0.4) {
								col = col * 0.7 + samco * 0.3;
							}
						}
				} break;
				case 1: // left side face
				{
					uv3.x = 1.0 - uv3.x;
					landingSlice = uv3.x / stou; // uv range is 0 .. 1, right??
					uv3.x = 0; // and with 0 on the left??
						for (int si = _slices; si >= 0; si--) {
							uvs.z = si;
							uvs.xy = uv3.xy + i.viewdir.yz * ((landingSlice - si) * stou) / i.viewdir.x;
							samco = UNITY_SAMPLE_TEX2DARRAY(_tex3d, uvs);
							if (samco.r + samco.g + samco.b > 0.4) {
								col = col * 0.7f + samco * 0.3f;
							}
						}
				} break;
				case 101: // left side face from back
				{
					uv3.x = 1.0 - uv3.x;
					landingSlice = uv3.x / stou; // uv range is 0 .. 1, right??
					uv3.x = 0; // and with 0 on the left??
					for (int si = 0; si < _slices; si++) {
						uvs.z = si;
						uvs.xy = uv3.xy + i.viewdir.yz * ((landingSlice - si) * stou) / i.viewdir.x;
						samco = UNITY_SAMPLE_TEX2DARRAY(_tex3d, uvs);
						if (samco.r + samco.g + samco.b > 0.4) {
							col = col * 0.7f + samco * 0.3f;
						}
					}
				}
				break;
				case 2: { // back face
					//uv3.x = 1.0 - uv3.x;
					for (int si = 0; si < _slices; si++) {
						uvs.z = si;
						uvs.xy = uv3.xy + i.viewdir.xz * ((_slices -si) * stou) / i.viewdir.y;
						uvs.x = 1.0 - uvs.x;
						samco = UNITY_SAMPLE_TEX2DARRAY(_tex3d, uvs);
						if ((samco.r + samco.g + samco.b) > 0.4) {
							col = col * 0.7 + samco * 0.3;
						}
					}
				} break;
				default: // just slice at _slices
				{
					uv3.z = _slices;
					col = UNITY_SAMPLE_TEX2DARRAY(_tex3d, uv3);
				} break; // superfluous break
				}

				return col;
			}
			ENDCG
		}
	}
}
