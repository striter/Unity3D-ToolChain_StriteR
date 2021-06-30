Shader "Game/2D/Uber"
{
    Properties
    {
    	[Header(Albedo)]
        _MainTex ("Texture", 2D) = "white" {}
    	_Color("Color Tint",Color)=(1,1,1,1)
    	[Toggle(_BILLBOARD)]_BillBoard("BillBoard",float)=1
        
    	[Header(Alpha)]
        [Toggle(_ALPHACLIP)]_AlphaClip("Clip",float)=0
        _AlphaClipRange("Range",Range(0.01,1))=0.01
        
    	[Header(Lighting)]
    	[Toggle(_LIGHTING)]_Lighting("Enable",float)=1
    	_Diffuse("Diffuse",Range(0,1))=.5
    	[Toggle(_RECEIVESHADOW)]_ReceiveShadow("ReceiveShadow",float)=1
		[ToggleTex(_BACKRIM)][NoScaleOffset]_BackRimTex("Back Rim",2D)="black"{}
    	[Foldout(_BACKRIM)]_RimIntensity("Rim Intensity",Range(0,10))=0.5
    		
		[Header(Depth)]
		[ToggleTex(_DEPTHMAP)][NoScaleOffset]_DepthTex("Texure",2D)="white"{}
		[Foldout(_DEPTHMAP)]_DepthScale("Scale",Range(0.001,.5))=1
		[Foldout(_DEPTHMAP)]_DepthOffset("Offset",Range(0,1))=.42
		[Toggle(_DEPTHBUFFER)]_DepthBuffer("Affect Buffer",float)=1
		[Toggle(_PARALLEX)]_Parallex("Parallex",float)=0
		[Enum(_16,16,_32,32,_64,64,_128,128)]_ParallexCount("Parallex Count",int)=16
    	
    	[Header(Wave)]
    	[Toggle(_WAVE)]_Wave("Enable",float)=0
		_WaveSpeed("Wind Speed",Range(0,5)) = 1
		_WaveFrequency("Wind Frequency",float)=5
		_WaveStrength("Wind Strength",float)=1
		_WaveDirection("Wind Direction",Vector)=(1,1,1)
    	
		[Header(Misc)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",int)=0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",int)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",int)=1
        [Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("Z Test",int)=2
        [Enum(Off,0,Front,1,Back,2)]_Cull("Cull",int)=1
    }
    SubShader
    {
		Tags { "Queue" = "Geometry" }
		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]
		ZTest [_ZTest]
		Cull [_Cull]

        HLSLINCLUDE
            #include "../CommonInclude.hlsl"
			#include "../CommonLightingInclude.hlsl"
            #pragma multi_compile_instancing

			#pragma shader_feature_local _LIGHTING
			#pragma shader_feature_local _BACKRIM
			#pragma shader_feature_local _BILLBOARD
			#pragma shader_feature_local _RECEIVESHADOW
			#pragma shader_feature_local _WAVE
            #pragma shader_feature_local _ALPHACLIP
			#pragma shader_feature_local _DEPTHMAP
			#pragma shader_feature_local _DEPTHBUFFER
			#pragma shader_feature_local _PARALLEX
        
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
			TEXTURE2D(_DepthTex);SAMPLER(sampler_DepthTex);
			TEXTURE2D(_BackRimTex);SAMPLER(sampler_BackRimTex);
			UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
			INSTANCING_PROP(float,_AlphaClipRange);
			INSTANCING_PROP(float,_Diffuse)
			INSTANCING_PROP(float3,_WaveDirection)
			INSTANCING_PROP(float,_WaveFrequency)
			INSTANCING_PROP(float,_WaveSpeed)
			INSTANCING_PROP(float,_WaveStrength)
			INSTANCING_PROP(float4,_MainTex_ST)
			INSTANCING_PROP(float4, _Color)
			INSTANCING_PROP(float,_DepthScale)
			INSTANCING_PROP(float,_DepthOffset)
			INSTANCING_PROP(int ,_ParallexCount)
			INSTANCING_PROP(float,_RimIntensity)
			UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)
        
            struct a2v
            {
                float3 positionOS : POSITION;
				half3 normalOS:NORMAL;
				half4 tangentOS:TANGENT;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
				half3 normalWS:TEXCOORD1;
				half3 tangentWS:TEXCOORD2;
				half3 biTangentWS:TEXCOORD3;
				float3 viewDirWS:TEXCOORD4;
            	float3 positionWS:TEXCOORD5;
            	half3 positionOS:TEXCOORD6;
            	half3 normalOS:TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			float3 BillBoard(float3 positionWS)
			{
				#if _BILLBOARD
					half3 centerWS=TransformObjectToWorld( 0.h);
					half3 forwardDir=normalize(GetCameraPositionWS()-centerWS);
					half3 upDir=float3(0,1,0);
					half3 rightDir=normalize(cross(upDir,forwardDir));
					half3 centerOffset=positionWS-centerWS;
					return centerWS+rightDir*centerOffset.x+upDir*centerOffset.y+forwardDir*centerOffset.z;
				#else
					return positionWS;
				#endif
			}
        
			float3 Wave(float3 positionWS)
			{
				#if _WAVE
					float wave =INSTANCE(_WaveStrength)*sin(_Time.y*INSTANCE(_WaveSpeed) + (positionWS.x + positionWS.y)*INSTANCE(_WaveFrequency)) / 100;
					return positionWS+INSTANCE(_WaveDirection)*wave;
				#else
					return positionWS;
				#endif
			}
        
            void AlphaClip(half alpha)
            {
                #if _ALPHACLIP
                clip(alpha-INSTANCE(_AlphaClipRange));
                #endif
            }

			void DepthParallex(inout half2 uv,inout float depth,inout float3 positionOS,inout float3 positionWS,float3x3 TBNWS,float3 viewDirWS,float3 normalOS)
			{
				#if _DEPTHMAP
				half depthOffsetOS=0.h;
				uv=ParallexMapping(_DepthTex,sampler_DepthTex, uv,mul(TBNWS, viewDirWS),INSTANCE(_DepthOffset),INSTANCE(_DepthScale),INSTANCE(_ParallexCount),depthOffsetOS);
				#if _DEPTHBUFFER
				positionOS = positionOS-normalize(normalOS)*depthOffsetOS*INSTANCE(_DepthScale);
            	positionWS=TransformObjectToWorld(positionOS);
				half4 dstHClip=TransformObjectToHClip(positionOS);
				depth=dstHClip.z/dstHClip.w;
				#endif
				#endif
			}
        
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				positionWS=BillBoard(positionWS);
				positionWS=Wave(positionWS);
				o.positionCS=TransformWorldToHClip(positionWS);
            	o.positionWS=positionWS;
				o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
				o.viewDirWS=GetCameraPositionWS()-o.positionWS;
            	o.positionOS=v.positionOS;
            	o.normalOS=v.normalOS;
                return o;
            }
        ENDHLSL
        
        Pass
        {
            Tags{"LightMode"="UniversalForward"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            float4 frag (v2f i,inout float depth:DEPTH) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(i.viewDirWS);
				half3 lightDirWS=normalize(_MainLightPosition.xyz);
            	depth=i.positionCS.z;
            	DepthParallex(i.uv,depth,i.positionOS,i.positionWS,TBNWS,viewDirWS,i.normalOS);
                float4 col = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex, i.uv)*_Color;
            	AlphaClip(col.a);
            	#ifndef _LIGHTING
            		return col;
            	#endif
            	half atten=1;
            	#if _RECEIVESHADOW
            		atten=MainLightRealtimeShadow(TransformWorldToShadowCoord(i.positionWS));
            	#endif
            	half diffuse=saturate(dot(normalWS,lightDirWS));
            	diffuse=diffuse*INSTANCE(_Diffuse)+(1.-INSTANCE(_Diffuse));
            	half3 albedo=col.rgb;
            	half3 ambient=_GlossyEnvironmentColor.xyz ;
            	half alpha=col.a;
            	half3 lightCol=_MainLightColor.rgb;
            	half3 diffuseCol=albedo*diffuse;
            	half specular=0;
            	#if _BACKRIM
				half backRim=SAMPLE_TEXTURE2D(_BackRimTex,sampler_BackRimTex,i.uv).r;
            	specular+=step(1,atten)*backRim*INSTANCE(_RimIntensity);
            	#endif
            	
            	half3 finalCol=diffuseCol*atten*lightCol+ambient+specular*lightCol;
			    uint pixelLightCount = GetAdditionalLightsCount();
			    for (uint lightIndex = 0u; lightIndex < pixelLightCount; ++lightIndex)
			    {
			        Light light = GetAdditionalLight(lightIndex,i.positionWS);
			    	finalCol+=albedo* light.distanceAttenuation*light.shadowAttenuation*light.color;
			    }
                return float4(finalCol,alpha);
            }
            ENDHLSL
        }
    	
        Pass
        {
            Tags{"LightMode"="ShadowCaster"}
            HLSLPROGRAM
			#pragma vertex ShadowVertex
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
				
			struct a2fSC
			{
				A2V_SHADOW_CASTER;
            	half2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2fSC
			{
				V2F_SHADOW_CASTER;
				half2 uv:TEXCOORD1;
			};

			v2fSC ShadowVertex(a2fSC v)
			{
				v2fSC o;
				UNITY_SETUP_INSTANCE_ID(v);
				float3 positionWS=TransformObjectToWorld(v.positionOS);
				float3 normalWS=TransformObjectToWorldNormal(v.normalOS);
				positionWS=BillBoard(positionWS);
				positionWS=Wave(positionWS);
				o.positionCS=ShadowCasterCS(positionWS,normalWS);
				o.uv=TRANSFORM_TEX_INSTANCE( v.uv,_MainTex);
				return o;
			}

			float4 ShadowFragment(v2fSC i) :SV_TARGET
			{
				AlphaClip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a*INSTANCE(_Color.a));
				return 0;
			}
            ENDHLSL
        }
    	
		Pass
		{
			NAME "MAIN"
			Tags{"LightMode" = "DepthOnly"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment ShadowFragment
			#pragma multi_compile_instancing
				
			float4 ShadowFragment(v2f i,inout float depth:DEPTH) :SV_TARGET
			{
				half3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(i.viewDirWS);
				depth=i.positionCS.z;
            	DepthParallex(i.uv,depth,i.positionOS,i.positionWS,TBNWS,viewDirWS,i.normalOS);
				AlphaClip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a*INSTANCE(_Color.a));
				return 0;
			}
			ENDHLSL
		}
    }
}
