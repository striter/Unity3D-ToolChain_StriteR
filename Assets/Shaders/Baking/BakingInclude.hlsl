
float4 TransformUVToPositionCS(float2 uv)
{
    float3 positionOS =  float3(uv.x * 2 - 1, uv.y * 2 - 1, 0);

    float4 positionCS = float4(positionOS.xy,UNITY_NEAR_CLIP_VALUE,1);
    #if UNITY_UV_STARTS_AT_TOP
        positionCS.y = -positionCS.y;
    #endif

    return positionCS;
}