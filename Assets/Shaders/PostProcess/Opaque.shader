Shader "Hidden/PostProcess/Opaque"
{
	Properties
	{
		[PreRenderData]_MainTex ("Texture", 2D) = "white" {}
	}
		SubShader
		{
			Cull Off ZWrite Off ZTest Always
			HLSLINCLUDE
				#include "Assets/Shaders/Library/PostProcess.hlsl"
				#include "Assets/Shaders/Library/Geometry.hlsl"
				#pragma multi_compile_local_fragment _ _AO
	            #pragma multi_compile_local_fragment _ _VOLUMETRICCLOUD
			ENDHLSL
			Pass
			{
				NAME "SAMPLE"
				HLSLPROGRAM
				#pragma vertex vert_img
				#pragma fragment frag
				
				#pragma multi_compile_local_fragment _ _SCAN
				#if _SCAN
					float4 _ScanColor;
					float3 _ScanOrigin;

					float _ScanMinSqrDistance;
					float _ScanMaxSqrDistance;
					float _ScanFadingPow;
					#pragma multi_compile_local_fragment _ _MASK_TEXTURE
					#if _MASK_TEXTURE
						TEXTURE2D( _ScanMaskTexture);SAMPLER(sampler_ScanMaskTexture);
						float _ScanMaskTextureScale;
					#endif
				#endif
				
				#pragma multi_compile_local_fragment _ _AREA
				#if _AREA
					TEXTURE2D(_AreaFillTexture);SAMPLER(sampler_AreaFillTexture);
					float3 _AreaOrigin;
					float4 _AreaFillColor;
					float _AreaTextureScale;
					float4 _AreaTextureFlow;
					float4 _AreaEdgeColor;
					float _AreaSqrEdgeMin;
					float _AreaSqrEdgeMax;
				#endif
				
				#pragma multi_compile_local_fragment _ _OUTLINE
				#if _OUTLINE
					// #pragma multi_compile_local _CONVOLUTION_SOBEL
					// #pragma multi_compile_local _DETECT_COLOR _DETECT_NORMAL
				
					static const half2 CONVOLUTION_SAMPLE[9]={
						half2(-1, -1), half2(0, -2),half2(1, -1),
						half2(-2, 0), half2(0, 0), half2(2, 0),
						half2(-1, 1), half2(0, 2), half2(1, 1)
					};
				
					// #ifdef _CONVOLUTION_SOBEL
						#define GX -1,-2,-1,0,0,0,1,2,1
						#define GY -1,0,1,-2,0,2,-1,0,1
					// #else
						// #define GX -1,-1,-1,0,0,0,1,1,1
						// #define GY -1,0,1,-1,0,1,-1,0,1
					// #endif
					static const int CONVOLUTION_GX[9]={GX};
					static const int CONVOLUTION_GY[9]={GY};
				
					half OutlineDiff(half3 col,float2 uv)
					{
						half diff;
						// #if _DETECT_COLOR
							// diff=Luminance(SampleMainTex(uv).rgb);
						// #elif _DETECT_NORMAL
							// diff=abs(dot(ClipSpaceNormalFromDepth(uv),float3(0,0,-1)));
						// #else
							diff=Sample01Depth(uv);
						// #endif
						return diff;
					}
				
					half3 _OutlineColor;
					half _OutlineWidth;
					half _Bias;
				#endif

				#pragma multi_compile_local_fragment _ _HIGHLIGHT
				#if _HIGHLIGHT
					half3 _HighlightColor;
					TEXTURE2D(_OUTLINE_MASK);SAMPLER(sampler_OUTLINE_MASK);
					TEXTURE2D(_OUTLINE_MASK_BLUR);SAMPLER(sampler_OUTLINE_MASK_BLUR);
				#endif

				#if _AO
					half3 _AOColor;
				#endif
				
	            #if _VOLUMETRICCLOUD
	                TEXTURE2D(_ColorRamp);SAMPLER(sampler_ColorRamp);
	            #endif
				TEXTURE2D(_Opaque_Sample);SAMPLER(sampler_Opaque_Sample);

				half4 frag (v2f_img i) : SV_Target
				{
					float3 positionWS=TransformNDCToWorld(i.uv);
					half3 col=SampleMainTex(i.uv).rgb;
					half3 sample=SAMPLE_TEXTURE2D(_Opaque_Sample,sampler_Opaque_Sample,i.uv).rgb;
					#if _AO
						half occlusion=sample.r;
						col=lerp(col,_AOColor, occlusion);
					#endif
					
	                #if _VOLUMETRICCLOUD
	                    half cloudDensity=sample.g;
	                    half lightIntensity=sample.b;
	                    half3 rampCol=SAMPLE_TEXTURE2D(_ColorRamp,sampler_ColorRamp, lightIntensity).rgb;
	                    half3 cloudLightCol = lerp(rampCol,_MainLightColor.rgb, cloudDensity*lightIntensity);
	                    col=lerp(cloudLightCol,col, cloudDensity) ;
	                #endif
					
					
					#if _SCAN
						half scanSQRDistance=sqrDistance(_ScanOrigin-positionWS);
						float scan = 1;
						scan *= _ScanColor.a;
						scan *= pow(saturate(invlerp( _ScanMinSqrDistance,_ScanMaxSqrDistance,scanSQRDistance)),_ScanFadingPow)*step(scanSQRDistance, _ScanMaxSqrDistance);

						#if _MASK_TEXTURE
						scan *= SAMPLE_TEXTURE2D(_ScanMaskTexture,sampler_ScanMaskTexture, positionWS.xz*_ScanMaskTextureScale).r;
						#endif
						col=lerp(col,col*_ScanColor.rgb, scan*_ScanColor.a);
					#endif

					#if _AREA
						half areaSQRDistance=sqrDistance(_AreaOrigin-positionWS);
						
						float fill = step(areaSQRDistance,_AreaSqrEdgeMin);
						float edge = saturate( invlerp(_AreaSqrEdgeMax,_AreaSqrEdgeMin,areaSQRDistance))*(1-fill);
						
						float2 uv = (positionWS.xz-_AreaOrigin.xz + _AreaTextureFlow.xy * _Time.y)* _AreaTextureScale;
						float fillMask=SAMPLE_TEXTURE2D(_AreaFillTexture,sampler_AreaFillTexture, uv ).r;
						float3 fillColor = lerp(col,_AreaFillColor.rgb,fillMask);
						float3 edgeColor = _AreaEdgeColor.rgb;
						
						col=lerp(col,fillColor,fill*_AreaFillColor.a);
						col=lerp(col,edgeColor,edge*_AreaEdgeColor.a);
					#endif

					#if _OUTLINE
						half edgeX=0;
						half edgeY=0;
						[unroll]
						for (int it = 0; it < 9; it++)
						{
							half diff=OutlineDiff(col,i.uv+CONVOLUTION_SAMPLE[it]*_MainTex_TexelSize.xy*_OutlineWidth);
							edgeX+=diff*CONVOLUTION_GX[it];
							edgeY+=diff*CONVOLUTION_GY[it];
						}
						half edgeDetect=step(_Bias,abs(edgeX)+abs(edgeY));
						col= lerp(col,_OutlineColor.rgb,edgeDetect);
					#endif
					
					#if _HIGHLIGHT
						float mask=SAMPLE_TEXTURE2D(_OUTLINE_MASK_BLUR,sampler_OUTLINE_MASK_BLUR,i.uv).r-SAMPLE_TEXTURE2D(_OUTLINE_MASK,sampler_OUTLINE_MASK,i.uv).r;
						col+=saturate(mask)*_HighlightColor;
					#endif

					return half4(col,1);
				}
				ENDHLSL
		}
			
		Pass
		{
			NAME "Volumetric Sample"
			HLSLPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#pragma multi_compile_local_fragment _ _DITHER
	            #pragma multi_compile_local_fragment _ _SHAPE

			#define MAX_SAMPLE_COUNT 64u
			half3 _AOSampleSphere[MAX_SAMPLE_COUNT];
			uint _AOSampleCount;
			float _AOIntensity;
			float _AORadius;
			float _AOBias;
			
            float _VerticalStart;
            float _VerticalEnd;
            
            int _RayMarchTimes;
            float _Distance;
            float _Density;
            float _DensityClip;
            float _DensitySmooth;
            float _Opacity;
            
            float _ScatterRange;
            float _ScatterStrength;

            sampler3D _MainNoise;
            float3 _MainNoiseScale;
            float3 _MainNoiseFlow;

			TEXTURE2D(_VOLUMETRIC_DEPTH);
			SAMPLER(sampler_VOLUMETRIC_DEPTH);

            float SampleDensity(float3 worldPos)  {
                return  tex3Dlod(_MainNoise,float4( (worldPos+_MainNoiseFlow*_Time.y)/_MainNoiseScale,0)).r*_Density;
            }
		
			half4 frag(v2f_img i):SV_TARGET
			{
				float2 uv=i.uv;
				float rawDepth=SampleRawDepth(uv);
				float eyeDepth=RawToEyeDepth(rawDepth);
				float3 cameraPositionWS=GetCameraPositionWS();
				half3 cameraDirectionWS=TransformNDCToFrustumCornersRay(uv);
				half eyeDepthProjection=length(cameraDirectionWS);
                half3 marchDirWS=cameraDirectionWS/eyeDepthProjection;
				
				float3 positionWS=cameraPositionWS+eyeDepth*marchDirWS;
				float actualDepth=eyeDepth*eyeDepthProjection;
				half3 normalWS=SampleNormalWS(uv);
                half ao=0;
				#if _AO
					float occlusion = 0;
					half radius=_AORadius;
					#if _DITHER
						radius*=remap(dither01(uv*_ScreenParams.xy),0.,1.,0.5,1.);
					#endif
					float rcpRadius=rcp(_AORadius);
					uint sample=max(_AOSampleCount,MAX_SAMPLE_COUNT);
					[unroll(MAX_SAMPLE_COUNT)]
					for (uint i = 0u; i < _AOSampleCount; i++) {
						half3 dir=_AOSampleSphere[i];
						float3 sampleWS=positionWS+dir *radius*sign(dot(normalWS,dir));
						float2 sampleUV;
						float sampleDepth;
						TransformHClipToUVDepth(mul(_Matrix_VP, float4(sampleWS,1)),sampleUV,sampleDepth);
						float depthOffset =  TransformWorldToEyeDepth(sampleWS,_Matrix_V)-SampleEyeDepth(sampleUV);
						float depthSample=saturate(depthOffset*rcpRadius)*step(depthOffset,_AOBias);
						occlusion+=depthSample;
					}
					occlusion*=rcp(_AOSampleCount);
					occlusion = saturate(occlusion  * _AOIntensity);
					occlusion*=step(HALF_MIN,abs(rawDepth-Z_END));		//Clip Skybox
					ao=occlusion;
				#endif
				
                half2 cloud=0;
                #if _VOLUMETRICCLOUD
	                float3 lightDirWS=normalize(_MainLightPosition.xyz);
	                GRay viewRayWS=GRay_Ctor(cameraPositionWS,marchDirWS);
					float3 marchBegin=cameraPositionWS;
					float marchDistance=0;
	            	#if _SHAPE
	            		float2 distances=SAMPLE_TEXTURE2D(_VOLUMETRIC_DEPTH,sampler_VOLUMETRIC_DEPTH,uv).rg;
						float distance1=RawToEyeDepth(distances.y)*eyeDepthProjection;
						float distance2=RawToEyeDepth(distances.x)*eyeDepthProjection;
		                distance1=min(actualDepth,distance1);
		                distance2=min(actualDepth,distance2);
						marchDistance=distance2-distance1;
						marchBegin=cameraPositionWS+marchDirWS*distance1;
	            	#else
		                GPlane planeStartWS=GPlane_Ctor( float3(0,1,0),_VerticalStart);
		                GPlane planeEndWS=GPlane_Ctor(float3(0,1,0),_VerticalEnd);
		                float distance1 = PlaneRayDistance(planeStartWS,viewRayWS);
						float distance2 = PlaneRayDistance(planeEndWS,viewRayWS);
		                distance1=min(actualDepth,distance1);
		                distance2=min(actualDepth,distance2);
		                if(_VerticalStart< cameraPositionWS.y && cameraPositionWS.y<_VerticalEnd)
		                {
		                    marchDistance=max(distance1,distance2);
		                }
		                else if(distance1>0)
		                {
		                    float distanceOffset=distance1-distance2;
		                    marchBegin=cameraPositionWS+marchDirWS* (distanceOffset>0?distance2:distance1);
		                    marchDistance=abs(distanceOffset);
		                }
	            	#endif

	                float cloudDensity=1;
	                float lightIntensity=1;
	                if(marchDistance>1)
	                {
	                    float cloudMarchDst= _Distance/_RayMarchTimes;
	                    float cloudMarchParam=1.0/_RayMarchTimes;
	                    float dstMarched=0;
	                    float totalDensity=0;
	                    for(int index=0;index<_RayMarchTimes;index++)
	                    {
	                        float3 marchPos=marchBegin+marchDirWS*dstMarched;
	                        float density=SampleDensity(marchPos)*cloudMarchParam;
	                        if(density>0)
	                        {
	                            cloudDensity*= exp(-density*_Opacity);
	                            lightIntensity -= density*cloudDensity;
	                        }

	                        dstMarched+=cloudMarchDst;
	                        if(cloudDensity<0.01||dstMarched>marchDistance)
	                            break;
	                    }
	                }
					cloudDensity=step(_DensityClip,cloudDensity);
				
	                cloud= half2(cloudDensity,lightIntensity);
                #endif
				return half4(ao,cloud,1);
			}
			ENDHLSL
		}
	}
}
