Shader "Game/Lit/Special/Hair_Specular"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
		[Toggle(_NORMALMAP)]_EnableNormalMap("Enable Normal Mapping",float)=1
        [NoScaleOffset]_NormalTex("Normal Tex",2D)="white"{}
        [NoScaleOffset]_WarpDiffuseTex("Warp Diffuse Tex",2D)="white"{}
        [Header(Strand Specular)]
        [Toggle(_BITANGENT)]_UseBiTangent("Use Bi Tangent",float)=0
        _SpecularExponent("Specular Exponent",float)=50
        _ShiftTex("Shift Tex",2D)="white"{}
        _ShiftOffset("Shift Offset",Range(-1,1))=-.5
        _ShiftStrength("Shift Strength",Range(0,1))=.5
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Cull Back
		Blend Off
		ZWrite On
		ZTest LEqual
        
		Pass
		{
			NAME "FORWARDBASE"
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
            #include "Assets/Shaders/Library/CommonInclude.hlsl"
            #include "Assets/Shaders/Library/CommonLightingInclude.hlsl"

            #pragma shader_feature_local _BITANGENT
		    #pragma shader_feature_local _NORMALMAP

            #pragma multi_compile_instancing
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_CALCULATE_SHADOWS
            #pragma multi_compile _ _SHADOWS_SOFT

            struct appdata
            {
                float3 positionOS : POSITION;
                float3 tangentOS:TANGENT;
                float3 normalOS:NORMAL;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS:NORMAL;
                float3 tangentWS:TANGENT;
                float4 uv : TEXCOORD0;
                float3 positionWS:TEXCOORD1;
                float3 viewDirWS:TEXCOORD2;
                float3 lightDirWS:TEXCOORD3;
			    float4 shadowCoordWS:TEXCOORD4;
			    #if _NORMALMAP
			    float3x3 TBNWS:TEXCOORD5;
			    #endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_WarpDiffuseTex); SAMPLER(sampler_WarpDiffuseTex);
            TEXTURE2D(_ShiftTex);SAMPLER(sampler_ShiftTex);
		    #if _NORMALMAP
            TEXTURE2D(_NormalTex);SAMPLER(sampler_NormalTex);
		    #endif
			INSTANCING_BUFFER_START
			INSTANCING_PROP(float4,_MainTex_ST)
            INSTANCING_PROP(float,_SpecularExponent)
            INSTANCING_PROP(float4,_ShiftTex_ST)
            INSTANCING_PROP(float,_ShiftStrength)
            INSTANCING_PROP(float,_ShiftOffset)
			INSTANCING_BUFFER_END

            v2f vert (appdata v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.tangentWS=normalize( TransformObjectToWorldNormal( v.tangentOS));
                o.normalWS=normalize(TransformObjectToWorldNormal(v.normalOS));
                o.positionWS=TransformObjectToWorld (v.positionOS);
                o.uv.xy = TRANSFORM_TEX_INSTANCE(v.uv,_MainTex);
                o.uv.zw=TRANSFORM_TEX_INSTANCE(v.uv,_ShiftTex);
                o.viewDirWS=GetCameraPositionWS()- o.positionWS;
                o.lightDirWS=_MainLightPosition.xyz;
			    #if _NORMALMAP
			    o.TBNWS=float3x3(o.tangentWS,cross(o.tangentWS,o.normalWS),o.normalWS);
			    #endif
                o.shadowCoordWS= TransformWorldToShadowCoord(o.positionWS);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
                float3 normalWS=normalize(i.normalWS);
                float3 tangentWS=normalize(i.tangentWS);
			    #if _NORMALMAP
			    float3 normalTS= DecodeNormalMap(SAMPLE_TEXTURE2D(_NormalTex,sampler_NormalTex,i.uv.xy));
			    normalWS = mul(normalTS,i.TBNWS);
			    #endif
                #if _BITANGENT
                    tangentWS=cross(normalWS,tangentWS);
                #endif

                float3 lightDirWS=normalize(i.lightDirWS);
                float3 viewDirWS=normalize(i.viewDirWS);
                float3 halfDirWS=normalize(lightDirWS+viewDirWS);
				float atten=MainLightRealtimeShadow(i.shadowCoordWS);

                float3 lightCol=_MainLightColor.rgb;
                float3 ambient=_GlossyEnvironmentColor.rgb;
                float3 finalCol=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv.xy).xyz+ambient;
                float diffuse=saturate(GetDiffuse(normalWS,lightDirWS))*atten;
                finalCol*=SAMPLE_TEXTURE2D(_WarpDiffuseTex,sampler_WarpDiffuseTex,diffuse).rgb*lightCol;
                float shiftAmount=SAMPLE_TEXTURE2D(_ShiftTex,sampler_ShiftTex,i.uv.zw).x*INSTANCE( _ShiftStrength)+INSTANCE( _ShiftOffset);
                float specular=saturate(StrandSpecular(tangentWS,normalWS,halfDirWS,INSTANCE( _SpecularExponent),shiftAmount))*atten*diffuse;
                finalCol+=specular*lightCol;
                return float4(finalCol,1);
            }
			ENDHLSL
		}
		USEPASS "Hidden/ShadowCaster/MAIN"
		USEPASS "Hidden/DepthOnly/MAIN"
    }
}
