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

half4 IndirectSpecular(half2 screenUV,float eyeDepth, half3 normalTS)
{
    screenUV += normalTS.xy * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionNormalDistort)*rcp(eyeDepth);
    half4 indirectSpecular=0;
    [branch]
    if (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureOn) == 1)
    {
        [branch]
        switch (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureIndex))
        {
            default:return 0;
            case 0u:
                indirectSpecular= tex2D(_CameraReflectionTexture0, screenUV);
                break;
            case 1u:
                indirectSpecular= tex2D(_CameraReflectionTexture1, screenUV);
                break;
            case 2u:
                indirectSpecular= tex2D(_CameraReflectionTexture2, screenUV);
                break;
            case 3u:
                indirectSpecular= tex2D(_CameraReflectionTexture3, screenUV);
                break;
            case 4u:
                indirectSpecular=tex2D(_ScreenSpaceReflectionTexture,screenUV);
                break;
        }
    }
    
    return indirectSpecular;
}

half3 SAMPLE_SH(half3 normal)
{
    float3 res = SHEvalLinearL0L1(normal, unity_SHAr, unity_SHAg, unity_SHAb);
    res += SHEvalLinearL2(normal, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
    #ifdef UNITY_COLORSPACE_GAMMA
    res = LinearToSRGB(res);
    #endif
    return res;
}

half3 SampleLightmap(inout Light mainLight,float2 lightmapUV,float3 normalWS)
{
    half3 lightSample=SampleLightmap(lightmapUV,normalWS);
    MixRealtimeAndBakedGI(mainLight,normalWS,lightSample);
    return lightSample;
}

#ifdef LIGHTMAP_ON
    #define A2V_LIGHTMAP float2 lightmapUV:TEXCOORD1;
    #define V2F_LIGHTMAP(index) float2 lightmapUV:TEXCOORD##index;
    #define LIGHTMAP_TRANSFER(v,o) o.lightmapUV=v.lightmapUV*unity_LightmapST.xy+unity_LightmapST.zw;
    #define IndirectBRDFDiffuse(mainLight,lightmapUV,normalWS) SampleLightmap(mainLight,lightmapUV,normalWS)
#else
    #define A2V_LIGHTMAP
    #define V2F_LIGHTMAP(index)
    #define LIGHTMAP_TRANSFER(v,o)
    #define IndirectBRDFDiffuse(mainLight,lightmapUV,normalWS) SAMPLE_SH(normalWS)
#endif

half3 IndirectBRDFCubeSpecular(half3 reflectDir, float perceptualRoughness)
{
    #if defined(_ENVIRONMENTREFLECTIONS_OFF)
        return _GlossyEnvironmentColor.rgb;
    #endif
    half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * UNITY_SPECCUBE_LOD_STEPS;
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDir, mip);
    return DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
}

half3 IndirectBRDFSpecular(float3 reflectDir, float perceptualRoughness, half4 positionHCS, half3 normalTS)
{
    half3 specular = IndirectBRDFCubeSpecular(reflectDir, perceptualRoughness);
    return specular;
    half4 indirectSpecular=IndirectSpecular(TransformHClipToNDC(positionHCS),RawToEyeDepth(positionHCS.z/positionHCS.w), normalTS);
    return lerp(specular,indirectSpecular.rgb,indirectSpecular.a);
}
