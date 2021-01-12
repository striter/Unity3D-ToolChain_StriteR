Shader "Unlit/BRDFTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)

        _Glossiness("Smoothness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0

        [KeywordEnum(BlinnPhong,Beckmann,Gaussian,GGX,TrowbridgeReitz,Anisotropic_TrowbridgeReitz,Anisotropic_Ward)]_NDF("Normal Distribution Function:",float) = 2
        [KeywordEnum(Implicit,AshikhminShirley,AshikhminPremoze,Duer,Neumann,Kelemen,CookTorrence,Ward,R_Kelemen_Modified,R_Kurt,R_WalterEtAl,R_SmithBeckmann,R_GGX,R_Schlick,R_Schlick_Beckmann,R_Schlick_GGX)]_GSF("Geometry Shadow Function:",float)=0
        [KeywordEnum(SCHLICK,SPHERICALGAUSSIAN)]_FRESNEL("Fresnel Function",float)=0
        _AnisoTropicValue("Normal Anisotropic",Range(0,1))=1
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
            #pragma multi_compile _GSF_IMPLICIT _GSF_ASHIKHMINSHIRLEY _GSF_ASHIKHMINPREMOZE _GSF_DUER _GSF_NEUMANN _GSF_KELEMEN _GSF_COOKTORRENCE _GSF_WARD _GSF_R_KELEMEN_MODIFIED _GSF_R_KURT _GSF_R_WALTERETAL _GSF_R_SMITHBECKMANN _GSF_R_GGX _GSF_R_SCHLICK _GSF_R_SCHLICK_BECKMANN _GSF_R_SCHLICK_GGX
            #pragma multi_compile _FRESNEL_SCHLICK _FRESNEL_SPHERICALGAUSSIAN
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
                
                float NDL = dot(normal, lightDir);
                float NDH = dot(normal, halfDir);
                float NDV = dot(normal, viewDir);
                float VDH = dot(viewDir, halfDir);
                float LDH = dot(lightDir, halfDir);
                float LDV = dot(lightDir, viewDir);
                float RDV = dot(reflectDir, viewDir);

                float3 albedo=tex2D(_MainTex,i.uv);
                float3 lightCol=_LightColor0.rgb;
                float3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;
                UNITY_LIGHT_ATTENUATION(atten, i,i.worldPos);
                float roughness=UnpackRoughness(_Glossiness);
                //GI
                UnityGI gi=GetUnityGI(lightCol,lightDir,NDL,normal,viewDir,reflectDir,atten,1-_Glossiness,i.worldPos);
                float3 indirectDiffuse=gi.indirect.diffuse.rgb;
                float3 indirectSpecular=gi.indirect.specular.rgb;
                //Specular
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
                
                //Geometric Shadow
                float geometricShadow = 1;
                NDL=saturate(NDL);
                NDV=saturate(NDV);
                LDH=saturate(LDH);
                VDH=saturate(VDH);
#if _GSF_IMPLICIT
                geometricShadow = GSF_Implicit(NDL, NDV);
#elif _GSF_ASHIKHMINSHIRLEY
                geometricShadow = GSF_AshikhminShirley(NDL, NDV, LDH);
#elif _GSF_ASHIKHMINPREMOZE
                geometricShadow = GSF_AshikhminPremoze(NDL, NDV);
#elif _GSF_DUER
                geometricShadow = GSF_Duer(NDL,NDV, lightDir,viewDir,normal);
#elif _GSF_NEUMANN
                geometricShadow=GSF_Neumann(NDL,NDV);
#elif _GSF_KELEMEN
                geometricShadow=GSF_Kelemen(NDL,NDV,VDH);
#elif _GSF_COOKTORRENCE
                geometricShadow=GSF_CookTorrence(NDL,NDV,VDH,NDH);
#elif _GSF_WARD
                geometricShadow=GSF_Ward(NDL,NDV);
#elif _GSF_R_KELEMEN_MODIFIED
                geometricShadow=GSFR_Kelemen_Modifed(NDL,NDV,roughness);
#elif _GSF_R_KURT
                geometricShadow=GSFR_Kurt(NDL,NDV,VDH,roughness);
#elif _GSF_R_WALTERETAL
                geometricShadow=GSFR_WalterEtAl(NDL,NDV,roughness);
#elif _GSF_R_SMITHBECKMANN
                geometricShadow=GSFR_SmithBeckmann(NDL,NDV,roughness);
#elif _GSF_R_GGX
                geometricShadow=GSFR_GGX(NDL,NDV,roughness);
#elif _GSF_R_SCHLICK
                geometricShadow=GSFR_Schlick(NDL,NDV,roughness);
#elif _GSF_R_SCHLICK_BECKMANN
                geometricShadow=GSFR_SchlickBeckmann(NDL,NDV,roughness);
#elif _GSF_R_SCHLICK_GGX
                geometricShadow=GSFR_SchlickGGX(NDL,NDV,roughness);
#endif
                //Fresnel
                float fresnel=0;
#if _FRESNEL_SCHLICK
                fresnel=Fresnel_Schlick(NDV);
#elif _FRESNEL_SPHERICALGAUSSIAN
                fresnel=Fresnel_SphericalGaussian(NDV);
#endif
                float3 diffuseColor=albedo*(1-_Metallic);
                diffuseColor*=F0(NDL,NDV,LDH,roughness);
                diffuseColor+=indirectDiffuse;
                float3 specColor=lerp(1,albedo,_Metallic*.5);
                 specColor=lerp(specColor,1,fresnel)* specular*geometricShadow/(4*(NDL*NDV));
                specColor+=indirectSpecular;
                specColor=max(0,specColor);
                float3 finalCol= lightCol* (diffuseColor+ specColor)*NDL;
                return float4(finalCol,1);
            }
            ENDCG
        }
        USEPASS "Hidden/ShadowCaster/Main"
    }
}
