Shader "Unlit/BRDFTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)

        _Glossiness("Smoothness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0

        [KeywordEnum(BlinnPhong,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("NDF Type",float) = 2
        _AnisoTropicValue("Anisotropic Value",Range(0,1))=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" "LightMode"="ForwardBase"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile _NDF_BLINNPHONG _NDF_BECKMANN _NDF_GAUSSIAN _NDF_GGX _NDF_TROWBRIDGEREITZ _NDF_ANISOTROPIC_TROWBRIDGEREITZ _NDF_ANISOTROPIC_WARD
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "BRDFInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float3 tangent:TANGENT;
                float2 uv : TEXCOORD0;
                float2 lightmapUV:TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 lightMapUV:TEXCOORD1;
                float3 worldPos:TEXCOORD2;
                float3 biTangent:TEXCOORD3;
                float3 tangent:TEXCOORD4;
                float3 lightDir:TEXCOORD5;
                float3 viewDir:TEXCOORD6;
                float3 normal:TEXCOORD7;
                SHADOW_COORDS(8)
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _Glossiness;
            float _Metallic;
            float _AnisoTropicValue;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightMapUV=v.lightmapUV;
                o.normal=UnityObjectToWorldNormal(v.normal);
                o.tangent=normalize(mul(unity_ObjectToWorld,v.tangent));
                o.biTangent=normalize(cross(o.normal,o.tangent));
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                o.lightDir=WorldSpaceLightDir(v.vertex);
                o.viewDir=WorldSpaceViewDir(v.vertex);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal=normalize(i.normal);
                float3 tangent = normalize(i.tangent);
                float3 biTangent = normalize(i.biTangent);
                float3 lightDir=normalize(i.lightDir);
                float3 viewDir=normalize(i.viewDir);
                float3 halfDir=normalize(viewDir+lightDir);
                float3 reflectDir=normalize(reflect(-viewDir,normal));
                
                float3 albedo=tex2D(_MainTex,i.uv);
                float3 lightCol=_LightColor0.rgb;
                float3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;
                UNITY_LIGHT_ATTENUATION(atten, i,i.worldPos);

                float NDL = dot(normal, lightDir);
                float NDH = dot(normal, halfDir);
                float NDV = dot(normal, viewDir);
                float VDH = dot(viewDir, halfDir);
                float LDH = dot(lightDir, halfDir);
                float LDV = dot(lightDir, viewDir);
                float RDV = dot(reflectDir, viewDir);

                float roughness=UnpackRoughness(_Glossiness);
                float3 diffuseColor=albedo*(1-_Metallic);
                float3 specColor=lerp(lightCol,albedo,_Metallic*.5);

                float specular=0;
#if _NDF_BLINNPHONG
                specular=NDF_BlinnPhong(NDH, _Glossiness,max(1, _Glossiness *40));
#elif _NDF_BECKMANN
                specular=NDF_Beckmann(NDH,roughness);
#elif _NDF_GAUSSIAN
                specular=NDF_Gaussian(NDH,roughness);
#elif _NDF_GGX
                specular=NDF_GGX(NDH,roughness);
#elif _NDF_TROWBRIDGEREITZ
                specular=NDF_TrowbridgeReitz(NDH,roughness);
#elif _NDF_ANISOTROPIC_TROWBRIDGEREITZ
                specular = NDFA_TrowbridgeReitz(NDH, dot(halfDir, tangent), dot(halfDir, biTangent), _AnisoTropicValue, _Glossiness);
#elif _NDF_ANISOTROPIC_WARD
                specular = NDFA_Ward(NDL, NDV, NDH, dot(halfDir, tangent), dot(halfDir, biTangent), _AnisoTropicValue, _Glossiness);
#endif 

                return specular * 2;

                float geometricShadow = 1;
                geometricShadow = GSF_Implicit(NDL, NDV);
                geometricShadow = GSF_AshikhminShirley(NDL, NDV, LDH);
                geometricShadow = GSF_AshikhminPremoze(NDL, NDV);
                return geometricShadow;

                float3 finalCol=albedo*atten*lightCol +ambient;
                return float4(finalCol,1);
            }
            ENDCG
        }
        USEPASS "Hidden/ShadowCaster/Main"
    }
}
