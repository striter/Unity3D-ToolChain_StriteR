Shader "Game/Lit/Hatching_Diffuse"
{
	Properties
	{
		_HatchScale("Hatch Scale",Float)=1
		_Lambert("Lambert",Range(0,1))=1
		[Toggle(_WORLD_UV)]_WORLDUV("World UV",float)=1
		[NoScaleOffset]_Hatch0("Hatch 0",2D) = "white"{}
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
				o.uv=v.uv/_HatchScale;
				TRANSFER_SHADOW(o);
				return o;
			}
			
			float3 SampleHatchMap(int index,float2 uv)
			{
				if(index==0)
					return tex2D(_Hatch0, uv);
				else if(index==1)
					return tex2D(_Hatch1, uv);
				else if(index==2)
					return tex2D(_Hatch2, uv);
				else if(index==3)
					return tex2D(_Hatch3, uv);
				else if(index==4)
					return tex2D(_Hatch4, uv);
				else if(index==5)
					return tex2D(_Hatch5, uv);
				else
					return 0;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				#if _WORLD_UV
				i.uv = TriplanarMapping(i.worldPos,normalize(i.worldNormal))/_HatchScale;
				#endif
				fixed diff =saturate(GetDiffuse(normalize(i.worldLightDir),normalize(i.worldNormal)));
				UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos);
				diff*=atten;
				diff=diff*_Lambert+(1-_Lambert);
				int hatchIndex=-1;
				float hatchWeight=0;
				float hatchFactor =diff * 6.0;
				hatchIndex= max(0, 4-floor(hatchFactor));
				hatchWeight=saturate(hatchFactor-(4-hatchIndex));
				return float4(SampleHatchMap(hatchIndex,i.uv)*hatchWeight+SampleHatchMap(hatchIndex+1,i.uv)*(1-hatchWeight),1);
			}
			ENDCG
		}

		USEPASS "Game/Lit/Standard_Specular/ShadowCaster"
	}
}
