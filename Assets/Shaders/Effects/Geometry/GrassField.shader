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
        HLSLINCLUDE
            #pragma multi_compile _SEGMENT_LOW _SEGMENT_NORMAL _SEGMENT_HIGH _SEGMENT_ULTRA
            #pragma target 4.6
            #include "../../CommonInclude.hlsl"
            #include "../../CommonLightingInclude.hlsl"

            struct a2v
            {
                float4 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float4 tangentOS:TANGENT;
                float3 color:COLOR;
            };

            struct v2g
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float4 tangentOS:TANGENT;
                float3 color:COLOR;
            };

            struct g2f
            {
                float4 positionCS : SV_POSITION;
                float3 color:COLOR;
			    float4 shadowCoordWS:TEXCOORD0;
            };

#if _SEGMENT_NORMAL
    #define _SEGMENTCOUNT 2
#elif _SEGMENT_HIGH
    #define _SEGMENTCOUNT 3
#elif _SEGMENT_ULTRA
    #define _SEGMENTCOUNT 4
#else
    #define _SEGMENTCOUNT 1
#endif
    
            float4 _GroundColor;
            float4 _TopColor;
            float4 _BottomColor;
            float _BendRotate;
            float _BendForward;

            float _Width;
            float _Height;
            float _RandomClip;

            float _SegmentFactor;

            sampler2D _WindFlowTex;
            float4 _WindFlowTex_ST;
            float _WindSpeed;
            float _WindStrength;
            
            v2g vert(a2v i)
            {
                v2g o;
                o=i;
                return o;
            }

            void Append(inout TriangleStream<g2f> stream,v2g v)
            {
                g2f o;
                o.color=v.color;
                #if UNITY_PASS_SHADOWCASTER
                SHADOW_CASTER_VERTEX(v,o);
                #else
                o.positionCS=TransformObjectToHClip(v.positionOS.xyz);
                #endif
		        o.shadowCoordWS=TransformWorldToShadowCoord(TransformObjectToWorld(v.positionOS.xyz));
                stream.Append(o);
            }

            [maxvertexcount(_SEGMENTCOUNT * 4 + 1 + 3)]
            void geom(triangle v2g i[3],inout TriangleStream<g2f> stream)
            {   
                [unroll(3)]
                for (uint index = 0u; index < 3u; index++)
                {
                    i[index].color=_GroundColor.xyz;
                    Append(stream,i[index]);
                }
                stream.RestartStrip();

                float3 pos = i[0].positionOS.xyz;
                float3 normal = i[0].normalOS;
                float4 tangent = i[0].tangentOS;
                float3 biNormal = cross(normal,tangent.xyz) * i[0].tangentOS.w;

                float3x3 tangentToLocal = float3x3(tangent.x,biNormal.x,normal.x,tangent.y,biNormal.y,normal.y,tangent.z,biNormal.z,normal.z);

                float3x3 facingRotation = AngleAxis3x3(random01(pos) * PI*2,float3(0,0,1));

                float2 windUV = TRANSFORM_TEX(pos.xz,_WindFlowTex) + _Time.yy * _WindSpeed;
                float2 windSample = tex2Dlod(_WindFlowTex,float4(windUV,0,0)).xy * 2 - 1;
                windSample *= _WindStrength;

                float blend = random01(pos.zzx) * _BendRotate + windSample.x;
                blend = clamp(blend,-1,1);
                float3x3 bendingRotation = AngleAxis3x3(blend * PI,float3(1,0,0));
                float3x3 windRotation = AngleAxis3x3(PI * windSample.x+windSample.y*PI,float3(windSample,0));

                float3x3 vertexTransform = mul(mul(tangentToLocal,facingRotation),bendingRotation);

                float randWidth = random01(pos.xzx) * _Width * _RandomClip + _Width * (1 - _RandomClip);
                float randHeight = random01(pos.zyx) * _Height * _RandomClip + _Height * (1 - _RandomClip);
                float randForward = random01(pos.zzx) * _RandomClip * _BendForward + _BendForward * (1 - _RandomClip);

                v2g vertexDatas[_SEGMENTCOUNT * 2];

                float3 lightNormal = normalize(mul(vertexTransform,float3(0,0,1)));
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float diffuse = (dot(lightNormal,lightDir) + 1) / 2;

                [unroll(2)]
                for (index = 0; index < _SEGMENTCOUNT; index++)
                {
                    float gradient = (float)index / _SEGMENTCOUNT;
                    gradient = pow(gradient,_SegmentFactor);

                    float forward = randForward * pow(gradient,index + 1);
                    float right = randWidth * (1 - gradient);
                    float up = randHeight * gradient;

                    float3 vertex1=pos+mul(vertexTransform,float3(-right,forward,up));
                    float3 vertex2=pos + mul(vertexTransform,float3(right,forward,up));
                    
                    vertexDatas[index*2].positionOS=float4(vertex1,1);
                    vertexDatas[index*2+1].positionOS=float4(vertex2,1);

                    vertexDatas[index*2].normalOS=normal;
                    vertexDatas[index*2+1].normalOS=normal;
                    vertexDatas[index*2].tangentOS=tangent;
                    vertexDatas[index*2+1].tangentOS=tangent;

                    float3 color=lerp(_BottomColor.rgb,_TopColor.rgb,gradient) * diffuse;
                    vertexDatas[index*2].color=color;
                    vertexDatas[index*2+1].color=color;
                }
                
                [unroll(2)]
                for (index = 0; index < _SEGMENTCOUNT; index++)
                {
                    Append(stream, vertexDatas[index*2]);
                    Append(stream, vertexDatas[index * 2+1]);
                }
                v2g v;
                v.positionOS=float4(pos + mul(vertexTransform,float3(0,randForward,randHeight)),1);
                v.normalOS=normal;
                v.tangentOS=tangent;
                v.color= diffuse * _TopColor.rgb;
                Append(stream,v);
                [unroll(2)]
                for (index = 0; index < _SEGMENTCOUNT; index++)
                {
                    Append(stream, vertexDatas[index * 2+1]);
                    Append(stream, vertexDatas[index*2]);
                }
                stream.RestartStrip();
            }
            
            float4 frag(g2f i) : SV_Target
            {
                #ifndef _UNITY_PASS_SHADOWCASTER
                float3 finalCol = i.color;
                float atten=MainLightRealtimeShadow(i.shadowCoordWS);
                return float4(finalCol*atten,1);
                #else
                    return 1;
                #endif
            }

            float4 fragDepth(g2f i):SV_TARGET0
            {
                return 0;
            }
        ENDHLSL

        Pass
        {
            Tags{ "LightMode" = "UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT
            ENDHLSL
        }

        Pass
        {
            Tags{"LightMode" = "ShadowCaster"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment fragDepth
            ENDHLSL
        }
        
        Pass
        {
            Tags{"LightMode" = "DepthOnly"}
            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment fragDepth
            ENDHLSL
        }
    }
}
