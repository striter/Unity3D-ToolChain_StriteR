Shader "Game/Lit/Special/River"
{
    Properties
    {
    	_Color("Color",Color)=(1,1,1,1)
		[ToggleTex(_NORMALTEX)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
    	[Foldout(_NORMALTEX)]_Scale("Scale",Range(1,50))=10
    	
    	[Header(Flow)]
    	[Toggle(_WAVE)]_Wave("Vertex Wave",int)=0
    	[Vector2]_FlowDirection1("Flow Direction 1",Vector)=(1,1,0,0)
    	[Foldout(_WAVE)]_Flow1Amplitude("Flow Amplitidue 1",float)=1
    	[Foldout(_WAVE)]_Spike1Amplitude("Spike Amplitidue 1",float)=1
    	[Foldout(_WAVE)]_Flow1Speed("Speed 1",float)=1
    	[Vector2]_FlowDirection2("Flow Direction 2",Vector)=(1,1,0,0)
    	[Foldout(_WAVE)]_Flow2Amplitude("Flow Amplitidue 2",float)=1
    	[Foldout(_WAVE)]_Spike2Amplitude("Spike Amplitidue 2",float)=1
    	[Foldout(_WAVE)]_Flow2Speed("Speed 2",float)=1
    	
    	[Header(Lighting)]
    	_SpecularAmount("Specular Amount",Range(.8,0.99999))=1
    	_SpecularStrength("Specular Strength",Range(0.5,5))=1
    	
    	[Header(_Fresnel)]
    	[Toggle(_FRESNEL)]_Fresnel("Enable",int)=1
    	
    	[Header(Reflection)]
		_Strength("Strength",Range(0,1))=1
    		
    	[Header(Depth)]
    	[Toggle(_DEPTH)]_Depth("Enable",int)=1
    	[Foldout(_DEPTH)]_DepthRamp("Ramp",2D)="white"{}
    	[Foldout(_DEPTH)]_DepthColor("Color",Color)=(1,1,1,1)
    	[Foldout(_DEPTH)]_DepthBegin("Begin",Range(0,20))=2
    	[Foldout(_DEPTH)]_DepthDistance("Distance",Range(0,40))=2
    	
    	[Header(_Refraction)]
    	[Toggle(_DEPTHREFRACTION)] _DepthRefraction("Enable",int)=1
    	[Foldout(_DEPTHREFRACTION)] _RefractionDistance("Refraction Distance",Range(0.01,5))=1 
    	[Foldout(_DEPTHREFRACTION)]_RefractionAmount("Refraction Amount",Range(0,.5))=0.1
    	
    	[Header(Foam)]
    	[Toggle(_FOAM)]_Foam("Enable",int)=1
    	[Foldout(_FOAM)][HDR]_FoamColor("Color",Color)=(1,1,1,1)
    	[Foldout(_FOAM)]_FoamBegin("Begin",Range(0,1))=.1
    	[Foldout(_FOAM)]_FoamWidth("Width",Range(0.01,.5))=.2
    	[Foldout(_FOAM)]_FoamDistort("Distort",Range(0,1))=1
    	
    	[Header(Caustic)]
    	[Toggle(_CAUSTIC)]_Caustic("Enable",int)=1
    	[Foldout(_CAUSTIC)][NoScaleOffset]_CausticTex("Caustic Tex",2D)="black"{}
    	[Foldout(_CAUSTIC)]_CausticStrength("Caustic Strength",Range(0.1,2))=1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent-1"}
		Blend Off
    	ZTest Less
    	ZWrite On
        Pass
        {
			Tags {"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_instancing
            #pragma shader_feature_local_vertex _WAVE
            #pragma shader_feature_local_fragment _NORMALTEX
			#pragma shader_feature_local_fragment _FOAM
            #pragma shader_feature_local_fragment _DEPTH
            #pragma shader_feature_local_fragment _DEPTHREFRACTION
			#pragma shader_feature_local_fragment _CAUSTIC
            #pragma shader_feature_local_fragment _FRESNEL
            
			#include "Assets/Shaders/Library/Common.hlsl"
            #define IGI
			#include "Assets/Shaders/Library/Lighting.hlsl"
            
			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
            TEXTURE2D(_CausticTex);SAMPLER(sampler_CausticTex);
            TEXTURE2D(_FlowTex);SAMPLER(sampler_FlowTex);
            TEXTURE2D(_DepthRamp);SAMPLER(sampler_DepthRamp);
			TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Color)
				INSTANCING_PROP(float,_Scale)
				INSTANCING_PROP(float2,_FlowDirection1)
				INSTANCING_PROP(float2,_FlowDirection2)
				INSTANCING_PROP(float,_Flow1Amplitude)
				INSTANCING_PROP(float,_Spike1Amplitude)
				INSTANCING_PROP(float,_Flow1Speed)
				INSTANCING_PROP(float,_Flow2Amplitude)
				INSTANCING_PROP(float,_Spike2Amplitude)
				INSTANCING_PROP(float,_Flow2Speed)
				INSTANCING_PROP(float,_SpecularAmount)
				INSTANCING_PROP(float,_SpecularStrength)
				INSTANCING_PROP(float,_RefractionDistance)
				INSTANCING_PROP(float,_RefractionAmount)
	            INSTANCING_PROP(float4,_FoamColor)
				INSTANCING_PROP(float,_FoamBegin)
				INSTANCING_PROP(float,_FoamWidth)
				INSTANCING_PROP(float,_FoamDistort)
	            INSTANCING_PROP(float4,_DepthColor)
				INSTANCING_PROP(float,_DepthBegin)
				INSTANCING_PROP(float,_DepthDistance)
				INSTANCING_PROP(float,_CausticStrength)
				INSTANCING_PROP(float,_Strength)
			INSTANCING_BUFFER_END

            #include "Assets/Shaders/Library/Additional/Local/WaveInteraction.hlsl"
            #pragma multi_compile_local _ _WAVEINTERACTION

            struct a2v
            {
			    float3 positionOS : POSITION;
			    float3 normalOS:NORMAL;
			    float4 tangentOS:TANGENT;
			    float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
			    float4 positionCS : SV_POSITION;
            	float4 positionHCS:TEXCOORD0;
            	float3 positionWS:TEXCOORD1;
				half3 normalWS:TEXCOORD2;
				half3 tangentWS:TEXCOORD3;
				half3 biTangentWS:TEXCOORD4;
				float3 viewDirWS:TEXCOORD5;
            	float2 uv:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			float3 GerstnerWave(float2 uv,float2 flow,float amplitude,float spikeAmplitude,float speed)
			{
				float2 flowUV=uv+_Time.y*flow*speed;
				float2 flowSin=flowUV.x*flow.x+flowUV.y*flow.y;
				float spherical=(flowSin.x*flow.x+flowSin.y*flow.y)*PI;
				float sinFlow;
				float cosFlow;
				sincos(spherical,sinFlow,cosFlow);
				float spike=spikeAmplitude*cosFlow;
				return float3(spike*flow.x, amplitude*sinFlow,spike*flow.y);
			}
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
            	float3 positionWS=TransformObjectToWorld(v.positionOS);
				float3 normalWS=normalize(mul((float3x3)UNITY_MATRIX_M,v.normalOS));
				float3 tangentWS=normalize(mul((float3x3)UNITY_MATRIX_M,v.tangentOS.xyz));
            	#if _WAVE
					positionWS+=GerstnerWave(positionWS.xz,INSTANCE(_FlowDirection1),INSTANCE(_Flow1Amplitude),INSTANCE(_Spike1Amplitude),INSTANCE(_Flow1Speed));
					positionWS+=GerstnerWave(positionWS.xz,INSTANCE(_FlowDirection2),INSTANCE(_Flow2Amplitude),INSTANCE(_Spike2Amplitude),INSTANCE(_Flow2Speed));
				#endif
            	o.positionWS=positionWS;
            	o.positionCS=TransformWorldToHClip(o.positionWS);
            	o.positionHCS=o.positionCS;
				o.normalWS=normalWS;
				o.tangentWS=tangentWS;
				o.biTangentWS=cross(normalWS,tangentWS)*v.tangentOS.w;
				o.viewDirWS=GetViewDirectionWS(o.positionWS);
            	o.uv=v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(i.viewDirWS);
				half3 lightDirWS=normalize(_MainLightPosition.xyz);
            	float3 positionWS=i.positionWS;
            	half3 lightCol=_MainLightColor.rgb;
				half3 normalTS=float3(0,0,1);
            	
            	float fresnel=1;
            	#if _FRESNEL
            		fresnel=1.-Pow4(dot(viewDirWS,normalWS));
				#endif

				float2 wave=WaveInteraction(positionWS);
            	float3 albedo=INSTANCE(_Color).rgb;
            	float2 screenUV=TransformHClipToNDC(i.positionHCS);
            	
            	half2 uvFlow1=INSTANCE(_FlowDirection1)*_Time.y;
				half2 uvFlow2=INSTANCE(_FlowDirection2)*_Time.y;
            	float uvScale=rcp(INSTANCE(_Scale));
            	
            	#if _NORMALTEX
            		float2 uv1=(positionWS.xz+uvFlow1)*uvScale;
            		float2 uv2=(positionWS.xz+uvFlow2)*uvScale;
            		float3 normalTS1=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,uv1));
            		float3 normalTS2=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,uv2));
            		normalTS=BlendNormal(normalTS1,normalTS2,4);
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
            	#endif

            	float underRawDepth=SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
            	float eyeDepthUnder=RawToEyeDepth(underRawDepth);
            	float eyeDepthSurface=RawToEyeDepth(i.positionCS.z);
            	float eyeDepthOffset=eyeDepthUnder-eyeDepthSurface;

            	float2 deepSurfaceUV=screenUV;
				#if _DEPTHREFRACTION
				float refraction=saturate(invlerp(0,INSTANCE(_RefractionDistance),eyeDepthOffset+wave.x))*INSTANCE(_RefractionAmount);
            	deepSurfaceUV+=normalTS.xy*refraction*rcp(eyeDepthUnder);
            	#endif
            	
            	float3 deepSurfaceColor=SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,deepSurfaceUV).rgb;
            	#if _CAUSTIC
					float3 positionWSDepth=TransformNDCToWorld(screenUV,underRawDepth);
            		float verticalDistance=positionWSDepth.y-positionWS.y;
            		float3 causticPositionWS=positionWSDepth+lightDirWS*verticalDistance* rcp(dot(float3(0,-1,0),lightDirWS));
            		float2 causticUV=causticPositionWS.xz;
            		float2 causticForward=(causticUV+uvFlow1)*uvScale;
            		float2 causticBackward=(causticUV-uvFlow2)*uvScale;
            		float caustic=min(SAMPLE_TEXTURE2D(_CausticTex,sampler_CausticTex,causticForward).r,SAMPLE_TEXTURE2D(_CausticTex,sampler_CausticTex,causticBackward).r);
            		deepSurfaceColor+=caustic*lightCol*INSTANCE(_CausticStrength);
            	#endif
            	
				#if _DEPTH
            		float depth=saturate(invlerp(INSTANCE(_DepthBegin),INSTANCE(_DepthBegin)+INSTANCE(_DepthDistance),eyeDepthOffset));
            		float4 depthSample=SAMPLE_TEXTURE2D_LOD(_DepthRamp,sampler_DepthRamp,1-depth,0)*INSTANCE(_DepthColor);
            		float3 depthCol=lerp(deepSurfaceColor,depthSample.rgb,depthSample.a);
					deepSurfaceColor=lerp(deepSurfaceColor,depthCol,depth);
            	#endif
            	
            	float3 aboveSurfaceColor=albedo;
            	float4 reflection=IndirectSpecular(screenUV,eyeDepthSurface,normalTS);
				aboveSurfaceColor=lerp(aboveSurfaceColor, reflection.rgb,reflection.a*INSTANCE(_Strength));
            	
            	float specular=GetSpecular(normalWS,lightDirWS,viewDirWS,INSTANCE(_SpecularAmount));
            	specular*=INSTANCE(_SpecularStrength);
            	aboveSurfaceColor=aboveSurfaceColor+lightCol*specular;
            	
            	float3 riverCol=lerp(deepSurfaceColor,aboveSurfaceColor,fresnel*INSTANCE(_Color).a);
            	
				#if _FOAM
            		float foam=smoothstep(INSTANCE(_FoamBegin)+INSTANCE(_FoamWidth),INSTANCE(_FoamBegin),eyeDepthOffset+max(normalTS.xy)*INSTANCE(_FoamDistort));
            		float3 foamColor=INSTANCE(_FoamColor).rgb;
					riverCol=lerp(riverCol,foamColor,foam*INSTANCE(_FoamColor).a);
            	#endif
            	
            	return float4(riverCol,1);
            }
            ENDHLSL
        }
	}
}
