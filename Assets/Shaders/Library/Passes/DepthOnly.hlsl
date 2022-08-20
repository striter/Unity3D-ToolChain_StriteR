
struct a2fd
{
	half3 positionOS:POSITION;
	half3 normalOS:NORMAL;
	
	#if defined(A2V_SHADOW_DEPTH)
		A2V_SHADOW_DEPTH
	#else
		float2 uv:TEXCOORD0;
	#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fd
{
	float4 positionCS:SV_POSITION;
	half3 normalWS:NORMAL;

	#if defined(V2F_SHADOW_DEPTH)
		V2F_SHADOW_DEPTH
	#else
		float2 uv:TEXCOORD0;
	#endif
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2fd DepthVertex(a2fd v)
{
	v2fd o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
	o.normalWS=TransformObjectToWorldNormal(v.normalOS);
	#if defined(GET_POSITION_WS)
		float3 positionWS= GET_POSITION_WS(v,o)
	#else
		float3 positionWS=TransformObjectToWorld(v.positionOS);
	#endif 
	o.positionCS=TransformWorldToHClip(positionWS);
	#if defined(VERTEX_SHADOW_DEPTH)
		VERTEX_SHADOW_DEPTH(v,o)
	#else
		o.uv=TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
	#endif
	
	return o;
}

float4 DepthFragment(v2fd i) :SV_TARGET
{
	#if defined(FRAGMENT_SHADOW_DEPTH)
		FRAGMENT_SHADOW_DEPTH(i)
	#endif
	return i.positionCS.z;
}