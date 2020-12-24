Shader "Unlit/Kajiya_Kay_Specular"
{
    Properties
    {
        _MainTex("Main Tex",2D)="white"{}
        [Toggle(_BITANGENT)]_UseBiTangent("Use Bi Tangent",float)=0
        _ShiftTex("Shift Tex",2D)="white"{}
        _ShiftAmount("Shift Amount",Range(-1,1))=0
        _Lambert("Lambert",Range(0,1))=.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _BITANGENT
            #pragma multi_compile_forwardbase
            #include "../CommonLightingInclude.cginc"
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            float3 shiftTangent(float3 T,float3 N,float shift)
            {
                float3 shiftedT=T+shift*N;
                return normalize(shiftedT);
            }

            float StrandSpecular(float3 T,float3 H,float exponent)
            {
                float dotTH=dot(T,H);
                float sinTH=sqrt(1.0-dotTH*dotTH);
                float dirAtten=smoothstep(-1,0,dotTH);
                return dirAtten*pow(sinTH,exponent);
            }

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
            };

            sampler2D _ShiftTex;
            float4 _ShiftTex_ST;
            float _Lambert;
            float _ShiftAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.tangent=v.tangent;
                o.normal=v.normal;
                o.uv.xy = v.uv;
                o.uv.zw=TRANSFORM_TEX(v.uv,_ShiftTex);
                o.viewDir=ObjSpaceViewDir(v.vertex);
                o.lightDir=ObjSpaceLightDir(v.vertex);
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal=normalize(i.normal);
                float3 tangent=normalize(i.tangent);
                #if _BITANGENT
                    tangent=cross(normal,tangent);
                #endif
                float3 lightDir=normalize(i.lightDir);
                float3 viewDir=normalize(i.viewDir);
                float3 halfDir=normalize(i.viewDir+i.lightDir);
                float shiftAmount=tex2D(_ShiftTex,i.uv.zw)-_ShiftAmount;

                tangent=shiftTangent(tangent,normal,shiftAmount);
                float specular=saturate( StrandSpecular(tangent,halfDir,50));
                return specular*_LightColor0;
            }
            ENDCG
        }
    }
}
