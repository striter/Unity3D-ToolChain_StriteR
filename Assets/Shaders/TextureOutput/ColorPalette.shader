Shader "Hidden/TextureOutput/ColorPalette"
{
    Properties
    {
        [ColorUsage(false,false)]_A ("A", Color) = (0.5, 0.5, 0.5, 1)
        [ColorUsage(false,false)]_B ("B",Color) = (0.5,0.5,0.5,1)
        [ColorUsage(false,false)]_C ("C",Color) = (1,1,1,1)
        [ColorUsage(false,false)]_D ("D",Color) = (0,0.1,0.2,1)
    }
    SubShader
    {
        Pass
        {
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
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_A)
                INSTANCING_PROP(float4,_B)
                INSTANCING_PROP(float4,_C)
                INSTANCING_PROP(float4,_D)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float4 a = _A;
                float4 b = _B;
                float4 c = _C;
                float4 d = _D;
                #if !UNITY_COLORSPACE_GAMMA
                    a = LinearToGamma_Accurate(a);
                    b = LinearToGamma_Accurate(b);
                    c = LinearToGamma_Accurate(c);
                    d = LinearToGamma_Accurate(d);
                #endif
                float4 output = a + b * cos(kPI2*(c*i.uv.x+d));
                return float4(output.rgb,1);
            }
            ENDHLSL
        }
    }
}
