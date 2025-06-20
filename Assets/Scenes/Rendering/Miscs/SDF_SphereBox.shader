Shader "Hidden/SDF_SphereBox"
{
    Properties
    {
        [Header(Sphere)]
        _SphereColor("Color",Color)=(1,1,1,1)
        _SphereRadius("Raidus",float)=3
        [Header(Round Box)]
        _RoundBoxColor("Color",Color)=(1,1,1,1)
        [Vector3]_RoundBoxSize("Size",Vector)=(1,1,1,0)
        _RoundBoxRoundness("Roundness",Range(0,.5))=.1
        [Header(Frame Box)]
        [HDR]_FrameBoxColor("Color",Color)=(1,1,1,1)
        [Vector3]_FrameBoxSize("Size",Vector)=(1,1,1,0)
        _FrameBoxExtend("Extend",Range(0,.5))=.1
        [Header(Box)]
        [HDR]_BoxColor("Color",Color)=(1,1,1,1)
        [Vector3]_BoxSize("Size",Vector)=(.5,.5,.5,0)
    }
    SubShader
    {
        Tags { "RenderType"="UniversalForward" "Queue"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vertSDF
            #pragma fragment fragSDF
            #define CSG_DATA_EXTRA float3 color;
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            float3 _SphereColor;
            float _SphereRadius;
            float3 _RoundBoxColor;
            float3 _RoundBoxSize;
            float _RoundBoxRoundness;
            float3 _BoxColor;
            float3 _BoxSize;
            float3 _FrameBoxColor;
            float3 _FrameBoxSize;
            float _FrameBoxExtend;
            #define SceneSDF(xxx) SDF_SphereBox(xxx) 
            SDFSurface SDF_SphereBox(float3 position)
            {
                float3 origin=TransformObjectToWorld(0);
                GSphere sphere=GSphere_Ctor(origin,_SphereRadius);
                GBoxRound roundBox=GRoundBox_Ctor(origin,_RoundBoxSize,_RoundBoxRoundness);
                GBoxFrame frameBox=GFrameBox_Ctor(origin,_FrameBoxSize,_FrameBoxExtend);
                GBox box=GBox_Ctor(origin,_BoxSize);
                SDFSurface distA ;
                distA.distance = sphere.SDF(position);
                distA.color = _SphereColor;
                SDFSurface distB = SDFSurface_Ctor(roundBox.SDF(RotateAround(position,origin,_Time.y,float3(1,0,0))),_RoundBoxColor);
                SDFSurface distC = SDFSurface_Ctor(frameBox.SDF(RotateAround(position,origin,_Time.y,float3(0,1,0))),_FrameBoxColor);
                SDFSurface distD = SDFSurface_Ctor(box.SDF(RotateAround(position,origin,_Time.y,float3(0,0,1))),_BoxColor);
                return Union( Difference(distB,distA),distC,distD);
            }
            #include "Assets/Shaders/Library/Passes/GeometrySDFPass.hlsl"
            
            ENDHLSL
        }
    }
}
