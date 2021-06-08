
half3 IndirectBRDFDiffuse(half3 normal)
{
    float3 res = SHEvalLinearL0L1(normal, unity_SHAr, unity_SHAg, unity_SHAb);
    res += SHEvalLinearL2(normal, unity_SHBr, unity_SHBg, unity_SHBb, unity_SHC);
#ifdef UNITY_COLORSPACE_GAMMA
	res = LinearToSRGB(res);
#endif
    return res;
}

uint _CameraReflectionTextureOn;
uint _CameraReflectionTextureIndex;
half _CameraReflectionNormalDistort;
sampler2D _CameraReflectionTexture0;
sampler2D _CameraReflectionTexture1;
sampler2D _CameraReflectionTexture2;
sampler2D _CameraReflectionTexture3;

half4 IndirectBRDFPlanarSpecular(half4 screenPos, half3 normalTS)
{
    half2 screenUV = screenPos.xy / screenPos.w + normalTS.xy * _CameraReflectionNormalDistort;
    [branch]
    switch (_CameraReflectionTextureIndex)
    {
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

half3 IndirectBRDFSpecular(float3 reflectDir, float perceptualRoughness, half4 screenPos, half3 normalTS)
{
    half3 specular = IndirectBRDFCubeSpecular(reflectDir, perceptualRoughness);
    [branch]
    if (_CameraReflectionTextureOn == 1u)
    {
        half4 planarReflection = IndirectBRDFPlanarSpecular(screenPos, normalTS);
        specular = lerp(specular, planarReflection.rgb, planarReflection.a);
    }
    return specular;
}