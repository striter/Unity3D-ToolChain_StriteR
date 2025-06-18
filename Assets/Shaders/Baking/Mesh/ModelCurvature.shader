Shader "Hidden/Baking/Mesh/ModelCurvature"
{
    Properties
    {
        _CurvatureStrength("Curvature Strength", Range(0, 1)) = 0.01
        _CurvaturePower("Curvature Power", Range(0, 100)) = 1
    }
    SubShader
    {
        Pass
        {
            Cull Back
            Blend One Zero
            ZTest LEqual
            ZWrite On
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Assets/Shaders/Library/Common.hlsl"
            #include "../BakingInclude.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS:TEXCOORD1;
                float3 normalWS:TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float,_CurvatureStrength)
                INSTANCING_PROP(float,_CurvaturePower)
            INSTANCING_BUFFER_END
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);//TransformUVToPositionCS(v.uv);
				o.positionWS = TransformObjectToWorld(v.positionOS);
				o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float curvature = length(fwidth(normalize(i.normalWS))) / length(fwidth(i.positionWS));
                curvature = pow(saturate(curvature * _CurvatureStrength),_CurvaturePower);
                return float4(curvature,curvature,curvature,1);
            }
            ENDHLSL
        }
    }
}
