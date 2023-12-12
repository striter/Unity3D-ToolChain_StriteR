#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"


struct a2vmeta
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 uv0          : TEXCOORD0;
    float2 uv1          : TEXCOORD1;
    float2 uv2          : TEXCOORD2;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2fmeta
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
    #ifdef EDITOR_VISUALIZATION
    float2 VizUV        : TEXCOORD1;
    float4 LightCoord   : TEXCOORD2;
    #endif
};

v2fmeta VertexMeta(a2vmeta input)
{
    v2fmeta output = (v2fmeta)0;
    output.positionCS = UnityMetaVertexPosition(input.positionOS.xyz, input.uv1, input.uv2);
    output.uv = TRANSFORM_TEX(input.uv0, _MainTex);
    #ifdef EDITOR_VISUALIZATION
        UnityEditorVizData(input.positionOS.xyz, input.uv0, input.uv1, input.uv2, output.VizUV, output.LightCoord);
    #endif
    return output;
}

float4 FragmentMeta(v2fmeta i) : SV_Target
{
    #if defined(GET_ALBEDO)
        half3 albedo = GET_ALBEDO(i);
    #else
        half3 albedo = SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv)*INSTANCE(_Color);
    #endif

    #if defined(GET_EMISSION)
        half3 emission = GET_EMISSION(i); 
    #else
        half3 emission = SAMPLE_TEXTURE2D(_EmissionTex,sampler_EmissionTex,i.uv).rgb*INSTANCE(_EmissionColor).rgb;
    #endif

    #ifdef EDITOR_VISUALIZATION
        metaInput.VizUV = fragIn.VizUV;
        metaInput.LightCoord = fragIn.LightCoord;
    #endif
    
    UnityMetaInput IN;

    IN.Albedo = albedo;
    IN.Emission = emission;

    return UnityMetaFragment(IN);
}