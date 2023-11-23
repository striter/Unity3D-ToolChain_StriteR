Shader "Game/Lit/CustomGI"
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
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
			INSTANCING_BUFFER_END
    	ENDHLSL
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM

			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON	

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fog

			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"

			SHL2Input()
			
			void OverrideGlobalIllumination(out half3 indirectDiffuse,out half3 indirectSpecular,v2ff i,BRDFSurface surface,Light mainLight)
			{
				half3 normal = normalize(surface.normal);
				indirectDiffuse = SHL2Sample(normal,);
				indirectSpecular = indirectDiffuse;//IndirectSpecular(surface.reflectDir,surface.perceptualRoughness,1000);
				#if LIGHTMAP_ON

					// half3 lightmap = IndirectDiffuse(mainLight,i,surface.normal);
					half3 lightmap = SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(unity_Lightmap,samplerunity_Lightmap), i.lightmapUV);
				
					float4 directionSample = SAMPLE_TEXTURE2D_LIGHTMAP(unity_LightmapInd,samplerunity_Lightmap,i.lightmapUV);

					half3 direction = (directionSample.xyz - 0.5) * 2;
					indirectDiffuse = SHL2Sample(direction,);

					half halfLambert = dot(surface.normal, direction.xyz - 0.5) + 0.5;
					half directionParam = halfLambert / max(1e-4,directionSample.w);

					surface.ao = directionSample.a;
					indirectDiffuse *= directionParam;

					indirectDiffuse *= lightmap;
					indirectSpecular *= lightmap;

				#endif
			}
			
			void Transfer(a2vf v,inout v2ff i)
			{
				i.uv = i.positionWS.xz + i.positionWS.yz + i.positionWS.xy;
				i.uv /= 3;
			}
			
			#define V2F_ADDITIONAL_TRANSFER(v,o) Transfer(v,o);
			#define GET_GI(indirectDiffuse,indirectSpecular,i,surface,mainLight) OverrideGlobalIllumination(indirectDiffuse,indirectSpecular,i,surface,mainLight);
			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
        
		Pass
		{
            Name "META"
            Tags{"LightMode" = "Meta"}
            Cull Back
			Blend Off
			ZWrite On
			ZTest LEqual

            HLSLPROGRAM
            #pragma vertex VertexMeta
            #pragma fragment FragmentMeta

			// float3 OverrideMetaAlbedo()
			// {
			// 	return 1;
			// }
   //          #define GET_ALBEDO(i) OverrideMetaAlbedo();
            #include "Assets/Shaders/Library/Passes/Meta.hlsl"
            ENDHLSL
		}
    }
}
