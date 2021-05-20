Shader "Game/Lit/Diffuse_Transparent"
{
	Properties
	{
		_MainTex("Color UV TEX",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		_Opacity("Opacity",Range(0,1)) = .7
	}
	SubShader
	{
		Tags {"Queue"="Transparent" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		HLSLINCLUDE
			#include "../CommonInclude.hlsl"
			#include "../CommonLightingInclude.hlsl"

			TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			INSTANCING_BUFFER_START
			INSTANCING_PROP(float4, _Color)
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float,_Opacity)
			INSTANCING_BUFFER_END
		ENDHLSL

		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex DiffuseVertex
			#pragma fragment DiffuseFragmentBase
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
				float2 uv:TEXCOORD0;
				float3 normalOS:TEXCOORD1;
				float3 lightDirOS:TEXCOORD2;
				float4 shadowCoordWS:TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f DiffuseVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = TRANSFORM_TEX_INSTANCE( v.uv,_MainTex);
				o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
				o.normalOS = v.normalOS;
				o.lightDirOS=TransformWorldToObjectNormal(_MainLightPosition.xyz);
				o.shadowCoordWS=TransformWorldToShadowCoord(TransformObjectToWorld(v.positionOS));
				return o;
			}

			float4 DiffuseFragmentBase(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float diffuse=saturate( GetDiffuse(normalize( i.normalOS),normalize(i.lightDirOS)));
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				diffuse*=atten;
				float3 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb* INSTANCE( _Color).rgb+UNITY_LIGHTMODEL_AMBIENT.xyz;
				finalCol*=_MainLightColor.rgb*diffuse;
				float opacity=INSTANCE(  _Opacity);
				opacity=min(opacity,diffuse);
				return float4(finalCol,opacity);
			}
			ENDHLSL
		}

		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			HLSLPROGRAM

			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
			
			sampler3D _DitherMaskLOD;
			struct a2f
			{
				A2V_SHADOW_CASTER;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 screenPos:TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f ShadowVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				SHADOW_CASTER_VERTEX(v,o);
				o.screenPos = ComputeScreenPos(o.positionCS);
				return o;
			}

			float4 ShadowFragment(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float2 vpos = i.screenPos.xy / i.screenPos.w;
				float dither = tex3D(_DitherMaskLOD, float3(vpos * 10,INSTANCE( _Opacity) * 0.9375)).a;
				clip(dither - 0.01);
				return 0;
			}
			ENDHLSL
		}

	}
}
