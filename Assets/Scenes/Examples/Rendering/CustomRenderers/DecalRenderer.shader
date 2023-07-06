Shader "Game/Unfinished/Decals"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags {"Queue" = "Geometry+1"}
        ZWrite Off
        Blend One One
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float4 color : COLOR;
                float3 normal :NORMAL;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color :COLOR;
                float2 uv : TEXCOORD0;
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
                o.positionCS = TransformObjectToHClip(v.positionOS + v.normal * 0.0001);
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb * INSTANCE(_Color).rgb * i.color.a;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
    }
}
