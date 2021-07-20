half3 IndirectBRDFDiffuse(half3 normal)
{
    float3 res = SHEvalLinearL0L1(normal, unity_SHAr, unity_SHAg, unity_SHAb);
    res += SHEvalLinearL2(normal, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
#ifdef UNITY_COLORSPACE_GAMMA
	res = LinearToSRGB(res);
#endif
    return res;
}

sampler2D _CameraReflectionTexture0;
sampler2D _CameraReflectionTexture1;
sampler2D _CameraReflectionTexture2;
sampler2D _CameraReflectionTexture3;
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial_PlanarReflection)
    INSTANCING_PROP(uint, _CameraReflectionTextureOn)
    INSTANCING_PROP(uint, _CameraReflectionTextureIndex)
    INSTANCING_PROP(half, _CameraReflectionNormalDistort)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial_PlanarReflection)

half4 IndirectBRDFPlanarSpecular(half2 screenUV, half3 normalTS)
{
    screenUV += normalTS.xy * UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionNormalDistort);
    [branch]
    switch (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureIndex))
    {
        default:return 0;
        case 0u:
            return tex2D(_CameraReflectionTexture0, screenUV);
        case 1u:
            return tex2D(_CameraReflectionTexture1, screenUV);
        case 2u:
            return tex2D(_CameraReflectionTexture2, screenUV);
        case 3u:
            return tex2D(_CameraReflectionTexture3, screenUV);
    }
    return 0;
}

half3 IndirectBRDFCubeSpecular(half3 reflectDir, float perceptualRoughness)
{
    half mip = perceptualRoughness * (1.7 - 0.7 * perceptualRoughness) * UNITY_SPECCUBE_LOD_STEPS;
    half4 encodedIrradiance = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, reflectDir, mip);
    half3 irradiance = DecodeHDREnvironment(encodedIrradiance, unity_SpecCube0_HDR);
    return irradiance;
}

half3 IndirectBRDFSpecular(float3 reflectDir, float perceptualRoughness, half4 positionHCS, half3 normalTS)
{
    half3 specular = IndirectBRDFCubeSpecular(reflectDir, perceptualRoughness);
    [branch]
    if (UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial_PlanarReflection, _CameraReflectionTextureOn) == 1u)
    {
        half4 planarReflection = IndirectBRDFPlanarSpecular(TransformHClipToNDC(positionHCS), normalTS);
        specular = lerp(specular, planarReflection.rgb,  planarReflection.a );
    }
    return specular;
}