Shader "Hidden/PBRSlicing"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		_SlicePlane("Plane",Vector) = (0,1,0,1)
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
		
		[Header(Tooness)]
    	[MinMaxRange]_GeometryShadow("Geometry Shadow",Range(0,1))=0
    	[HideInInspector]_GeometryShadowEnd("",float)=0.1
		_IndirectSpecularOffset("Indirect Specular Offset",Range(-7,7))=0
		
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
			#include "Assets/Shaders/Library/Geometry.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float,_GeometryShadow)
				INSTANCING_PROP(float,_GeometryShadowEnd)
				INSTANCING_PROP(float,_IndirectSpecularOffset)
			INSTANCING_BUFFER_END
			float4 _SlicePlane;
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			
			float GetGeometryShadow(BRDFSurface surface,BRDFLightInput lightSurface)
			{
			    return saturate(invlerp(_GeometryShadow,_GeometryShadowEnd, lightSurface.NDL));
			}

			float GetNormalDistribution(BRDFSurface surface,BRDFLightInput lightSurface)
			{
			    half sqrRoughness=surface.roughness2;
			    half NDH=lightSurface.NDH;

			    NDH = saturate(NDH);
			    float d = NDH * NDH * (sqrRoughness-1.f) +1.00001f;
							
			    float specular= sqrRoughness / (d * d);
			    specular = clamp(specular,0,100);
			    
			    half steps = max(((1 - surface.smoothness) * 2), 0.01);
			    float toonSpecular = round(specular * steps) / steps;
			    return toonSpecular;
			}

			float4 OverrideAlbedo(float3 positionWS,float2 uv)
			{
				GPlane plane = GPlane_Ctor(_SlicePlane.xyz, _SlicePlane.w);
				clip(- Distance(plane,positionWS));
				return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv)*INSTANCE(_Color);
			}

			void OverrideGlobalIllumination(out half3 indirectDiffuse,out half3 indirectSpecular,v2ff i,BRDFSurface surface,Light mainLight)
			{
				indirectDiffuse = IndirectDiffuse(mainLight,i,surface.normal);
				indirectSpecular = IndirectSpecular(surface.reflectDir, surface.perceptualRoughness,INSTANCE(_IndirectSpecularOffset));
			}
			
			#define GET_ALBEDO(i) OverrideAlbedo(i.positionWS,i.uv)
			#define GET_GEOMETRYSHADOW(surface,lightSurface) GetGeometryShadow(surface,lightSurface)
	        #define GET_NORMALDISTRIBUTION(surface,input) GetNormalDistribution(surface,input)
			#define GET_GI(indirectDiffuse,indirectSpecular,i,surface,mainLight) OverrideGlobalIllumination(indirectDiffuse,indirectSpecular,i,surface,mainLight);
    	ENDHLSL
    	
		Pass
		{
			Cull Back
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
			
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}

		Pass
		{
			Cull Front
			NAME "FORWARD1"
			HLSLPROGRAM

			void VertexTransfer(a2vf v,inout v2ff o)
			{
				float3 positionWS = o.positionWS;
				GRay cameraRay = GRay_Ctor(GetCameraRealPositionWS(positionWS),GetCameraRealDirectionWS(positionWS));
				GPlane plane = GPlane_Ctor(_SlicePlane.xyz , _SlicePlane.xyz * _SlicePlane.w );
				o.normalWS = _SlicePlane.xyz;
				float distance = Distance(plane,cameraRay);
				positionWS = cameraRay.GetPoint(distance);
				o.uv = positionWS.xz;
			}
			
			#define V2F_ADDITIONAL_TRANSFER(v,o) VertexTransfer(v,o);
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
            #pragma target 3.5
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			ENDHLSL
		}

		Pass
		{
			NAME "SHADOWCASTER"
			Tags{"LightMode" = "ShadowCaster"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull Off
			
			HLSLPROGRAM
			
            #include "Assets/Shaders/Library/Passes/ShadowCaster.hlsl"
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			ENDHLSL
		}

		Pass
		{
			NAME "DEPTH"
			Tags{"LightMode" = "DepthOnly"}
			
			Blend Off
			ZWrite On
			ZTest LEqual
			Cull Off
			
			HLSLPROGRAM
			#pragma vertex DepthVertex
			#pragma fragment DepthFragment
            #include "Assets/Shaders/Library/Passes/DepthOnly.hlsl"
			ENDHLSL
		}

		Pass
		{
            Tags{"LightMode" = "SceneSelectionPass"}
			Blend Off
			Cull Off

            HLSLPROGRAM
            #pragma vertex VertexSceneSelection
            #pragma fragment FragmentSceneSelection
            #include "Assets/Shaders/Library/Passes/SceneOutlinePass.hlsl"
            ENDHLSL
		}
    }


}
