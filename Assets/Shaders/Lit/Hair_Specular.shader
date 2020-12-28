Shader "Game/Lit/Hair_Specular"
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
			Tags{"LightMode" = "ForwardBase"}
			Cull Back
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			ENDCG
		}

		USEPASS "Hidden/ShadowCaster/MAIN"


        CGINCLUDE
            #pragma shader_feature _BITANGENT
		    #pragma shader_feature _NORMALMAP
            #include "../CommonLightingInclude.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 tangent:TANGENT;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                float3 normal:TEXCOORD1;
                float3 tangent:TEXCOORD2;
                float3 worldPos:TEXCOORD3;
                float3 viewDir:TEXCOORD4;
                float3 lightDir:TEXCOORD5;
			    SHADOW_COORDS(6)
			    #if _NORMALMAP
			    float3x3 worldToTangent:TEXCOORD7;
			    #endif
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _WarpDiffuseTex;
		    #if _NORMALMAP
		    sampler2D _NormalTex;
		    #endif

            float _SpecularExponent;
            sampler2D _ShiftTex;
            float4 _ShiftTex_ST;
            float _ShiftStrength;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.tangent=v.tangent;
                o.normal=v.normal;
                o.uv.xy = TRANSFORM_TEX(v.uv,_MainTex);
                o.uv.zw=TRANSFORM_TEX(v.uv,_ShiftTex);
                o.viewDir=ObjSpaceViewDir(v.vertex);
                o.lightDir=ObjSpaceLightDir(v.vertex);
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
			    #if _NORMALMAP
                float3 worldNormal=mul(unity_ObjectToWorld,o.normal);
			    float3 worldTangent=mul(unity_ObjectToWorld,v.tangent);
			    float3 worldBitangent=cross(worldTangent,worldNormal);
			    o.worldToTangent=float3x3(normalize(worldTangent),normalize(worldBitangent),normalize(worldNormal));
			    #endif
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal=normalize(i.normal);
                float3 tangent=normalize(i.tangent);
			    #if _NORMALMAP
			    float3 tangentSpaceNormal= DecodeNormalMap(tex2D(_NormalTex,i.uv));
			    normal= mul(tangentSpaceNormal,i.worldToTangent);
			    #endif
                #if _BITANGENT
                    tangent=cross(normal,tangent);
                #endif
                float3 lightDir=normalize(i.lightDir);
                float3 viewDir=normalize(i.viewDir);
                float3 halfDir=normalize(i.viewDir+i.lightDir);
                UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos)

                float3 finalCol=tex2D(_MainTex,i.uv.xy)+UNITY_LIGHTMODEL_AMBIENT.xyz;
                float diffuse=GetDiffuse(normal,lightDir)*atten;
                finalCol*=tex2D(_WarpDiffuseTex,diffuse)*_LightColor0;
                float shiftAmount=tex2D(_ShiftTex,i.uv.zw)*_ShiftStrength;
                float specular=saturate(StrandSpecular(tangent,normal,halfDir,_SpecularExponent,shiftAmount))*atten;
                finalCol+=specular*_LightColor0;
                return float4(finalCol,1);
            }
        ENDCG
    }
}
