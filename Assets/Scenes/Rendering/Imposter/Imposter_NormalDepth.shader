Shader "Hidden/Imposter_NormalDepth"
{
    Properties
    {
        _NormalTex("_NormalTex",2D) = "white"
    }
    SubShader
    {
		Tags{"LightMode" = "UniversalForward"}
        Pass
        {
            Blend Off
            Cull Back
            ZWrite On
            ZTest LEqual
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
                float4 tangentOS :TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : NORMAL;
                half3 tangentWS : TEXCOORD3;
                half3 biTangentWS : TEXCOORD4;
            	float eyeDepth : TEXCOORD5;

				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_NormalTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_NormalTex);
	            o.normalWS = TransformObjectToWorldNormal(v.normalOS);
	            o.tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
	            o.biTangentWS = cross(o.normalWS,o.tangentWS)*v.tangentOS.w;

            	float3 view = TransformObjectToView(v.positionOS);
            	o.eyeDepth = -view.z;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 normalWS = normalize(i.normalWS);
                float3 tangentWS=normalize(i.tangentWS);
	            float3 biTangentWS=normalize(i.biTangentWS);
		        float3 normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv));
	            float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
	            normalWS = normalize(mul(transpose(TBNWS), normalTS));

            	float eyeDepth = i.eyeDepth; 
				float depthOS = ( -1.0 / UNITY_MATRIX_P[2].z );
                return float4(normalWS * 0.5f + 0.5f,( eyeDepth + depthOS ) / depthOS ) ;
            }
            ENDHLSL
        }
    }
}
