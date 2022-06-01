Shader "Game/Effects/EnergyShield"
{
	Properties
	{
		[Header(Color)]
	    [HDR]_RimColor("Rim Color", Color) =(1,1,1,1)
	    _RimWidth("Rim Width", Range(0.2,20.0)) = 3.0
	    _EdgeMultiplier("Rim Glow Multiplier", Range(0.0,9.0)) = 1.0
		_MaskTex("Mask Texture",2D)="white"{}
		[HDR]_InnerColor("Inner Color",Color)=(.5,.5,.5,.5)

		[Header(Inner Glow)]
		[Toggle(_INNERGLOW)]_EnableInnerGlow("Enable Inner Glow",float)=1
		[HDR]_InnerGlow("Inner Glow",Color)=(1,1,1,1)
		_InnerGlowFrequency("Inner Glow Frequency",Range(0,20))=5
		_InnerGlowClip("Inner Glow Clip",Range(0,1))=.5
		_InnerGlowSpeed("Inner Glow Speed",Range(0,5))=1

		[Header(Vertical Smoothen)]
		[Toggle(_VERTICALSMOOTHEN)]_EnableVerticalSmoothen("Enable Vertical Smoothen",float)=1
		_VerticalSmoothenStart("Vertical Smoothen Start",Range(0,1))=.48
		_VerticalSmoothenDistance("Vertical Smoothen Distance",Range(0,.5))=.1

		[Header(Depth)]
		[Toggle(_DEPTHOFFSET)]_EnableDepthOffset("Enable Depth Offset",float)=1
		_DepthMultiplier("Depth Multiplier",Range(0,1))=.5

		[Header(Vertex Random Distort)]
		[Toggle(_VERTEXRANDOMDISTORT)]_EnableRandomDistort("Enable Random Distort",float)=1
		_DistortStrength("Distort Strength",Range(0,0.2))=0.01
		_DistortFrequency("Distort Frequency",Range(1,144))=30
	}
	SubShader
	{	
		Tags { "RenderType"="EnergyShield" "Queue"="Transparent" }
		Pass
		{
			Name "MAIN"
			ZWrite Off
			Cull Back
			Blend One One
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma shader_feature_local _DEPTHOFFSET
			#pragma shader_feature_local _VERTICALSMOOTHEN
			#pragma shader_feature_local _INNERGLOW
			#pragma shader_feature_local _VERTEXRANDOMDISTORT
			#include "Assets/Shaders/Library/Common.hlsl"
			
			TEXTURE2D( _MaskTex); SAMPLER(sampler_MaskTex);
			TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
			INSTANCING_BUFFER_START
			INSTANCING_PROP(float4,_MaskTex_ST)
			INSTANCING_PROP(float4,_InnerColor)
		    INSTANCING_PROP(float4,_RimColor)
		    INSTANCING_PROP(float,_RimWidth)
		    INSTANCING_PROP(float,_EdgeMultiplier)
			
			INSTANCING_PROP(float4,_InnerGlow)
			INSTANCING_PROP(float,_InnerGlowFrequency)
			INSTANCING_PROP(float,_InnerGlowClip)
			INSTANCING_PROP(float,_InnerGlowSpeed)

			INSTANCING_PROP(float,_VerticalSmoothenStart)
			INSTANCING_PROP(float,_VerticalSmoothenDistance)

			INSTANCING_PROP(float,_DepthMultiplier)

			INSTANCING_PROP(float,_DistortStrength)
			INSTANCING_PROP(float,_DistortFrequency)
			INSTANCING_BUFFER_END

			struct a2v
			{
				float3 positionOS : POSITION;
				float3 normalOS:NORMAL;
				float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float3 positionOS:TEXCOORD0;
				float2 uv:TEXCOORD1;
				float3 viewDirWS:TEXCOORD2;
				float3 normalWS:TEXCOORD3;
				#if _DEPTHOFFSET
				float4 screenPos:TEXCOORD4;
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			v2f vert (a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
				#if _VERTEXRANDOMDISTORT
				v.positionOS*=lerp(1-INSTANCE(_DistortStrength),1+INSTANCE(_DistortStrength),random01(v.positionOS+floor(_Time.y*INSTANCE(_DistortFrequency)%INSTANCE(_DistortFrequency))));
				#endif
				o.positionOS=v.positionOS;
				o.positionCS = TransformObjectToHClip(v.positionOS);
				o.uv=TRANSFORM_TEX_INSTANCE(v.uv,_MaskTex);
				o.viewDirWS= TransformObjectToWorld(v.positionOS)-GetCameraPositionWS();
				o.normalWS=TransformObjectToWorldNormal( v.normalOS);
				#if _DEPTHOFFSET
				o.screenPos=ComputeScreenPos(o.positionCS);
				#endif
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				float3 finalCol=0;
				float3 outerCol=INSTANCE(_RimColor).rgb;
				float3 innerCol=INSTANCE(_InnerColor).rgb;
				#if _INNERGLOW
				float glowParam=abs(lerp(-1,1,frac(i.positionOS.y*INSTANCE(_InnerGlowFrequency)+_Time.y*INSTANCE(_InnerGlowSpeed))));
			 	glowParam=smoothstep(INSTANCE(_InnerGlowClip),1,glowParam);
				innerCol=lerp(innerCol,INSTANCE(_InnerGlow).rgb,glowParam);
				#endif

				innerCol*=SAMPLE_TEXTURE2D(_MaskTex,sampler_MaskTex,i.uv).r;

				float outerRim = pow(abs(1+dot(normalize(i.viewDirWS),normalize(i.normalWS))),INSTANCE(_RimWidth))*INSTANCE(_EdgeMultiplier);

				#if _DEPTHOFFSET
				float worldDepthDst=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r-i.screenPos.w;
				float depthOffset=pow(max(0,1-worldDepthDst*PI),INSTANCE(_RimWidth))*INSTANCE(_EdgeMultiplier)*INSTANCE(_DepthMultiplier);
				outerRim=max(outerRim,depthOffset);
				#endif

				outerRim=saturate(outerRim);
				finalCol= lerp(innerCol,outerCol, outerRim);
				
				#if _VERTICALSMOOTHEN
				float verticalParam=abs(i.positionOS.y);
				verticalParam=saturate(invlerp(INSTANCE(_VerticalSmoothenStart),INSTANCE(_VerticalSmoothenStart)+INSTANCE(_VerticalSmoothenDistance),verticalParam));
				finalCol=lerp(finalCol,outerCol,verticalParam);
				#endif
				return float4(finalCol,1);
			}
			ENDHLSL
		}
	}
}
