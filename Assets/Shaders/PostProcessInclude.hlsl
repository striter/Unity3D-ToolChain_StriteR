#include "CommonInclude.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
half4 _MainTex_TexelSize;
#define _MainTex_TexelRight half2(1.h,0.h)*_MainTex_TexelSize.xy
#define _MainTex_TexelUp half2(0.h,1.h)*_MainTex_TexelSize.xy

TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
half4 _CameraDepthTexture_TexelSize;

float4 SampleMainTex(float2 uv){return  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);}
float SampleRawDepth(float2 uv){return SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,uv).r;}
float SampleEyeDepth(float2 uv){return RawToEyeDepth(SampleRawDepth(uv));}
float Sample01Depth(float2 uv){return RawTo01Depth(SampleRawDepth(uv));}
float3 TransformNDCToWorld(float2 uv){return TransformNDCToWorld(uv,SampleRawDepth(uv));}

float3 WorldSpaceNormalFromDepth(float2 uv,inout float3 positionWS,inout half depth)
{
    depth=SampleRawDepth(uv);
    positionWS=TransformNDCToWorld(uv,depth);
    float3 position1=TransformNDCToWorld(uv+_MainTex_TexelRight);
    float3 position2=TransformNDCToWorld(uv+_MainTex_TexelUp);
    return normalize(cross(position2-positionWS,position1-positionWS));
}
half3 ClipSpaceNormalFromDepth(float2 uv)
{
    half depth = SampleEyeDepth(uv);
    half depth1 = SampleEyeDepth(uv + _MainTex_TexelRight);
    half depth2 = SampleEyeDepth(uv + _MainTex_TexelUp);
				
    half3 p1 = half3(_MainTex_TexelRight, depth1 - depth);
    half3 p2 = half3(_MainTex_TexelUp, depth2 - depth);
    return normalize(cross(p1, p2));
}

half luminance(half3 color){ return 0.299h * color.r + 0.587h * color.g + 0.114h * color.b; }

struct a2v_img
{
    half3 positionOS : POSITION;
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