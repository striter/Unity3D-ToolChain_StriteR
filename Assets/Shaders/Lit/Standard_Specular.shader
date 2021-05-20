Shader "Game/Lit/Standard_Specular"
{
	Properties
	{
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color",Color) = (1,1,1,1)
		
		[Header(Diffuse Setting)]
		_Lambert("Lambert",Range(0,1))=.5

		[Header(Specular Setting)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularRange("Specular Range",Range(.9,1))=.98

		[Header(Additional Mapping)]
		[Header(_Normal)]
		[Toggle(_NORMALMAP)]_EnableNormalMap("_Normal Mapping",float)=0
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Header(_Parallex)]
		[Toggle(_PARALLEXMAP)]_EnableParallexMap("_Parallex Mapping",float)=0
		[NoScaleOffset]_ParallexTex("Parallex Tex",2D)="white"{}
		_ParallexScale("Parallex Scale",Range(0.001,.2))=1
		_ParallexOffset("Parallex Offset",Range(0,1))=.42
		[Toggle(_PARALLEX_STEEP)]_SteepParallex("Steep Parallex",float)=0
		[Enum(_8,8,_16,16,_32,32,_64,64,_128,128)]_SteepCount("Steep Count",int)=16
		[Header(_AO)]
		[Toggle(_AOMAP)]_EnableAOMap("_AO Mapping",float)=0
		[NoScaleOffset]_AOTex("AO Tex",2D)="white"{}
		[Header(_Roughness)]
		[Toggle(_ROUGHNESSMAP)]_EnableRoughnessMap("_Rougheness Mapping",float)=0
		[NoScaleOffset]_RoughnessTex("Roughness Tex",2D)="white"{}
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

			#pragma shader_feature_local _SPECULAR
			#pragma shader_feature_local _NORMALMAP
			#pragma shader_feature_local _PARALLEXMAP
			#pragma shader_feature_local _PARALLEX_STEEP
			#pragma shader_feature_local _AOMAP
			#pragma shader_feature_local _ROUGHNESSMAP
		
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_ParallexTex);SAMPLER(sampler_ParallexTex);
			TEXTURE2D(_AOTex);SAMPLER(sampler_AOTex);
			TEXTURE2D(_RoughnessTex);SAMPLER(sampler_RoughnessTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			UNITY_DEFINE_INSTANCED_PROP(int ,_SteepCount)
			UNITY_DEFINE_INSTANCED_PROP(float4,_MainTex_ST)
			UNITY_DEFINE_INSTANCED_PROP(float,_Lambert)
			UNITY_DEFINE_INSTANCED_PROP(float,_SpecularRange)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
			UNITY_DEFINE_INSTANCED_PROP(float,_ParallexScale)
			UNITY_DEFINE_INSTANCED_PROP(float,_ParallexOffset)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

			struct a2f
			{
				float3 positionOS : POSITION;
				float2 uv:TEXCOORD0;
				float3 normalOS:NORMAL;
				float4 tangentOS:TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				float3 normalTS:TEXCOORD1;
				float3 positionTS:TEXCOORD2;
				float3 cameraPosTS:TEXCOORD3;
				float4 shadowCoordWS:TEXCOORD4;
				float3x3 TBNWS:TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f DiffuseVertex(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
				o.positionCS = TransformObjectToHClip(v.positionOS);
				float3 positionWS =  TransformObjectToWorld(v.positionOS);
				o.shadowCoordWS=TransformWorldToShadowCoord(positionWS);

				float3 normalWS=normalize(TransformObjectToWorldDir(v.normalOS));
				float3 tangentWS=normalize(TransformObjectToWorldDir(v.tangentOS.xyz));
				float3 biTangentWS=cross(normalWS,tangentWS)*v.tangentOS.w;
				o.TBNWS=float3x3(tangentWS,biTangentWS,normalWS);
				o.positionTS=mul(o.TBNWS,positionWS);
				o.normalTS=mul(o.TBNWS,normalWS);
				o.cameraPosTS=mul(o.TBNWS, GetCameraPositionWS());
				return o;
			}
			#if _PARALLEXMAP
			float GetParallex(float2 uv)
			{
				return 1.0-SAMPLE_TEXTURE2D(_ParallexTex,sampler_ParallexTex,uv);
			}
			float2 ParallexMap(float2 uv,float3 viewDirTS)
			{
				float3 viewDir=normalize(viewDirTS);
				viewDir.z+=INSTANCE(_ParallexOffset);
				float2 uvOffset=viewDir.xy/viewDir.z*INSTANCE(_ParallexScale);
				#if _PARALLEX_STEEP
				int marchCount=lerp(INSTANCE(_SteepCount),INSTANCE(_SteepCount)/4,saturate(dot(float3(0,0,1),viewDirTS)));
				marchCount=min(marchCount,128);
				float deltaDepth=1.0/marchCount;
				float2 deltaUV=uvOffset/marchCount;
				float depthLayer=0;
				float2 curUV=uv;
				float curDepth;
				for(int i=0;i<marchCount;i++)
				{
					curDepth=GetParallex(curUV).r;
					if(curDepth<=depthLayer)
						break;
					curUV-=deltaUV;
					depthLayer+=deltaDepth;
				}
				float2 preUV=curUV+deltaUV;
				float beforeDepth=GetParallex(preUV)-depthLayer+deltaDepth;
				float afterDepth=curDepth-depthLayer;
				float weight=afterDepth/(afterDepth-beforeDepth);
				curUV=preUV*weight+curUV*(1-weight);
				return curUV;
				#else
				float2 offset=uvOffset*GetParallex(uv).r;
				return uv-offset;
				#endif
			}
			#endif
			float4 DiffuseFragmentBase(v2f i) :SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalTS=normalize(i.normalTS);
				float3 lightDirTS=mul(i.TBNWS,_MainLightPosition.xyz);
				float3 viewDirTS=normalize(i.cameraPosTS-i.positionTS);
				#if _PARALLEXMAP
				i.uv=ParallexMap(i.uv,viewDirTS);
				#endif
				#if _NORMALMAP
				normalTS=normalize( DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv).xyz));
				#endif

				float atten=MainLightRealtimeShadow(i.shadowCoordWS);

				float3 ambient=_GlossyEnvironmentColor.rgb;
				float3 lightCol=_MainLightColor.rgb;
				float3 albedo=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).xyz*INSTANCE( _Color).rgb;
				float3 finalCol=albedo+ambient;
				float diffuse= GetDiffuse(normalTS,lightDirTS,INSTANCE(_Lambert),atten);
				finalCol*=_MainLightColor.rgb*diffuse;
				#if _AOMAP
				finalCol*=SAMPLE_TEXTURE2D(_AOTex,sampler_AOTex,i.uv).r;
				#endif
				#if _SPECULAR
				float specular = GetSpecular(normalTS,lightDirTS,viewDirTS,INSTANCE(_SpecularRange));
				#if _ROUGHNESSMAP
				specular*=SAMPLE_TEXTURE2D(_RoughnessTex,sampler_RoughnessTex,i.uv);
				#endif
				finalCol += _MainLightColor.rgb*albedo*specular*atten;
				#endif
				return float4(finalCol,1);
			}
			ENDHLSL
		}

		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
	}
}