Shader "Hidden/SDF_SphereBox"
{
    Properties
    {
        [Header(Capsule)]
        [HDR]_CapsuleColor("Color",Color)=(1,1,1,1)
        _CapsuleHeight("Height",Range(0,1))=1
        _CapsuleRadius("Raidus",Range(0,1))=0.5
        [Header(Cylinder)]
        [HDR]_CylinderColor("Color",Color)=(1,1,1,1)
        _CylinderRadius("Radius",Range(0,1))=1
        [Header(CappedCylinder)]
        [HDR]_CylinderCappedColor("Color",Color)=(1,1,1,1)
        _CylinderCappedRadius("Radius",Range(0,1))=1
        _CylinderCappedHeight("Height",Range(0,1))=0.5
        [Header(CylinderRound)]
        [HDR]_CylinderRoundColor("Color",Color)=(1,1,1,1)
        _CylinderRoundRadius("Radius",Range(0,1))=0.3
        _CylinderRoundHeight("Height",Range(0,1))=0.5
        _CylinderRoundRoundness("Round",Range(0,0.5))=0.1
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
            #define IGeometrySDF
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            float _CylinderRadius;
            float3 _CylinderColor;
            float _CapsuleHeight;
            float _CapsuleRadius;
            float3 _CapsuleColor;
            float _CylinderCappedRadius;
            float _CylinderCappedHeight;
            float3 _CylinderCappedColor;
            float _CylinderRoundRadius;
            float _CylinderRoundHeight;
            float _CylinderRoundRoundness;
            float3 _CylinderRoundColor;
            #define SceneSDF(xxx) SDF_TorusLink(xxx) 
            SDFSurface SDF_TorusLink(float3 position)
            {
                float3 origin = TransformObjectToWorld(0);
                GCylinder cylinder = GCylinder_Ctor(origin,_CylinderRadius);
                GCapsule capsule = GCapsule_Ctor(origin,_CapsuleRadius,float3(0,1,0),_CapsuleHeight);
                GCylinderCapped cylinderCapped = GCylinderCapped_Ctor(origin,_CylinderCappedRadius,_CylinderCappedHeight);
                GCylinderRound cylinderRound = GCylinderRound_Ctor(origin,_CylinderRoundRadius,_CylinderRoundHeight,_CylinderRoundRoundness);
                float3 samplePosition = RotateAround(position,origin,_Time.y,float3(1,0,0) );
                SDFSurface distA = SDFSurface_Ctor(cylinder.SDF(samplePosition) ,_CylinderColor);
                SDFSurface distB = SDFSurface_Ctor(capsule.SDF(samplePosition) , _CapsuleColor);

                samplePosition = RotateAround(position,origin,_Time.y,float3(0,0,1));
                SDFSurface distC = SDFSurface_Ctor(cylinderCapped.SDF(samplePosition) , _CylinderCappedColor);
                SDFSurface distD = SDFSurface_Ctor(cylinderRound.SDF(samplePosition) , _CylinderRoundColor);
                SDFSurface output = Union( Difference( distB,distA),Difference( distD,distC));
                return Difference(output,distC);
            }
            #include "Assets/Shaders/Library/Passes/GeometrySDFPass.hlsl"
            ENDHLSL
        }
    }
}
