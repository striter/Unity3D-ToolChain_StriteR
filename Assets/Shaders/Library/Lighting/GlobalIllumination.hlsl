//Indirect Diffuse
// float4 _SHAr,_SHAg,_SHAb,_SHBr,_SHBg,_SHBb,_SHC;
// #ifdef ENVIRONMENT_CUSTOM
//     #define SHAr _SHAr
//     #define SHAg _SHAg
//     #define SHAb _SHAb
//     #define SHBr _SHBr
//     #define SHBg _SHBg
//     #define SHBb _SHBb
//     #define SHC _SHC
// #else
    #define SHAr unity_SHAr
    #define SHAg unity_SHAg
    #define SHAb unity_SHAb
    #define SHBr unity_SHBr
    #define SHBg unity_SHBg
    #define SHBb unity_SHBb
    #define SHC unity_SHC
// #endif


half3 IndirectDiffuse_SH(half3 normal)
{
    float3 res = SHEvalLinearL0L1(normal, SHAr, SHAg, SHAb);
    // res += SHEvalLinearL2(normal, SHBr, SHBg, SHBb, SHC);
    #ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToSRGB(res);
    #endif
    return res;
}

//Lightmaps
#if defined(LIGHTMAP_ON) || defined(LIGHTMAP_CUSTOM) || defined(LIGHTMAP_INTERPOLATE)
    #define ILIGHTMAPPED
#endif

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
    half3 illuminance = SampleLightmapSubtractive(lightmapTex,lightmapSampler,lightmapUV);
    float4 directionSample = SAMPLE_TEXTURE2D_LIGHTMAP(lightmapDirTex,lightmapDirSampler,lightmapUV);
    
    half3 direction = directionSample.xyz - 0.5;
    half directionParam = dot(normalWS,direction) / max(1e-4,directionSample.w);
    return illuminance * directionParam;
}

#if  defined(LIGHTMAP_CUSTOM) || defined(LIGHTMAP_INTERPOLATE)
    float _Lightmap_Interpolation;

    SAMPLER(sampler_Lightmap0);
    TEXTURE2D(_Lightmap0); TEXTURE2D(_Lightmap_Interpolate0); 
    TEXTURE2D(_Lightmap1); TEXTURE2D(_Lightmap_Interpolate1);
    TEXTURE2D(_Lightmap2); TEXTURE2D(_Lightmap_Interpolate2);
    TEXTURE2D(_Lightmap3); TEXTURE2D(_Lightmap_Interpolate3);
    TEXTURE2D(_Lightmap4); TEXTURE2D(_Lightmap_Interpolate4);
    TEXTURE2D(_Lightmap5); TEXTURE2D(_Lightmap_Interpolate5);
    TEXTURE2D(_Lightmap6); TEXTURE2D(_Lightmap_Interpolate6);
    TEXTURE2D(_Lightmap7); TEXTURE2D(_Lightmap_Interpolate7);
    TEXTURE2D(_Lightmap8); TEXTURE2D(_Lightmap_Interpolate8);
    TEXTURE2D(_Lightmap9); TEXTURE2D(_Lightmap_Interpolate9);

    half3 SampleCustomLightmap(float2 _lightmapUV)
    {
        half3 illuminance = 0;
        [branch]
        switch(_LightmapIndex)
        {
            case 0:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap0,sampler_Lightmap0),_lightmapUV);break;
            case 1:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap1,sampler_Lightmap0),_lightmapUV);break;
            case 2:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap2,sampler_Lightmap0),_lightmapUV);break;
            case 3:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap3,sampler_Lightmap0),_lightmapUV);break;
            case 4:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap4,sampler_Lightmap0),_lightmapUV);break;
            case 5:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap5,sampler_Lightmap0),_lightmapUV);break;
            case 6:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap6,sampler_Lightmap0),_lightmapUV);break;
            case 7:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap7,sampler_Lightmap0),_lightmapUV);break;
            case 8:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap8,sampler_Lightmap0),_lightmapUV);break;
            case 9:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap9,sampler_Lightmap0),_lightmapUV);break;
        }

        #if LIGHTMAP_INTERPOLATE
        half3 interpolate=0;
        [branch]
        switch(_LightmapIndex)
        {
            case 0:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate0,sampler_Lightmap0),_lightmapUV);break;
            case 1:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate1,sampler_Lightmap0),_lightmapUV);break;
            case 2:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate2,sampler_Lightmap0),_lightmapUV);break;
            case 3:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate3,sampler_Lightmap0),_lightmapUV);break;
            case 4:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate4,sampler_Lightmap0),_lightmapUV);break;
            case 5:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate5,sampler_Lightmap0),_lightmapUV);break;
            case 6:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate6,sampler_Lightmap0),_lightmapUV);break;
            case 7:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate7,sampler_Lightmap0),_lightmapUV);break;
            case 8:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate8,sampler_Lightmap0),_lightmapUV);break;
            case 9:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate9,sampler_Lightmap0),_lightmapUV);break;
        }
        illuminance = lerp(illuminance,interpolate,_Lightmap_Interpolation);
        #endif
        return illuminance;
    }
