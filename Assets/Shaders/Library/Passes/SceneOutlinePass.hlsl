#include "Assets/Shaders/Library/Additional/Local/AlphaClip.hlsl"
struct a2fs
{
	float3 positionOS:POSITION;
	float3 normalOS:NORMAL;
	half4 tangentOS:TANGENT;
	half4 color:COLOR;
	float2 uv:TEXCOORD0;
#if defined(A2V_ADDITIONAL)
	A2V_ADDITIONAL
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fs
{
	float4 positionCS:SV_POSITION;
	float3 normalWS:NORMAL;
	float2 uv:TEXCOORD0;
	float3 positionWS : TEXCOORD1;
#if defined(V2F_ADDITIONAL)
	V2F_ADDITIONAL
#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fs VertexSceneSelection(a2fs v)
{
	v2fs o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
	o.normalWS=TransformObjectToWorldNormal(v.normalOS);
	float3 positionWS =
	#if defined(GET_POSITION_WS)
		GET_POSITION_WS(v,o);
	#else
			TransformObjectToWorld(v.positionOS);
	#endif
	o.positionWS = positionWS;
	o.positionCS = TransformWorldToHClip(positionWS);
	o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
#if defined(V2F_ADDITIONAL_TRANSFER)
	V2F_ADDITIONAL_TRANSFER(v,o)
#endif
	return o;
}

int _ObjectId;
int _PassValue;
float4 FragmentSceneSelection(v2fs i) :SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);
	half4 albedoAlpha = 
	#if defined(GET_ALBEDO)
		GET_ALBEDO(i);
	#else
			SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*INSTANCE(_Color);
	#endif

	AlphaClip(albedoAlpha.a);
	return float4(_ObjectId, _PassValue, 1.0, 1.0);
}