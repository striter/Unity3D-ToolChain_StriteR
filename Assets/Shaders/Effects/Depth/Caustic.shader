Shader "Game/Effects/Depth/Caustic"
{
	Properties
	{
    	[NoScaleOffset]_CausticTex("Caustic Tex",2D)="black"{}
    	[Vector4]_CausticFlowST1("Flow ST 1",Vector)=(.1,.1,.1,.1)
    	[Vector4]_CausticFlowST2("Flow ST 2",Vector)=(.1,.1,.1,.1)
    	_Strength("Caustic Strength",Range(0.1,5))=1
    	_Chromatic("Chromatic",Range(0,5))=1
	}
	SubShader
	{
		Tags{"Queue" = "Geometry+1"  "IgnoreProjector" = "True" "DisableBatching"="True"  }
		Pass
		{
			Blend One One
			Cull Front
			ZWrite Off
			ZTest LEqual

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"

			struct v2f
			{
				half4 positionCS:SV_POSITION;
				half3 positionWS:TEXCOORD0;
				half4 positionHCS: TEXCOORD1;
			};
			
            TEXTURE2D(_CausticTex);SAMPLER(sampler_CausticTex);
			TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
			CBUFFER_START(UnityPerMaterial)
				INSTANCING_PROP(float,_Strength)
				INSTANCING_PROP(float4,_CausticTex_TexelSize)
				INSTANCING_PROP(float4,_CausticFlowST1)
				INSTANCING_PROP(float4,_CausticFlowST2)
				INSTANCING_PROP(float,_Chromatic)
			CBUFFER_END
			v2f vert(half3 positionOS:POSITION)
			{
				v2f o;
				o.positionCS = TransformObjectToHClip(positionOS);
				o.positionWS=TransformObjectToWorld(positionOS);
				o.positionHCS = o.positionCS;
				return o;
			}

			float SampleCaustic(float2 uv)
			{
				return SAMPLE_TEXTURE2D(_CausticTex,sampler_CausticTex,uv).r;	
			}

			float3 SampleCausticChromatic(float2 uv)
			{
				float3 output = 0;
				float chromatic = INSTANCE(_Chromatic);
			    half2 uv1 = uv + half2(chromatic, chromatic) * _CausticTex_TexelSize.xy;
			    half2 uv2 = uv + half2(chromatic, -chromatic) * _CausticTex_TexelSize.xy;
			    half2 uv3 = uv + half2(-chromatic, -chromatic) * _CausticTex_TexelSize.xy;
				output.r = SampleCaustic(uv1);
				output.g = SampleCaustic(uv2);
				output.b = SampleCaustic(uv3);
				return output;
			}
			
			half4 frag(v2f i):SV_Target
			{
				float2 ndc=TransformHClipToNDC(i.positionHCS);
				float3 positionWSDepth= TransformNDCToWorld(ndc ,SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, ndc).r);

				float3 positionWS = i.positionWS;
				float3 lightDirWS = normalize(_MainLightPosition.xyz);
            	float verticalDistance=positionWS.y-positionWSDepth.y;
            	float3 causticPositionWS=positionWSDepth+lightDirWS*verticalDistance* rcp(dot(float3(0,-1,0),lightDirWS));
				
				half2 causticUV = causticPositionWS.xz;
            	float2 causticForward=TransformTex_Flow(causticUV,_CausticFlowST1);
            	float2 causticBackward=TransformTex_Flow(causticUV,_CausticFlowST2);

            	float3 caustic = min(SampleCausticChromatic(causticForward),SampleCausticChromatic(causticBackward));
				caustic *= saturate(verticalDistance);
				// half atten=MainLightRealtimeShadow(TransformWorldToShadowCoord(positionWSDepth));
				// caustic*=atten;
				return float4(caustic * _Strength,1);
			}
			ENDHLSL
		}
	}
}
