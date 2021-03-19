sampler2D _MainTex;
half4 _MainTex_TexelSize;
float4 _FrustumCornersRayBL;
float4 _FrustumCornersRayBR;
float4 _FrustumCornersRayTL;
float4 _FrustumCornersRayTR;

float4 GetInterpolatedRay(float2 uv)
{
	bool right  =uv.x > .5;
	bool top = uv.y > .5;
	return right ? (top ? _FrustumCornersRayTR : _FrustumCornersRayBR) : (top ? _FrustumCornersRayTL : _FrustumCornersRayBL);
}

float2 GetDepthUV(float2 uv)
{
#if UNITY_UV_STARTS_AT_TOP
	if (_MainTex_TexelSize.y < 0)
		uv.y = 1 - uv.y;
#endif
	return uv;
}

sampler2D _CameraDepthTexture;

float LinearEyeDepth(float2 uv){return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));}

float3 ClipSpaceNormalFromDepth(float2 uv)
{
    float2 offset1 = float2(0, 1) * _MainTex_TexelSize.xy;
    float2 offset2 = float2(1, 0) * _MainTex_TexelSize.xy;

    float depth = LinearEyeDepth(uv);
    float depth1 = LinearEyeDepth(uv + offset1);
    float depth2 = LinearEyeDepth(uv + offset2);
				
    float3 p1 = float3(offset1, depth1 - depth);
    float3 p2 = float3(offset2, depth2 - depth);
    return normalize(cross(p1, p2));
}