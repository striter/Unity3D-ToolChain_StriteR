Shader "Game/Surface/AlphaBlend"
{
    Properties
    {
        _MainTex("Texture",2D) = "black"{}
        _Color("Color",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags{"Queue" = "Transparent"}
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma multi_compile_instancing
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

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);

                float4 texSample=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv);
                float3 albedo=texSample.rgb;
                float alpha=texSample.a;
                float4 color = INSTANCE(_Color);
                albedo*= color.rgb;
                alpha*=color.a;
                return float4(albedo.rgb,saturate(alpha));
            }
            ENDHLSL
        }
    }
}
