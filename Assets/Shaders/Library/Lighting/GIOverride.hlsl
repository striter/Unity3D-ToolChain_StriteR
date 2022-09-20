float _Interpolation;
float4 _SHAr,_SHAg,_SHAb,_SHBr,_SHBg,_SHBb,_SHC;
TEXTURE2D(_Lightmap);SAMPLER(sampler_Lightmap);
float4 _LightmapST;

TEXTURE2D(_Lightmap_Interpolate);SAMPLER(sampler_Lightmap_Interpolate);
float4 _LightmapInterpolateST;

float3 IndirectDiffuseOverride(Light mainLight,float4 lightmapUV,float3 normalWS)
{
    float3 illuminance = SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap,sampler_Lightmap),lightmapUV.xy);
    MixRealtimeAndBakedGI(mainLight,normalWS,illuminance);

#if GI_INTERPOLATE
    float3 illuminanceInterpolate = SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate,sampler_Lightmap_Interpolate),lightmapUV.zw);
    MixRealtimeAndBakedGI(mainLight,normalWS,illuminanceInterpolate);
    illuminance = lerp(illuminance,illuminanceInterpolate,_Interpolation);
#endif
    
    return illuminance;
}

TEXTURECUBE(_SpecCube);SAMPLER(sampler_SpecCube); half4 _SpecCube_HDR;
TEXTURECUBE(_SpecCube_Interpolate);SAMPLER(sampler_SpecCube_Interpolate); half4 _SpecCube_Interpolate_HDR;
half _SpecCube_Intensity; half _SpecCube_Intensity_Interpolate;

float3 IndirectSpecularOverride(float3 reflectDir,float perceptualRoughness,float offset)
{
    half3 specular = SampleCubeSpecular(TEXTURECUBE_ARGS(_SpecCube,sampler_SpecCube),_SpecCube_HDR,reflectDir,perceptualRoughness,offset)*_SpecCube_Intensity;
#if GI_INTERPOLATE
    specular = lerp(specular , SampleCubeSpecular(TEXTURECUBE_ARGS(_SpecCube_Interpolate,sampler_SpecCube_Interpolate),_SpecCube_Interpolate_HDR,reflectDir,perceptualRoughness,offset),_Interpolation)*_SpecCube_Intensity_Interpolate,_Interpolation;
#endif
    return specular;
}

#if defined(GI_OVERRIDE)||defined(GI_INTERPOLATE)
    #if defined(LIGHTMAP_ON)
        #if defined(GI_OVERRIDE)
            #define A2V_LIGHTMAP float2 lightmapUV:TEXCOORD1;
            #define V2F_LIGHTMAP(index) float4 lightmapUV:TEXCOORD##index;
            #define IndirectDiffuse(mainLight,i,normalWS) IndirectDiffuseOverride(mainLight,i.lightmapUV,normalWS)
            #define LIGHTMAP_TRANSFER(v,o) o.lightmapUV=float4(v.lightmapUV*_LightmapST.xy+_LightmapST.zw,v.lightmapUV*_LightmapInterpolateST.xy + _LightmapInterpolateST.zw);
        #elif defined(GI_INTERPOLATE)
            #define A2V_LIGHTMAP float2 lightmapUV:TEXCOORD1;
            #define V2F_LIGHTMAP(index) float4 lightmapUV:TEXCOORD##index;
            #define IndirectDiffuse(mainLight,i,normalWS) IndirectDiffuseOverride(mainLight,i.lightmapUV,normalWS)
            #define LIGHTMAP_TRANSFER(v,o) o.lightmapUV=float4(v.lightmapUV*_LightmapST.xy+_LightmapST.zw,v.lightmapUV*_LightmapInterpolateST.xy + _LightmapInterpolateST.zw);
        #endif
    #else
        #define IndirectDiffuse(mainLight,i,normalWS) SampleSHL2(normalWS,_SHAr,_SHAg,_SHAb,_SHBr,_SHBg,_SHBb,_SHC)
    #endif

    #define IndirectSpecular(reflectDir,perceptualRoughness,offset) IndirectSpecularOverride(reflectDir,perceptualRoughness,offset)
#endif