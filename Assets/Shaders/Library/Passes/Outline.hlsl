
struct a2vo
{
	float3 positionOS : POSITION;
	half3 normalOS:NORMAL;
	half4 tangentOS:TANGENT;
	float2 uv:TEXCOORD0;
	half3 uv1:TEXCOORD1;
	half3 uv2:TEXCOORD2;
	half3 uv3:TEXCOORD3;
	half3 uv4:TEXCOORD4;
	half3 uv5:TEXCOORD5;
	half3 uv6:TEXCOORD6;
	half3 uv7:TEXCOORD7;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fo
{
	float4 positionCS:SV_POSITION;
	half3 normalWS:NORMAL;
	float4 color:COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


float4 _gOutlineParameters;
float4 _gOutlineColor;
			
v2fo vertOutline(a2vo v) {
	v2fo o;
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v,o);
				
	float3 positionOS=v.positionOS;
	float3 normalOS;
#if defined(_OUTLINE_TANGENT)
	normalOS=normalize(v.tangentOS.xyz);
#elif defined(_OUTLINE_UV1)
	normalOS=normalize(v.uv1);
#elif defined(_OUTLINE_UV2)
	normalOS=normalize(v.uv2);
#elif defined(_OUTLINE_UV3)
	normalOS=normalize(v.uv3);
#elif defined(_OUTLINE_UV4)
	normalOS=normalize(v.uv4);
#elif defined(_OUTLINE_UV5)
	normalOS=normalize(v.uv5);
#elif defined(_OUTLINE_UV6)
	normalOS=normalize(v.uv6);
#elif defined(_OUTLINE_UV7)
	normalOS=normalize(v.uv7);
#else
	normalOS=normalize(v.normalOS);
#endif

#if defined(_OUTLINE_UV1)||defined(_OUTLINE_UV2)||defined(_OUTLINE_UV3)||defined(_OUTLINE_UV4)||defined(_OUTLINE_UV5)||defined(_OUTLINE_UV6)||defined(_OUTLINE_UV7)
	float3x3 TBNOS=float3x3(v.tangentOS.xyz,cross(v.normalOS,v.tangentOS.xyz)*v.tangentOS.w,v.normalOS);
	normalOS=mul(normalOS,TBNOS);
#endif

	half width = _gOutlineParameters.r;
	o.color = _gOutlineColor;
#if defined(_OUTLINE_MAINTEX)
	o.color *= SAMPLE_TEXTURE2D_LOD(_MainTex,sampler_MainTex,TRANSFORM_TEX_INSTANCE(v.uv,_MainTex),0);
#endif
#if defined(_OUTLINE_DISTANCEFADE)
	float4 positionVS = TransformObjectToView(v.positionOS);
	half fadeParameter = saturate(invlerp(_gOutlineParameters.w,_gOutlineParameters.z,-positionVS.z));
	o.color.a *= fadeParameter;
	width *= fadeParameter;
#endif
	
	float3 worldPos=TransformObjectToWorld(positionOS);
	o.normalWS = normalize(mul((float3x3)unity_ObjectToWorld,normalOS));
	worldPos+=o.normalWS*width;
	o.positionCS= TransformWorldToHClip(worldPos);
	return o;
}

float4 fragOutline(v2fo i) :SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(i);
	return i.color;
}