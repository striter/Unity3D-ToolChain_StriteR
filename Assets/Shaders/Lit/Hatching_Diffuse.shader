Shader "Game/Lit/Hatching_Diffuse"
{
	Properties
	{
		_HatchScale("Hatch Scale",Float)=1
		_Lambert("Lambert",Range(0,1))=1
		[Toggle(_WORLD_UV)]_WORLDUV("World UV",float)=1
		[NoScaleOffset]_Hatch0("Hatch 0",2D)="white"{}
		[NoScaleOffset]_Hatch1("Hatch 1",2D) = "white"{}
		[NoScaleOffset]_Hatch2("Hatch 2",2D) = "white"{}
		[NoScaleOffset]_Hatch3("Hatch 3",2D) = "white"{}
		[NoScaleOffset]_Hatch4("Hatch 4",2D) = "white"{}
		[NoScaleOffset]_Hatch5("Hatch 5",2D) = "white"{}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque"  "Queue"="Geometry"}

		Pass
		{
			Tags{"LightMode"="ForwardBase"}
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma shader_feature _WORLD_UV
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "../CommonInclude.cginc"
			#include "../CommonLightingInclude.cginc"

			fixed _HatchScale;
			fixed _Lambert;
			sampler2D _Hatch0, _Hatch1, _Hatch2, _Hatch3, _Hatch4, _Hatch5;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal:NORMAL;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 worldPos:TEXCOORD1;
				float3 worldNormal:TEXCOORD2;
				float3 worldLightDir:TEXCOORD3;
				SHADOW_COORDS(4)
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldLightDir = UnityWorldSpaceLightDir(o.worldPos);
				#if _WORLD_UV
				o.uv = TriplanarMapping(o.worldPos,v.normal,_HatchScale);
				#else
				o.uv=v.uv;
				#endif

			
				TRANSFER_SHADOW(o);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed diff =saturate(GetDiffuse(normalize(i.worldLightDir),normalize(i.worldNormal)));
				UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);
				diff*=atten;
				diff=diff*_Lambert+(1-_Lambert);
				float3 hatchWeight012 =0;
				float3 hatchWeight345 = 0;

				float hatchFactor = diff * 7.0;

				if (hatchFactor > 5)
				{
					hatchWeight012.x = 1;
				}
				else if (hatchFactor > 4)
				{
					hatchWeight012.x = hatchFactor - 4;
					hatchWeight012.y = 1 - hatchWeight012.x;
				}
				else if (hatchFactor > 3)
				{
					hatchWeight012.y = hatchFactor - 3;
					hatchWeight012.z = 1 - hatchWeight012.y;
				}
				else if (hatchFactor > 2)
				{
					hatchWeight012.z = hatchFactor - 2;
					hatchWeight345.x = 1 - hatchWeight012.z;
				}
				else if(hatchFactor>1)
				{
					hatchWeight345.x = hatchFactor - 1;
					hatchWeight345.y = 1 - hatchWeight345.x;
				}
				else
				{
					hatchWeight345.y = hatchFactor;
					hatchWeight345.z = 1 - hatchWeight345.y;
				}


				float3 hatchColor=0;
				hatchColor += tex2D(_Hatch0, i.uv)*hatchWeight012.x;
				hatchColor += tex2D(_Hatch1, i.uv)*hatchWeight012.y;
				hatchColor += tex2D(_Hatch2, i.uv)*hatchWeight012.z;
				hatchColor += tex2D(_Hatch3, i.uv)*hatchWeight345.x;
				hatchColor += tex2D(_Hatch4, i.uv)*hatchWeight345.y;
				hatchColor += tex2D(_Hatch5, i.uv)*hatchWeight345.z;

				float3 whiteCol = (1 - hatchWeight012.x - hatchWeight012.y - hatchWeight012.z - hatchWeight345.x - hatchWeight345.y - hatchWeight345.z);
				hatchColor += whiteCol;
				return float4( hatchColor,1);
			}
			ENDCG
		}

		USEPASS "Game/Lit/Standard_Specular/ShadowCaster"
	}
}
