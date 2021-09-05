//#pragma multi_compile_local _ _ANIM_BONE _ANIM_VERTEX

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

float4x4 SampleBoneMatrix(uint sampleFrame,uint boneIndex)
{
    float2 index=float2(.5h+boneIndex*3,.5h+sampleFrame);
    return float4x4(SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, index * _AnimTex_TexelSize.xy, 0)
    , SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, (index + float2(1, 0)) * _AnimTex_TexelSize.xy, 0)
    , SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex,  (index + float2(2, 0)) * _AnimTex_TexelSize.xy, 0)
    ,float4(0,0,0,1));
}

float4x4 SampleBoneMatrix(uint sampleFrame,uint4 boneIndexes,float4 boneWeights)
{
#if _OPTIMIZE_1BONE
        return SampleBoneMatrix(sampleFrame, boneIndexes.x)*1;
#elif _OPTIMIZE_2BONE 
        return SampleBoneMatrix(sampleFrame, boneIndexes.x)*boneWeights.x
            + SampleBoneMatrix(sampleFrame, boneIndexes.y)*boneWeights.y;
#else
        return SampleBoneMatrix(sampleFrame, boneIndexes.x) * boneWeights.x
            + SampleBoneMatrix(sampleFrame, boneIndexes.y) * boneWeights.y
            + SampleBoneMatrix(sampleFrame, boneIndexes.z) * boneWeights.z
            + SampleBoneMatrix(sampleFrame, boneIndexes.w) * boneWeights.w;
#endif
}

void SampleBoneInstance(uint4 boneIndexes,float4 boneWeights,inout float3 positionOS,inout float3 normalOS)
{
    float4x4 sampleMatrix = lerp(SampleBoneMatrix(_FrameBegin, boneIndexes, boneWeights), SampleBoneMatrix(_FrameEnd, boneIndexes, boneWeights), _FrameInterpolate);
    normalOS=mul((float3x3)sampleMatrix,normalOS);
    positionOS=mul(sampleMatrix,float4(positionOS,1)).xyz;
}

float3 SampleVertex(uint vertexID,uint frame)
{
    return SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, float2((vertexID * 2 + .5) * _AnimTex_TexelSize.x, frame * _AnimTex_TexelSize.y), 0).xyz;
}

float3 SampleNormal(uint vertexID,uint frame)
{
    return SAMPLE_TEXTURE2D_LOD(_AnimTex,sampler_AnimTex, float2((vertexID * 2 + 1 + .5) * _AnimTex_TexelSize.x, frame * _AnimTex_TexelSize.y), 0).xyz;
}
            
void SampleVertexInstance(uint vertexID,inout float3 positionOS,inout float3 normalOS)
{
    positionOS = lerp(SampleVertex(vertexID, _FrameBegin), SampleVertex(vertexID, _FrameEnd), _FrameInterpolate);
    normalOS = lerp(SampleNormal(vertexID, _FrameBegin), SampleNormal(vertexID, _FrameEnd), _FrameInterpolate);
}