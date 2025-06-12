Shader "Hidden/TextureOutput/PreIntergratedSkinSSSLUT"
{
    Properties
    {
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
            
            float Gaussian(float v,float r)
            {
                return 1.0/sqrt(2.0 * PI * v) * exp(-(r*r)/(2.0*v));
            }

            float3 DiffusionProfile(float r)
            {
                return float3(0.0, 0.0, 0.0)
                    + Gaussian(0.0064, r) * float3(0.233, 0.455, 0.649)
                    + Gaussian(0.0484, r) * float3(0.100, 0.336, 0.344)
                    + Gaussian(0.187, r) * float3(0.118, 0.198, 0.0)
                    + Gaussian(0.567, r) * float3(0.113, 0.007, 0.007)
                    + Gaussian(1.99, r) * float3(0.358, 0.004, 0.0)
                    + Gaussian(7.41, r) * float3(0.233, 0.0, 0.0);
            }

            // ring integrate 
            float3 BakeSkinLUT(float2 uv)
            {
                float NoL = uv.x;
                float INV_R = uv.y;
                float theta = acos(NoL * 2.0 - 1.0);
                float R = 1.0 / INV_R;
                
                float3 Integral = float3(0,0,0);
	            float3 NormalizationFactor = float3(0,0,0);
                
                for (float x = -PI; x < PI; x += PI * 0.001)
                {
                    //R(2r*sin(2/x))
                    float dis = 2.0 * R * sin(x * 0.5);
                    
                    //saturate(cos(θ + x)) * R(dis)
                    Integral += saturate(cos(x + theta)) * DiffusionProfile(dis);
                    
                    NormalizationFactor += DiffusionProfile(dis);
                }
                
                return Integral / NormalizationFactor;
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
                float2 uv = i.uv;
                return float4(LinearToGamma_Accurate(BakeSkinLUT(uv)),1);
            }
            ENDHLSL
        }
    }
}
