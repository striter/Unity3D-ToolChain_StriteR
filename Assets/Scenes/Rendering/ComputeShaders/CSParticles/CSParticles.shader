Shader "Hidden/CSParticles"
{
    Properties
    {
        _MainTex("_MainTex",2D)="white"{}
        _PointSize("Point Size",float) = 5
    }
    SubShader
    {
        Blend One One
        ZWrite Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            uniform float _PointSize;
            #include "CSParticles.hlsl"
            #include "Assets/Shaders/Library/Common.hlsl"

            StructuredBuffer<FParticle> _ParticleBuffer;
            
            struct a2v
            {
                uint vertex_id : SV_VertexID;
                uint instance_id : SV_InstanceID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float4 color :COLOR;
                float life: LIFE;
                float2 uv :TEXCOORD0;
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
                FParticle particle = _ParticleBuffer[ v.instance_id];
                float3 position = particle.position;
                float2 uv = 0;
                switch (v.vertex_id)
                {
                    case 0:
                        {
                            uv = float2(0,0);
                        }
                        break;
                    case 1:
                        {
                            uv = float2(0,1);
                        }
                        break;
                    case 2:
                        {
                            uv = float2(1,1);
                            
                        }
                        break;
                    case 3:
                        {
                            uv = float2(0,0);
                            
                        }
                        break;
                    case 4:
                        {
                            uv = float2(1,1);
                            
                        }
                        break;
                    case 5:
                        {
                            uv = float2(1,0);
                        }
                        break;
                }
                

                position.xy += uv * _PointSize;
                o.positionCS = TransformWorldToHClip(position);
                o.life = particle.life;
                float lerpVal = o.life*0.25;
                o.color = saturate(float4(1-lerpVal+.5,lerpVal+.5,1,lerpVal));
                o.uv = uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                return  SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).r * i.color;
            }
            ENDHLSL
        }
        
    }
}
