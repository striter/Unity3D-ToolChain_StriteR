Shader "Game/Effects/Geometry/Spike"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SpikeStrength("Spike Strength",Range(-1,1))=.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  }
        HLSLINCLUDE       
            #pragma target 4.0
            #pragma target 3.5
            #include "../../CommonInclude.hlsl"

            struct a2v
            {
                float4 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 uv:TEXCOORD0;
            };

            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float4 color:COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SpikeStrength;

            v2g vert (a2v v)
            {
                v2g o;
                o.positionOS = v.positionOS;
                o.normalOS=v.normalOS;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

             [maxvertexcount(9)]
            void geom(triangle v2g p[3],inout TriangleStream<g2f> stream)
            {
                g2f o;
                o.uv=0;
                float3 normal=normalize(p[0].normalOS+p[1].normalOS+p[2].normalOS);
                float4 baryCenter=TransformObjectToHClip((p[0].positionOS+p[1].positionOS+p[2].positionOS)/3+normal*_SpikeStrength);
                for(int i=0;i<3;i++)
                {
                    uint next=(i+1u)%3u;
                    o.positionCS=baryCenter;
                    o.color=1;
                    stream.Append(o);

                    o.positionCS=TransformObjectToHClip(p[next].positionOS);
                    o.color=0;
                    stream.Append(o);

                    o.positionCS= TransformObjectToHClip(p[i].positionOS);
                    o.color=0;
                    stream.Append(o);
                }
                stream.RestartStrip();
            }

            float4 frag (g2f i) : SV_Target
            {
                float4 col = tex2D(_MainTex, i.uv)*i.color;
                return col;
            }
        ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Tags{"LightMode" = "ShadowCaster"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            ENDHLSL
        }

    }
}
