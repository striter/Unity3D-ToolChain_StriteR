Shader "PCGOcean"
{
    Properties
    {
    	_Color("Color",Color)=(1,1,1,1)
    	_Radius("Radius",Range(0,100)) = 51.8
		[ToggleTex(_NORMALTEX)][NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
    	_NormalStrength("Normal Strength",Range(0,1))=1
    	
    	[Header(Flow)]
    	_FlowST1("Flow ST 1",Vector)=(1,1,1,1)
    	_FlowST2("Flow ST 2",Vector)=(1,1,1,1)
    	
    	[Toggle(_WAVE)]_Wave("Vertex Wave",int)=0
    	[Header(Wave 1)]
    	[Foldout(_WAVE)]_WaveST1("Wave ST 1",Vector)=(1,1,1,1)
    	[Foldout(_WAVE)]_WaveAmplitude1("Flow Amplitidue 1",float)=1
    	[Foldout(_WAVE)]_WaveST2("Wave ST 2",Vector)=(1,1,1,1)
    	[Foldout(_WAVE)]_WaveAmplitude2("Amplitidue 2",float)=1
    	
    	[Header(Lighting)]
    	_SpecularAmount("Specular Amount",Range(0.01,40))=1
    	_SpecularStrength("Specular Strength",Range(0,10))=1
    	
    	[Header(_Reflection)]
    	_ReflectionColor("Color",Color)=(1,1,1,1)
    	_ReflectionOffset("Indirect Specular Offset",Range(-7,7)) = 8
    	_ReflectionDistort("Distort",Range(0,1))=0.1	
    	
    	[Header(Depth)]
    	[Toggle(_DEPTH)]_Depth("Enable",int)=1 
    	[Foldout(_DEPTH)]_DepthColor("Color",Color)=(1,1,1,1)
		[MinMaxRange]_DepthRange("Range ",Range(0.01,20))=0
    	[HideInInspector]_DepthRangeEnd("",float)=0.1
    	
    	[Header(Fresnel)]
    	[Toggle(_FRESNEL)]_Fresnel("Enable",int)=1
		[MinMaxRange]_FresnelRange("Range ",Range(0,1))=0.1
    	[HideInInspector]_FresnelRangeEnd("",float)=0.2
    	
    	[Header(_Refraction)]
    	[Toggle(_DEPTHREFRACTION)] _DepthRefraction("Enable",int)=1
    	[Foldout(_DEPTHREFRACTION)] _RefractionDistance("Refraction Distance",Range(0.01,5))=1 
    	[Foldout(_DEPTHREFRACTION)]_RefractionAmount("Refraction Amount",Range(0,2))=0.1
    	
    	[Header(Foam)]
    	[Toggle(_FOAM)]_Foam("Enable",int)=1
    	[Foldout(_FOAM)][HDR]_FoamColor("Color",Color)=(1,1,1,1)
		[MinMaxRange]_FoamRange("Depth Range ",Range(0,1))=0
    	[HideInInspector]_FoamRangeEnd("",float)=0.1
    	[MinMaxRange]_FoamNormalRange("Normal Range ",Range(0,1))=0.9
    	[HideInInspector]_FoamNormalRangeEnd("",float)=1
    	[Foldout(_FOAM)]_FoamDistort("Distort",Range(0.01,1))=1
    	
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
            #pragma multi_compile_fog
			#pragma multi_compile_fragment _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #pragma shader_feature_local_vertex _WAVE
            #pragma shader_feature_local_fragment _NORMALTEX
			#pragma shader_feature_local_fragment _FOAM
            #pragma shader_feature_local_fragment _DEPTH
            #pragma shader_feature_local_fragment _DEPTHREFRACTION
            #pragma shader_feature_local_fragment _FRESNEL
            
			#include "Assets/Shaders/Library/Common.hlsl"
			#include "Assets/Shaders/Library/Lighting.hlsl"
			#include "Assets/Shaders/Library/Geometry.hlsl"

			TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
            TEXTURE2D(_FlowTex);SAMPLER(sampler_FlowTex);
			TEXTURE2D(_CameraOpaqueTexture);SAMPLER(sampler_CameraOpaqueTexture);
            TEXTURE2D(_CameraDepthTexture);SAMPLER(sampler_CameraDepthTexture);
			INSTANCING_BUFFER_START
				INSTANCING_PROP(float4,_Color)
				INSTANCING_PROP(float,_Radius)
				INSTANCING_PROP(float,_NormalStrength)
				INSTANCING_PROP(float4,_FlowST1)
				INSTANCING_PROP(float4,_FlowST2)

				INSTANCING_PROP(float4,_WaveST1)
				INSTANCING_PROP(float,_WaveAmplitude1)
            	INSTANCING_PROP(float4,_WaveST2)
				INSTANCING_PROP(float,_WaveAmplitude2)
            
				INSTANCING_PROP(float,_SpecularAmount)
				INSTANCING_PROP(float,_SpecularStrength)
            
				INSTANCING_PROP(float,_RefractionDistance)
				INSTANCING_PROP(float,_RefractionAmount)
            
				INSTANCING_PROP(float4,_ReflectionColor)
				INSTANCING_PROP(int,_ReflectionOffset)
				INSTANCING_PROP(float,_ReflectionDistort)
            
	            INSTANCING_PROP(float4,_FoamColor)
				INSTANCING_PROP(float,_FoamRange)
				INSTANCING_PROP(float,_FoamRangeEnd)
				INSTANCING_PROP(float,_FoamNormalRange)
				INSTANCING_PROP(float,_FoamNormalRangeEnd)
            
				INSTANCING_PROP(float,_FoamDistort)
	            INSTANCING_PROP(float4,_DepthColor)
				INSTANCING_PROP(float,_DepthRange)
				INSTANCING_PROP(float,_DepthRangeEnd)

				INSTANCING_PROP(float,_FresnelRange)
				INSTANCING_PROP(float,_FresnelRangeEnd)
            
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
            	float3 cameraDirWS:TEXCOORD6;
            	float2 uv:TEXCOORD7;
            	V2F_FOG(8)
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			float3 GerstnerWave(float2 uv,float4 waveST,float amplitude,float3 normal)
			{
				float2 flowUV=uv+_Time.y*waveST.xy*waveST.zw;
				float flowSin=flowUV.x*waveST.x+flowUV.y*waveST.y;
				float sinFlow = sin(flowSin*PI);
				return normal*sinFlow*amplitude;
			}
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
	          	float3 positionWSNormalized = normalize(TransformObjectToWorld(v.positionOS));
            	float3 positionWS= positionWSNormalized * INSTANCE(_Radius);
				float3 normalWS= positionWSNormalized;
				float3 tangentWS=normalize(mul((float3x3)UNITY_MATRIX_M,v.tangentOS.xyz));
				positionWS += GerstnerWave(v.uv,INSTANCE(_WaveST1),INSTANCE(_WaveAmplitude1),normalWS);
				positionWS += GerstnerWave(v.uv,INSTANCE(_WaveST2),INSTANCE(_WaveAmplitude2),normalWS);
            	o.positionWS=positionWS;
            	o.positionCS=TransformWorldToHClip(o.positionWS);
            	o.positionHCS=o.positionCS;
				o.normalWS=normalWS;
				o.tangentWS=tangentWS;
				o.biTangentWS=cross(normalWS,tangentWS)*v.tangentOS.w;
				o.viewDirWS=GetViewDirectionWS(o.positionWS);
				o.cameraDirWS= GetCameraRealDirectionWS (o.positionWS);
            	o.uv=v.uv;
				FOG_TRANSFER(o);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				half3 normalWS=normalize(i.normalWS);
            	half3 srcNormalWS=normalWS;
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
				half3 viewDirWS=normalize(i.viewDirWS);
				half3 cameraDirWS=normalize(i.cameraDirWS);
            	float3 positionWS=i.positionWS;
				half3 normalTS=float3(0,0,1);
            	
            	float fresnel=1;
            	#if _FRESNEL
            		fresnel = 1-saturate(invlerp(_FresnelRange,_FresnelRangeEnd,dot(viewDirWS,normalWS)));
				#endif

            	float3 albedo=INSTANCE(_Color).rgb;
            	float2 screenUV=TransformHClipToNDC(i.positionHCS);
            	float2 uv = i.uv;
            	float2 uv1= TransformTex_Flow(uv,INSTANCE(_FlowST1));
            	float2 uv2=TransformTex_Flow(uv,INSTANCE(_FlowST2));
				float normalDelta = 1;
            	#if _NORMALTEX
            		float3 normalTS1 = DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,uv1));
            		float3 normalTS2 = DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,uv2));
            		normalTS = BlendNormal(normalTS1,normalTS2,4);
            		normalDelta = dot(normalTS,float3(0,0,1));
            		normalDelta = lerp(1,normalDelta,INSTANCE(_NormalStrength));
					normalWS = lerp(normalWS,normalize(mul(transpose(TBNWS), normalTS)),INSTANCE(_NormalStrength));
            	#endif

            	float3 reflectDirWS = normalize(reflect(cameraDirWS,lerp(srcNormalWS,normalWS,INSTANCE(_ReflectionDistort))));
            	float3 lightPositionWS = i.positionWS+float3(normalWS.x,0,normalWS.z)*INSTANCE(_RefractionAmount);
            	Light light = GetMainLight(TransformWorldToShadowCoord(lightPositionWS),lightPositionWS,unity_ProbesOcclusion);
				half3 lightDirWS=normalize(light.direction);
            	half3 lightCol=light.color;
            	
            	float underRawDepth=SAMPLE_TEXTURE2D(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
            	float eyeDepthUnder=RawToEyeDepth(underRawDepth);
            	float eyeDepthSurface=RawToEyeDepth(i.positionCS.z);
            	float eyeDepthOffset=eyeDepthUnder-eyeDepthSurface;

            	float2 deepSurfaceUV=screenUV;
				#if _DEPTHREFRACTION
					float refraction=saturate(invlerp(0,INSTANCE(_RefractionDistance),eyeDepthOffset+wave.x))*INSTANCE(_RefractionAmount);
            		deepSurfaceUV+=normalTS.xy*refraction*rcp(eyeDepthUnder);
            	#endif
            	
            	half3 indirectDiffuse = IndirectDiffuse_SH(normalWS);
            	half3 indirectSpecular = IndirectCubeSpecular(reflectDirWS,1,INSTANCE(_ReflectionOffset));
            	
            	float3 deepSurfaceColor=SAMPLE_TEXTURE2D(_CameraOpaqueTexture,sampler_CameraOpaqueTexture,deepSurfaceUV).rgb*indirectDiffuse;
            	
				#if _DEPTH
            		float depth=saturate(invlerp(INSTANCE(_DepthRange),INSTANCE(_DepthRangeEnd),eyeDepthOffset));
            		float4 depthSample=INSTANCE(_DepthColor);
            		float3 depthCol=lerp(deepSurfaceColor,depthSample.rgb,depthSample.a)*indirectDiffuse;
					deepSurfaceColor=lerp(deepSurfaceColor,depthCol,depth);
            	#endif
            	
            	float3 aboveSurfaceColor=albedo*indirectDiffuse+indirectSpecular;
            	
            	float4 reflectionSample = IndirectSSRSpecular(screenUV,eyeDepthSurface,normalTS);
            	float4 reflectionColor =  INSTANCE(_ReflectionColor);

            	float reflectionAmount = max(step(0.01,reflectionSample.r),(1-light.shadowAttenuation));
				aboveSurfaceColor = lerp(aboveSurfaceColor,aboveSurfaceColor * reflectionColor.rgb,reflectionAmount*_ReflectionColor.a);
            	
            	float specular=pow(max(0,dot(normalWS,normalize(lightDirWS+viewDirWS))),INSTANCE(_SpecularAmount)*40);
            	specular*=INSTANCE(_SpecularStrength);
            	aboveSurfaceColor=aboveSurfaceColor+lightCol*specular;
            	
            	float3 riverCol=lerp(deepSurfaceColor,aboveSurfaceColor,fresnel*INSTANCE(_Color).a*normalDelta);
            	
				#if _FOAM
            		float depthFoam= invlerp(INSTANCE(_FoamRangeEnd),INSTANCE(_FoamRange),eyeDepthOffset+max(normalTS.xy)*INSTANCE(_FoamDistort))*step(0,eyeDepthOffset);
            		float normalFoam = invlerp(INSTANCE(_FoamNormalRangeEnd),INSTANCE(_FoamNormalRange),normalDelta);
            		float foamParameter = saturate(max(depthFoam,normalFoam));
            		float3 foamColor=indirectDiffuse * INSTANCE(_FoamColor).rgb;
					riverCol=lerp(riverCol,foamColor,foamParameter*INSTANCE(_FoamColor).a);
            	#endif

            	FOG_MIX(i,riverCol);
            	return float4(riverCol,1);
            }
            ENDHLSL
        }
	}
}
