
half3 SampleLUT( half3 sampleCol,TEXTURE2D_PARAM(_lutTex,_lutSampler),float4 _lutTextelSize,uint _lutCellCount) {
    half width=_lutCellCount;

    int lutCellPixelCount = _lutTextelSize.z / width;
    int x0CellIndex =  floor(sampleCol.b * width);
    int x1CellIndex = x0CellIndex+1;

    int maxIndex=width-1;
    x0CellIndex=min(x0CellIndex,maxIndex);
    x1CellIndex=min(x1CellIndex,maxIndex);

    half x0PixelCount = x0CellIndex* lutCellPixelCount + (lutCellPixelCount -1)* sampleCol.r;
    half x1PixelCount = x1CellIndex * lutCellPixelCount + (lutCellPixelCount - 1) * sampleCol.r;
    half yPixelCount = sampleCol.g*_lutTextelSize.w;

    half2 uv0 = float2(x0PixelCount, yPixelCount) * _lutTextelSize.xy;
    half2 uv1= float2(x1PixelCount, yPixelCount) * _lutTextelSize.xy;

    half zOffset = fmod(sampleCol.b * width, 1.0h);
    return lerp( SAMPLE_TEXTURE2D(_lutTex,_lutSampler,uv0).rgb,SAMPLE_TEXTURE2D(_lutTex,_lutSampler,uv1).rgb,zOffset) ;
}

static const float kRGBMRange=8.0;
static const float kInvRGBMRange=.125;
static const float k8Byte=255.;
static const float kInv8Byte=0.003921568627451;
half4 EncodeRGBM(float3 color)
{
    color*=kInvRGBMRange;
    half m=max(color);
    m=ceil(m*k8Byte)*kInv8Byte;
    return half4(color*rcp(m),m);
}
half3 DecodeRGBM(half4 rgbm)
{
    return rgbm.rgb*rgbm.w*kRGBMRange;
}