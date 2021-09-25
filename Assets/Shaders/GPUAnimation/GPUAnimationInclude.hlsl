//#pragma multi_compile_local _ _ANIM_TRANSFORM _ANIM_VERTEX

TEXTURE2D(_AnimTex);SAMPLER(sampler_AnimTex);
float4 _AnimTex_TexelSize;
            
UNITY_INSTANCING_BUFFER_START(PropsGPUAnim)
    UNITY_DEFINE_INSTANCED_PROP(int, _AnimFrameBegin)
    UNITY_DEFINE_INSTANCED_PROP(int, _AnimFrameEnd)
    UNITY_DEFINE_INSTANCED_PROP(float, _AnimFrameInterpolate)
UNITY_INSTANCING_BUFFER_END(PropsGPUAnim)

#define _FrameBegin UNITY_ACCESS_INSTANCED_PROP(PropsGPUAnim,_AnimFrameBegin)
#define _FrameEnd UNITY_ACCESS_INSTANCED_PROP(PropsGPUAnim,_AnimFrameEnd)
#define _FrameInterpolate UNITY_ACCESS_INSTANCED_PROP(PropsGPUAnim,_AnimFrameInterpolate)

float4x4 SampleTransformMatrix(uint sampleFrame,uint transformIndex)
{
    float2 index=float2(.5h+transformIndex*3,.5h+sampleFrame);
    return float4x4(SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, index * _AnimTex_TexelSize.xy, 0)
    , SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, (index + float2(1, 0)) * _AnimTex_TexelSize.xy, 0)
    , SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex,  (index + float2(2, 0)) * _AnimTex_TexelSize.xy, 0)
    ,float4(0,0,0,1));
}

float4x4 SampleTransformMatrix(uint sampleFrame,uint4 transformIndex,float4 transformWeights)
{
        return SampleTransformMatrix(sampleFrame, transformIndex.x) * transformWeights.x
            + SampleTransformMatrix(sampleFrame, transformIndex.y) * transformWeights.y
            + SampleTransformMatrix(sampleFrame, transformIndex.z) * transformWeights.z
            + SampleTransformMatrix(sampleFrame, transformIndex.w) * transformWeights.w;
}

void SampleTransform(uint4 transformIndexes,float4 transformWeights,inout float3 positionOS,inout float3 normalOS)
{
    float4x4 sampleMatrix = lerp(SampleTransformMatrix(_FrameBegin, transformIndexes, transformWeights), SampleTransformMatrix(_FrameEnd, transformIndexes, transformWeights), _FrameInterpolate);
    normalOS=mul((float3x3)sampleMatrix,normalOS);
    positionOS=mul(sampleMatrix,float4(positionOS,1)).xyz;
}

float3 SamplePosition(uint vertexID,uint frame)
{
    return SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, float2((vertexID * 2 + .5) * _AnimTex_TexelSize.x, frame * _AnimTex_TexelSize.y), 0).xyz;
}

float3 SampleNormal(uint vertexID,uint frame)
{
    return SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, float2((vertexID * 2 + 1 + .5) * _AnimTex_TexelSize.x, frame * _AnimTex_TexelSize.y), 0).xyz;
}
            
void SampleVertex(uint vertexID,inout float3 positionOS,inout float3 normalOS)
{
    positionOS = lerp(SamplePosition(vertexID, _FrameBegin), SamplePosition(vertexID, _FrameEnd), _FrameInterpolate);
    normalOS = lerp(SampleNormal(vertexID, _FrameBegin), SampleNormal(vertexID, _FrameEnd), _FrameInterpolate);
}