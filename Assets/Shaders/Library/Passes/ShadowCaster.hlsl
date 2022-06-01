
struct a2fs
{
	A2V_SHADOW_CASTER;
	#if defined(A2V_SHADOW_DEPTH)
		A2V_SHADOW_DEPTH
	#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fs
{
	V2F_SHADOW_CASTER;
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

#if defined(GET_POSITION_WS)
	float3 positionWS= GET_POSITION_WS(v)
#else
	float3 positionWS=TransformObjectToWorld(v.positionOS);
#endif 
	
	SHADOW_CASTER_VERTEX(v,positionWS);
	#if defined(TRANSFER_SHADOW_DEPTH)
		TRANSFER_SHADOW_DEPTH(v,o)
	#endif
	return o;
}

float4 ShadowFragment(v2fs i) :SV_TARGET
{
	#if defined(MIX_SHADOW_DEPTH)
		MIX_SHADOW_DEPTH(i)
	#endif
	return 0;
}