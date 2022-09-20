Shader "Game/Additive/ExteriorAO"
{
    Properties
    {
        [NoScaleOffset]_SampleTex("Main Tex",2D)="white"{}
        _AOColor("AO Color Tint",Color)=(1,1,1,1)
        _AOIntensity("AO Intensity",Range(0.1,5))=1
        [HDR]_EmissionColor("Emission Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        Tags{ "Queue" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag


            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS:NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_SampleTex);SAMPLER(sampler_SampleTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_AOColor)
                INSTANCING_PROP(float4,_EmissionColor)
                INSTANCING_PROP(float,_AOIntensity)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                
                float3 sh = IndirectDiffuse_SH(normalize(i.normalWS));
                float4 sample =  SAMPLE_TEXTURE2D(_SampleTex,sampler_SampleTex,i.uv);
                float aoSample = (1-saturate(invlerp(0,0.9,sample.a)))*_AOIntensity;
                float3 emissionSample = sample.rgb;

                float4 aoColor = float4(sh,aoSample)*INSTANCE(_AOColor);
                float4 emissionColor = float4(emissionSample,max(emissionSample))*INSTANCE(_EmissionColor);
                float emission = emissionColor.a;
                
                float4 finalCol;
                finalCol.rgb = lerp(aoColor.rgb ,emissionColor.rgb,emission);
                finalCol.a = lerp(aoColor.a,emission,emission);
                return saturate(finalCol);
            }
            ENDHLSL
        }
    }
}
