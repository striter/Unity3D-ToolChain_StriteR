#ifndef ANIMATION_INSTANCE
#define ANIMATION_INSTNACE
    sampler2D _AnimTex;
    float4 _AnimTex_TexelSize;
            
    UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(int, _BeginFrame)
        UNITY_DEFINE_INSTANCED_PROP(int, _EndFrame)
        UNITY_DEFINE_INSTANCED_PROP(float, _FrameLerp)
    UNITY_INSTANCING_BUFFER_END(Props)

    half4x4 SampleBoneMatrix(uint sampleFrame,uint boneIndex)
    {
        half2 uv1=half2(.5h+boneIndex*3,.5h+sampleFrame);
        return float4x4(tex2Dlod(_AnimTex,half4(uv1*_AnimTex_TexelSize.xy,0,0))
        ,tex2Dlod(_AnimTex,half4((uv1+half2(1,0))*_AnimTex_TexelSize.xy,0,0))
        ,tex2Dlod(_AnimTex,half4((uv1+half2(2,0))*_AnimTex_TexelSize.xy,0,0))
        ,float4(0,0,0,1));
    }

    half4x4 SampleBoneMatrix(uint sampleFrame,uint4 boneIndexes,half4 boneWeights)
    {
        return SampleBoneMatrix(sampleFrame, boneIndexes.x)*boneWeights.x+SampleBoneMatrix(sampleFrame, boneIndexes.y)*boneWeights.y+SampleBoneMatrix(sampleFrame, boneIndexes.z)*boneWeights.z+SampleBoneMatrix(sampleFrame, boneIndexes.w)*boneWeights.w;
    }

    void SampleBoneInstance(uint4 boneIndexes,half4 boneWeights,inout half4 position,inout half3 normal)
    {
        half4x4 sampleMatrix= lerp(SampleBoneMatrix( UNITY_ACCESS_INSTANCED_PROP(Props, _BeginFrame),boneIndexes,boneWeights),SampleBoneMatrix(UNITY_ACCESS_INSTANCED_PROP( Props, _EndFrame),boneIndexes,boneWeights),UNITY_ACCESS_INSTANCED_PROP(Props, _FrameLerp));
        normal=mul(sampleMatrix,normal);
        position=mul(sampleMatrix,position);
    }

            
    float4 SampleVertex(uint vertexID,uint frame)
    {
        return tex2Dlod(_AnimTex,float4((vertexID*2+.5)*_AnimTex_TexelSize.x,frame*_AnimTex_TexelSize.y,0,0)).xyzw;
    }
    float3 SampleNormal(uint vertexID,uint frame)
    {
        return tex2Dlod(_AnimTex,float4((vertexID*2+1+.5)*_AnimTex_TexelSize.x,frame*_AnimTex_TexelSize.y,0,0)).xyz;
    }

    void SampleVertexInstance(uint vertexID,inout half4 position,inout half3 normal)
    {
        position= lerp(SampleVertex(vertexID,UNITY_ACCESS_INSTANCED_PROP(Props, _BeginFrame)),SampleVertex(vertexID,UNITY_ACCESS_INSTANCED_PROP(Props, _EndFrame)),UNITY_ACCESS_INSTANCED_PROP(Props, _FrameLerp));
        normal= lerp(SampleNormal(vertexID,UNITY_ACCESS_INSTANCED_PROP(Props, _BeginFrame)),SampleNormal(vertexID,UNITY_ACCESS_INSTANCED_PROP(Props, _EndFrame)),UNITY_ACCESS_INSTANCED_PROP(Props, _FrameLerp));
    }

#endif