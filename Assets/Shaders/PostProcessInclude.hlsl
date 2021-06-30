#include "CommonInclude.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
half4 _MainTex_TexelSize;
#define _MainTex_TexelRight half2(1.h,0.h)*_MainTex_TexelSize.xy
#define _MainTex_TexelUp half2(0.h,1.h)*_MainTex_TexelSize.xy

TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
half4 _CameraDepthTexture_TexelSize;

float4 Sample_MainTex(half2 uv){return  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);}
float Sample_Depth(half2 uv){return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,uv).r;}
float LinearEyeDepth(half2 uv){return LinearEyeDepth(Sample_Depth(uv),_ZBufferParams);}
float Linear01Depth(half2 uv){return Linear01Depth(Sample_Depth(uv),_ZBufferParams);}

float3 GetPositionWS(half2 uv,half depth){return GetPositionWS_Frustum(uv,depth);}
float3 GetPositionWS(half2 uv){return GetPositionWS(uv,Sample_Depth(uv));}

float3 WorldSpaceNormalFromDepth(half2 uv,inout float3 positionWS,inout half depth)
{
    depth=Sample_Depth(uv);
    positionWS=GetPositionWS(uv,depth);
    float3 position1=GetPositionWS(uv+_MainTex_TexelRight);
    float3 position2=GetPositionWS(uv+_MainTex_TexelUp);
    return normalize(cross(position2-positionWS,position1-positionWS));
}
half3 ClipSpaceNormalFromDepth(half2 uv)
{
    half depth = LinearEyeDepth(uv);
    half depth1 = LinearEyeDepth(uv + _MainTex_TexelRight);
    half depth2 = LinearEyeDepth(uv + _MainTex_TexelUp);
				
    half3 p1 = half3(_MainTex_TexelRight, depth1 - depth);
    half3 p2 = half3(_MainTex_TexelUp, depth2 - depth);
    return normalize(cross(p1, p2));
}


half luminance(half3 color){ return 0.299h * color.r + 0.587h * color.g + 0.114h * color.b; }

struct a2v_img
{
    half3 positionOS : POSITION;
    half2 uv : TEXCOORD0;
};

struct v2f_img
{
    float4 positionCS : SV_Position;
    half2 uv : TEXCOORD0;
};

v2f_img vert_img(a2v_img v)
{
    v2f_img o;
    o.positionCS = TransformObjectToHClip(v.positionOS);
    o.uv = v.uv;
    return o;
}