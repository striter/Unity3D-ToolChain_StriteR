Shader "Game/Effects/Depth/LightVolume"
{
    Properties
    {
        [HDR]_Color("Color",Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags{"Queue"="Transparent"}
        Pass
        {
            Tags{"LightMode" = "LightVolume"}
            Blend One One
            ZWrite Off
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
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
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
            INSTANCING_BUFFER_END
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
                float rawDepth=SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,sampler_CameraDepthTexture,screenUV).r;
                float3 positionOS = TransformWorldToObject( TransformNDCToWorld(screenUV,rawDepth));
                float dst = length(positionOS);
                float pointLightStrength = (pow3(1-dst*2))*step(dst,.5);
                return float4(pointLightStrength * _Color.rgb*_Color.a,1);
            }
            ENDHLSL
        }
    }
}
