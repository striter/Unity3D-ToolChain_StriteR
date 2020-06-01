Shader "Hidden/PostEffect/PE_DepthCircleScan"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
		_Texture(" Scan Texture",2D) = "white"{}
		_TextureScale("Scan Tex Scale",float)=15
		_Color("Scan Color",Color)=(1,1,1,1)
		_MinSqrDistance("Min Squared Distance",float) = .5
		_MaxSqrDistance("Max Squared Distance",float)=.5
		_Origin("Scan Origin",Vector)=(1,1,1,1)
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "../CommonInclude.cginc"
				#include "UnityCG.cginc"
				#include "PostEffectInclude.cginc"

			sampler2D _Texture;
			float _TextureScale;
			float4 _Color;
			float4 _Origin;
			float _MinSqrDistance;
			float _MaxSqrDistance;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				half2 uv_depth:TEXCOORD1;
				float3 interpolatedRay:TEXCOORD2;
			};

			v2f vert (appdata_img v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.texcoord;
				o.uv_depth = GetDepthUV(v.texcoord);
				o.interpolatedRay = GetInterpolatedRay(o.uv);
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float linearDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,i.uv_depth));
				float3 worldPos = _WorldSpaceCameraPos + i.interpolatedRay*linearDepth;
				float squareDistance = sqrdistance(_Origin.xyz,worldPos);

				float scan = 1;
				scan *= _Color.a;
				scan *= step(_MinSqrDistance, squareDistance)*step(squareDistance, _MaxSqrDistance);
				scan *= tex2D(_Texture, worldPos.xz*_TextureScale).r;
				return tex2D(_MainTex,i.uv)+_Color* scan;
			}
			ENDCG
		}
	}
}
