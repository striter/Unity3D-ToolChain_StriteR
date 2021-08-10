// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Game/Effects/PlantsWaving_Vertex"
{
	Properties
	{
		_MainTex("Color UV TEX",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		_WaveSpeed("Wind Speed",Range(0,5)) = 1
		_WaveFrequency("Wind Frequency",float)=5
		_WaveStrength("Wind Strength",float)=1
		_WaveDirection("Wind Direction",Vector)=(1,1,1)
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue"="AlphaTest+1" }

		HLSLINCLUDE
		#include "Assets/Shaders/Library/Common.hlsl"
		#include "Assets/Shaders/Library/Lighting.hlsl"
		TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
		INSTANCING_BUFFER_START
		INSTANCING_PROP(float3,_WaveDirection)
		INSTANCING_PROP(float,_WaveFrequency)
		INSTANCING_PROP(float,_WaveSpeed)
		INSTANCING_PROP(float,_WaveStrength)
		INSTANCING_PROP(float4,_MainTex_ST)
		INSTANCING_PROP(float4,_Color)
		INSTANCING_BUFFER_END

		float3 Wave(float3 worldPos)
		{
			float wave =INSTANCE(_WaveStrength)*sin(_Time.y*INSTANCE(_WaveSpeed) + (worldPos.x + worldPos.y)*INSTANCE(_WaveFrequency)) / 100;
			return  INSTANCE(_WaveDirection)*wave;
		}
		ENDHLSL

		Pass		//Base Pass
		{
			Tags{ "LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			struct appdata
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID

			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float3 viewDirWS:TEXCOORD2;
				float3 normalWS:TEXCOORD3;
				float4 shadowCoordWS:TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				o.uv = TRANSFORM_TEX_INSTANCE( v.uv,_MainTex);
				o.positionWS = TransformObjectToWorld( v.positionOS);
				o.positionWS +=Wave(o.positionWS);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.viewDirWS=GetCameraPositionWS()-o.positionWS;
				o.normalWS=TransformObjectToWorldNormal(v.normalOS);
				o.shadowCoordWS=TransformWorldToShadowCoord(o.positionWS);
				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb*INSTANCE(_Color).rgb;
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				float3 ambient = _GlossyEnvironmentColor.rgb;
				float diffuse = GetDiffuse(normalize(i.normalWS),normalize(i.viewDirWS),.5,atten); 
				return float4(diffuse*(albedo+ ambient)*_MainLightColor.rgb,1);
			}
			ENDHLSL
		}
			
		Pass
		{
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM
			#pragma vertex vertshadow
			#pragma fragment fragshadow
			#pragma multi_compile_instancing
			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				V2F_SHADOW_CASTER;
			};

			v2f vertshadow(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float3 positionWS = TransformObjectToWorld( v.positionOS);
				positionWS += Wave(positionWS);
				SHADOW_CASTER_VERTEX(v,positionWS);
				return o;
			}

			float4 fragshadow(v2f i) :SV_TARGET
			{
				return 1;
			}
			ENDHLSL
		}

		Pass
		{
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vertshadow
			#pragma fragment fragshadow

			float4 vertshadow(float3 positionOS:POSITION):SV_POSITION
			{
				float3 positionWS = TransformObjectToWorld(positionOS);
				positionWS += Wave(positionWS);
				return TransformWorldToHClip(positionWS);
			}

			float4 fragshadow() :SV_TARGET
			{
				return 1;
			}
			ENDHLSL
		}
	}
}
