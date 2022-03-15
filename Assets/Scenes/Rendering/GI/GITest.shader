Shader "Game/Unfinished/GITest"
{
    Properties
    {
		[Foldout(LIGHTMAP_ON,LIGHTMAP_INTERPOLATE)]_LightmapST("CLightmap UV",Vector)=(1,1,1,1)
		[Foldout(LIGHTMAP_ON,LIGHTMAP_INTERPOLATE)]_LightmapIndex("CLightmap Index",int)=0
		[Foldout(LIGHTMAP_INTERPOLATE)]_LightmapInterpolateST("CLightmap Interpolate UV",Vector)=(1,1,1,1)
		[Foldout(LIGHTMAP_INTERPOLATE)]_LightmapInterpolateIndex("CLightmap Interpolate Index",int)=0
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_LightmapST)
			    INSTANCING_PROP(uint,_LightmapIndex)
				INSTANCING_PROP(float4,_LightmapInterpolateST)
			    INSTANCING_PROP(uint,_LightmapInterpolateIndex)
            INSTANCING_BUFFER_END
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ ENVIRONMENT_CUSTOM ENVIRONMENT_INTERPOLATE
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 lightmapUV:TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                float4 lightmapUV:TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS=TransformObjectNormalToWorld(v.normalOS);
				o.lightmapUV=float4(v.lightmapUV*_LightmapST.xy+_LightmapST.zw,v.lightmapUV*_LightmapInterpolateST.xy + _LightmapInterpolateST.zw);
                return o;
            }
            
			float _EnvironmentInterpolate;
	        SAMPLER(sampler_Lightmap0); SAMPLER(sampler_Lightmap_Interpolate0);
	        TEXTURE2D(_Lightmap0); TEXTURE2D(_Lightmap_Interpolate0); 
	        TEXTURE2D(_Lightmap1); TEXTURE2D(_Lightmap_Interpolate1);
	        TEXTURE2D(_Lightmap2); TEXTURE2D(_Lightmap_Interpolate2);
	        TEXTURE2D(_Lightmap3); TEXTURE2D(_Lightmap_Interpolate3);
	        TEXTURE2D(_Lightmap4); TEXTURE2D(_Lightmap_Interpolate4);
	        TEXTURE2D(_Lightmap5); TEXTURE2D(_Lightmap_Interpolate5);
	        TEXTURE2D(_Lightmap6); TEXTURE2D(_Lightmap_Interpolate6);
	        TEXTURE2D(_Lightmap7); TEXTURE2D(_Lightmap_Interpolate7);
	        TEXTURE2D(_Lightmap8); TEXTURE2D(_Lightmap_Interpolate8);
	        TEXTURE2D(_Lightmap9); TEXTURE2D(_Lightmap_Interpolate9);

            //Lightmaps
			half3 SampleLightmapSubtractive(TEXTURE2D_PARAM(lightmapTex,lightmapSampler),float2 lightmapUV)
			{
			    #ifdef UNITY_LIGHTMAP_FULL_HDR
			        return SAMPLE_TEXTURE2D(lightmapTex,lightmapSampler,lightmapUV).rgb;
			    #else
			        return DecodeLightmap(SAMPLE_TEXTURE2D(lightmapTex,lightmapSampler,lightmapUV).rgba, half4(LIGHTMAP_HDR_MULTIPLIER,LIGHTMAP_HDR_MULTIPLIER,0.0h,0.0h));
			    #endif
			}

			half3 SampleLightmapDirectional(TEXTURE2D_PARAM(lightmapTex,lightmapSampler),TEXTURE2D_LIGHTMAP_PARAM(lightmapDirTex,lightmapDirSampler),float2 lightmapUV,half3 normalWS)
			{
			    half3 illuminance = SampleLightmapSubtractive(lightmapTex,lightmapSampler,lightmapUV);
			    float4 directionSample = SAMPLE_TEXTURE2D_LIGHTMAP(lightmapDirTex,lightmapDirSampler,lightmapUV);
			    
			    half3 direction = directionSample.xyz - 0.5;
			    half directionParam = dot(normalWS,direction) / max(1e-4,directionSample.w);
			    return illuminance * directionParam;
			}
			
	        half3 SampleCustomLightmap(float2 _lightmapUV)
	        {
	            half3 illuminance = 0;
	            float2 lightmapUV = _lightmapUV.xy;
	            switch(_LightmapIndex)
	            {
	                case 0:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap0,sampler_Lightmap0),lightmapUV);break;
	                case 1:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap1,sampler_Lightmap0),lightmapUV);break;
	                case 2:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap2,sampler_Lightmap0),lightmapUV);break;
	                case 3:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap3,sampler_Lightmap0),lightmapUV);break;
	                case 4:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap4,sampler_Lightmap0),lightmapUV);break;
	                case 5:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap5,sampler_Lightmap0),lightmapUV);break;
	                case 6:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap6,sampler_Lightmap0),lightmapUV);break;
	                case 7:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap7,sampler_Lightmap0),lightmapUV);break;
	                case 8:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap8,sampler_Lightmap0),lightmapUV);break;
	                case 9:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap9,sampler_Lightmap0),lightmapUV);break;
	            }
	            return illuminance;
	        }
			
	        half3 SampleInterpolateLightmap(float2 _lightmapUV)
	        {
	            half3 interpolate=0;
	            float2 interpolateUV = _lightmapUV;
	            switch(_LightmapInterpolateIndex)
	            {
	                case 0:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate0,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 1:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate1,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 2:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate2,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 3:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate3,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 4:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate4,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 5:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate5,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 6:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate6,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 7:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate7,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 8:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate8,sampler_Lightmap_Interpolate0),interpolateUV);break;
	                case 9:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate9,sampler_Lightmap_Interpolate0),interpolateUV);break;
	            }
	            return interpolate;
	        }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	#if LIGHTMAP_ON
            		#if defined(ENVIRONMENT_CUSTOM) || defined(ENVIRONMENT_INTERPOLATE)
		                Light mainLight = GetMainLight();
            				// return _EnvironmentInterpolate;
				        half3 illuminance =  SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap0,sampler_Lightmap0),i.lightmapUV.xy);

            			#if ENVIRONMENT_INTERPOLATE
			                half3 interpolate = SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate0,sampler_Lightmap_Interpolate0),i.lightmapUV.zw);
			                illuminance = lerp(illuminance,interpolate,_EnvironmentInterpolate);
            			#endif
			            MixRealtimeAndBakedGI(mainLight,i.normalWS,illuminance);
            			return float4(illuminance,1);
            		#endif
            	#endif
			return 0;
            }
            ENDHLSL
        }
        
        USEPASS "Hidden/DepthOnly/MAIN"
        USEPASS "Hidden/ShadowCaster/MAIN"
    }
}
