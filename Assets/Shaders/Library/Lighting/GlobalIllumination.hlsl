//SH
half3 SampleSHL2(half3 _normalWS,half4 _SHAr,half4 _SHAg,half4 _SHAb,half4 _SHBr,half4 _SHBg,half4 _SHBb,half4 _SHC)
{
    float3 res = SHEvalLinearL0L1(_normalWS, _SHAr, _SHAg, _SHAb);
    res += SHEvalLinearL2(_normalWS, _SHBr, _SHBg, _SHBb, _SHC);
    #ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToSRGB(res);
    #endif
    return res;
}

#define SHL2Input(_prefix) float4 _##_prefix##SHAr;float4 _##_prefix##SHAg;float4 _##_prefix##SHAb;float4 _##_prefix##SHBr;float4 _##_prefix##SHBg;float4 _##_prefix##SHBb;float4 _##_prefix##SHC;
#define SHL2Sample(_normalWS,_prefix) SampleSHL2(_normalWS,##_prefix##_SHAr,##_prefix##_SHAg,##_prefix##_SHAb,##_prefix##_SHBr,##_prefix##_SHBg,##_prefix##_SHBb,##_prefix##_SHC)

half3 IndirectDiffuse_SH(half3 _normalWS)
{
    return SampleSHL2(_normalWS, unity_SHAr, unity_SHAg, unity_SHAb,unity_SHBr,unity_SHBg, unity_SHBb,unity_SHC);
    // return SHL2Sample(_normalWS , unity);
}

half3 SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex,lightmapSampler),float2 lightmapUV)
{
    #ifdef UNITY_LIGHTMAP_FULL_HDR
        return SAMPLE_TEXTURE2D(lightmapTex,lightmapSampler,lightmapUV).rgb;
    #else
        return DecodeLightmap(SAMPLE_TEXTURE2D(lightmapTex,lightmapSampler,lightmapUV).rgba, half4(LIGHTMAP_HDR_MULTIPLIER,LIGHTMAP_HDR_MULTIPLIER,0.0h,0.0h));
    #endif
}

half3 SampleLightmapDirectional(TEXTURE2D_LIGHTMAP_PARAM(lightmapTex,lightmapSampler),TEXTURE2D_LIGHTMAP_PARAM(lightmapDirTex,lightmapDirSampler),float2 lightmapUV,half3 normalWS)
{
    // return SampleLightmap(lightmapUV,normalWS);
    half3 illuminance = SampleLightmapSubtractive(lightmapTex,lightmapSampler,lightmapUV);
    float4 directionSample = SAMPLE_TEXTURE2D_LIGHTMAP(lightmapDirTex,lightmapDirSampler,lightmapUV);
    
    half halfLambert = dot(normalWS, directionSample.xyz - 0.5) + 0.5;
    half directionParam = halfLambert / max(1e-4,directionSample.w);
    return illuminance * directionParam;
}

half3 IndirectDiffuse_Lightmap(inout Light mainLight,float2 lightmapUV,half3 normalWS)
{
    half3 illuminance= 
    #if defined(DIRLIGHTMAP_COMBINED)
        SampleLightmapDirectional(TEXTURE2D_LIGHTMAP_ARGS(unity_Lightmap,samplerunity_Lightmap),TEXTURE2D_LIGHTMAP_ARGS(unity_LightmapInd,samplerunity_Lightmap),lightmapUV,normalWS);
    #else
        SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(unity_Lightmap,samplerunity_Lightmap), lightmapUV);
    #endif

    MixRealtimeAndBakedGI(mainLight,normalWS,illuminance);
    return illuminance;
}

half3 SampleCubeSpecular(TEXTURECUBE_PARAM(cube,cubeSampler),half4 _decodeInstruction ,half3 reflectDir,float perceptualRoughness,half offset=0)
{
    half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * (UNITY_SPECCUBE_LOD_STEPS+offset);
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(cube, cubeSampler, reflectDir, mip);
    return DecodeHDREnvironment(encodedIrradiance, _decodeInstruction);
}

//Indirect Specular
half3 IndirectCubeSpecular(half3 reflectDir, float perceptualRoughness,int offset = 0)
{
    #if defined(_ENVIRONMENTREFLECTIONS_OFF)
        return _GlossyEnvironmentColor.rgb;
    #endif
    
    return SampleCubeSpecular(TEXTURECUBE_ARGS(unity_SpecCube0,samplerunity_SpecCube0),unity_SpecCube0_HDR,reflectDir,perceptualRoughness,offset);
}

//Indirect Specular
sampler2D _CameraReflectionTexture0;
sampler2D _CameraReflectionTexture1;
sampler2D _CameraReflectionTexture2;
sampler2D _CameraReflectionTexture3;

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial_PlanarReflection)
    INSTANCING_PROP(uint, _CameraReflectionTextureOn)
    INSTANCING_PROP(uint, _CameraReflectionTextureIndex)
    INSTANCING_PROP(half, _CameraReflectionNormalDistort)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial_PlanarReflection)

sampler2D _ScreenSpaceReflectionTexture;

half4 IndirectSSRSpecular(float2 screenUV,float eyeDepth, half3 normalTS)
{
    [branch]
    if (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureOn) == 1)
    {
        screenUV += normalTS.xy * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionNormalDistort)*rcp(eyeDepth);
        [branch]
        switch (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureIndex))
        {
            default:return 0;
            case 0:return tex2D(_CameraReflectionTexture0, screenUV);
            case 1:return tex2D(_CameraReflectionTexture1, screenUV);
            case 2:return tex2D(_CameraReflectionTexture2, screenUV);
            case 3:return tex2D(_CameraReflectionTexture3, screenUV);
        }
    }
    else    //Avoid warning
    {
        return 0;//tex2D(_ScreenSpaceReflectionTexture,screenUV);
    }
}

half3 IndirectSpecularWithSSR(float3 reflectDir, float perceptualRoughness, half4 positionHCS, half3 normalTS)
{
    half3 specular = IndirectCubeSpecular(reflectDir, perceptualRoughness,-5);
    half4 indirectSpecular=IndirectSSRSpecular(TransformHClipToNDC(positionHCS),RawToEyeDepth(positionHCS.z / max(FLT_EPS,positionHCS.w)), normalTS);
    specular = lerp(specular,indirectSpecular.rgb,indirectSpecular.a);
    return specular;
}

#if !defined(LIGHTMAP_ST)
    #define LIGHTMAP_ST unity_LightmapST
#endif

#if defined(LIGHTMAP_ON)
    #define A2V_LIGHTMAP float2 lightmapUV:TEXCOORD1;
    #define V2F_LIGHTMAP(index) float2 lightmapUV:TEXCOORD##index;
    #define LIGHTMAP_TRANSFER(v,o) o.lightmapUV=v.lightmapUV*LIGHTMAP_ST.xy+LIGHTMAP_ST.zw; 
    #define IndirectDiffuse(mainLight,i,normalWS) IndirectDiffuse_Lightmap(mainLight,i.lightmapUV,normalWS)
#else
    #define A2V_LIGHTMAP
    #define V2F_LIGHTMAP(index)
    #define LIGHTMAP_TRANSFER(v,o)
    #define IndirectDiffuse(mainLight,i,normalWS) IndirectDiffuse_SH(normalWS)
#endif
#define IndirectSpecular(reflectDir,perceptualRoughness,offset) IndirectCubeSpecular(reflectDir, perceptualRoughness,offset)
