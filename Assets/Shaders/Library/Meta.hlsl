#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

struct a2vMeta
{
    float4 positionOS:POSITION;
    float3 normalOS:NORMAL;
    float2 uv0:TEXCOORD0;
    float2 uv1:TEXCOORD1;
    float2 uv2:TEXCOORD2;
};

struct v2fMeta
{
    float4 positionCS:SV_POSITION;
    float2 uv:TEXCOORD0;
};

v2fMeta MetaVertex(a2vMeta v)
{
    v2fMeta o;
    o.positionCS=MetaVertexPosition(v.positionOS,v.uv1,v.uv2,unity_LightmapST,unity_DynamicLightmapST);
    o.uv=TRANSFORM_TEX(v.uv0,_MainTex);
    return o;
}

float4 MetaFragment(v2fMeta i):SV_TARGET
{
    half3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb;
    half3 emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,i.uv).rgb*INSTANCE(_EmissionColor).rgb;
    half4 res = 0;
    if (unity_MetaFragmentControl.x)
    {
        res = half4(albedo, 1.0);
        res.rgb = clamp(PositivePow(res.rgb, saturate(unity_OneOverOutputBoost)), 0, unity_MaxOutputValue);
    }
    if (unity_MetaFragmentControl.y)
    {
        if (!unity_UseLinearSpace)
            emission = LinearToSRGB(emission);

        res = half4(emission, 1.0);
    }
    return res;
}