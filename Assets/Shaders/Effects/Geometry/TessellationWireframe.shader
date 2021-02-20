Shader "Game/Effects/Geometry/TessellationWireframe"
{
    Properties
    {
        _TessellationAmount("Tessellation Amount",Range(1,64))=2
        _WireframeWidth("Wireframe Width",Range(0.0001,0.1))=0.01
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma target 4.6
            #pragma vertex vert
            #pragma hull hullProgram
            #pragma domain domainProgram
            #pragma geometry geomProgram
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float3 tangent:TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2t
            {
                float4 vertex:INTERNALTESSPOS;
                float3 normal:NORMAL;
                float3 tangent:TANGENT;
                float2 uv:TEXCOORD0;
            };

            struct t2g
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float3 tangent:TANGENT;
                float2 uv : TEXCOORD0;
            };


            struct g2f
            {
                float4 vertex : SV_POSITION;
                float4 color:COLOR;
            };

            struct tData
            {
                float edge[3]:SV_TESSFACTOR;
                float inside:SV_INSIDETESSFACTOR;
            };

            float _TessellationAmount;
            float _WireframeWidth;

            v2t vert (a2v v)
            {
                v2t o;
                o=v;
                return o;
            }
            
            [UNITY_domain("tri")]
            [UNITY_outputcontrolpoints(3)]
            [UNITY_outputtopology("triangle_cw")]
            [UNITY_partitioning("fractional_odd")]
            [UNITY_patchconstantfunc("patchConstant")]
            v2t hullProgram(InputPatch<v2t,3> patch,uint id:SV_OUTPUTCONTROLPOINTID)
            {
                return patch[id];
            }

            tData patchConstant(InputPatch<v2t,3> patch)
            {
                tData f;
                f.edge[0]=_TessellationAmount;
                f.edge[1]=_TessellationAmount;
                f.edge[2]=_TessellationAmount;
                f.inside=_TessellationAmount;
                return f;
            }
            
            #define DOMAIN_DATA_INTERPOLATE(output,field) output.field=patch[0].field*barycentricCoordinates.x+patch[1].field*barycentricCoordinates.y+patch[2].field*barycentricCoordinates.z;

            [UNITY_domain("tri")]
            t2g domainProgram(tData data,OutputPatch<v2t,3> patch,float3 barycentricCoordinates:SV_DOMAINLOCATION )
            {
                v2t v;
                DOMAIN_DATA_INTERPOLATE(v,vertex);
                DOMAIN_DATA_INTERPOLATE(v,normal);
                DOMAIN_DATA_INTERPOLATE(v,tangent);
                DOMAIN_DATA_INTERPOLATE(v,uv);

                t2g o;
                o.vertex=v.vertex;
                o.normal=v.normal;
                o.tangent=v.tangent;
                o.uv=v.uv;
                return o;
            }

            void Append(inout TriangleStream<g2f> stream,float4 vertex,float4 color)
            {
                g2f o;
                o.vertex=UnityObjectToClipPos(vertex);
                o.color=color;
                stream.Append(o);
            }

            void AppendWireFrame(inout TriangleStream<g2f> stream ,float4 vertex1,float4 vertex2,float3 vertex3,float3 normal)
            {
                Append(stream,vertex1+float4(normal,0),0);
                Append(stream,vertex2+float4(normal,0),0);
                
                float3 direction;
                
                direction=normalize(vertex3-vertex1)*_WireframeWidth;
                Append(stream,vertex1+float4(direction+normal,0),0);
                direction=normalize(vertex3-vertex2)*_WireframeWidth;
                Append(stream,vertex2+float4(direction+normal ,0),0);

                stream.RestartStrip();
            }

            [maxvertexcount(15)]
            void geomProgram(triangle t2g p[3],inout TriangleStream<g2f> stream)
            {
                for(int i=0;i<3;i++)
                {
                    Append(stream,p[i].vertex,1);
                }
                stream.RestartStrip();
                

                float3 normal=p[0].normal+p[1].normal+p[2].normal;
                normal/=3;
                normal*=0.0001;
                
                AppendWireFrame(stream,p[0].vertex,p[1].vertex,p[2].vertex,normal);
                AppendWireFrame(stream,p[1].vertex,p[2].vertex,p[0].vertex,normal);
                AppendWireFrame(stream,p[2].vertex,p[0].vertex,p[1].vertex,normal);
            }

            fixed4 frag (g2f i) : SV_Target
            {
                return i.color;
            }
            ENDCG
        }
    }
}
