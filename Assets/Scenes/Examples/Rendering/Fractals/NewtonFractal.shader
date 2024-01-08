Shader "Game/Unfinished/NewtonFractal"
{
    Properties
    {
        _ST("Scale And Tilling",Vector) = (1,1,0,0)
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
                INSTANCING_PROP(float4,_ST)
            INSTANCING_BUFFER_END

            float2 Polynomial(float2 x)
            {
                // return  cpow(x,5) + cpow(x,2) - x + float2(1,0);
                 return cpow(x,3) - float2(1,0);
            }

            float2 Derivative(float2 x)
            {
                // return 5 * cpow(x, 4) + 2 * x - float2(1,0);
                return 3 * cpow(x,2);
            }

           float2 NewtonsFractal(float2 _startGuess,float _sqrApproximation = 0.0000001,int _maxIteration = 512 )
            {
                float2 guess = _startGuess;
                float2 value = Polynomial(guess);
                int iteration = 0;
                while (sqrDistance(value) > _sqrApproximation && iteration++<_maxIteration)
                {
                    guess -= cdiv(value ,Derivative(guess));
                    value = Polynomial(guess);
                }
                return guess;
            }

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
                float2 root2 = NewtonsFractal( TransformTex((i.uv-.5f)*2, _ST));
                return saturate(float4(root2.x, root2.y, -root2.y, 1.0));
            }
            ENDHLSL
        }
    }
}
