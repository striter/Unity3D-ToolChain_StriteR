Shader "Game/Effects/Geometry/GrassField"
{
    Properties
    {
        _TopColor("Top Color",Color)=(1,1,1,1)
        _BottomColor("Bottom Color",Color)=(0,0,.6,0)
        _GroundColor("Ground Color",Color)=(0,0,.5,1)
        [Header(Shape)]
        _Width("Width",Range(0,.2))=0.05
        _Height("Height",Range(0.01,3))=1
        _RandomClip("Random Clip",Range(0,1))=.5
        [Header(Segment)]
		[KeywordEnum(Low,Normal,High,Ultra)]_SEGMENT("Segment",float)=3
        _SegmentFactor("Segment Factor",Range(0.01,1))=.3
        _BendForward("Bend Forward",Range(0,1))=.2
        _BendRotate("Bend Rotate",Range(0,1))=.2
        [Header(Wind)]
        _WindFlowTex("Wind Flow Tex",2D)="black"{}
        _WindStrength("Wind Strength",Range(0,1))=.5
        _WindSpeed("Wind Speed",Range(0.001,1))=0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGINCLUDE
            #pragma multi_compile _SEGMENT_LOW _SEGMENT_NORMAL _SEGMENT_HIGH _SEGMENT_ULTRA
            #include "UnityCG.cginc"
            #include "../../CommonInclude.cginc"

            struct a2v
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
            };

            struct v2g
            {
                float4 vertex:POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
            };

            struct g2f
            {
                float4 vertex : SV_POSITION;
                float3 color:COLOR;
            };

            v2g vert(a2v i)
            {
                v2g o;
                o = i;
                return o;
            }

            float4 _GroundColor;
            float4 _TopColor;
            float4 _BottomColor;
            float _BendRotate;
            float _BendForward;

            float _Width;
            float _Height;
            float _RandomClip;
#if _SEGMENT_NORMAL
    #define _SEGMENTCOUNT 2
#elif _SEGMENT_HIGH
    #define _SEGMENTCOUNT 3
#elif _SEGMENT_ULTRA
    #define _SEGMENTCOUNT 4
#else
    #define _SEGMENTCOUNT 1
#endif
            float _SegmentFactor;

            sampler2D _WindFlowTex;
            float4 _WindFlowTex_ST;
            float _WindSpeed;
            float _WindStrength;

            [maxvertexcount(_SEGMENTCOUNT * 4 + 1 + 3)]
            void geom(triangle v2g i[3],inout TriangleStream<g2f> stream)
            {
                g2f o;
                
                for (int index = 0; index < 3; index++)
                {
                    o.vertex = UnityObjectToClipPos(i[index].vertex);
                    o.color = _GroundColor;
                    stream.Append(o);
                }
                stream.RestartStrip();

                float3 pos = i[0].vertex;
                float3 normal = i[0].normal;
                float3 tangent = i[0].tangent;
                float3 biNormal = cross(normal,tangent) * i[0].tangent.w;

                float3x3 tangentToLocal = float3x3(tangent.x,biNormal.x,normal.x,tangent.y,biNormal.y,normal.y,tangent.z,biNormal.z,normal.z);

                float3x3 facingRotation = AngleAxis3x3(random3(pos) * UNITY_TWO_PI,float3(0,0,1));

                float2 windUV = TRANSFORM_TEX(pos.xz,_WindFlowTex) + _Time.yy * _WindSpeed;
                float2 windSample = tex2Dlod(_WindFlowTex,float4(windUV,0,0)).xy * 2 - 1;
                windSample *= _WindStrength;

                float blend = random3(pos.zzx) * _BendRotate + windSample.x;
                blend = clamp(blend,-1,1);
                float3x3 bendingRotation = AngleAxis3x3(blend * UNITY_PI * 0.5,float3(1,0,0));
                float3x3 windRotation = AngleAxis3x3(UNITY_PI * 0.5 * windSample * _WindStrength,float3(windSample,0));

                float3x3 vertexTransform = mul(mul(tangentToLocal,facingRotation),bendingRotation);

                float randWidth = random3(pos.xzx) * _Width * _RandomClip + _Width * (1 - _RandomClip);
                float randHeight = random3(pos.zyx) * _Height * _RandomClip + _Height * (1 - _RandomClip);
                float randForward = random3(pos.zzx) * _RandomClip * _BendForward + _BendForward * (1 - _RandomClip);

                float4 verticies[_SEGMENTCOUNT * 2];
                float4 colors[_SEGMENTCOUNT];

                float3 lightNormal = normalize(mul(vertexTransform,float3(0,0,1)));
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float diffuse = (dot(lightNormal,lightDir) + 1) / 2;

                [unroll(2)]
                for (index = 0; index < _SEGMENTCOUNT; index++)
                {
                    float gradient = (float)index / _SEGMENTCOUNT;
                    gradient = pow(gradient,_SegmentFactor);

                    float forward = randForward * pow(gradient,index + 1);
                    float right = randWidth * (1 - gradient);
                    float up = randHeight * gradient;

                    verticies[index * 2] = UnityObjectToClipPos(pos + mul(vertexTransform,float3(-right,forward,up)));
                    verticies[index * 2 + 1] = UnityObjectToClipPos(pos + mul(vertexTransform,float3(right,forward,up)));
                    colors[index] = lerp(_BottomColor,_TopColor,gradient) * diffuse;
                }

                [unroll(2)]
                for (index = 0; index < _SEGMENTCOUNT; index++)
                {
                    o.color = colors[index];
                    o.vertex = verticies[index * 2];
                    stream.Append(o);
                    o.vertex = verticies[index * 2 + 1];
                    stream.Append(o);
                }
                o.vertex = UnityObjectToClipPos(pos + mul(vertexTransform,float3(0,randForward,randHeight)));
                o.color = diffuse * _TopColor;
                stream.Append(o);

                [unroll(2)]
                for (index = 0; index < _SEGMENTCOUNT; index++)
                {
                    o.color = colors[index];
                    o.vertex = verticies[index * 2 + 1];
                    stream.Append(o);
                    o.vertex = verticies[index * 2];
                    stream.Append(o);
                }
                stream.RestartStrip();
            }

            fixed4 frag(g2f i) : SV_Target
            {
                float3 finalCol = i.color;
                return float4(finalCol,1);
            }

        ENDCG

        Pass
        {
            Tags{ "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            ENDCG
        }

        Pass
        {
            Tags{"LightMode" = "ShadowCaster"}
            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            ENDCG
        }

    }
}
