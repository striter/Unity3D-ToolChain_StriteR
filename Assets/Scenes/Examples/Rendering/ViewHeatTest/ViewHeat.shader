Shader "Unlit/ViewHeatTest"
{
    Properties
    {
        [HDR]_Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float4 color:COLOR;
                float3 normalOS:NORMAL;
            };

            struct v2f
            {
                float4 positionHCS : SV_POSITION;
                float4 color:COLOR;
                float3 normalWS:NORMAL;
            };

            float4 _Color;
            
            v2f vert (a2v v)
            {
                v2f o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.color=v.color;
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float ndl=saturate(dot(normalize(i.normalWS),normalize(_MainLightPosition.xyz)))*.5+.5;
                return lerp(ndl,_Color,i.color.r);
            }
            ENDHLSL
        }
    }
}
