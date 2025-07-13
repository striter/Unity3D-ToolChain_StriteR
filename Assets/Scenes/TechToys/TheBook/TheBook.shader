Shader "Hidden/TheBook"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		[NoScaleOffset]_PBRTex("PBR Tex(Glossiness.Metallic.AO)",2D)="black"{}
    	[Toggle(_PICTURE)]_PICTURE("Picture Book",int)=1
        
		[Header(Pages)]
		[NoScaleOffset]_Page1Tex("Page 1",2D)="white"{}
		[NoScaleOffset]_Page2Tex("Page 2",2D)="white"{}
		[Fold(_PICTURE)][NoScaleOffset]_Page3Tex("Page 3",2D)="white"{}
		[Fold(_PICTURE)][NoScaleOffset]_Page4Tex("Page 4",2D)="white"{}
    	
    	[Header(Flip)]
		_Progress("Progress", Range(0,1)) = 0
		_Anticipation("Wave Antecipation", range(-3, 3)) = 2
		_WaveHeight("Wave Height", range(0, 0.1)) = 0.026
		_WaveLength("Wave Lenght", float) = 0.43
    }
    SubShader
    {
		HLSLINCLUDE

			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_BlendTex_ST)
				INSTANCING_PROP(float4,_BlendColor)
				INSTANCING_PROP(float4,_EmissionColor)
				INSTANCING_PROP(float,_GeometryShadow)
				INSTANCING_PROP(float,_GeometryShadowEnd)
				INSTANCING_PROP(float,_IndirectSpecularOffset)
				INSTANCING_PROP(float,_Progress)
				INSTANCING_PROP(float,_Anticipation)
				INSTANCING_PROP(float,_WaveHeight)
				INSTANCING_PROP(float,_WaveLength)
				INSTANCING_PROP(float,_FlipPageOffset)
			INSTANCING_BUFFER_END

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			#define A2V_ADDITIONAL float2 uvPage :TEXCOORD1;
			float3x3 GetRotationZMatrix(float angles){

				angles *= PI/180;

				return float3x3(
					cos(angles),-sin(angles),0,
					sin(angles),cos(angles),0,
					0,0,1);
			}

			half ClampSign(half value){
				return clamp(sign(value),0,1);
			}

			void OutputFlipOS(inout float4 color,inout float3 positionOS,inout float3 normalOS,inout float4 tangentOS)
			{
				half _Flip = _Progress;

				half rotateAngles = 0;
							
				float3 vert = positionOS;

				float3 defaultVert = positionOS; 
						
				float i = 1 - _Flip;
						
				half a = _Flip * _Flip * _Flip;
				half b = 3 * _Flip * i * i + 3 * _Flip * _Flip * i + a;
							
				half flipLerp = lerp(a,b,(1 + positionOS.z * _Anticipation) * 0.5);
						
				half pageRotateAmmount = 180 * flipLerp;

				half y = vert.y;
				vert.y = 0;
				vert = mul(GetRotationZMatrix(pageRotateAmmount), vert).xyz;
							
				rotateAngles += color.r * pageRotateAmmount;
							

				half waveVal = sin(abs(positionOS.x) * 2 * PI / _WaveLength) * _WaveHeight * color.b;
							
				rotateAngles -= cos(abs(positionOS.x) * 2 * PI / _WaveLength) * 360 * color.b * _WaveHeight;
							
				defaultVert.y += waveVal;

				vert.y += y + waveVal + _FlipPageOffset * color.r;

				positionOS = lerp(defaultVert, vert, color.r);
							
				float3x3 rotateMatrix = GetRotationZMatrix(rotateAngles);

				normalOS = mul(rotateMatrix, normalOS);
				tangentOS.xyz = mul(rotateMatrix, tangentOS.xyz);
			}
			#define A2V_TRANSFER(v) OutputFlipOS(v.color,v.positionOS,v.normalOS,v.tangentOS);

			void VertexToFragment(float2 uvPage,out uint _pageIndex,out float2 _pageUV)
			{
				float2 pageUV = uvPage;
				pageUV.x = pageUV.x * 4;
				uint pageIndex =  floor(pageUV.x) ;
				pageUV.x = pageUV.x % 1;
				pageUV.y = 1- pageUV.y;

				#if _PICTURE
					pageUV.x = pageIndex % 2 == 0? pageUV.x / 2 : (0.5+pageUV.x/2);
					pageIndex = _Progress > 0 ? (pageIndex /2 ) : 0;
				#endif
				
				_pageIndex = pageIndex + 1;
				_pageUV = pageUV;
			}
			#define V2F_ADDITIONAL float2 uvPage :TEXCOORD6; uint pageIndex :TEXCOORD8;
			#define V2F_ADDITIONAL_TRANSFER(v,o) VertexToFragment(v.uvPage,o.pageIndex,o.uvPage);
		ENDHLSL
	
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
			#pragma multi_compile _ _PICTURE

			TEXTURE2D( _Page1Tex); SAMPLER(sampler_Page1Tex);
			TEXTURE2D( _Page2Tex); SAMPLER(sampler_Page2Tex);
			TEXTURE2D( _Page3Tex); SAMPLER(sampler_Page3Tex);
			TEXTURE2D( _Page4Tex); SAMPLER(sampler_Page4Tex);

			
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			

			float4 GetAlbedo(v2ff i)
			{
				half4 blendAlbedo = 0;
				if(i.pageIndex == 1)
					blendAlbedo = SAMPLE_TEXTURE2D(_Page1Tex,sampler_Page1Tex,i.uvPage);
				else if(i.pageIndex == 2)
					blendAlbedo = SAMPLE_TEXTURE2D(_Page2Tex,sampler_Page2Tex,i.uvPage);
				else if(i.pageIndex== 3)
					blendAlbedo = SAMPLE_TEXTURE2D(_Page3Tex,sampler_Page3Tex,i.uvPage);
				else if(i.pageIndex == 4)
					blendAlbedo = SAMPLE_TEXTURE2D(_Page4Tex,sampler_Page4Tex,i.uvPage);

				half4 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*INSTANCE(_Color);
				albedo.rgb = lerp(albedo.rgb,albedo.rgb*blendAlbedo.rgb, blendAlbedo.a * (i.color.g + i.color.r));
				return albedo;
			}

			#define GET_ALBEDO(i) GetAlbedo(i);
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFLighting.hlsl"
			
			#pragma vertex ForwardVertex
			#pragma fragment ForwardFragment
			#include "Assets/Shaders/Library/Passes/ForwardPBR.hlsl"
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
			ZWrite On
			ZTest LEqual
			Cull Off

            HLSLPROGRAM
            #pragma vertex VertexSceneSelection
            #pragma fragment FragmentSceneSelection
            #include "Assets/Shaders/Library/Passes/SceneOutlinePass.hlsl"
            ENDHLSL
		}
    }
}
