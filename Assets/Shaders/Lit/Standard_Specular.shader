Shader "Game/Lit/Standard_Specular"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		
		[Header(Normal Map)]
		[Toggle(_NORMALMAP)]_EnableNormalMap("Enable Normal Mapping",float)=0
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Header(Diffuse Setting)]
		_Lambert("Lambert",Range(0,1))=.5

		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularRange("Specular Range",Range(.9,1))=.98
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull Back
		Blend Off

		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex DiffuseVertex
			#pragma fragment DiffuseFragmentBase
			
			#include "../CommonInclude.hlsl"
			#include "../CommonLightingInclude.hlsl"
			
			#pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			#pragma shader_feature _SPECULAR
			#pragma shader_feature _NORMALMAP
		
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
		    #if _NORMALMAP
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			#endif
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			UNITY_DEFINE_INSTANCED_PROP(float4,_MainTex_ST)
			UNITY_DEFINE_INSTANCED_PROP(float,_Lambert)
			UNITY_DEFINE_INSTANCED_PROP(float,_SpecularRange)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct a2f
			{
				float3 positionOS : POSITION;
				float2 uv:TEXCOORD0;
				float3 normalOS:NORMAL;
				#if _NORMALMAP
				float4 tangentOS:TANGENT;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float3 normalWS:TEXCOORD2;
				float3 viewDirWS:TEXCOORD3;
				float4 shadowCoordWS:TEXCOORD4;
				#if _NORMALMAP
				float3x3 TBNWS:TEXCOORD5;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f DiffuseVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.positionWS =  TransformObjectToWorld(v.positionOS);
				o.normalWS=TransformObjectToWorldNormal(v.normalOS);
				o.viewDirWS=GetCameraPositionWS() -o.positionWS;
				o.shadowCoordWS=TransformWorldToShadowCoord(o.positionWS);
				#if _NORMALMAP
				float3 tangentWS=TransformObjectToWorldNormal(v.tangentOS.xyz*v.tangentOS.w);
				float3 biTangentWS=cross(tangentWS,o.normalWS);
				o.TBNWS=float3x3(normalize(tangentWS),normalize(biTangentWS),normalize(o.normalWS));
				#endif
				return o;
			}

			float4 DiffuseFragmentBase(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normal=normalize(i.normalWS);
				float3 lightDir=normalize(_MainLightPosition.xyz);
				float3 viewDir=normalize(i.viewDirWS);
				#if _NORMALMAP
				float3 normalTS= DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv).xyz);
				normal= mul(normalTS,i.TBNWS);
				#endif

				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				float3 ambient=_GlossyEnvironmentColor.rgb;
				float3 lightCol=_MainLightColor.rgb;
				float3 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).xyz*INSTANCE( _Color).rgb+ambient;
				float diffuse= GetDiffuse(normal,lightDir,INSTANCE(_Lambert),atten);
				finalCol*=_MainLightColor.rgb*diffuse;
				#if _SPECULAR
				float specular = GetSpecular(normal,lightDir,viewDir,INSTANCE(_SpecularRange));
				specular*=atten;
				finalCol += _MainLightColor.rgb*specular;
				#endif
				return float4(finalCol,1);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}