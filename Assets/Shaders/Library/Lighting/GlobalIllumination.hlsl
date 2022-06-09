//Indirect Diffuse
half3 SampleSHL2(half3 _normalWS,half4 _SHAr,half4 _SHAg,half4 _SHAb,half4 _SHBr,half4 _SHBg,half4 _SHBb,half4 _SHC)
{
    float3 res = SHEvalLinearL0L1(_normalWS, _SHAr, _SHAg, _SHAb);
    res += SHEvalLinearL2(_normalWS, _SHBr, _SHBg, _SHBb, _SHC);
    #ifdef UNITY_COLORSPACE_GAMMA
        res = LinearToSRGB(res);
    #endif
    return res;
}


float4 _SHAr,_SHAg,_SHAb,_SHBr,_SHBg,_SHBb,_SHC;

half3 IndirectDiffuse_SH(half3 _normal)
{
    half3 shl2=0;
    #if defined(ENVIRONMENT_CUSTOM) || defined(ENVIRONMENT_INTERPOLATE)
        shl2 = SampleSHL2(_normal,_SHAr,_SHAg,_SHAb,_SHBr,_SHBg,_SHBb,_SHC);
    #else
        shl2 = SampleSHL2(_normal,unity_SHAr,unity_SHAg,unity_SHAb,unity_SHBr,unity_SHBg,unity_SHBb,unity_SHC);
    #endif

    return shl2;
}

//Lightmaps
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


float _EnvironmentInterpolate;
#if defined(LIGHTMAP_ON)
    #define A2V_LIGHTMAP float2 lightmapUV:TEXCOORD1;
    // #if defined(ENVIRONMENT_CUSTOM) || defined(ENVIRONMENT_INTERPOLATE)
    //     SAMPLER(sampler_Lightmap0);
    //     TEXTURE2D(_Lightmap0); TEXTURE2D(_Lightmap_Interpolate0); 
    //     TEXTURE2D(_Lightmap1); TEXTURE2D(_Lightmap_Interpolate1);
    //     TEXTURE2D(_Lightmap2); TEXTURE2D(_Lightmap_Interpolate2);
    //     TEXTURE2D(_Lightmap3); TEXTURE2D(_Lightmap_Interpolate3);
    //     TEXTURE2D(_Lightmap4); TEXTURE2D(_Lightmap_Interpolate4);
    //     TEXTURE2D(_Lightmap5); TEXTURE2D(_Lightmap_Interpolate5);
    //     TEXTURE2D(_Lightmap6); TEXTURE2D(_Lightmap_Interpolate6);
    //     TEXTURE2D(_Lightmap7); TEXTURE2D(_Lightmap_Interpolate7);
    //     TEXTURE2D(_Lightmap8); TEXTURE2D(_Lightmap_Interpolate8);
    //     TEXTURE2D(_Lightmap9); TEXTURE2D(_Lightmap_Interpolate9);
    //     half3 SampleCustomLightmap(float4 _lightmapUV)
    //     {
    //         half3 illuminance = 0;
    //         float2 lightmapUV = _lightmapUV.xy;
    //         [branch]
    //         switch(_LightmapIndex)
    //         {
    //             case 0:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap0,sampler_Lightmap0),lightmapUV);break;
    //             case 1:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap1,sampler_Lightmap0),lightmapUV);break;
    //             case 2:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap2,sampler_Lightmap0),lightmapUV);break;
    //             case 3:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap3,sampler_Lightmap0),lightmapUV);break;
    //             case 4:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap4,sampler_Lightmap0),lightmapUV);break;
    //             case 5:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap5,sampler_Lightmap0),lightmapUV);break;
    //             case 6:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap6,sampler_Lightmap0),lightmapUV);break;
    //             case 7:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap7,sampler_Lightmap0),lightmapUV);break;
    //             case 8:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap8,sampler_Lightmap0),lightmapUV);break;
    //             case 9:illuminance= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap9,sampler_Lightmap0),lightmapUV);break;
    //         }
    //
    //         #if defined(ENVIRONMENT_INTERPOLATE)
    //             half3 interpolate=0;
    //             float2 interpolateUV = _lightmapUV.zw;
    //             [branch]
    //             switch(_LightmapInterpolateIndex)
    //             {
    //                 case 0:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate0,sampler_Lightmap0),interpolateUV);break;
    //                 case 1:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate1,sampler_Lightmap0),interpolateUV);break;
    //                 case 2:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate2,sampler_Lightmap0),interpolateUV);break;
    //                 case 3:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate3,sampler_Lightmap0),interpolateUV);break;
    //                 case 4:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate4,sampler_Lightmap0),interpolateUV);break;
    //                 case 5:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate5,sampler_Lightmap0),interpolateUV);break;
    //                 case 6:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate6,sampler_Lightmap0),interpolateUV);break;
    //                 case 7:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate7,sampler_Lightmap0),interpolateUV);break;
    //                 case 8:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate8,sampler_Lightmap0),interpolateUV);break;
    //                 case 9:interpolate= SampleLightmapSubtractive(TEXTURE2D_LIGHTMAP_ARGS(_Lightmap_Interpolate9,sampler_Lightmap0),interpolateUV);break;
    //             }
    //             illuminance = lerp(illuminance,interpolate,_EnvironmentInterpolate);
    //         #endif
    //         return illuminance;
    //     }
    //
    //     half3 IndirectDiffuse_CustomLightmap(inout Light mainLight,float4 lightmapUV,half3 normalWS)
    //     {
    //         half3 illuminance =  SampleCustomLightmap(lightmapUV);
    //         MixRealtimeAndBakedGI(mainLight,normalWS,illuminance);
    //         return illuminance;
    //     }
    //
    //     #define V2F_LIGHTMAP(index) float4 lightmapUV:TEXCOORD##index;
    //     #define LIGHTMAP_TRANSFER(v,o) o.lightmapUV=float4(v.lightmapUV*_LightmapST.xy+_LightmapST.zw,v.lightmapUV*_LightmapInterpolateST.xy + _LightmapInterpolateST.zw);
    //     #define IndirectDiffuse(mainLight,i,normalWS) IndirectDiffuse_CustomLightmap(mainLight,i.lightmapUV,normalWS)
    // #else

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

        #define V2F_LIGHTMAP(index) float2 lightmapUV:TEXCOORD##index;
        #define LIGHTMAP_TRANSFER(v,o) o.lightmapUV=v.lightmapUV*unity_LightmapST.xy+unity_LightmapST.zw;
        #define IndirectDiffuse(mainLight,i,normalWS) IndirectDiffuse_Lightmap(mainLight,i.lightmapUV,normalWS)
    // #endif

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
TEXTURECUBE(_SpecCube0);SAMPLER(sampler_SpecCube0); half4 _SpecCube0_HDR;
TEXTURECUBE(_SpecCube0_Interpolate);SAMPLER(sampler_SpecCube0_Interpolate); half4 _SpecCube0_Interpolate_HDR;
half _SpecCube0_Intensity;

