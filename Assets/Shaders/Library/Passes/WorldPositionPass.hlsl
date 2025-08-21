﻿#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
struct a2fs
{
	float3 positionOS:POSITION;
	half4 color:COLOR;
	float2 uv:TEXCOORD0;
	float3 normalOS:NORMAL;
	float4 tangentOS:TANGENT;
	#if defined(A2V_ADDITIONAL)
	A2V_ADDITIONAL
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fs
{
	float4 positionCS:SV_POSITION;
	float2 uv:TEXCOORD0;
	float3 positionWS : TEXCOORD1;
	half3 tangentWS:TEXCOORD3;
	float3 normalWS:NORMAL;
	half3 biTangentWS:TEXCOORD4;
	#if defined(V2F_ADDITIONAL)
	V2F_ADDITIONAL
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fs vert(a2fs v)
{
	v2fs o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
	#if defined(A2V_TRANSFER)
	A2V_TRANSFER(v)
#endif
	float3 positionWS =
#if defined(GET_POSITION_WS)
	GET_POSITION_WS(v,o);
	#else
		TransformObjectToWorld(v.positionOS);
	#endif
	o.positionCS = TransformWorldToHClip(positionWS);
	o.positionWS = positionWS;
	o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
	o.normalWS = TransformObjectToWorldNormal(v.normalOS);
	o.tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
	o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
	#if defined(V2F_ADDITIONAL_TRANSFER)
	V2F_ADDITIONAL_TRANSFER(v,o)
#endif
	return o;
}

float4 frag(v2fs i) :SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);
	half4 albedoAlpha = 
	#if defined(GET_ALBEDO)
		GET_ALBEDO(i);
	#else
			SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*INSTANCE(_Color);
	#endif
	AlphaClip(albedoAlpha.a);
	return float4(i.positionWS,1);
}