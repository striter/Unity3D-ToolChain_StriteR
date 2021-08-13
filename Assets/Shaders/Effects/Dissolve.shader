Shader "Game/Effects/Dissolve"
{
	Properties
	{
		_DissolveAmount("_Dissolve Amount",Range(0,1)) = 1
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		_DissolveTex("Dissolve Map",2D) = "white"{}
		_DissolveWidth("_Dissolve Width",Range(0,1)) = .1

	}
	SubShader
	{
		Tags{"RenderType" = "Dissolve"  "Queue" = "Geometry"}
		Cull Off

		HLSLINCLUDE
		#include "Assets/Shaders/Library/Common.hlsl"
		#include "Assets/Shaders/Library/Lighting.hlsl"

		TEXTURE2D(_DissolveTex);SAMPLER(sampler_DissolveTex);
		TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
		INSTANCING_BUFFER_START
		INSTANCING_PROP(float4,_ScanColor)
		INSTANCING_PROP(float4,_DissolveTex_ST)
		INSTANCING_PROP(float4,_MainTex_ST)
		INSTANCING_PROP(float,_DissolveAmount)
		INSTANCING_PROP(float,_DissolveWidth)
		INSTANCING_BUFFER_END
		ENDHLSL

		Pass
		{
			Tags{ "LightMode" = "UniversalForward" }
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

			struct a2f
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float4 uv:TEXCOORD0;
				float3 normalWS:TEXCOORD1;
				float4 shadowCoordWS:TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};


			v2f vert (a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv.xy =  TRANSFORM_TEX_INSTANCE( v.uv, _MainTex);
				o.uv.zw = TRANSFORM_TEX_INSTANCE(v.positionOS.xz,_DissolveTex);
				o.normalWS= TransformObjectToWorldNormal(v.normalOS);
				o.shadowCoordWS=TransformWorldToShadowCoord(TransformObjectToWorld(v.positionOS));
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float dissolve = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex,i.uv.zw).r -INSTANCE( _DissolveAmount)-INSTANCE(_DissolveWidth);
				clip(dissolve);

				float diffuse=GetDiffuse(normalize(i.normalWS),normalize(_MainLightPosition.xyz));
				float3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy).rgb* INSTANCE(_ScanColor).rgb+_GlossyEnvironmentColor.rgb;
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				float3 finalCol=albedo*diffuse*atten*_MainLightColor.rgb;
				return float4(finalCol,1);
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
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			v2f vertshadow(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				SHADOW_CASTER_VERTEX(v,TransformObjectToWorld(v.positionOS));
				o.uv = TRANSFORM_TEX_INSTANCE(v.positionOS.xz,_DissolveTex);
				return o;
			}

			float4 fragshadow(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				const float dissolve = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex,i.uv).r - INSTANCE(_DissolveAmount)-INSTANCE(_DissolveWidth);
				clip(dissolve);
				return 0;
			}
			ENDHLSL
		}
		
		Pass
		{
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vertshadow
			#pragma fragment fragshadow

			struct v2f
			{
				float4 positionCS:SV_POSITION;
				float2 uv:TEXCOORD0;
			};
			v2f vertshadow(float3 positionOS:POSITION)
			{
				v2f o;
				o.positionCS=TransformObjectToHClip(positionOS);
				o.uv = TRANSFORM_TEX_INSTANCE(positionOS.xz,_DissolveTex);
				return o;
			}

			float4 fragshadow(v2f i) :SV_TARGET
			{
				float dissolve = SAMPLE_TEXTURE2D(_DissolveTex,sampler_DissolveTex,i.uv).r - INSTANCE(_DissolveAmount)-INSTANCE(_DissolveWidth);
				clip(dissolve);
				return 0;
			}
			ENDHLSL
		}
	}
}
