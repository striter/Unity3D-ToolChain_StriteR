Shader "Game/Unfinished/Lambert_BlinnPhong"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        _Color("Color Tint",Color)=(1,1,1,1)
        _Lambert("Lambert",Range(0,1))=.5
        _SpecularAmount("Specular Amount",float)=1
        _SpecularPower("Specular Power",float)=64
        
		[Header(Render Options)]
        [HideInInspector]_ZWrite("Z Write",int)=1
        [HideInInspector]_ZTest("Z Test",int)=2
        [HideInInspector]_Cull("Cull",int)=2
    }
    SubShader
    {
        Pass
        {
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
                float2 uv : TEXCOORD0;
                float3 normalWS:TEXCOORD1;
                float3 viewDirWS:TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_Color)
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float,_SpecularPower)
                INSTANCING_PROP(float,_SpecularAmount)
                INSTANCING_PROP(float,_Lambert)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _MainTex);
                o.normalWS=TransformObjectToWorldNormal(v.normalOS);
                o.viewDirWS=GetCameraPositionWS()-TransformObjectToWorld(v.positionOS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 albedo=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).rgb * INSTANCE(_Color).rgb;

                float3 normalWS=normalize(i.normalWS);
                float3 viewDirWS=normalize(i.viewDirWS);
                float3 lightDirWS=normalize(_MainLightPosition.xyz);
                float3 halfDirWS=normalize(viewDirWS+lightDirWS);
                float NDL=saturate(dot(normalWS,lightDirWS));

                float halfLambert=NDL*INSTANCE(_Lambert)+(1-INSTANCE(_Lambert));

                float NDH=max(0,dot(normalWS,halfDirWS));
                float specular=pow(NDH,INSTANCE(_SpecularPower))*INSTANCE(_SpecularAmount);

                float3 finalCol=albedo*halfLambert+specular*_MainLightColor.rgb;
                
                return float4(finalCol,1);
            }
            ENDHLSL
        }
        
        USEPASS "Game/Additive/DepthOnly/MAIN"
        USEPASS "Game/Additive/ShadowCaster/MAIN"
    }
}