half3 SampleCubeSpecular(TEXTURECUBE_PARAM(cube,cubeSampler),half4 _decodeInstruction ,half3 reflectDir,float perceptualRoughness,half offset=0)
{
    half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * (UNITY_SPECCUBE_LOD_STEPS+offset);
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(cube, cubeSampler, reflectDir, mip);
    return DecodeHDREnvironment(encodedIrradiance, _decodeInstruction);
}

half3 IndirectCubeSpecular(half3 reflectDir, float perceptualRoughness,int offset = 0)
{
    #if defined(_ENVIRONMENTREFLECTIONS_OFF)
        return _GlossyEnvironmentColor.rgb;
    #endif
    
    #if defined(ENVIRONMENT_CUSTOM) || defined(ENVIRONMENT_INTERPOLATE)
    half3 specular = SampleCubeSpecular(TEXTURECUBE_ARGS(_SpecCube0,sampler_SpecCube0),_SpecCube0_HDR,reflectDir,perceptualRoughness,offset);
        #if ENVIRONMENT_INTERPOLATE
            specular = lerp(specular , SampleCubeSpecular(TEXTURECUBE_ARGS(_SpecCube0_Interpolate,sampler_SpecCube0_Interpolate),_SpecCube0_Interpolate_HDR,reflectDir,perceptualRoughness,offset),_EnvironmentInterpolate);
        #endif
    specular *= _SpecCube0_Intensity;
    #else
        half3 specular = SampleCubeSpecular(TEXTURECUBE_ARGS(unity_SpecCube0,samplerunity_SpecCube0),unity_SpecCube0_HDR,reflectDir,perceptualRoughness,offset);
    #endif
    
    return specular;
}

half3 IndirectSpecular(float3 reflectDir, float perceptualRoughness, half4 positionHCS, half3 normalTS)
{
    half3 specular = IndirectCubeSpecular(reflectDir, perceptualRoughness);
    half4 indirectSpecular=IndirectSpecular(TransformHClipToNDC(positionHCS),RawToEyeDepth(positionHCS.z / max(FLT_EPS,positionHCS.w)), normalTS);
    return lerp(specular,indirectSpecular.rgb,indirectSpecular.a);
}
