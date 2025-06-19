#ifndef _BRDFINPUT
#define _BRDFINPUT


#define DIELETRIC_SPEC half4(0.04h,0.04h,0.04h,0.96h)

struct a2vf
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float3 positionOS : POSITION;
    float3 normalOS:NORMAL;
    float4 tangentOS:TANGENT;
    float4 color:COLOR;
    float2 uv:TEXCOORD0;
    A2V_LIGHTMAP
    #if defined(A2V_ADDITIONAL)
        A2V_ADDITIONAL
    #endif
};

struct v2ff
{
    UNITY_VERTEX_INPUT_INSTANCE_ID
    float4 positionCS : SV_POSITION;
    float4 color:COLOR;
    float3 normalWS:NORMAL;
    float2 uv:TEXCOORD0;
    float3 positionWS:TEXCOORD1;
    float4 positionHCS:TEXCOORD2;
    half3 tangentWS:TEXCOORD3;
    half3 biTangentWS:TEXCOORD4;
    half3 viewDirWS:TEXCOORD5;
    V2F_FOG(6)
    V2F_LIGHTMAP(7)
    #if defined(V2F_ADDITIONAL)
        V2F_ADDITIONAL
    #endif
};

struct f2of
{
    float4 result:SV_TARGET;

    #if defined(F2O_ADDITIONAL)
    F2O_ADDITIONAL
#endif
};


struct BRDFInitializeInput
{
    float2 uv;
    float3 normalWS;
    float3 viewDirWS;
    float4 positionHCS;
    float4 color;
    float3 positionWS;
    half3 tangentWS;
    half3 biTangentWS;
    half3x3 TBNWS;
    half3 normalTS;

    #if defined(BRDF_SURFACE_INITIALIZE_ADDITIONAL)
        BRDF_SURFACE_INITIALIZE_ADDITIONAL
    #endif
};
    
struct BRDFSurface
{
    half3 diffuse;
    half3 specular;
    half3 emission;
    half alpha;
    
    half metallic;
    half ao;
    half grazingTerm;
    half perceptualRoughness;
    half smoothness;
    half roughness;
    half roughness2;

    half3 normal;
    half3 tangent;
    half3 biTangent;
    half3 viewDir;
    half3 reflectDir;
    half NDV;
    half TDV;
    half BDV;

    half3 normalTS;
    
    #if defined(BRDF_SURFACE_ADDITIONAL)
        BRDF_SURFACE_ADDITIONAL
    #endif
};


struct BRDFLightInput
{
    half3 lightDirection;
    half3 lightColor;
    half3 halfDir;
    half shadowAttenuation;
    half distanceAttenuation;
    half NDL;
    half VDH;
    half LDV;
    half NDH;
    half LDH;
    half TDH;
    half BDH;
    half TDL;
    half BDL;
};

BRDFLightInput BRDFLightInput_Ctor(BRDFSurface surface,half3 lightDir,half3 lightColor,half shadowAttenuation,half distanceAttenuation)
{
    half3 viewDir = surface.viewDir;
    half3 normal = surface.normal;
    float3 halfDir = SafeNormalize(float3(viewDir) + float3(lightDir));

    BRDFLightInput input;
    input.halfDir = halfDir;
    input.lightDirection=lightDir;
    input.lightColor=lightColor;
    input.shadowAttenuation=shadowAttenuation;
    input.distanceAttenuation=distanceAttenuation;
    input.NDL = max(0,dot(normal, lightDir));
    input.VDH = dot(viewDir, halfDir);
    input.LDV = dot(lightDir, viewDir);
    input.NDH = dot(normal, halfDir);
    input.LDH = dot(lightDir, halfDir);
    input.TDH = dot(surface.tangent,halfDir);
    input.BDH = dot(surface.biTangent,halfDir);
    input.TDL = dot(surface.tangent,lightDir);
    input.BDL = dot(surface.biTangent,lightDir);
    return input;
}

BRDFLightInput BRDFLightInput_Ctor(BRDFSurface surface,Light light)
{
    return BRDFLightInput_Ctor(surface,light.direction,light.color,light.shadowAttenuation,light.distanceAttenuation);
}

#endif