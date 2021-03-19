Shader "Game/Effects/Depth/River"
{
	Properties
	{
		_MainTex("Color UV TEX",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		_FlowDirectionX("Flow X",Range(-1,1))=0.1
		_FlowDirectionY("Flow Y",Range(-1,1))=0.1
		[Toggle(_WAVE)]_EnableWave("Enable Wave",float)=1
		[Header(Wave))]
		_WaveStrength("Wave Strength",Range(0.01,2))=1
		_WaveFrequency("Wave Frequency",Range(0.01,10))=5
		[Header(_Specular)]
		_SpecularRange("Specular Range",Range(.90,1)) = 1
		_SpecularDistort("Specular Distort",Range(0,2))=.5
		[Header(Distort)]
		[NoScaleOffset]_DistortTex("Distort Texure",2D) = "white"{}
		_DistortStrength("Distort Strength",Range(0,2))=.5
		_RefractionStrength("Refraction Strength",Range(0,2))=.5
		[Header(_Depth)]
		_FresnelBase("Fresnel Base",Range(0,1))=.2
		_FresnelMax("Fresnal Max",Range(0,1))=.5
		_FresnelScale("Fresnel Scale",Range(0,2))=2
		_DepthStart("Depth Start",Range(0,1))=.5
		_DepthWidth("Depth Width",Range(0,2))=.5
		[Header(Foam)]
		[Toggle(_SOFTFOAM)]_SoftFoam("Soft Foam",float)=1
		[HDR]_FoamColor("Foam Color",Color)=(1,1,1,1)
		_FoamWidth("Foam Width",Range(0,1))=.2
	}
	SubShader
	{
		Tags {  "Queue"="Transparent-1"  }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Pass		//Base Pass
		{
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
			#pragma shader_feature _SOFTFOAM
			#pragma shader_feature _WAVE
			#include "../../CommonInclude.hlsl"
			#include "../../CommonLightingInclude.hlsl"
			
			struct appdata
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv:TEXCOORD0;
				float3 positionWS:TEXCOORD1;
				float3 viewDirWS:TEXCOORD2;
				float3 normalWS:TEXCOORD3;
				float4 screenPos:TEXCOORD4;
				float4 shadowCoordWS:TEXCOORD5;
			};

			TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
			TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
			TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
			TEXTURE2D(_DistortTex);SAMPLER(sampler_DistortTex);
			CBUFFER_START(UnityPerMaterial)
			float4 _MainTex_ST;
			float4 _Color;

			float _FlowDirectionX;
			float _FlowDirectionY;
			float _DistortStrength;
			float _RefractionStrength;
			float _SpecularDistort;
			float _SpecularRange;
			float _FresnelBase;
			float _FresnelMax;
			float _FresnelScale;
			float _DepthStart;
			float _DepthWidth;
			float _WaveStrength;
			float _WaveFrequency;
			float _FoamWidth;
			float4 _FoamColor;
			CBUFFER_END
			float Wave(float3 worldPos)
			{
				return sin(worldPos.x *_FlowDirectionX+worldPos.y*_FlowDirectionY +_Time.y*_WaveFrequency)*_WaveStrength;
			}

			float4 _DistortParam;
			float2 Distort(float2 uv)
			{
				return SAMPLE_TEXTURE2D(_DistortTex,sampler_DistortTex, uv+ float2(_FlowDirectionX,_FlowDirectionY) *_Time.y*_RefractionStrength).rg*_DistortStrength;
			}

			float4 _FresnelParam;
			float FresnelOpacity(float3 normal, float3 viewDir) {
				return lerp( _FresnelBase ,_FresnelBase+_FresnelMax, saturate( _FresnelScale* (1 - dot(normal, viewDir))));
			}
			
			float Specular(float2 distort, float3 normal, float3 viewDir, float3 lightDir)
			{
				return GetSpecular(normal,lightDir,viewDir,_SpecularRange)*_SpecularDistort*_DistortStrength;
			}

			float4 _FoamDepthParam;
			float Foam(float depthOffset) {
			#if _SOFTFOAM
				return smoothstep(_FoamWidth, 0, depthOffset);
			#else
				return 1-step( _FoamWidth,depthOffset);
			#endif
			}

			float DepthOpacity(float depthOffset)
			{
				return smoothstep(_DepthStart, _DepthStart+_DepthWidth,depthOffset);
			}

			float2 DepthDistort(float depthOffset, float2 distort)
			{
				return step(_DepthStart, depthOffset) * distort;
			}

			v2f vert (appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				o.positionWS = TransformObjectToWorld(v.positionOS);
				#if _WAVE
				o.positionWS += float3(0, Wave(o.positionWS),0);
				#endif
				o.uv = TRANSFORM_TEX( o.positionWS.xz,_MainTex);
				o.positionCS = TransformWorldToHClip(o.positionWS);
				o.normalWS =TransformObjectToWorldNormal(v.normalOS) ;
				o.viewDirWS=GetCameraPositionWS()-o.positionWS;
				o.screenPos= ComputeScreenPos(o.positionCS);
				o.shadowCoordWS=TransformWorldToShadowCoord(o.positionWS);
				return o;
			}

			
			float4 frag (v2f i) : SV_Target
			{
				float3 normal = normalize(i.normalWS);
				float3 viewDir = normalize(i.viewDirWS);
				float3 lightDir = normalize(_MainLightPosition.xyz);
				float linearDepthOffset = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r - i.screenPos.w;

				float2 distort = Distort(i.uv);

				float fresnelOpacity = FresnelOpacity(normal, viewDir);
				float depthOpacity = DepthOpacity(linearDepthOffset);
				float totalOpacity = saturate( fresnelOpacity + depthOpacity);
				

				float3 transparentTexture = SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture, i.screenPos.xy / i.screenPos.w + DepthDistort(linearDepthOffset, distort) ).rgb;
				float3 albedo = (SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv+distort)*_Color).rgb;
				float3 finalCol=lerp(transparentTexture, albedo, totalOpacity);
				
				float foam = Foam(linearDepthOffset) * _FoamColor.a;
				float3 foamColor = _FoamColor.rgb * foam;

				finalCol=lerp(finalCol,foamColor,saturate(foam));
				
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);
				float specular =lerp(Specular(distort, normal, viewDir, lightDir),0,foam);
				float3 specularColor = _MainLightColor.rgb * specular*atten;
				finalCol*=atten;

				finalCol+=specularColor;
				return  float4(finalCol,1);
			}
			ENDHLSL
		}
	}

}
