// #pragma shader_feature_fragment _TEXTURE_OUTPUT_SRGB

float3 TransformUVToPositionOS(float2 uv)
{
    return float3(uv.x * 2 - 1, uv.y * 2 - 1, 0);
}

float3 Output(float3 _color)
{
    #if _TEXTURE_OUTPUT_SRGB
        return LinearToGamma_Accurate(_color);
    #else
        return _color;
    #endif
}
