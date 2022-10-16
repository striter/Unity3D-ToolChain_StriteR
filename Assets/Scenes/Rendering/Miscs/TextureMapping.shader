Shader "Game/Unfinished/TextureMapping"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
        _Sharpness("Sharpness",float)=1
        [Toggle(_BIPLANAR)]_BiPlanar("Biplanar Mapping",int)=0
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float3 normalOS : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : NORMAL;
                float3 positionWS : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            #pragma multi_compile _ _BIPLANAR
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float,_Sharpness)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.positionWS = TransformObjectToWorld(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                half3 normalWS = normalize(i.normalWS);
                float halfLambert = saturate(dot(normalWS,normalize(_MainLightPosition.xyz)))*.5f+.5f;
                float3 sh = SampleSH(normalWS);
                half3 albedo =
                    #if _BIPLANAR
                        BiPlanarMapping(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.positionWS/_MainTex_ST.xyz,normalize(i.normalWS),_Sharpness).rgb;
                    #else
                        TriPlanarMapping(TEXTURE2D_ARGS(_MainTex,sampler_MainTex),i.positionWS/_MainTex_ST.xyz,normalize(i.normalWS),_Sharpness).rgb;
                    #endif
                float3 finalCol = albedo * halfLambert * sh;
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
