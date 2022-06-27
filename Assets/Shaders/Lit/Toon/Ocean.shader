Shader "Game/Lit/Toon/Ocean"
{
    Properties
    {
    	[NoScaleOffset]_DepthRamp("Ramp",2D)="white"{}
    	[ColorUsage(false,false)]_DepthColor("Depth Color",Color)=(0,0,0,0)
		[MinMaxRange]_DepthRange("Range",Range(0.01,20))=0
    	[HideInInspector]_DepthRangeEnd("",float)=0
		[MinMaxRange]_FresnelRange("Fresnel",Range(0.01,1))=0
    	[HideInInspector]_FresnelRangeEnd("",float)=0
    	
    	[Header(Specular)]
        _SurfaceNoise("Specular Noise",2D)="white"{}
    	[MinMaxRange]_NoiseRange("Noise Sample",Range(0,1))=0
    	[HideInInspector]_NoiseRangeEnd("",float)=0.1
    	[ColorUsage(false,true)] _NoiseColor("Noise Color",Color)=(1,1,1,1)
    	
    	[Header(Tide)]
    	_RisingTide("Rising Tide",2D)="white"{}
    	_RisingTideColor("Color",Color)=(1,1,1,1)
		[MinMaxRange]_RisingTideRange("Range",Range(0.01,1))=0
    	[HideInInspector]_RisingTideRangeEnd("",float)=0
    	
    	[Header(Flow)]
    	_NoiseFlow("Flow",2D)="black"{}
    	_NoiseFlowStrength("Flow Strength",Range(0,2))=1
    	
    	[Header(Reflection)]
    	_ReflectionOffset("Reflection Offset",Range(-7,7)) = 8
    	_ReflectionDistort("Reflection Distort",Range(0,.1))=.1
    	
    	[Header(Foam)]
		[MinMaxRange]_FoamRange("Foam",Range(0.01,3))=0
    	_FoamDistort("Distort",Range(0,2))=1
    	[HideInInspector]_FoamRangeEnd("",float)=0
    	_FoamColor("Color",Color)=(1,1,1,1)
    	
        [Toggle(_WAVE)]_Wave("Vertex Wave",int)=0
    	[Foldout(_WAVE)]_WaveST1("Wave ST 1",Vector)=(1,1,1,1)
    	[Foldout(_WAVE)]_WaveAmplitude1("Flow Amplitidue 1",float)=1
    	[Foldout(_WAVE)]_WaveST2("Wave ST 2",Vector)=(1,1,1,1)
    	[Foldout(_WAVE)]_WaveAmplitude2("Spike Amplitidue 2",float)=1
    }
    SubShader
    {
        Tags {"Queue"="Transparent"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
            	float3 normalOS : NORMAL;
            	float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            	float3 positionVS:TEXCOORD1;
            	float4 positionHCS:TEXCOORD2;
            	float3 positionWS:TEXCOORD3;
            	float3 normalWS:TEXCOORD4;
            	float3 viewDirWS:TEXCOORD5;
            	float3 cameraDirWS:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            #pragma shader_feature_local _WAVE
            #pragma multi_compile _ ENVIRONMENT_CUSTOM ENVIRONMENT_INTERPOLATE

            TEXTURE2D(_SurfaceNoise);SAMPLER(sampler_SurfaceNoise);
            TEXTURE2D(_NoiseFlow);SAMPLER(sampler_NoiseFlow);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_RisingTide);SAMPLER(sampler_RisingTide);
            TEXTURE2D(_DepthRamp);SAMPLER(sampler_DepthRamp);
            INSTANCING_BUFFER_START
				INSTANCING_PROP(float3,_DepthColor);
				INSTANCING_PROP(float,_DepthRange)
				INSTANCING_PROP(float,_DepthRangeEnd)
	
				INSTANCING_PROP(float,_FresnelRange);
				INSTANCING_PROP(float,_FresnelRangeEnd);
            
				INSTANCING_PROP(float,_ReflectionOffset)
				INSTANCING_PROP(float,_ReflectionDistort)

				INSTANCING_PROP(float4,_RisingTide_ST)
				INSTANCING_PROP(float4,_RisingTideColor)
				INSTANCING_PROP(float,_RisingTideRange)
				INSTANCING_PROP(float,_RisingTideRangeEnd)
            
				INSTANCING_PROP(float,_FoamRange)
				INSTANCING_PROP(float,_FoamRangeEnd)
				INSTANCING_PROP(float3,_FoamColor)
				INSTANCING_PROP(float,_FoamDistort)
            
				INSTANCING_PROP(float4,_SurfaceNoise_ST)
				INSTANCING_PROP(float4,_NoiseFlow_ST)
				INSTANCING_PROP(float,_NoiseFlowStrength)
				INSTANCING_PROP(float3,_NoiseColor)
				INSTANCING_PROP(float,_NoiseRange)
				INSTANCING_PROP(float,_NoiseRangeEnd)

				INSTANCING_PROP(float4,_WaveST1);
            	INSTANCING_PROP(float,_WaveAmplitude1);
            	INSTANCING_PROP(float4,_WaveST2);
            	INSTANCING_PROP(float,_WaveAmplitude2);
            
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float3 positionWS = TransformObjectToWorld(v.positionOS);
            	#if _WAVE
					positionWS.y += sin(sum(TransformTex_Flow( positionWS.xz,_WaveST1)))*_WaveAmplitude1+sin(sum(TransformTex_Flow( positionWS.xz,_WaveST2)))*_WaveAmplitude2;
            	#endif
            	o.positionWS = positionWS;
                o.positionCS = TransformWorldToHClip(positionWS);
				o.positionVS = TransformWorldToView(positionWS);
            	o.positionHCS = o.positionCS;
                o.uv = TRANSFORM_TEX_FLOW_INSTANCE(o.positionWS.xz, _NoiseFlow);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
				o.viewDirWS=GetViewDirectionWS(o.positionWS);
				o.cameraDirWS=GetCameraRealDirectionWS(o.positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	
            	float2 screenUV=TransformHClipToNDC(i.positionHCS);
            	float3 normalWS=normalize(i.normalWS);
            	float3 viewDirWS=normalize(i.viewDirWS);
            	float3 cameraDirWS=normalize(i.cameraDirWS);
            	float3 positionWS = i.positionWS;
            	
            	float2 flow = SAMPLE_TEXTURE2D(_NoiseFlow,sampler_NoiseFlow,i.uv).xy*2-1;

                float rawDepth=SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
            	float eyeDepthUnder=RawToEyeDepth(rawDepth);
            	float eyeDepthSurface=RawToEyeDepth(i.positionCS.z);
            	float eyeDepthOffset=eyeDepthUnder-eyeDepthSurface;

            	float3 positionVS=i.positionVS;
            	positionVS.z = -eyeDepthUnder;
            	positionVS.z = max(positionVS.z , -eyeDepthSurface - _DepthRangeEnd);
            	float3 deepSurfacePosition = mul(UNITY_MATRIX_I_V,float4(positionVS,1)).xyz;
            	float2 risingTideUV = TRANSFORM_TEX_FLOW_INSTANCE(deepSurfacePosition.xz,_RisingTide);
            	float risingTideSample =saturate(invlerp(_RisingTideRange,_RisingTideRangeEnd, SAMPLE_TEXTURE2D(_RisingTide,sampler_RisingTide,risingTideUV+flow*_NoiseFlowStrength).r));
            	
            	float3 indirectDiffuse = IndirectDiffuse_SH(normalWS);
            	float3 reflectDirWS=normalize(reflect(cameraDirWS, normalize(normalWS+float3(flow.x,0,flow.y)*_ReflectionDistort)));
                float3 indirectSpecular = IndirectCubeSpecular(reflectDirWS,1,_ReflectionOffset);
            	
				float noiseSample = SAMPLE_TEXTURE2D(_SurfaceNoise,sampler_SurfaceNoise,TransformTex_Flow(positionWS.xz,_SurfaceNoise_ST)+flow*_NoiseFlowStrength).r;
            	float noiseSpecular = saturate(invlerp(_NoiseRange,_NoiseRangeEnd,noiseSample));
				float3 specular = noiseSpecular * _MainLightColor.rgb * _NoiseColor;
            	
            	float depthParameters = saturate(invlerp(INSTANCE(_DepthRange),INSTANCE(_DepthRangeEnd),eyeDepthOffset));
            	float fresnelParameters = pow2(saturate(invlerp(_FresnelRange,_FresnelRangeEnd, dot(viewDirWS,normalWS))));
				float foamParameters = saturate(invlerp(INSTANCE(_FoamRange),INSTANCE(_FoamRangeEnd),eyeDepthOffset+(noiseSample*2-1)*_FoamDistort));
            	
				float3 depthColor = SAMPLE_TEXTURE2D(_DepthRamp,sampler_DepthRamp,depthParameters)*_DepthColor.rgb;
            	float3 baseCol=depthColor*indirectDiffuse;

            	baseCol = lerp(baseCol,_RisingTideColor.rgb*indirectDiffuse, risingTideSample*_RisingTideColor.a*fresnelParameters);
            	baseCol += indirectSpecular * (1-fresnelParameters);
            	baseCol = lerp(_FoamColor*indirectDiffuse,baseCol,foamParameters);

                float3 finalCol = baseCol + specular;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Hidden/DepthOnly/MAIN"
        USEPASS "Hidden/ShadowCaster/MAIN"
    }
}
