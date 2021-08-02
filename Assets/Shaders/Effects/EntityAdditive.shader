Shader "Game/Effects/EntityAdditive"
{
    Properties
    {
        [HDR]_Color ("Color Tine", Color) = (1,1,1,.1)
        [Header(Shape)]
        _NoiseTex("Noise Tex",2D)="white"{}
        _NoiseStrength("Noise Strength",Range(0,5))=1.5
        _NoisePow("Noise Pow",Range(.1,5))=2
        [Header(Flow)]
        _NoiseFlowX("Noise Flow X",Range(-2,2))=.1
        _NoiseFlowY("Noise Flow Y",Range(-2,2))=.1
    }
    SubShader
    {
        Name "Main"
        Tags { "RenderType" ="EntityAdditive" "Queue"="Geometry+100" }
        ZWrite Off
        Blend One One
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Assets/Shaders/Library/CommonInclude.hlsl"

            TEXTURE2D(_NoiseTex); SAMPLER(sampler_NoiseTex);
            
            INSTANCING_BUFFER_START
            INSTANCING_PROP(float4,_NoiseTex_ST)
            INSTANCING_PROP(float,_NoiseStrength)
            INSTANCING_PROP(float,_NoisePow)
            INSTANCING_PROP(float,_NoiseFlowX)
            INSTANCING_PROP(float,_NoiseFlowY)
            INSTANCING_PROP(float4,_Color)
            INSTANCING_BUFFER_END

            struct appdata
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


            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v,o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv,_NoiseTex);
                o.uv+=_Time.y*float2(INSTANCE( _NoiseFlowX),INSTANCE(_NoiseFlowY));
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                float3 finalCol=INSTANCE(_Color).rgb*INSTANCE(_Color).a;
                float noise= SAMPLE_TEXTURE2D(_NoiseTex,sampler_NoiseTex,i.uv).r*INSTANCE(_NoiseStrength);
                noise=pow(abs(noise),INSTANCE(_NoisePow));
                finalCol*=noise;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
    }
}
