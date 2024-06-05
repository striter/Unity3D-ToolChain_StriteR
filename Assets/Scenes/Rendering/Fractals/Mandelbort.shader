Shader "Hidden/Mandelbort"
{
    Properties
    {
        [NoScaleOffset]_MainTex("MainTex",2D) = "white"
        _ST("Scale And Tilling",Vector)=(1,1,0,0)
        _Param("Parameter",Int)=2
    }
    SubShader
    {
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_ST)
                INSTANCING_PROP(int,_Param)
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
            
            float mandelbrot(float2 uv)
            {
                float2 c = uv;
                float2 z = c;

                float maxIterations = 100;
                int i=1;
                while (i<maxIterations)
                {
                    i =i+1;
                    z = cpow(z,_Param) + c;

                    if (length(z) > 16.0)
                        return i / maxIterations;
                }

                return 0;
            }
            
            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                
                float2 uv = TransformTex((i.uv - .5) * 2,_ST);
                float t = mandelbrot(uv);
                return float4(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,t).rgb,t);
            }
            ENDHLSL
        }
    }
}