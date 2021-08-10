Shader "Game/Lit/Toon/Hatching_Diffuse"
{
	Properties
	{
		_Lambert("Lambert",Range(0,1))=1
		[Toggle(_WORLD_UV)]_WORLDUV("World UV",float)=1
		_Hatch0("Hatch 0",2D) = "white"{}
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
			Tags{"LightMode"="UniversalForward"}
			ZWrite On
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Assets/Shaders/Library/CommonInclude.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			#pragma shader_feature_local _WORLD_UV
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

			TEXTURE2D(_Hatch0); SAMPLER(sampler_Hatch0);
			TEXTURE2D(_Hatch1); SAMPLER(sampler_Hatch1);
			TEXTURE2D(_Hatch2); SAMPLER(sampler_Hatch2);
			TEXTURE2D(_Hatch3); SAMPLER(sampler_Hatch3);
			TEXTURE2D(_Hatch4); SAMPLER(sampler_Hatch4);
			TEXTURE2D(_Hatch5); SAMPLER(sampler_Hatch5);

			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			UNITY_DEFINE_INSTANCED_PROP(float4,_Hatch0_ST)
            UNITY_DEFINE_INSTANCED_PROP(float,_Lambert)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct appdata
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float3 normalWS:TEXCOORD2;
				float4 shadowCoordWS:TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionWS = TransformObjectToWorld(v.positionOS);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				#if _WORLD_UV
				o.uv = TRANSFORM_TEX_INSTANCE( UVRemap_Triplanar(o.positionWS,normalize(o.normalWS)),_Hatch0);
				#else
				o.uv=TRANSFORM_TEX_INSTANCE(v.uv,_Hatch0);
				#endif
				o.shadowCoordWS=TransformWorldToShadowCoord(o.positionWS);
				return o;
			}
			
			float3 SampleHatchMap(int index,float2 uv,float weight)
			{
				float3 col=0;
				if(weight==0)
					return col;

				if(index==0)
					col= SAMPLE_TEXTURE2D(_Hatch0,sampler_Hatch0, uv).xyz;
				else if(index==1)
					col= SAMPLE_TEXTURE2D(_Hatch1,sampler_Hatch1, uv).xyz;
				else if(index==2)
					col= SAMPLE_TEXTURE2D(_Hatch2,sampler_Hatch2, uv).xyz;
				else if(index==3)
					col= SAMPLE_TEXTURE2D(_Hatch3,sampler_Hatch3, uv).xyz;
				else if(index==4)
					col= SAMPLE_TEXTURE2D(_Hatch4,sampler_Hatch4, uv).xyz;
				else if(index==5)
					col= SAMPLE_TEXTURE2D(_Hatch5,sampler_Hatch5, uv).xyz;
				return col*weight;
			}

			float4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS=normalize(i.normalWS);
				float3 lightDirWS=normalize(_MainLightPosition.xyz);
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				float diffuse =saturate(GetDiffuse(lightDirWS,normalWS,INSTANCE( _Lambert),atten));
				float hatchFactor =diffuse * 6.0;
				float3 lightCol=_MainLightColor.rgb;
				float3 ambient=_GlossyEnvironmentColor.rgb;
				float3 hatchCol=0;
				float3 hatchWeight012 =0;
				float3 hatchWeight345 = 0;
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
				
				hatchCol += SampleHatchMap(0, i.uv,hatchWeight012.x);
				hatchCol += SampleHatchMap(1, i.uv,hatchWeight012.y);
				hatchCol += SampleHatchMap(2, i.uv,hatchWeight012.z);
				hatchCol += SampleHatchMap(3, i.uv,hatchWeight345.x);
				hatchCol += SampleHatchMap(4, i.uv,hatchWeight345.y);
				hatchCol += SampleHatchMap(5, i.uv,hatchWeight345.z);
				hatchCol=lightCol*(hatchCol+ambient);
				return float4(hatchCol,1);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}
