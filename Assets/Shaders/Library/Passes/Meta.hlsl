#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"

struct a2vMeta
{
    float4 positionOS:POSITION;
    float2 uv0:TEXCOORD0;
    float2 uv1:TEXCOORD1;
    float2 uv2:TEXCOORD2;
    float4 color:COLOR;
};

struct v2fMeta
{
    float4 positionCS:SV_POSITION;
    float2 uv:TEXCOORD0;
    float4 color:COLOR;
};

v2fMeta VertexMeta(a2vMeta v)
{
    v2fMeta o;
    o.positionCS=MetaVertexPosition(v.positionOS,v.uv1,v.uv2,unity_LightmapST,unity_DynamicLightmapST);
    o.uv=TRANSFORM_TEX(v.uv0,_MainTex);
    o.color=v.color;
    return o;
}

float4 FragmentMeta(v2fMeta i):SV_TARGET
{
#if defined(GET_ALBEDO)
    half3 albedo = GET_ALBEDO(i);
#else
    half3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*INSTANCE(_Color)*i.color.rgb;
#endif

#if defined(GET_EMISSION)
    half3 emission = GET_EMISSION(i); 
#else
    half3 emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,i.uv).rgb*INSTANCE(_EmissionColor).rgb;
#endif

    
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