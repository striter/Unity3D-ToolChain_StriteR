Shader "Hidden/ContactShadow"
{
    Properties
    {
    }
    SubShader
    {
        Pass
        {
            Name "Contact Shadow"
            ZTest Always
            Blend Off
            ZWrite Off
            HLSLPROGRAM
            #include "Assets/Shaders/Library/PostProcess.hlsl"
            #pragma vertex vert
            #pragma fragment frag

            struct a2v
            {
                float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 ndc : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = float4(v.positionOS.xyz, 1.0);
                #if UNITY_UV_STARTS_AT_TOP
                    o.positionCS.y *= -1;
                #endif
                o.ndc = ComputeNormalizedDeviceCoordinates(o.positionCS.xyz).xy;
                return o;
            }
            
			float CastScreenSpaceShadowRay(
				float3 RayOriginTranslatedWorld, float3 RayDirection, float RayLength, int NumSteps,
				float Dither, float CompareToleranceScale)
			{
            	float4x4 worldToHClip = GetWorldToHClipMatrix();
				float4 RayStartClip = mul(worldToHClip,float4(RayOriginTranslatedWorld, 1));
				float4 RayDirClip = mul(worldToHClip,float4(RayDirection * RayLength, 0));
				float4 RayEndClip = RayStartClip + RayDirClip;

				float3 RayStartScreen = RayStartClip.xyz / RayStartClip.w;
				float3 RayEndScreen = RayEndClip.xyz / RayEndClip.w;

				float3 RayStepScreen = RayEndScreen - RayStartScreen;

				float3 RayStartUVz = float3(ComputeNormalizedDeviceCoordinates(RayStartScreen), RayStartScreen.z);
				float3 RayStepUVz = float3(ComputeNormalizedDeviceCoordinates(RayEndScreen) - RayStartUVz.xy, RayStepScreen.z);

				float4 RayDepthClip = RayStartClip + mul(GetViewToHClipMatrix(),float4(0, 0, RayLength, 0));
				float3 RayDepthScreen = RayDepthClip.xyz / RayDepthClip.w;
            	

            	float StepOffset = Dither - 0.5f;
            	float Step = 1.0 / NumSteps;

            	float CompareTolerance = abs(RayDepthScreen.z - RayStartScreen.z) * Step * CompareToleranceScale;

				float SampleTime = StepOffset * Step + Step;

				 float StartDepth = SampleRawDepth(RayStartUVz.xy);

				for (int i = 0; i < NumSteps; i++)
				{
					float3 SampleUVz = RayStartUVz + RayStepUVz * SampleTime;
					float SampleDepth = SampleRawDepth(SampleUVz.xy);

					// Avoid self-intersection with the start pixel (exact comparison due to point sampling depth buffer)
					// Exception is made for hair for occluding transmitted light with non-shadow casting light
					if (SampleDepth != StartDepth)
					{
						float DepthDiff = SampleUVz.z - SampleDepth;
						bool Hit = abs(DepthDiff + CompareTolerance) < CompareTolerance;

						if (Hit)
						{
							// Off screen masking
							bool bValidUV = 0.0 < SampleUVz.xy && SampleUVz.xy < 1.0;
							return bValidUV ? RayLength * SampleTime : -1;
						}
					}

					SampleTime += Step;
				}

				return -1;
			}

            float4 frag (v2f input) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 samplePos = TransformNDCToWorld_Perspective(input.ndc,SampleRawDepth(input.ndc));
                float3 lightDir = -normalize(_MainLightPosition.xyz);
                
                float maxMarchDistance = .1;
                int marchStepCount = 32;
                return pow(1 - CastScreenSpaceShadowRay(samplePos,-lightDir,maxMarchDistance,marchStepCount,0,1),2);
            }

            ENDHLSL
        }
    }
}
