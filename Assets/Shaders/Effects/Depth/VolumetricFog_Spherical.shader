Shader "Game/Effects/Depth/VolumetricFog_Spherical"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
        _Density("Density",Range(0,5)) = 1
    }
    SubShader
    {
        Tags{"Queue"="Transparent" "DisableBatching"="True" "IgnoreProjector" = "True"  }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "../../CommonInclude.hlsl"
            #include "../../BoundingCollision.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS:TEXCOORD0;
                float3 viewDirOS:TEXCOORD1;
                float4 screenPos : TEXCOORD2;
            };

            
            TEXTURE2D(_CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Density;
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


            float4 frag(v2f i) : SV_Target
            {
                half3 viewDirOS = normalize(i.viewDirOS);
                half viewDstOS = BSRayDistance(0,.5,i.positionOS, viewDirOS).y;
                half depthDstWS = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r - i.screenPos.w;
                half depthDstOS =depthDstWS*unity_WorldToObject[0][0]+depthDstWS*unity_WorldToObject[0][1]+depthDstWS*unity_WorldToObject[0][2];
                half maxDstOS = max(0, min(depthDstOS, viewDstOS));
                half dstParam=maxDstOS*_Density;
                return float4( _Color*  dstParam);
            }
            ENDHLSL
        }
    }
}
