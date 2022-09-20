
struct a2fs
{
	float3 positionOS:POSITION;
	float3 normalOS:NORMAL;
	half4 color:COLOR;
	
	#if defined(A2V_SHADOW_DEPTH)
		A2V_SHADOW_DEPTH
	#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fs
{
	float4 positionCS:SV_POSITION;
	float3 normalWS:NORMAL;
	#if defined(V2F_SHADOW_DEPTH)
		V2F_SHADOW_DEPTH
	#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fs ShadowVertex(a2fs v)
{
	v2fs o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
	o.normalWS=TransformObjectToWorldNormal(v.normalOS);
#if defined(GET_POSITION_WS)
	float3 positionWS= GET_POSITION_WS(v,o)
#else
	float3 positionWS=TransformObjectToWorld(v.positionOS);
#endif 
	o.positionCS= ShadowCasterCS(positionWS,o.normalWS);
	
	#if defined(VERTEX_SHADOW_DEPTH)
		VERTEX_SHADOW_DEPTH(v,o)
	#endif
	return o;
}

float4 ShadowFragment(v2fs i) :SV_TARGET
{
	#if defined(FRAGMENT_SHADOW_DEPTH)
		FRAGMENT_SHADOW_DEPTH(i)
	#endif
	return 0;
}