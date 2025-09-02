Shader "Hidden/ReflectiveShadowMapReceiver"
{
    Properties
    {
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[ToggleTex(_PBRMAP)] [NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="white"{}
		[Fold(_PBRMAP)]_Smoothness("Smoothness",Range(0,1))=.5
		[Fold(_PBRMAP)]_Metallic("Metallic",Range(0,1))=0
		
		[Header(Detail Tex)]
		_EmissionTex("Emission",2D)="white"{}
		[HDR]_EmissionColor("Emission Color",Color)=(0,0,0,0)
    	
		[Header(Render Options)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull",int)=2
    }
    SubShader
    {
    	HLSLINCLUDE
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float,_Smoothness)
				INSTANCING_PROP(float,_Metallic)
			INSTANCING_BUFFER_END
		ENDHLSL
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			void GetPBRParameters(float2 uv,inout float smoothness,inout float metallic,inout float ao)
			{
				#if _PBRMAP
					float3 pbr = SAMPLE_TEXTURE2D(_PBRTex,sampler_PBRTex,uv).rgb;
					smoothness = pbr.x;
					metallic = pbr.y;
					ao = pbr.z;
				#else
					smoothness = INSTANCE(_Smoothness);
					metallic = INSTANCE(_Metallic);
					ao = 1;
				#endif
			}
			#define GET_PBRPARAM(i,smoothness,metallic,ao) GetPBRParameters(i.uv,smoothness,metallic,ao)

			#pragma shader_feature _RSM

			float4 _RSMParams;
			TEXTURE2D(_RSMSample);SAMPLER(sampler_RSMSample);
			float3 OverrideGlobalIllumination(v2ff i,BRDFSurface surface,Light mainLight)
			{
				half3 indirectDiffuse =  IndirectDiffuse(mainLight,i,surface.normal);
			#if _RSM
				indirectDiffuse += SAMPLE_TEXTURE2D(_RSMSample,sampler_RSMSample,surface.positionNDC) * _RSMParams.x;
			#endif
				half3 indirectSpecular = IndirectCubeSpecular(surface.reflectDir,surface.perceptualRoughness,1000);
				return BRDFGlobalIllumination(surface,indirectDiffuse,indirectSpecular);
			}
			
			#define GET_GI(i,surface,mainLight) OverrideGlobalIllumination(i,surface,mainLight);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}

		Pass
		{
			Name "Albedo Alpha"
            Tags{"LightMode" = "AlbedoAlpha"}
			Blend [_SrcBlend] [_DstBlend]
			Cull [_Cull]
			ZWrite [_ZWrite]
			ZTest [_ZTest]

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Shaders/Library/Passes/AlbedoAlphaPass.hlsl"
            ENDHLSL
		}

		Pass
		{
			Name "Normal Depth"
			Tags{"LightMode" = "DepthNormals"}
			Blend One Zero
			Cull [_Cull]
			ZWrite [_ZWrite]
			ZTest [_ZTest]

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/Passes\DepthNormalsPass.hlsl"
			ENDHLSL
		}
		
		Pass
		{
			Name "World Position"
			Tags{"LightMode" = "WorldPosition"}
			Blend One Zero
			Cull [_Cull]
			ZWrite [_ZWrite]
			ZTest [_ZTest]

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/Passes/WorldPositionPass.hlsl"
			ENDHLSL
		}
		Pass
		{
			NAME "DEPTH"
			Tags{"LightMode" = "DepthOnly"}
			
			Blend Off
			ZWrite On
			ZTest [_ZTest]
			Cull [_Cull]
			
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
            #include "Assets/Shaders/Library/Passes/DepthOnly.hlsl"
			ENDHLSL
		}
		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull [_Cull]
			
			HLSLPROGRAM
			
            #include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}
    }
}
