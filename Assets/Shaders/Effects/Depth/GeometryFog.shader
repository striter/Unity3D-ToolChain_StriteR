Shader "Game/Effects/Depth/GeometryFog"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
        [KeywordEnum(Point,Cube,Spot)]_Type("Type",float)=0
        _Density("Density",Range(0,10)) = 1
        _Pow("Density Pow",Range(0,10))=2
    }
    SubShader
    {
        Tags{"Queue"="Transparent" }
        Pass
        {
            Blend One One
            ZWrite Off
            Cull Front
            ZTest Always
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local_fragment _TYPE_POINT _TYPE_CUBE _TYPE_SPOT

            #define IGeometryDetection
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Geometry.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                half3 positionOS : POSITION;
            };

            struct v2f
            {
                half4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD1;
                float4 positionHCS:TEXCOORD2;
            };
            
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                half _Density;
                half _Pow;
            CBUFFER_END
            v2f vert(a2v v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                o.positionHCS =o.positionCS;
                return o;
            }
            
            half4 frag(v2f i) : SV_Target
            {
                float2 screenUV = TransformHClipToNDC(i.positionHCS);
                half3 directionWS = GetCameraRealDirectionWS(i.positionWS);
                float3 cameraPosWS = GetCameraRealPositionWS(screenUV);

                half3 directionOS = TransformWorldToObjectDir(directionWS);
                half3 origin=0.h;
                half density=1.h;
                float2 distances=0.;
                GRay viewRayOS=GRay_Ctor(TransformWorldToObject(cameraPosWS),directionOS);
                #if _TYPE_POINT
                    //radius =.5 inv = 2
                    distances= Distance(GSphere_Ctor(origin,.5) ,viewRayOS);
                #elif _TYPE_CUBE
                    distances=Distance( GBox_Ctor(origin,1.h),viewRayOS);
                    distances.y+=distances.x;
                #elif _TYPE_SPOT
                    GHeightCone cone= GHeightCone_Ctor( float3(.0,.5,.0),float3(.0,-1.,.0),55.,1);
                    distances =Distance(cone,viewRayOS);
                #endif
                
                // return TransformWorldToEyeDepth(world,UNITY_MATRIX_V)/10;
                float rawDepth=SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
                float depthDstWS = RawToDistance(rawDepth,screenUV);
                float depthDstOS = length( TransformWorldToObjectDir (directionWS*depthDstWS,false));
                distances.y=min(depthDstOS,distances.y);
                
                float travelDst=saturate(distances.y-distances.x);
                density*=travelDst;

                #if _TYPE_POINT
                    half closestDistance= Projection(viewRayOS,origin);
                    closestDistance=min(closestDistance,distances.y);
                    half originDistance= length(origin-viewRayOS.GetPoint(closestDistance));
                    density *= saturate(.5-originDistance);
                #elif _TYPE_CUBE
                    half distance0=saturate(viewRayOS.GetPoint(distances.x).z+.5);
                    half distance1=saturate(viewRayOS.GetPoint(distances.y).z+.5);
                    half bottomDistance = min(distance0,distance1);
                    density*=(1-bottomDistance);
                #endif
                
                density=saturate(density);
                density=pow(density,_Pow);
                density*=_Density;

                float3 lightCol = _Color.rgb*_Color.a;
                return float4(lightCol*density,1);
            }
            ENDHLSL
        }
    }
}
