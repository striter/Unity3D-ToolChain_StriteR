Shader "Game/Unfinished/Glass"
{
    Properties
    {
        [ToggleTex(_NORMALTEX)]_NormalTex("Normal Tex",2D)="white"{}
        _ColorTint("Color Tint",Color)=(1,1,1,1)
    }
    SubShader
    {
        ZWrite Off
        Blend Off
    	Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local_fragment _NORMALTEX

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "Assets/Shaders/Library/Lighting.hlsl"

            struct appdata
            {
                float3 positionOS : POSITION;
                float3 normalOS:NORMAL;
                float4 tangentOS:TANGENT;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS:TEXCOORD1;
				float4 positionHCS:TEXCOORD2;
				half3 normalWS:TEXCOORD3;
				half3 tangentWS:TEXCOORD4;
				half3 biTangentWS:TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_ColorTint)
                INSTANCING_PROP(float4,_NormalTex_ST)
            INSTANCING_BUFFER_END
            
            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.positionHCS=o.positionCS;
                o.uv = TRANSFORM_TEX_INSTANCE(v.uv, _NormalTex);
                o.positionWS=TransformObjectToWorld(v.positionOS);
				o.normalWS=normalize(mul((float3x3)unity_ObjectToWorld,v.normalOS));
				o.tangentWS=normalize(mul((float3x3)unity_ObjectToWorld,v.tangentOS.xyz));
				o.biTangentWS=cross(o.normalWS,o.tangentWS)*v.tangentOS.w;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
				float3 normalWS=normalize(i.normalWS);
				half3 biTangentWS=normalize(i.biTangentWS);
				half3 tangentWS=normalize(i.tangentWS);
				half3 normalTS=half3(0,0,1);
				#if _NORMALTEX
					float3x3 TBNWS=half3x3(tangentWS,biTangentWS,normalWS);
					normalTS=DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv));
					normalWS=normalize(mul(transpose(TBNWS), normalTS));
				#endif
                float3 viewDirWS=normalize(GetCameraPositionWS()-i.positionWS);
            	float3 reflectDirWS=normalize(reflect(-viewDirWS, normalWS));
            	
                float ndv=pow5(1-dot(viewDirWS,normalWS));
                float3 specular=  IndirectBRDFSpecular(reflectDirWS,0,i.positionHCS,normalTS)*INSTANCE(_ColorTint.rgb);
                return float4(specular,ndv);
            }
            ENDHLSL
        }
    }
}
