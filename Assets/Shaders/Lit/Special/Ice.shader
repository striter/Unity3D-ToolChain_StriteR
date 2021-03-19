Shader "Game/Lit/Special/Ice"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		[Header(Normal Mappping)]
		[Toggle(_NORMALMAP)]_EnableNormalMap("Enable Normal Mapping",float)=1
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[Toggle(_THICK)]_EnableThick("Enable Ice Thick",float)=1
		_Thickness("Ice Thicknesss",Range(0.01,1))=.1

		[Header(Lighting)]
		[Header(_ Diffuse)]
		_Lambert("Lambert",Range(0,1))=.5
		[Header(_ Specular)]
		[Toggle(_SPECULAR)]_EnableSpecular("Enable Specular",float)=1
		_SpecularRange("Specular Range",Range(.9,1))=.98
		
		[Header(Crack)]
		[Toggle(_CRACK)]_EnableCrack("Enable Crack",Range(0,1))=.5
        [NoScaleOffset]_CrackTex("Crack Tex",2D)="white" {}
		[HDR]_CrackColor("Crack Color",Color)=(1,1,1,1)
		[Header(_ CrackTop)]
		[Toggle(_CRACKTOP)]_EnableCrackTop("Enable Crack Top",float)=1
		_CrackTopStrength("Crack Top Strength",Range(0.1,1))=.2
		[Header(_ Parallex)]
		[Toggle(_CRACKPARALLEX)]_CrackParallex("Enable Crack Parallex",float)=1
        [Enum(_4,4,_8,8,_16,16,_32,32,_64,64)]_CrackParallexTimes("Crack Parallex Times",int)=8
		_CrackDistance("Crack Distance",Range(0,1))=.5
		_CrackPow("Crack Pow",Range(0.1,5))=2

		[Header(Opacity)]
		[Toggle(_OPACITY)]_EnableOpacity("Enable Opacity",float)=1
		_BeginOpacity("Begin Opacity",Range(0,1))=.5
		[Header(_ Distort)]
		[Toggle(_DISTORT)]_EnableDistort("Enable Normal Distort",float)=1
		_DistortStrength("Distort Strength",Range(.1,2))=1
		[Header(_ Depth)]
		[Toggle(_DEPTH)]_EnableDepth("Enable Depth",float)=1
		_DepthDistance("Depth Distance",Range(0,3))=1
		_DepthPow("Depth Pow",Range(0.1,5))=2
		[Header(_ Fresnel)]
		[Toggle(_FRESNEL)]_EnableFresnel("Enable Fresnel",float)=1
		_FresnelPow("Fresnel Pow",Range(0.1,10))=1
    }
    SubShader
    {
        Tags { "Queue"="Transparent-1"}
		Blend Off
        Pass
        {
			Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_instancing

			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _THICK
			#pragma shader_feature _CRACK
			#pragma shader_feature _CRACKTOP
			#pragma shader_feature _CRACKPARALLEX
			#pragma shader_feature _OPACITY
			#pragma shader_feature _DISTORT
			#pragma shader_feature _DEPTH
			#pragma shader_feature _FRESNEL
			#pragma shader_feature _SPECULAR

			#include "../../CommonInclude.hlsl"
			#include "../../CommonLightingInclude.hlsl"
			
			TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
			TEXTURE2D(_CrackTex);SAMPLER(sampler_CrackTex);
			TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
			TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
			TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);

			INSTANCING_BUFFER_START
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float,_Thickness)
			INSTANCING_PROP(float,_Lambert)
			INSTANCING_PROP(float,_SpecularRange)

			INSTANCING_PROP(float,_CrackTopStrength)
			INSTANCING_PROP(float,_CrackDistance)
			INSTANCING_PROP(float,_CrackPow)
			INSTANCING_PROP(uint,_CrackParallexTimes)
			INSTANCING_PROP(float4,_CrackColor)

			INSTANCING_PROP(float,_BeginOpacity)
			INSTANCING_PROP(float,_DistortStrength)
			INSTANCING_PROP(float,_DepthDistance)
			INSTANCING_PROP(float,_DepthPow)
			INSTANCING_PROP(float,_FresnelPow)
			INSTANCING_BUFFER_END

            struct appdata
            {
			    float3 positionOS : POSITION;
			    float3 normalOS:NORMAL;
			    float4 tangentOS:TANGENT;
			    float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
			    float4 positionCS : SV_POSITION;
			    float2 uv:TEXCOORD0;
			    float3 normalTS:TEXCOORD1;
			    float3 viewDirTS:TEXCOORD2;
				float3 lightDirTS:TEXCOORD3;
				float4 shadowCoordWS:TEXCOORD4;
				#if _OPACITY
				float4 screenPos:TEXCOORD5;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
			
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
			    o.uv = TRANSFORM_TEX_INSTANCE( v.uv,_MainTex);
			    o.positionCS = TransformObjectToHClip(v.positionOS);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
			    o.shadowCoordWS=TransformWorldToShadowCoord(positionWS);

				float3x3 TBNOS=float3x3(v.tangentOS.xyz,cross(v.normalOS,v.tangentOS.xyz)*v.tangentOS.w,v.normalOS);
				o.viewDirTS=mul(TBNOS, TransformWorldToObjectNormal(GetCameraPositionWS()-positionWS));
				o.lightDirTS=mul(TBNOS,TransformWorldToObjectNormal(_MainLightPosition.xyz));
				o.normalTS=mul(TBNOS,v.normalOS);

				#if _OPACITY
				o.screenPos=ComputeScreenPos(o.positionCS);
				#endif
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalTS=normalize(i.normalTS);
				float3 lightDirTS=normalize(i.lightDirTS);
				float3 viewDirTS=normalize(i.viewDirTS);
				float2 thickOffset=-viewDirTS.xy/viewDirTS.z;
				
				#if _NORMALMAP
				float2 normalUV=i.uv;
				#if _THICK
				normalUV+=thickOffset*INSTANCE(_Thickness);
				#endif
				normalTS= DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,normalUV).xyz);
				#endif
				
				float3 albedo=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv).rgb;
				float3 lightCol=_MainLightColor.rgb;
				float3 ambient=_GlossyEnvironmentColor.xyz;
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				
				float diffuse=saturate( GetDiffuse(normalTS,lightDirTS,INSTANCE(_Lambert),atten));
				float3 finalCol=lightCol*diffuse*(albedo+ambient);

				#if _CRACK
				float crackAmount=0;
				float4 crackCol=INSTANCE(_CrackColor);
				#if _CRACKTOP
				crackAmount=SAMPLE_TEXTURE2D(_CrackTex,sampler_CrackTex,i.uv)* crackCol.a* INSTANCE(_CrackTopStrength);
				#endif
				#if _CRACKPARALLEX
				uint crackParallexTimes=INSTANCE(_CrackParallexTimes);
				float crackPow=INSTANCE(_CrackPow);
				float crackDistance=INSTANCE(_CrackDistance);

				float parallexParam=1.0/crackParallexTimes;
				float offsetDistance=crackDistance/crackParallexTimes;
				float totalParallex=0;
				for(uint index=0u;index<crackParallexTimes;index++)
				{
					float distance=crackDistance*totalParallex;
					distance+=random2(frac(i.uv))*offsetDistance;
					float2 parallexUV=i.uv+thickOffset*distance;
					crackAmount+=SAMPLE_TEXTURE2D(_CrackTex,sampler_CrackTex,parallexUV)*parallexParam*pow(1-totalParallex,crackPow);
					totalParallex+=parallexParam;
				}
				crackAmount=saturate(crackAmount*diffuse*crackCol.a);
				#endif
				finalCol= lerp(finalCol,crackCol.rgb*lightCol, crackAmount);
				#endif

				#if _SPECULAR
				float specular = GetSpecular(normalTS,lightDirTS,viewDirTS,INSTANCE( _SpecularRange));
				specular*=diffuse;
				finalCol += lightCol*albedo *specular;
				#endif

				#if _OPACITY
				float opacity=0;
				float2 screenUV=i.screenPos.xy/i.screenPos.w;
				#if _DEPTH
				float depthOffset=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, screenUV),_ZBufferParams) - i.screenPos.w;
				depthOffset=saturate(depthOffset/INSTANCE(_DepthDistance));
				depthOffset= pow(1-depthOffset,INSTANCE(_DepthPow));
				opacity+=depthOffset;
				#endif
				#if _FRESNEL
				float NDV=dot(normalTS,viewDirTS);
				opacity+=pow(NDV,INSTANCE(_FresnelPow))*.5;
				#endif
				opacity=lerp(1-INSTANCE(_BeginOpacity),1,saturate(opacity));
				#if _DISTORT
				float2 screenDistort=normalTS.xy*INSTANCE(_DistortStrength);
				screenDistort*=1-opacity;
				screenUV+=screenDistort;
				#endif
				float3 geometryTex=SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,screenUV);
				finalCol=lerp( finalCol,geometryTex,opacity);
				#endif
				
				return float4(finalCol,1);
            }
            ENDHLSL
        }
	}
}
