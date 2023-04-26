Shader "Game/Lit/ShellPBR"
{
	Properties
	{
		_ShellDelta("Shell Delta",float)=0
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_RootColor("Root Color",Color)=(0,0,0,0)
		_EdgeColor("Edge Color",Color)=(1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[NoScaleOffset]_PBRTex("PBR Tex(Roughness.Metallic.AO)",2D)="white"{}
		
		[Toggle(_ANISOTROPIC)]_Anisotropic("Anisotropic",int)=0
		[Foldout(_ANISOTROPIC)]_AnisoTropicValue("Anisotropic Value:",Range(0,1))=1
		
		[Header(Fur)]
		_FurTex("Texure",2D)="white"{}
		_FurLength("Length",Range(0,2))=0.1
		_FurAlphaClip("Alpha Clip",Range(0,1))=0.5
		_FurShadow("Inner Shadow",Range(0,1))=0.5

		[Header(Flow)]
		_FlowStrength("Flow Strength",Range(0,1))=0.2
		[NoScaleOffset]_FlowTex("Flow Tex",2D)="black"{}
		_FlowScale1("Scale 1",float)=1
		_FlowScale2("Scale 2",float)=5
		[Vector2]_FlowSpeed1("Speed 1",Vector)=(1,0,0,0)
		[Vector2]_FlowSpeed2("Speed 2",Vector)=(0,5,0,0)
		
	
		[Header(Render Options)]
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Toggle(_ALPHACLIP)]_AlphaClip("Alpha Clip",float)=0
        [Foldout(_ALPHACLIP)]_AlphaClipRange("Range",Range(0.01,1))=0.01
	}
	SubShader
	{
		Tags { "Queue" = "Geometry" }
		Blend Off
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		
		HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#pragma multi_compile_instancing

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_FurTex);SAMPLER(sampler_FurTex);
			TEXTURE2D(_FlowTex);SAMPLER(sampler_FlowTex);
		
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float,_ShellDelta)
				INSTANCING_PROP(float,_Glossiness)
				INSTANCING_PROP(float,_Metallic)
				INSTANCING_PROP(float,_AnisoTropicValue)
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4,_FurTex_ST)
				INSTANCING_PROP(float4, _RootColor)
				INSTANCING_PROP(float4, _EdgeColor)
				INSTANCING_PROP(float,_FurAlphaClip)
				INSTANCING_PROP(float,_FurLength)
				INSTANCING_PROP(float,_FurShadow)

				INSTANCING_PROP(float,_FlowStrength)
				INSTANCING_PROP(float,_FlowScale1)
				INSTANCING_PROP(float2,_FlowSpeed1)
				INSTANCING_PROP(float,_FlowScale2)
				INSTANCING_PROP(float2,_FlowSpeed2)
			INSTANCING_BUFFER_END
			
			#pragma shader_feature_local_fragment _ANISOTROPIC
		
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
			
            #pragma multi_compile_fog
            #pragma target 3.5


		ENDHLSL
		Pass
		{
			NAME "Shell"
			Tags{"LightMode" = "UniversalForward"}
			Cull Off
			HLSLPROGRAM
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
				half sqrRoughness=surface.roughness2;
				half NDH=max(0., lightSurface.NDH);
				half normalDistribution
				#if _ANISOTROPIC
					= NDFA_TrowbridgeReitz(NDH,lightSurface.TDH,lightSurface.BDH,surface.roughnessT,surface.roughnessB);
				#else
					= NDF_CookTorrance(NDH,sqrRoughness);
				#endif

				normalDistribution=clamp(normalDistribution,0,100.h);
				return normalDistribution;
			}

			float3 GetPositionWS(float3 positionOS,float3 normalWS)
			{
				float3 positionWS = TransformObjectToWorld(positionOS);
				
				float delta=INSTANCE(_ShellDelta);
				float amount=delta+=(delta- delta*delta);
				
				positionWS+=normalWS*INSTANCE(_FurLength)*amount;
				return positionWS;
			}

			float3 GetAlbedo(float2 uv)
			{
				float delta= INSTANCE(_ShellDelta);
							
				float2 flow1UV=(uv+_FlowSpeed1*_Time.y)/_FlowScale1;
				float2 flow1=SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,flow1UV).xy*2-1;
				float2 flow2UV=(uv+_FlowSpeed2*_Time.y)/_FlowScale2;
				float2 flow2=SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,flow2UV).xy*2-1;
				float2 finalFlow=(flow1*flow2);
				uv+=finalFlow*_FlowStrength*delta;
				
				float furSample=SAMPLE_TEXTURE2D(_FurTex,sampler_FurTex,uv).r;
				clip(furSample-delta*delta*INSTANCE(_FurAlphaClip));
				
				half4 color=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);
				half3 albedo=color.rgb;
				albedo*=lerp(INSTANCE(_RootColor).rgb,INSTANCE(_EdgeColor).rgb,delta);
				return albedo;
			}

			#define V2F_ADDITIONAL float2 furUV:TEXCOORD8;
			#define V2F_ADDITIONAL_TRANSFER(v,o) o.furUV = TRANSFORM_TEX(v.uv,_FurTex);
			
			#define GET_POSITION_WS(v,o) GetPositionWS(v.positionOS,o.normalWS)
			#define GET_ALBEDO(i)  GetAlbedo(i.furUV)
			#define GET_PBRPARAM(glossiness,metallic,ao) ao=saturate(ao-(1-INSTANCE(_ShellDelta))*INSTANCE(_FurShadow));
	        #define GET_NORMALDISTRIBUTION(surface,input) GetNormalDistribution(surface,input)
			#define GET_EMISSION(i) 0
			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
			ENDHLSL
		}
		USEPASS "Game/Additive/ShadowCaster/MAIN"
        USEPASS "Game/Additive/DepthOnly/MAIN"
	}
}