#include "Common.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
half4 _MainTex_TexelSize;
#define _MainTex_TexelRight half2(1.h,0.h)*_MainTex_TexelSize.xy
#define _MainTex_TexelUp half2(0.h,1.h)*_MainTex_TexelSize.xy
#define _MainTex_TexelUpRight half2(1.h,1.h)*_MainTex_TexelSize.xy
#define _MainTex_TexelUpLeft half2(-1.h,1.h)*_MainTex_TexelSize.xy

float4 SampleMainTex(float2 uv){return  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv);}

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
#ifdef ICOLOR
#include "PostProcess/Color.hlsl"
#endif

#ifdef IDEPTH
#include "PostProcess/Depth.hlsl"
#endif