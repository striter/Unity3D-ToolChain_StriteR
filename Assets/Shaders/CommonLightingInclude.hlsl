#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
//Diffuse-Lambert
float GetDiffuse(float3 normal,float3 lightDir)
{
	return  dot(normal, lightDir);
}
float GetDiffuse(float3 normal, float3 lightDir,float lambert,float atten)
{
    float diffuse = saturate(GetDiffuse(normal, lightDir));
    diffuse *= atten;
    return lambert + (1 - lambert) * diffuse;
}


//Blinn-Phong Specular Optimized, range 0.9-1
float GetSpecular(float3 normal,float3 halfDir,float range)
{
	float specular = dot(normal,halfDir);
	return smoothstep(range, 1, specular);
}

float GetSpecular(float3 normal,float3 lightDir,float3 viewDir,float range)
{
	return GetSpecular(normal,normalize(lightDir+viewDir),range);
}


//Kajiya-Kay Anisotropic Lighting Model
float StrandSpecular(float3 T,float3 N,float3 H,float exponent,float3 shift)
{
    T=normalize(T+shift*N);
    float dotTH=dot(T,H);
    float sinTH=sqrt(1.0-dotTH*dotTH);
    float dirAtten=smoothstep(-1,0,dotTH);
    return dirAtten*pow(sinTH,exponent);
}

float3 DecodeNormalMap(float3 normal)
{
	return normal*2-1;
}

#if UNITY_PASS_SHADOWCASTER
float3 _LightDirection;
float4 ShadowCasterCS(float3 positionOS, float3 normalOS)
{
    float3 positionWS = TransformObjectToWorld(positionOS);
    float3 normalWS = TransformObjectToWorldNormal(normalOS);
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
#if UNITY_REVERSED_Z
	positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif
    return positionCS;
}
#define A2F_SHADOW_CASTER float3 positionOS:POSITION; float3 normalOS:NORMAL
#define V2F_SHADOW_CASTER float4 positionCS:SV_POSITION
#define SHADOW_CASTER_FRAGMENT(v,o) o.positionCS= ShadowCasterCS(v.positionOS.xyz,v.normalOS)
#endif