Shader "Game/Effects/Depth/VolumetricFog_Spherical"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
        _Density("Density",Range(0,10)) = 1
        _Pow("Density Pow",Range(0,10))=2
        _Depth("Depth Sensitivity",Range(0,1))=.5
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "DisableBatching"="True" "IgnoreProjector" = "True"  }
        Pass
        {
            Blend One One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../CommonInclude.hlsl"
            #include "../../BoundingCollision.hlsl"

            struct appdata
            {
                half3 positionOS : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                half4 positionCS : SV_POSITION;
                half3 positionOS:TEXCOORD0;
                half3 viewDirOS:TEXCOORD1;
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
                o.viewDirOS = TransformWorldToObjectNormal(TransformObjectToWorld(v.positionOS)-GetCameraPositionWS());
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half3 viewDirOS = normalize(i.viewDirOS);
                half viewDstOS = BSRayDistance(0.0h,.5h,i.positionOS, viewDirOS).y;
                half depthDstWS = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r - i.screenPos.w;
                half depthDstOS =length(mul(unity_WorldToObject,float3(0,depthDstWS,0)));
                half travelDst = max(0, min(depthDstOS, viewDstOS));
                half closestDst=dot(-i.positionOS ,viewDirOS);
                closestDst=length(i.positionOS+i.viewDirOS*closestDst)*2;

                half dstParam= 1-closestDst;
                dstParam*=saturate( smoothstep(0,_Depth,travelDst));
                dstParam=pow(dstParam,_Pow);
                dstParam*=_Density;
                dstParam=saturate(dstParam);
                return half4( _Color*  dstParam);
            }
            ENDHLSL
        }
    }
}
