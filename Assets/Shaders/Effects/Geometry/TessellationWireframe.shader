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
            HLSLPROGRAM
            #pragma target 4.6
            #pragma vertex vert
            #pragma hull hullProgram
            #pragma domain domainProgram
            #pragma geometry geomProgram
            #pragma fragment frag

            #include "../../CommonInclude.hlsl"

            struct a2v
            {
                float4 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float3 tangentOS:TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2t
            {
                float4 positionOS:INTERNALTESSPOS;
                float3 normalOS:NORMAL;
                float3 tangentOS:TANGENT;
                float2 uv:TEXCOORD0;
            };

            struct t2g
            {
                float4 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float3 tangentOS:TANGENT;
                float2 uv : TEXCOORD0;
            };


            struct g2f
            {
                float4 positionCS : SV_POSITION;
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
            
            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [partitioning("fractional_odd")]
            [patchconstantfunc("patchConstant")]
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

            [domain("tri")]
            t2g domainProgram(tData data,OutputPatch<v2t,3> patch,float3 barycentricCoordinates:SV_DOMAINLOCATION )
            {
                v2t v;
                DOMAIN_DATA_INTERPOLATE(v,positionOS);
                DOMAIN_DATA_INTERPOLATE(v,normalOS);
                DOMAIN_DATA_INTERPOLATE(v,tangentOS);
                DOMAIN_DATA_INTERPOLATE(v,uv);

                t2g o;
                o.positionOS=v.positionOS;
                o.normalOS=v.normalOS;
                o.tangentOS=v.tangentOS;
                o.uv=v.uv;
                return o;
            }

            void Append(inout TriangleStream<g2f> stream,float3 vertex,float4 color)
            {
                g2f o;
                o.positionCS=TransformObjectToHClip(vertex);
                o.color=color;
                stream.Append(o);
            }

            void AppendWireFrame(inout TriangleStream<g2f> stream ,float3 vertex1,float3 vertex2,float3 vertex3,float3 normal)
            {
                Append(stream,vertex1+normal,0);
                Append(stream,vertex2+normal,0);
                
                float3 direction;
                
                direction=normalize(vertex3-vertex1)*_WireframeWidth;
                Append(stream,vertex1+direction,0);
                direction=normalize(vertex3-vertex2)*_WireframeWidth;
                Append(stream,vertex2+direction+normal,0);

                stream.RestartStrip();
            }

            [maxvertexcount(15)]
            void geomProgram(triangle t2g p[3],inout TriangleStream<g2f> stream)
            {
                for(int i=0;i<3;i++)
                {
                    Append(stream,p[i].positionOS.xyz,1);
                }
                stream.RestartStrip();
                

                float3 normal=p[0].normalOS+p[1].normalOS+p[2].normalOS;
                normal/=3;
                normal*=0.0001;
                
                AppendWireFrame(stream,p[0].positionOS.xyz,p[1].positionOS.xyz,p[2].positionOS.xyz,normal);
                AppendWireFrame(stream,p[1].positionOS.xyz,p[2].positionOS.xyz,p[0].positionOS.xyz,normal);
                AppendWireFrame(stream,p[2].positionOS.xyz,p[0].positionOS.xyz,p[1].positionOS.xyz,normal);
            }

            float4 frag (g2f i) : SV_Target
            {
                return i.color;
            }
            ENDHLSL
        }
    }
}
