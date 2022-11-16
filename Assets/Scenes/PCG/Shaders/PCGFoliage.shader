Shader "PCG/Foliage"
{
    Properties
    {
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
		
		[Header(Detail Tex)]
		_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
    	
		[Header(Flow)]
        _WindFlowTex("Flow Texture",2D)="white"{}
        _BendStrength("Strength (Angle)",float)=1
    	
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
    	_AlphaCutoff("Cutoff",Range(0,1))=.9
    }
    SubShader
    {
    	HLSLINCLUDE

			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#include "PCGInclude.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_WindFlowTex);SAMPLER(sampler_WindFlowTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float,_AlphaCutoff)
			
                INSTANCING_PROP(float4,_WindFlowTex_ST)
			    INSTANCING_PROP(float,_WiggleStrength)
			    INSTANCING_PROP(float,_WiggleDensity)
			    INSTANCING_PROP(float,_BendStrength)
			INSTANCING_BUFFER_END
			#include "Assets/Shaders/Library/BRDF/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/BRDF/BRDFMethods.hlsl"

            float3 Wind(float3 positionWS,float windEffect,float _bendStrength,float3 normal,float3 tangent)
            {
                float4 windParameters=INSTANCE(_WindFlowTex_ST);
                float2 bendUV= positionWS.xz+_Time.y*windParameters.zw;
                bendUV*=windParameters.xy;
                float2 flowSample=SAMPLE_TEXTURE2D_LOD(_WindFlowTex,sampler_WindFlowTex,bendUV,0).rg;
                float windInput= flowSample.x-flowSample.y;
            	
                float3x3 bendRotation=Rotate3x3(_bendStrength*windInput*Deg2Rad,tangent);

                float3 bendOffset=mul(bendRotation,normal*windEffect);
                bendOffset-=normal*windEffect;

                #if _WIGGLE
				    float wiggleDensity=INSTANCE(_WiggleDensity);
				    float2 wiggleClip=abs((positionWS.xz+_Time.y*windParameters.zw)%wiggleDensity-wiggleDensity*.5f)/wiggleDensity;
				    float wiggle=wiggleClip.x+wiggleClip.y;
				    bendOffset +=  normal*wiggle*INSTANCE(_WiggleStrength)*windEffect;
				#endif
            	
                return bendOffset;
            }
			
			float3 GetPositionWS(float3 _positionOS,float3 _normalOS,float3 _tangentOS,float4 _color)
			{
				float3 positionWS = TransformObjectToWorld(_positionOS);
				float3 normalWS = TransformObjectToWorldNormal(_normalOS);
				float3 tangentWS = TransformObjectToWorldDir(_tangentOS);
				return positionWS + Wind(positionWS,_color.r,INSTANCE(_BendStrength),normalWS,tangentWS);
			}
			float3 GetAlbedoOverride(float2 uv,float3 color)
			{
				float4 sample =SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, uv);
				clip(sample.a-_AlphaCutoff);
				return sample.rgb  * _Color;
			}

			#define GET_POSITION_WS(v,o) GetPositionWS(v.positionOS,v.color,v.normalOS,v.tangentOS)
			#define GET_ALBEDO(i) GetAlbedoOverride(i.uv,i.color.rgb);
    	ENDHLSL
        Pass
        {
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#include "Assets/Shaders/Library/BRDF/BRDFLighting.hlsl"
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
