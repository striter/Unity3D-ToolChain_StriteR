sampler2D _InstanceAnimationTex;
float4 _InstanceAnimationTex_TexelSize;
            
UNITY_INSTANCING_BUFFER_START(PropsAnimInstance)
    UNITY_DEFINE_INSTANCED_PROP(int, _InstanceFrameBegin)
    UNITY_DEFINE_INSTANCED_PROP(int, _InstanceFrameEnd)
    UNITY_DEFINE_INSTANCED_PROP(float, _InstanceFrameInterpolate)
UNITY_INSTANCING_BUFFER_END(PropsAnimInstance)

float4x4 SampleBoneMatrix(uint sampleFrame,uint boneIndex)
{
    float2 uv1=float2(.5h+boneIndex*3,.5h+sampleFrame);
    return float4x4(tex2Dlod(_InstanceAnimationTex, float4(uv1 * _InstanceAnimationTex_TexelSize.xy, 0, 0))
    , tex2Dlod(_InstanceAnimationTex, float4((uv1 + float2(1, 0)) * _InstanceAnimationTex_TexelSize.xy, 0, 0))
    , tex2Dlod(_InstanceAnimationTex, float4((uv1 + float2(2, 0)) * _InstanceAnimationTex_TexelSize.xy, 0, 0))
    ,float4(0,0,0,1));
}

float4x4 SampleBoneMatrix(uint sampleFrame,uint4 boneIndexes,float4 boneWeights)
{

#if _OPTIMIZE_1BONE
            return SampleBoneMatrix(sampleFrame, boneIndexes.x)*1;
#elif _OPTIMIZE_2BONE 
            return SampleBoneMatrix(sampleFrame, boneIndexes.x)*boneWeights.x+SampleBoneMatrix(sampleFrame, boneIndexes.y)*boneWeights.y;
#else
    return SampleBoneMatrix(sampleFrame, boneIndexes.x) * boneWeights.x + SampleBoneMatrix(sampleFrame, boneIndexes.y) * boneWeights.y + SampleBoneMatrix(sampleFrame, boneIndexes.z) * boneWeights.z + SampleBoneMatrix(sampleFrame, boneIndexes.w) * boneWeights.w;
#endif
}

void SampleBoneInstance(uint4 boneIndexes,float4 boneWeights,inout float3 position,inout float3 normal)
{
    float4x4 sampleMatrix = lerp(SampleBoneMatrix(UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameBegin), boneIndexes, boneWeights), SampleBoneMatrix(UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameEnd), boneIndexes, boneWeights), UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameInterpolate));
    normal=mul((float3x3)sampleMatrix,normal);
    position=mul(sampleMatrix,float4(position,1)).xyz;
}

void SampleBoneInstance(uint4 boneIndexes, float4 boneWeights, inout float3 position)
{
    float4x4 sampleMatrix = lerp(SampleBoneMatrix(UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameBegin), boneIndexes, boneWeights), SampleBoneMatrix(UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameEnd), boneIndexes, boneWeights), UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameInterpolate));
    position=mul(sampleMatrix,float4(position,1)).xyz;
}
            
float3 SampleVertex(uint vertexID,uint frame)
{
    return tex2Dlod(_InstanceAnimationTex, float4((vertexID * 2 + .5) * _InstanceAnimationTex_TexelSize.x, frame * _InstanceAnimationTex_TexelSize.y, 0, 0)).xyz;
}
float3 SampleNormal(uint vertexID,uint frame)
{
    return tex2Dlod(_InstanceAnimationTex, float4((vertexID * 2 + 1 + .5) * _InstanceAnimationTex_TexelSize.x, frame * _InstanceAnimationTex_TexelSize.y, 0, 0)).xyz;
}

void SampleVertexInstance(uint vertexID,inout float3 position,inout float3 normal)
{
    position = lerp(SampleVertex(vertexID, UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameBegin)), SampleVertex(vertexID, UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameEnd)), UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameInterpolate));
    normal = lerp(SampleNormal(vertexID, UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameBegin)), SampleNormal(vertexID, UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameEnd)), UNITY_ACCESS_INSTANCED_PROP(PropsAnimInstance, _InstanceFrameInterpolate));
}