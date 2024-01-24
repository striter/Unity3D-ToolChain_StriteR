
half3 SampleLUT( half3 sampleCol,TEXTURE2D_PARAM(_lutTex,_lutSampler),float4 _lutTextelSize,uint4 _lutCellCount) {
    half cellCount=_lutCellCount.x;
    int cellPixelSize = _lutCellCount.z;
    
    int x0CellIndex = floor(sampleCol.b * cellCount);
    int x1CellIndex = x0CellIndex+1;

    int maxIndex=cellCount-1;
    x0CellIndex=min(x0CellIndex,maxIndex);
    x1CellIndex=min(x1CellIndex,maxIndex);

    half x0PixelCount = x0CellIndex* cellPixelSize + (cellPixelSize -1)* sampleCol.r;
    half x1PixelCount = x1CellIndex * cellPixelSize + (cellPixelSize - 1) * sampleCol.r;
    half yPixelCount = sampleCol.g*_lutTextelSize.w;

    half2 uv0 = float2(x0PixelCount, yPixelCount) * _lutTextelSize.xy;
    half2 uv1= float2(x1PixelCount, yPixelCount) * _lutTextelSize.xy;

    half zOffset = fmod(sampleCol.b * cellCount, 1.0h);
    return lerp( SAMPLE_TEXTURE2D(_lutTex,_lutSampler,uv0).rgb,SAMPLE_TEXTURE2D(_lutTex,_lutSampler,uv1).rgb,zOffset) ;
}

float3 ConvertRGBToYCoCg(float3 color)
{
    float3x3 mat = {
        .25,  .5, .25,
        .5,   0,  -.5,
        -.25, .5, -.25
    };
    return mul(mat, color);
}

float3 ConvertYCoCgToRGB(float3 color)
{
    float3x3 mat = {
        1,  1,  -1,
        1,  0,   1,
        1, -1,  -1
    };
    return mul(mat, color);
}