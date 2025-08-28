Shader "Hidden/OffScreenParticle"
{
    SubShader
    {
        Cull Off ZWrite On ZTest Always Cull Off
        Pass
        {
            Name "Clear & Depth DownSample"
            Blend Off
            ZWrite On
            
            HLSLPROGRAM
            #include "Assets/Shaders/Library/PostProcess.hlsl"

            struct f2o
            {
                float depth : SV_Depth;
                float4 color : SV_Target;
            };
            
            #pragma vertex vert_fullScreenMesh
            #pragma fragment frag

            #pragma multi_compile _ _MAX_DEPTH
            int _DownSample;

            f2o frag (v2f_img i) 
            {
                f2o o;
                #if _MAX_DEPTH
                    o.depth = Z_NEAR;
                    float2 startUV = i.uv - _Output_TexelSize.xy * .5f + .5f * _CameraDepthTexture_TexelSize.xy;
                    for (int x = 0;x < _DownSample; x++)
                    {
                        for (int y = 0;y<_DownSample; y++)
                        {
                            float depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,startUV + _CameraDepthTexture_TexelSize.xy * float2(x,y)).r;
                            if (DepthGreater(depth,o.depth))
                                o.depth = depth;
                        }
                    }
                #else
                    o.depth = SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,i.uv).r;
                #endif

                o.color = float4(0,0,0,1);
                
                return o;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "Blend"
            Blend One SrcAlpha
            HLSLPROGRAM
            #pragma vertex vert_fullScreenMesh
            #pragma fragment frag

            #include "Assets/Shaders/Library/PostProcess.hlsl"

            #pragma multi_compile_fragment _ _VDM
            TEXTURE2D(_OffScreenParticleTexture);SAMPLER(sampler_OffScreenParticleTexture);
            TEXTURE2D(_VDMDepthTexture);SAMPLER(sampler_VDMDepthTexture);

            float erf(float x)
            {
            	if (abs(x) > 2.629639)
					return sign(x);
            	float z = 0.289226f *x *x - 1;
            	return (0.0145688 * z * z *z * z * z *z
						-0.0348595f * z * z *z * z * z
						+0.0503913 * z * z *z * z
						-0.0897001 * z * z *z
						+0.156097 * z * z
						-0.249431 * z
						+0.533201 * z
            	) * x;
            }
            float GaussianCDF(float depth, float mean, float sqrVariance)
			{
				return 0.5f * (1 + erf( (depth - mean) / (sqrt(sqrVariance) * kSQRT2)));
			}
            
            float4 frag (v2f_img i) : SV_Target
            {
            	#if _VDM
            		float realDepth = Linear01Depth(SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,i.uv).r,_ZBufferParams);
            		float3 depthTransition = SAMPLE_TEXTURE2D(_VDMDepthTexture,sampler_VDMDepthTexture,i.uv);
            		float2 transition = depthTransition.rg / (depthTransition.b);
					float variance = max(transition.y - transition.x * transition.x, 0.0);
					float cdf = GaussianCDF(realDepth, transition.x, variance);
            	
            		float4 vdmbuffer = SAMPLE_TEXTURE2D(_OffScreenParticleTexture,sampler_OffScreenParticleTexture,i.uv);
					float finalAlpha = 1.0 + cdf * (vdmbuffer.a - 1.0);
					half3 finalColor = vdmbuffer.rgb * cdf;
					return float4(finalColor, finalAlpha);
            	#else
					return SAMPLE_TEXTURE2D(_OffScreenParticleTexture,sampler_OffScreenParticleTexture,i.uv);
				#endif
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "VDM Particle Depth Replacement"
            Blend Off
            ZTest LEqual
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

			#pragma multi_compile_instancing
			#include "Assets/Shaders/Library/Common.hlsl"
			
			struct a2f
			{
				float3 positionOS:POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS:SV_POSITION;
			};

			v2f vert(a2f v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				float3 positionWS = TransformObjectToWorld(v.positionOS);
				o.positionCS = TransformWorldToHClip(positionWS);
				return o;
			}

			float4 frag(v2f i) :SV_TARGET
			{
				float depth = Linear01Depth(i.positionCS.z ,_ZBufferParams);
				return float4(depth,depth * depth,1,1);
			}
            ENDHLSL
        }
    }
}
