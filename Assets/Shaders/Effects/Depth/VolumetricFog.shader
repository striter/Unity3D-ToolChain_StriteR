Shader "Game/Effects/Depth/VolumetricFog"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
        [KeywordEnum(Point,Cube,Spot)]_Type("Type",float)=0
        _Density("Density",Range(0,10)) = 1
        _Pow("Density Pow",Range(0,10))=2
        [Header(Depth)]
        _Depth("Depth Sensitivity",Range(0,1))=.5
        [Header(Misc)]
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",float)=1
    }
    SubShader
    {
        Tags{"Queue"="Transparent" }
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _TYPE_POINT _TYPE_CUBE _TYPE_SPOT

            #define IGeometryDetection
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"

            struct a2v
            {
                half3 positionOS : POSITION;
            };

            struct v2f
            {
                half4 positionCS : SV_POSITION;
                half3 positionOS:TEXCOORD0;
                half4 screenPos : TEXCOORD2;
            };
            
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Density;
            half _Pow;
            half _Depth;
            CBUFFER_END
            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionOS = v.positionOS;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                half3 viewDirOS=TransformWorldToObjectNormal(TransformObjectToWorld(i.positionOS)-GetCameraPositionWS());
                half depthDstWS = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r - i.screenPos.w;
                half depthDstOS =length(mul((float3x3)unity_WorldToObject,float3(0,depthDstWS,0)));
                half3 origin=0.h;
                half density=1.h;
                float2 distances=0.;
                GRay viewRayOS=GRay_Ctor(i.positionOS,viewDirOS);
                #if _TYPE_POINT
                //radius =.5 inv = 2
                distances= SphereRayDistance(GSphere_Ctor(origin,.5) ,viewRayOS);
                half3 closestPoint=viewRayOS.GetPoint(PointRayProjection(viewRayOS,origin));
                half originDistance= length(origin-closestPoint);
                density= 1;//saturate(1-originDistance*2);
                #elif _TYPE_CUBE
                distances=AABBRayDistance( GBox_Ctor(origin,1.h),viewRayOS);
                distances.y+=distances.x;

                half distance0=saturate(viewRayOS.GetPoint(distances.x).z+.5);
                half distance1=saturate(viewRayOS.GetPoint(distances.y).z+.5);
                half bottomDistance = min(distance0,distance1);
                density*=(1-bottomDistance);
                #elif _TYPE_SPOT
                GHeightCone cone= GHeightCone_Ctor( float3(.0,.5,.0),float3(.0,-1.,.0),55.,1);
                distances =ConeRayDistance(cone,viewRayOS);
                #endif
                distances.y=min(depthDstOS,distances.y);
                float travelDst=saturate(distances.y-distances.x);
                density*=saturate( smoothstep(0,_Depth,travelDst));
                density=pow(density,_Pow);
                density*=_Density;
                density=saturate(density);
                return half4( _Color*  density);
            }
            ENDHLSL
        }
    }
}
