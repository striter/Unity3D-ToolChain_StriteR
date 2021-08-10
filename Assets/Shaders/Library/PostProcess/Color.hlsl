half luminance(half3 color){ return 0.299h * color.r + 0.587h * color.g + 0.114h * color.b; }

float3 Saturation(float3 c,float _saturation)
{
    float luma =  dot(c, float3(0.2126729, 0.7151522, 0.0721750));
    return luma.xxx + _saturation.xxx * (c - luma.xxx);
}

half3 SampleLUT(half3 sampleCol,Texture2D _lutTex,SamplerState _lutSampler,float4 _lutTextelSize,uint _lutCellCount) {
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