Shader "Game/Effects/Geometry/Disintegrate"
{
    Properties
    {
        _DisintegrateAmount("Disintegrate Amount",Range(0,1))=1

        _MainTex("Main Tex",2D)="white"{}

        [Header(Dissolve)]
        _DissolveTex("Dissolve Tex",2D)="white"{}
        _DissolveEdgeWidth("Dissolve Edge Width",Range(0,1))=0.1
        _DissolveEdgeColor("Dissolve Edge Color",Color)=(1,1,1,1)

        [Header(Shape)]
        _ParticleShape("Particle Shape",2D)="white"{}
        _ParticleShapeClip("Particle Shape Clip",Range(0,1))=.1
        _ParticleColor("Particle Color",Color)=(1,1,1,1)
        _ParticleSize("Particle Size",Range(0,1))=.1

        [Header(Flow)]
        _FlowTex("Flow Tex",2D)="grey"{}
        _FlowDirection("Flow Direction",Vector)=(0,1,0,0)
        _FlowExpand("Flow Expand",Range(0,5))=.2
    }
    SubShader
    {
        Tags { "RenderType"="Disintegrate" "Queue"="Transparent" }
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
            #include "UnityCG.cginc"
            float _DisintegrateAmount;
            sampler2D _DissolveTex;
            float4 _DissolveTex_ST;
            float _DissolveEdgeWidth;

            float GetDissolve(float2 uv) { return tex2D(_DissolveTex,uv).r-_DisintegrateAmount*2+_DissolveEdgeWidth; }
        ENDCG

        Pass
        {
            Name "Main"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            struct a2v
            {
                float4 vertex:POSITIOn;
                float2 uv:TEXCOORD0;
            };
            struct v2f
            {
                float4 vertex:SV_POSITION;
                float4 uv:TEXCOORD0;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert(a2v v)
            {
                v2f o;
                o.vertex=UnityObjectToClipPos(v.vertex);
                o.uv=float4( TRANSFORM_TEX(v.uv,_MainTex),TRANSFORM_TEX(v.uv,_DissolveTex));
                return o;
            }

            float4 frag(v2f i):SV_TARGET
            {
                clip(GetDissolve(i.uv.zw)-0.001);
                return tex2D(_MainTex,i.uv.xy);
            }
            
            ENDCG
        }

        Pass
        {   
            Name "DISINTEGRATE"
            CGPROGRAM
            #pragma target 4.0
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

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
                float2 uv : TEXCOORD0;
                uint particleMask:TEXCOORD1;
            };
            
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            float4 _DissolveEdgeColor;

            float4 _ParticleColor;
            sampler2D _ParticleShape;
            float4 _ParticleShape_ST;
            float _ParticleShapeClip;
            float _ParticleSize;

            sampler2D _FlowTex;
            float4 _FlowTex_ST;
            float3 _FlowDirection;
            float _FlowExpand;

            v2g vert (a2v v)
            {
                v2g o;
                o.vertex = v.vertex;
                o.normal=v.normal;
                o.uv = v.uv;
                return o;
            }
            
            float3 remapFlowVector(float3 rgb){rgb.rg=rgb.rg*2-1; return float3(rgb.r,0,rgb.g);}
            [maxvertexcount(7)]
            void geom(triangle v2g p[3],inout TriangleStream<g2f> stream)
            {
                float3 avgPos=(p[0].vertex+p[1].vertex+p[2].vertex)/3;
                float2 avgUV=(p[0].uv+p[1].uv+p[2].uv)/3;
                float3 flowVector=remapFlowVector(tex2Dlod(_FlowTex,float4( TRANSFORM_TEX(avgPos.xz,_FlowTex),0,0)));
                float disintegrate=tex2Dlod(_DissolveTex,float4(TRANSFORM_TEX(avgUV,_DissolveTex),0,0)).r;
                disintegrate=saturate(_DisintegrateAmount*2-disintegrate);
                
                g2f o;
                o.uv=0;
                o.particleMask=0;
                if(disintegrate>0)
                {   
                    float3 targetPos= avgPos+(_FlowDirection+flowVector)*_FlowExpand*disintegrate;
                    float size=_ParticleSize*(1-disintegrate);

                    float halfSize=size/2;
                    float3 right= UNITY_MATRIX_IT_MV[0].xyz*halfSize;
                    float3 up=UNITY_MATRIX_IT_MV[1].xyz*halfSize;

                    o.vertex=UnityObjectToClipPos(float4(targetPos+right-up,1));
                    o.uv=TRANSFORM_TEX(float2(1,0),_ParticleShape);
                    stream.Append(o);
                    o.vertex=UnityObjectToClipPos(float4(targetPos+right+up,1));
                    o.uv=TRANSFORM_TEX(float2(1,1),_ParticleShape);
                    stream.Append(o);
                    o.vertex=UnityObjectToClipPos(float4(targetPos-right-up,1));
                    o.uv=TRANSFORM_TEX(float2(0,0),_ParticleShape);
                    stream.Append(o);
                    o.vertex=UnityObjectToClipPos(float4(targetPos-right+up,1));
                    o.uv=TRANSFORM_TEX(float2(0,1),_ParticleShape);
                    stream.Append(o);
                }
                
                stream.RestartStrip();
                o.particleMask=1;
                for(int i=0;i<3;i++)
                {
                    o.vertex=UnityObjectToClipPos(p[i].vertex);
                    o.uv=TRANSFORM_TEX(p[i].uv,_DissolveTex);
                    stream.Append(o);
                }
                stream.RestartStrip();
            }

            fixed4 frag (g2f i) : SV_Target
            {
                if(i.particleMask==1)
                {
                    float dissolve=GetDissolve(i.uv);
                    float dissolveEdge=step(dissolve,_DissolveEdgeWidth);
                    clip(dissolve*dissolveEdge-0.001);
                    return _DissolveEdgeColor;
                }
                clip(tex2D(_ParticleShape,i.uv).r-_ParticleShapeClip-0.01);
                return _ParticleColor;
            }
            ENDCG
        }
        
    }
}
