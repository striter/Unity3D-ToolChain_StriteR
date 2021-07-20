Shader "Game/Lit/Special/River"
{
    Properties
    {
    	_Color("Color",Color)=(1,1,1,1)
		[ToggleTex(_NORMALTEX)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
    	[Foldout(_NORMALTEX)]_Scale("Scale",Range(1,20))=10
    	
    	[Header(Flow)]
		[ToggleTex(_FLOWTEX)][NoScaleOffset]_FlowTex("Flow Tex",2D)="black"{}
    	[Fold(_FLOWTEX)]_FlowDirection("Flow Direction",Vector)=(1,1,0,0)
    	_FlowSpeed("Flow Speed",Range(0.01,10))=2
    	
    	[Header(Lighting)]
    	_SpecularAmount("Specular Amount",Range(.3,1))=1
    	[Toggle(_RECEIVESHADOW)]_ReceiveShadow("Receive Shadow",int)=1
    	
    	[Header(_Refraction)]
    	[Toggle(_DEPTHREFRACTION)] _DepthRefraction("Enable",int)=1
    	[Foldout(_DEPTHREFRACTION)] _RefractionDistance("Refraction Distance",Range(0.01,5))=1 
    	[Foldout(_DEPTHREFRACTION)]_RefractionAmount("Refraction Amount",Range(0,.5))=0.1
    	
    	[Header(Depth)]
    	[Toggle(_DEPTH)]_Depth("Enable",int)=1
    	[Foldout(_DEPTH)]_DepthColor("Color",Color)=(1,1,1,1)
    	[Foldout(_DEPTH)]_DepthBegin("Begin",Range(0,10))=2
    	[Foldout(_DEPTH)]_DepthDistance("Distance",Range(0,10))=2
    	
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
            #pragma shader_feature_local _NORMALTEX
            #pragma shader_feature_local _FLOWTEX
            #pragma shader_feature_local _RECEIVESHADOW
			#pragma shader_feature_local _FOAM
            #pragma shader_feature_local _DEPTH
            #pragma shader_feature_local _DEPTHREFRACTION
			#pragma shader_feature_local _CAUSTIC
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            
			#include "../../CommonInclude.hlsl"
			#include "../../CommonLightingInclude.hlsl"
            #include "../../GlobalIlluminationInclude.hlsl"

			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
            TEXTURE2D(_CausticTex);SAMPLER(sampler_CausticTex);
            TEXTURE2D(_FlowTex);SAMPLER(sampler_FlowTex);
			TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Color)
				INSTANCING_PROP(float,_Scale)
				INSTANCING_PROP(float2,_FlowDirection)
				INSTANCING_PROP(float,_FlowSpeed)
				INSTANCING_PROP(float,_SpecularAmount)
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
			INSTANCING_BUFFER_END

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
            	float4 shadowCoords:TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
			
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
            	o.positionCS=TransformObjectToHClip(v.positionOS);
            	o.positionHCS=o.positionCS;
            	o.positionWS=TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)UNITY_MATRIX_M,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)UNITY_MATRIX_M,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.viewDirWS=TransformWorldToViewDir(o.positionWS,UNITY_MATRIX_V);
            	o.uv=v.uv;
            	o.shadowCoords=TransformWorldToShadowCoord(o.positionWS);
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
            	half3 positionWS=i.positionWS;
            	half3 lightCol=_MainLightColor.rgb;
				half3 normalTS=float3(0,0,1);
            	
            	float atten=1;
            	#if _RECEIVESHADOW
            		atten=MainLightRealtimeShadow(i.shadowCoords);
            	#endif

            	float3 albedo=_Color.rgb;
            	float2 screenUV=TransformHClipToNDC(i.positionHCS);
            	
            	half2 uvFlow;
            	#if _FLOWTEX
            		half2 flowDir=SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,i.uv).xy;		//To Be Continued
					uvFlow=flowDir*frac(_Time.y)*_FlowSpeed;
            	#else
					uvFlow=_FlowDirection*_Time.y*_FlowSpeed;
            	#endif
            	
            	float uvScale=rcp(_Scale);
            	
            	#if _NORMALTEX
				half2 baseUV=positionWS.xz;
            	baseUV+=uvFlow;
            	baseUV*=uvScale;
            	normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,baseUV));
				normalWS=normalize(mul(transpose(TBNWS), normalTS));
            	#endif

            	float underRawDepth=SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
            	float eyeDepthUnder=RawToEyeDepth(underRawDepth);
            	float eyeDepthSurface=RawToEyeDepth(i.positionCS.z);
            	float eyeDepthOffset=eyeDepthUnder-eyeDepthSurface;
            	
            	float2 underSurfaceUV=screenUV;
				#if _DEPTHREFRACTION
				float refraction=saturate(invlerp(0,_RefractionDistance,eyeDepthOffset))*_RefractionAmount;
            	underSurfaceUV+=normalTS.xy*refraction;
            	#endif
            	float3 underSurfaceColor=SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,underSurfaceUV).rgb;
            	
            	#if _CAUSTIC
				float3 positionWSDepth=TransformNDCToWorld(screenUV,underRawDepth);
            	float verticalDistance=positionWSDepth.y-positionWS.y;
            	float3 causticPositionWS=positionWSDepth+lightDirWS*verticalDistance* rcp(dot(float3(0,-1,0),lightDirWS));
            	float causticAtten=1;
            	#if _RECEIVESHADOW
            		causticAtten=MainLightRealtimeShadow(TransformWorldToShadowCoord(causticPositionWS));
				#endif
            	float2 causticUV=causticPositionWS.xz+uvFlow;
				causticUV*=uvScale;
            	float caustic=SAMPLE_TEXTURE2D(_CausticTex,sampler_CausticTex,causticUV);
            	underSurfaceColor+=caustic*lightCol*_CausticStrength*causticAtten;
            	#endif
            	
				#if _DEPTH
            	float depth=smoothstep(_DepthBegin,_DepthBegin+_DepthDistance,eyeDepthOffset)*_DepthColor.a;
            	float3 depthCol=underSurfaceColor*_DepthColor.rgb;
				underSurfaceColor=lerp(underSurfaceColor,depthCol,depth);
            	#endif
            	
            	float3 aboveSurfaceColor=albedo;
            	float4 reflection=IndirectBRDFPlanarSpecular(screenUV,normalTS);
				aboveSurfaceColor=lerp(aboveSurfaceColor, reflection.rgb,reflection.a);
            	
            	float specular=GetSpecular(normalWS,lightDirWS,viewDirWS,_SpecularAmount);
            	specular*=atten;
            	aboveSurfaceColor=lerp(aboveSurfaceColor,lightCol,specular);
            	
				#if _FOAM
            	float foam=smoothstep(_FoamBegin+_FoamWidth,_FoamBegin,eyeDepthOffset+max(normalTS.xy)*_FoamDistort);
            	float3 foamColor=lightCol*_FoamColor.rgb;
				aboveSurfaceColor=lerp(aboveSurfaceColor,foamColor,foam*atten*_FoamColor.a);
            	#endif
            	
				float fresnel=dot(viewDirWS,normalWS);
            	fresnel=1.-Pow4(fresnel);
            	float3 riverCol=lerp(underSurfaceColor,aboveSurfaceColor,fresnel);
            	
            	return float4(riverCol,1);
            }
            ENDHLSL
        }
	}
}
