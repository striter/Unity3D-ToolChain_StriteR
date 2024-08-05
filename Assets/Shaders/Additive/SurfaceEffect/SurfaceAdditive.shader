Shader "Game/Surface/Additive"
{
    Properties
    {
        _AdditiveTexture("Additive Texture",2D) = "black"{}
        [HDR]_AdditiveColor("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags{"Queue" = "Transparent"}
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest Equal
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_AdditiveTexture);SAMPLER(sampler_AdditiveTexture);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_AdditiveColor)
                INSTANCING_PROP(float4,_AdditiveTexture_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_AdditiveTexture);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float4 colorSample = SAMPLE_TEXTURE2D(_AdditiveTexture, sampler_AdditiveTexture, i.uv) * INSTANCE(_AdditiveColor);
                return float4(colorSample.rgb * colorSample.a,1);
            }
            ENDHLSL
        }
    }
}
