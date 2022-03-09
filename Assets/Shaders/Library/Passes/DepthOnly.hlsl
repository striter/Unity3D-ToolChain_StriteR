
struct a2fd
{
	float3 positionOS:POSITION;
	A2V_SHADOW_DEPTH
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fd
{
	float4 positionCS:SV_POSITION;
	V2F_SHADOW_DEPTH
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fd DepthVertex(a2fd v)
{
	v2fd o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
	o.positionCS=TransformWorldToHClip(TRANSFER_POSITION_WS(v));
	TRANSFER_SHADOW_DEPTH(v,o)
	return o;
}

float4 DepthFragment(v2fd i) :SV_TARGET
{
	MIX_SHADOW_DEPTH(i)
	return i.positionCS.z;
}