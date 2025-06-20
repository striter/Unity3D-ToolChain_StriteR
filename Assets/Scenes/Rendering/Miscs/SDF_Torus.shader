Shader "Hidden/SDF_Torus"
{
    Properties
    {
        [Header(Torus)]
        [HDR]_TorusColor("Color",Color)=(1,1,1,1)
        _TorusMajorRadius("Major Radius",Range(0,1))=0.5
        _TorusMinorRadius("Minor Radius",Range(0,0.2))=0.1
        [Header(TorusCapped)]
        [HDR]_TorusCappedColor("Color",Color)=(1,1,1,1)
        _TorusCappedMajorRadius("Major Radius",Range(0,1))=1
        _TorusCappedMinorRadius("Minor Radius",Range(0,.2))=0.1
        _TorusCappedSin("Cap Sin",Range(-1,1))=1
        _TorusCappedCos("Cap Cos",Range(-1,1))=0
        [Header(TorusLink)]
        [HDR]_TorusLinkColor("Color",Color)=(1,1,1,1)
        _TorusLinkMajorRadius("Major Radius",Range(0,1))=0.3
        _TorusLinkMinorRadius("MinorRadius",Range(0,0.2))=0.1
        _TorusLinkExtend("Extend",Range(0,0.5))=0.1
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
            float3 _TorusColor;
            float _TorusMajorRadius;
            float _TorusMinorRadius;
            float3 _TorusCappedColor;
            float _TorusCappedMajorRadius;
            float _TorusCappedMinorRadius;
            float _TorusCappedSin;
            float _TorusCappedCos;
            float3 _TorusLinkColor;
            float _TorusLinkMajorRadius;
            float _TorusLinkMinorRadius;
            float _TorusLinkExtend;
            #define SceneSDF(xxx) SDF_TorusLink(xxx) 
            SDFSurface SDF_TorusLink(float3 position)
            {
                float3 origin=TransformObjectToWorld(0);
                GTorus torus=GTorus_Ctor(origin,_TorusMajorRadius,_TorusMinorRadius);
                GTorusCapped torusCapped=GTorusCapped_Ctor(origin,_TorusCappedMajorRadius,_TorusCappedMinorRadius,float2(_TorusCappedSin,_TorusCappedCos));
                GTorusLink torusLink=GTorusLink_Ctor(origin,_TorusLinkMajorRadius,_TorusLinkMinorRadius,_TorusLinkExtend);
                
                float3 samplePos=RotateAround(position,origin,_Time.y,float3(1,0,0) );
                SDFSurface distA=SDFSurface_Ctor(torus.SDF(samplePos),_TorusColor);
                SDFSurface distB=SDFSurface_Ctor(torusCapped.SDF( samplePos),_TorusCappedColor);
                SDFSurface distC=SDFSurface_Ctor(torusLink.SDF(RotateAround(position,origin,_Time.y,float3(0,1,0) )) ,_TorusLinkColor);

                SDFSurface distAB = UnionSmin(distA,distB,.2,5);
                return Union(distAB,distC);
            }
            #include "Assets/Shaders/Library/Passes/GeometrySDFPass.hlsl"
            ENDHLSL
        }
    }
}
