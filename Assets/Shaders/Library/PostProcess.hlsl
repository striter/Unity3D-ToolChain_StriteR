#include "Common.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
float4 _MainTex_ST;
float4 _MainTex_TexelSize;
#define _MainTex_TexelRight float2(1,0)*_MainTex_TexelSize.xy
#define _MainTex_TexelUp float2(0,1)*_MainTex_TexelSize.xy
#define _MainTex_TexelUpRight float2(1,1)*_MainTex_TexelSize.xy
#define _MainTex_TexelUpLeft float2(-1,1)*_MainTex_TexelSize.xy

float4 SampleMainTex(float2 uv){return  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);}

struct a2v_img
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f_img
{
    float4 positionCS : SV_Position;
    float2 uv : TEXCOORD0;
};

v2f_img vert_blit(a2v_img v)
{
    v2f_img o;
    o.positionCS = v.positionOS;
    o.positionCS.xy = float2(o.positionCS.xy * 2 - 1);
    #if UNITY_UV_STARTS_AT_TOP
        o.positionCS.y = -o.positionCS.y;
    #endif
    o.uv = v.uv;
    return o;
}

v2f_img vert_fullScreenMesh(a2v_img v)
{
    v2f_img o;
    o.positionCS = v.positionOS;
    #if UNITY_UV_STARTS_AT_TOP
        o.positionCS.y = -o.positionCS.y;
    #endif
    o.uv = v.uv;
    return o;
}

v2f_img vert_img_procedural( uint vertexID:SV_VertexID)
{
    v2f_img o;
    o.positionCS = GetFullScreenTriangleVertexPosition(vertexID);
    o.uv = GetFullScreenTriangleTexCoord(vertexID);
    return o;
}
#include "PostProcess/Color.hlsl"
#include "PostProcess/Depth.hlsl"
#include "PostProcess/Normal.hlsl"
#include "PostProcess/MotionVector.hlsl"



