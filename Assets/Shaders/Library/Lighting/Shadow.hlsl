//Caster
float3 _LightDirection;
float4 ShadowCasterCS(float3 positionWS, float3 normalWS)
{
    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
    #if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif
    return positionCS;
}

//Receiver
float SampleHardShadow(TEXTURE2D_SHADOW_PARAM(_ShadowMap, _sampler_ShadowMap),float3 _shadowCoords,float _shadowStrength)
{
    real attenuation=SAMPLE_TEXTURE2D_SHADOW(_ShadowMap, _sampler_ShadowMap,_shadowCoords);
    attenuation = LerpWhiteTo(attenuation, _shadowStrength);
    return BEYOND_SHADOW_FAR(_shadowCoords) ? 1.0 : attenuation;
}

#define A2V_SHADOW_CASTER float3 positionOS:POSITION; float3 normalOS:NORMAL
#define V2F_SHADOW_CASTER float4 positionCS:SV_POSITION
#define SHADOW_CASTER_VERTEX(v,positionWS) o.positionCS= ShadowCasterCS(positionWS,TransformObjectToWorldNormal(v.normalOS))
