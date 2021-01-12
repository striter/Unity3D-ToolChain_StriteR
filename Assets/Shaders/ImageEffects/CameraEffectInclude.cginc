sampler2D _MainTex;
half4 _MainTex_TexelSize;
sampler2D _CameraDepthTexture;

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

float random(float2 st)
{
    return frac(sin(dot(st.xy,float2(12.9898,78.233)))*43758.543123);
}