Shader "Runtime/Surface/Rim"
{
    Properties
    {
        [HDR] _AdditiveRimColor("Color Tint",Color)=(1,1,1,1)
        _AdditiveRimWidth("Rim Width",Range(0.1,10))=2
    }
    SubShader
    {
        Tags{"Queue" = "Transparent"}
        Pass
        {
            Blend One One
            ZWrite Off
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                float2 uv : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_AdditiveRimColor)
                INSTANCING_PROP(float,_AdditiveRimWidth)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS = GetWorldSpaceViewDir(TransformObjectToWorld(v.positionOS));
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float ndv = pow(1-saturate(dot(normalize(i.viewDirWS),normalize(i.normalWS))),INSTANCE(_AdditiveRimWidth));
                float4 color = INSTANCE(_AdditiveRimColor);
                return float4(color.rgb,1)*ndv*color.a;
            }
            ENDHLSL
        }
    }
}
