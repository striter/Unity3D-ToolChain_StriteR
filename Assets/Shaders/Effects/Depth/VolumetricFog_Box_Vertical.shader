Shader "Game/Effects/Depth/VolumetricFog_Box_Vertical"
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
            
            TEXTURE2D( _CameraDepthTexture); SAMPLER(sampler_CameraDepthTexture);
            CBUFFER_START(UnityPerMaterial)
            float4 _Color;
            float _Density;
            CBUFFER_END
            struct appdata
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionOS:TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 viewDirOS:TEXCOORD2;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionOS = v.positionOS;
                o.viewDirOS = TransformWorldToObjectNormal( TransformObjectToWorld( v.positionOS)-GetCameraPositionWS());
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                half3 viewDirOS = normalize(i.viewDirOS);
                half viewDstOS = AABBRayDistance(-.5,.5,i.positionOS, viewDirOS).y;
                half depthDstWS = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture, i.screenPos.xy/i.screenPos.w),_ZBufferParams).r - i.screenPos.w;
                half depthDstOS = length(mul((float3x3)unity_WorldToObject, float3(0, depthDstWS, 0)));
                half maxDst = min(depthDstOS, viewDstOS);

                half3 maxViewPos = i.positionOS + viewDirOS * maxDst;
                half yParam = min(i.positionOS.y, maxViewPos.y);
                
                half dstParam = saturate( smoothstep(.5,-.5, yParam)*_Density);
                
                return _Color*  dstParam;
            }
            ENDHLSL
        }
    }
}
