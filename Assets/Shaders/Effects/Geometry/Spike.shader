Shader "Game/Effects/Geometry/Spike"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SpikeStrength("Spike Strength",Range(-1,1))=.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGINCLUDE       
            #pragma target 4.0
            #include "UnityCG.cginc"

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2g
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
                float2 uv:TEXCOORD0;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SpikeStrength;

            v2g vert (a2v v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal=v.normal;
                return o;
            }

             [maxvertexcount(9)]
            void geom(triangle v2g p[3],inout TriangleStream<g2f> stream)
            {
                g2f o;
                o.uv=0;
                float3 normal=normalize(p[0].normal+p[1].normal+p[2].normal);
                float4 baryCenter=UnityObjectToClipPos((p[0].vertex+p[1].vertex+p[2].vertex)/3+normal*_SpikeStrength);
                for(int i=0;i<3;i++)
                {
                    uint next=(i+1)%3;
                    o.vertex=baryCenter;
                    o.color=1;
                    stream.Append(o);

                    o.vertex=UnityObjectToClipPos(p[next].vertex);
                    o.color=0;
                    stream.Append(o);

                    o.vertex= UnityObjectToClipPos(p[i].vertex);
                    o.color=0;
                    stream.Append(o);
                }
                stream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv)*i.color;
                return col;
            }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            ENDCG
        }
    }
}
