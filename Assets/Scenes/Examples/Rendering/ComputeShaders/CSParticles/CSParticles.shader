Shader "Hidden/CSParticles"
{
    Properties
    {
        _PointSize("Point Size",float) = 5
    }
    SubShader
    {
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
                float size : PSIZE;
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
                FParticle particle = _ParticleBuffer[v.instance_id];
                o.positionCS = TransformWorldToHClip(particle.position);
                o.life = particle.life;
                float lerpVal = o.life*0.25;
                o.color = float4(1-lerpVal+.5,lerpVal+.5,1,lerpVal);
                o.size = _PointSize;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                return i.color;
            }
            ENDHLSL
        }
        
    }
}
