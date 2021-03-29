
TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
half4 _MainTex_TexelSize;

TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
half4 _CameraDepthTexture_TexelSize;

float2 GetDepthUV(float2 uv)
{
#if UNITY_UV_STARTS_AT_TOP
	if (_MainTex_TexelSize.y < 0)
		uv.y = 1 - uv.y;
#endif
    return uv;
}

float LinearEyeDepth(float2 uv){return LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, uv),_ZBufferParams);}
float Linear01Depth(float2 uv){return Linear01Depth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, uv),_ZBufferParams);}

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

float3 _FrustumCornersRayBL;
float3 _FrustumCornersRayBR;
float3 _FrustumCornersRayTL;
float3 _FrustumCornersRayTR;

float3 GetInterpolatedRay(float2 uv)
{
    bool right = uv.x > .5;
    bool top = uv.y > .5;
    return right ? (top ? _FrustumCornersRayTR : _FrustumCornersRayBR) : (top ? _FrustumCornersRayTL : _FrustumCornersRayBL);
}

float luminance(float3 color){ return 0.299 * color.r + 0.587 * color.g + 0.114 * color.b; }

struct a2v_img
{
    float3 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f_img
{
    float4 positionCS : SV_Position;
    float2 uv : TEXCOORD0;
};

v2f_img vert_img(a2v_img v)
{
    v2f_img o;
    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.uv = v.uv;
    return o;
}