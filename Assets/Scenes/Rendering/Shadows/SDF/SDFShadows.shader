Shader "Hidden/Unfinished/Shadows"
{
    Properties
	{
		[Header(Base Tex)]
		_MainTex("Main Tex",2D) = "white"{}
		_Color("Color Tint",Color) = (1,1,1,1)
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
		
		[Header(PBR)]
		[NoScaleOffset]_PBRTex("PBR Tex(Smoothness.Metallic.AO)",2D)="black"{}
		
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
		Pass
		{
			NAME "FORWARD"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

			TEXTURE2D( _MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_EmissionTex);SAMPLER(sampler_EmissionTex);
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
			TEXTURE2D(_PBRTex);SAMPLER(sampler_PBRTex);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_MainTex_ST)
				INSTANCING_PROP(float4, _Color)
				INSTANCING_PROP(float4,_EmissionColor)
			INSTANCING_BUFFER_END
            #include "Assets/Shaders/Library/Geometry.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFInput.hlsl"
			#include "Assets/Shaders/Library/PBR/BRDFMethods.hlsl"

			#define MAX_SDF_VOLUME_COUNT 32
			int _SDFVolumeCount;
			int _SDFVolumeIndexes[MAX_SDF_VOLUME_COUNT];
			float4 _SDFVolumeShapes[MAX_SDF_VOLUME_COUNT];
			#define MAX_SDF_COUNT 1024
			float4 _SDFParameters1[MAX_SDF_COUNT];
			float4 _SDFParameters2[MAX_SDF_COUNT];
			
			SDFOutput SDF(GRay _ray,float3 _positionWS)
			{
				SDFOutput output;
				output.distance = FLT_MAX;
				output.color = 0;

				SDFInput input = SDFInput_Ctor(_positionWS,0);
				
				int elementIndex = 0;
				for(int i=0;i<_SDFVolumeCount;i++)
				{
					float4 sphereParam = _SDFVolumeShapes[i];
					GSphere sphere = GSphere_Ctor(sphereParam.xyz,sphereParam.w);
					int elementStartIndex = elementIndex;
					elementIndex = elementIndex + _SDFVolumeIndexes[i];
					// output = SDFUnion(output,GSphere_SDF(sphere,input));
					if(sum(Distance(sphere,_ray)) > 0)
					{
						for(int j=elementStartIndex;j<elementIndex;j++)
						{
							float4 parameters1 = _SDFParameters1[j];
							float4 parameters2 = _SDFParameters2[j];
							GCapsule capsule = GCapsule_Ctor(parameters1.xyz,parameters1.w,parameters2.xyz,parameters2.w);
							output = SDFUnion(output,GCapsule_SDF(capsule,input));
						}
					}
				}
				
				return output;
			}
			
			float RaymarchSDFSoftShadow(float3 _position,float3 _lightPosition,float _softConstant,float _bias  ,int _maxSteps )
			{
			    float3 direction = _lightPosition - _position;
			    GRay shadowRay = GRay_Ctor(_position,normalize(direction));
			    float maxMarchLength = length(direction);
			    
			    float res = 1.0;
			    float t = _bias;
			    for( int i=0; i<_maxSteps && t<maxMarchLength; i++ )
			    {
			        float h = SDF(shadowRay,shadowRay.GetPoint(t)).distance;
			        res = min( res, h/(_softConstant*t) );
			        t += clamp(h, 0.005, 0.50);
			        if( res<-1.0 || t>maxMarchLength ) break;
			    }
			    res = max(res,-1.0);
			    return 0.25*(1.0+res)*(1.0+res)*(2.0-res);
			}

			float4 _ShadowParams;
			TEXTURE2D(_ShadowmapTexture);
			Light GetMainLight(v2ff i)
			{
			    Light light = GetMainLight();
				float sdf = RaymarchSDFSoftShadow(i.positionWS,light.direction * 1000,0.005,0.1,128);
				light.shadowAttenuation = sdf;
				return light;
			}
			
			#define GET_MAINLIGHT(i) GetMainLight(i)
			
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
