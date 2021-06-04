Shader "Game/Effects/Depth/VolumetricFog"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
        [KeywordEnum(Point,Spot)]_Type("Type",float)=0
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
        Tags{"Queue"="Transparent" "DisableBatching"="True" "IgnoreProjector" = "True"  }
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _TYPE_POINT _TYPE_SPOT

            #include "../../CommonInclude.hlsl"
            #include "../../GeometryInclude.hlsl"

            struct appdata
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
            v2f vert(appdata v)
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
                float density=.0h;
                float2 sdfDstOS=0.;
                GRay viewRayOS=GetRay(i.positionOS,viewDirOS);
                #if _TYPE_POINT
                sdfDstOS= SphereRayDistance(GetSphere(origin,.5) ,viewRayOS);
                half closestDst=PointRayProjection(viewRayOS,0);
                closestDst= length(0.-viewRayOS.GetPoint(closestDst))*2;
                density= saturate(1-closestDst);
                #elif _TYPE_SPOT
                GHeightCone cone=GetHeightCone( float3(.0,.5,.0),float3(.0,-1.,.0),55.,1);
                sdfDstOS =ConeRayDistance(cone,viewRayOS);
                density=1;
                #endif
                sdfDstOS.y=min(depthDstOS,sdfDstOS.y);
                float travelDst=saturate(sdfDstOS.y-sdfDstOS.x);
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