#endif

half3 IndirectDiffuse_Lightmap(float2 lightmapUV,half3 normalWS)
{
    half3 illuminance = 0.0h;
    #if defined(ILIGHTMAPPED)
        #if defined(LIGHTMAP_CUSTOM) || defined(LIGHTMAP_INTERPOLATE)
            illuminance = SampleCustomLightmap(lightmapUV);
        #elif defined(DIRLIGHTMAP_COMBINED)
            illuminance=SampleLightmapDirectional(TEXTURE2D_LIGHTMAP_ARGS(unity_Lightmap,samplerunity_Lightmap),TEXTURE2D_LIGHTMAP_ARGS(unity_LightmapInd,samplerunity_Lightmap),lightmapUV,normalWS);
        #else
            illuminance=SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(unity_Lightmap,samplerunity_Lightmap), lightmapUV);
        #endif
    #endif
    return illuminance;
}


half3 IndirectDiffuse_Lightmap(inout Light mainLight,float2 lightmapUV,half3 normalWS)
{
    half3 lightSample=IndirectDiffuse_Lightmap(lightmapUV,normalWS);
    MixRealtimeAndBakedGI(mainLight,normalWS,lightSample);
    return lightSample;
}


#ifdef ILIGHTMAPPED
    #if defined(LIGHTMAP_CUSTOM) || defined(LIGHTMAP_INTERPOLATE)
        #define LIGHTMAP_ST _LightmapST
    #else
        #define LIGHTMAP_ST unity_LightmapST
    #endif

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

half4 IndirectSpecular(float2 screenUV,float eyeDepth, half3 normalTS)
{
    [branch]
    if (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureOn) == 1)
    {
        screenUV += normalTS.xy * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionNormalDistort)*rcp(eyeDepth);
        [branch]
        switch (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureIndex))
        {
            default:return 0;
            case 0:
                return tex2D(_CameraReflectionTexture0, screenUV);
            case 1:
                return tex2D(_CameraReflectionTexture1, screenUV);
            case 2:
                return tex2D(_CameraReflectionTexture2, screenUV);
            case 3:
                return tex2D(_CameraReflectionTexture3, screenUV);
            case 4:
                return tex2D(_ScreenSpaceReflectionTexture,screenUV);
        }
    }
    else    //Avoid warning
    {
        return 0;
    }
}


//Indirect Specular
TEXTURECUBE(_SpecCube0);SAMPLER(sampler_SpecCube0);
TEXTURECUBE(_SpecCube0_Interpolate);SAMPLER(sampler_SpecCube0_Interpolate);
#if defined(ENVIRONMENT_CUSTOM) || defined(ENVIRONMENT_INTERPOLATE)
    #define SPECCUBE0 _SpecCube0
    #define SPECCUBESAMPLER sampler_SpecCube0
#else
    #define SPECCUBE0 unity_SpecCube0
    #define SPECCUBESAMPLER samplerunity_SpecCube0
#endif

half3 SampleCubeSpecular(TEXTURECUBE_PARAM(cube,cubeSampler) ,half3 reflectDir,float perceptualRoughness)
{
    half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * UNITY_SPECCUBE_LOD_STEPS;
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(cube, cubeSampler, reflectDir, mip);
    return DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
}

half3 IndirectCubeSpecular(half3 reflectDir, float perceptualRoughness)
{
    #if defined(_ENVIRONMENTREFLECTIONS_OFF)
        return _GlossyEnvironmentColor.rgb;
    #endif
    half3 specular = SampleCubeSpecular(TEXTURECUBE_ARGS(SPECCUBE0,SPECCUBESAMPLER),reflectDir,perceptualRoughness);
    #if LIGHTMAP_INTERPOLATE
        specular = lerp(specular , SampleCubeSpecular(TEXTURECUBE_ARGS(_SpecCube0_Interpolate,sampler_SpecCube0_Interpolate),reflectDir,perceptualRoughness),_Lightmap_Interpolation);
    #endif
    return specular;
}

half3 IndirectSpecular(float3 reflectDir, float perceptualRoughness, half4 positionHCS, half3 normalTS)
{
    half3 specular = IndirectCubeSpecular(reflectDir, perceptualRoughness);
    half4 indirectSpecular=IndirectSpecular(TransformHClipToNDC(positionHCS),RawToEyeDepth(positionHCS.z / max(FLT_EPS,positionHCS.w)), normalTS);
    return lerp(specular,indirectSpecular.rgb,indirectSpecular.a);
}
