Shader "Hidden/MotionVectorObject"
{
    SubShader
    {
        ZWrite On
        Pass
        {
            Tags{ "LightMode" = "MotionVectors" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Assets/Shaders/Library/PostProcess.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 positionHCS : TEXCOORD0;
                float4 prePositionHCS : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                
                o.positionCS = TransformObjectToHClip(v.positionOS);
                
                #if defined(UNITY_REVERSED_Z)
                    o.positionCS.z -= unity_MotionVectorsParams.z * o.positionCS.w;
                #else
                    o.positionCS.z += unity_MotionVectorsParams.z * o.positionCS.w;
                #endif
                o.positionHCS = o.positionCS;
                o.prePositionHCS = mul(_Matrix_VP_Pre,mul(UNITY_PREV_MATRIX_M,float4(v.positionOS,1)));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float4 prePositionCS = i.prePositionHCS;
                float4 curPositionCS = i.positionHCS;
                float2 preUV = TransformHClipToNDC(prePositionCS);
                float2 curUV = TransformHClipToNDC(curPositionCS);
                float2 delta = curUV - preUV;
                return float4(delta,0,0);
            }
            ENDHLSL
        }
    }
}
