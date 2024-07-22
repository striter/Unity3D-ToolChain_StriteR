Shader "Hidden/Imposter_Position"
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
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"
            #include "Imposter.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 positionWS:TEXCOORD1;
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
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 positionOS = (i.positionWS - _BoundingSphere.xyz)/_BoundingSphere.w  * .5 + .5;
                return float4(positionOS,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
