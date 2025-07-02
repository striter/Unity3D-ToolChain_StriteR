﻿#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
struct a2fd
{
	half3 positionOS:POSITION;
	half3 normalOS:NORMAL;
	half4 tangentOS:TANGENT;
	float4 color :COLOR;
	float2 uv:TEXCOORD0;
	
#if defined(A2V_ADDITIONAL)
	A2V_ADDITIONAL
#endif
	
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fd
{
	float4 positionCS:SV_POSITION;
	half3 normalWS:NORMAL;
	float2 uv:TEXCOORD0;
	float3 positionWS : TEXCOORD1;
	
#if defined(V2F_ADDITIONAL)
	V2F_ADDITIONAL
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fd DepthVertex(a2fd v)
{
	v2fd o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
#if defined(A2V_TRANSFER)
	A2V_TRANSFER(v)
#endif
	o.normalWS=TransformObjectToWorldNormal(v.normalOS);
#if defined(GET_POSITION_WS)
	float3 positionWS= GET_POSITION_WS(v,o);
#else
	float3 positionWS=TransformObjectToWorld(v.positionOS);
#endif

	o.positionWS = positionWS;
	o.positionCS=TransformWorldToHClip(positionWS);
	o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
#if defined(V2F_ADDITIONAL_TRANSFER)
	V2F_ADDITIONAL_TRANSFER(v,o)
#endif

	return o;
}

float4 DepthFragment(v2fd i) :SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);
	half4 albedoAlpha = 
	#if defined(GET_ALBEDO)
		GET_ALBEDO(i);
	#else
		SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*INSTANCE(_Color);
	#endif

	AlphaClip(albedoAlpha.a);
	return i.positionCS.z;
}