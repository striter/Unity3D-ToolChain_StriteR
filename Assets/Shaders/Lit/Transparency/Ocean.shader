Shader "Game/Lit/Transparency/Ocean"
{
    Properties
    {
    	_Color("Color",Color)=(1,1,1,1)
    	
		[NoScaleOffset]_NormalTex("Nomral Tex",2D)="white"{}
    	_Scale("Scale",Range(1,50))=10
    	
    	[Header(Flow1)]
    	[Vector2]_Direction1("Direction",Vector)=(1,1,0,0)
    	_Amplitude1("Amplitidue",float)=1
    	_Length1("Length",float)=1
    	_Speed1("Speed",float)=1
    	_Steepness1("Steepness",float)=.5
    	
    	[Header(Lighting)]
    	_SpecularAmount("Specular Amount",Range(.8,0.99999))=1
    	_SpecularStrength("Specular Strength",Range(0.5,5))=1
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
            #pragma shader_feature_local_fragment _NORMALTEX
            
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
				INSTANCING_PROP(float2,_Direction1)
				INSTANCING_PROP(float,_Amplitude1)
				INSTANCING_PROP(float,_Length1)
				INSTANCING_PROP(float,_Speed1)
				INSTANCING_PROP(float,_Steepness1)
				INSTANCING_PROP(float,_SpecularAmount)
				INSTANCING_PROP(float,_SpecularStrength)
				INSTANCING_PROP(float,_Strength)
			INSTANCING_BUFFER_END

            struct a2v
            {
			    float3 positionOS : POSITION;
			    float3 normalOS:NORMAL;
			    float4 tangentOS:TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
			    float4 positionCS : SV_POSITION;
            	float3 positionWS:TEXCOORD1;
				half3 normalWS:TEXCOORD2;
				half3 tangentWS:TEXCOORD3;
				half3 biTangentWS:TEXCOORD4;
				float3 viewDirWS:TEXCOORD5;
            	float2 uv:TEXCOORD6;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

			float3 GerstnerWave(float2 uv,float2 flow,float amplitude,float length,float speed,float steepness,out float3 normalWS,out float3 tangentWS,out float3 biNormalWS)
			{
				flow=normalize(flow);

				float frequency=2*PI/length;
				float a=steepness/frequency;

				float f=frequency*(uv.x*flow.x+uv.y*flow.y+speed*_Time.y);
				float sinFlow,cosFlow;
				sincos(f,sinFlow,cosFlow);

				float s=a*frequency;
				tangentWS=float3(
					1-flow.x*flow.x*s*sinFlow,
					flow.x*s*cosFlow,
					-flow.x*flow.y*s*sinFlow
				);

				biNormalWS=float3(
					-flow.x*flow.y*s*sinFlow,
					flow.y*s*cosFlow,
					1-flow.y*flow.y*s*sinFlow
				);

				normalWS=normalize(cross(biNormalWS,tangentWS));

				return float3(a*flow.x*cosFlow, amplitude*sinFlow,a*flow.y*cosFlow);
			}
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v,o);
            	float3 positionWS=TransformObjectToWorld(v.positionOS);
				float3 normalWS;
				float3 tangentWS;
				float3 biTangentWS;
				positionWS+=GerstnerWave(positionWS.xz,INSTANCE(_Direction1),INSTANCE(_Amplitude1),INSTANCE(_Length1),INSTANCE(_Speed1),INSTANCE(_Steepness1),normalWS,tangentWS,biTangentWS);
            	o.positionWS=positionWS;
            	o.positionCS=TransformWorldToHClip(o.positionWS);
				o.normalWS=normalWS;
				o.tangentWS=tangentWS;
				o.biTangentWS=biTangentWS;
				o.viewDirWS=GetViewDirectionWS(o.positionWS);
            	o.uv=positionWS.xz*rcp(INSTANCE(_Scale));
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
            	float3 normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv));
				normalWS=normalize(mul(transpose(TBNWS), normalTS));
            	
            	float3 albedo=INSTANCE(_Color).rgb;

            	float3 diffuse=SAMPLE_SH(normalWS);
            	float specular=GetSpecular(normalWS,lightDirWS,viewDirWS,INSTANCE(_SpecularAmount));
            	specular*=INSTANCE(_SpecularStrength);
            	float3 riverCol = diffuse*albedo+ albedo*lightCol*specular;
            	return float4(riverCol,1);
            }
            ENDHLSL
        }
	}
}
