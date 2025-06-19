Shader "Hidden/Baking/Gradient/BeckmannBRDFLUT"
{
    Properties
    {
    }
    SubShader
    {
        Tags{"PreviewType" = "Plane"}
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "../BakingInclude.hlsl"

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
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;
                return o;
            }

            //&https://developer.nvidia.com/gpugems/gpugems3/part-iii-rendering/chapter-14-advanced-techniques-realistic-real-time-skin
            float PHBeckmann(float ndoth, float m)
            {
                float alpha = acos(ndoth);
                float ta = tan(alpha);
                float val = 1.0 / (m * m * pow(ndoth, 4.0)) * exp(-(ta * ta) / (m * m));
                return val;
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float2 uv = i.uv;
                return 0.5 * pow(PHBeckmann(uv.x, uv.y), 0.1);
            }
            ENDHLSL
        }
    }
}
