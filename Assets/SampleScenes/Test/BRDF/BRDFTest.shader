Shader "Unlit/BRDFTest"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color",Color)=(1,1,1,1)
        _Glossiness("Smoothness",Range(0,1))=1
        _Metallic("Metalness",Range(0,1))=0
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
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "BRDFInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float4 tangent:TANGENT;
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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightMapUV=v.lightmapUV;
                o.normal=UnityObjectToWorldNormal(v.normal);
                o.tangent=normalize(mul(unity_ObjectToWorld,v.tangent));
                o.biTangent=normalize(cross(o.normal,o.tangent)*v.tangent.w);
                o.worldPos=mul(unity_ObjectToWorld,v.vertex);
                o.lightDir=WorldSpaceLightDir(v.vertex);
                o.viewDir=WorldSpaceViewDir(v.vertex);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 normal=normalize(i.normal);
                float3 lightDir=normalize(i.lightDir);
                float3 viewDir=normalize(i.viewDir);
                float3 halfDir=normalize(viewDir+lightDir);
                float3 reflectDir=normalize(reflect(-viewDir,normal));
                
                float3 albedo=tex2D(_MainTex,i.uv);
                float3 lightCol=_LightColor0.rgb;
                float3 ambient=UNITY_LIGHTMODEL_AMBIENT.xyz;
                UNITY_LIGHT_ATTENUATION(atten, i,i.worldPos);

                float NDL=saturate(dot(normal,lightDir));
                float NDH=saturate(dot(normal,halfDir));
                float NDV=saturate(dot(normal,viewDir));
                float VDH=saturate(dot(viewDir,halfDir));
                float LDH=saturate(dot(lightDir,halfDir));
                float LDV=saturate(dot(lightDir,viewDir));
                float RDV=saturate(dot(reflectDir,viewDir));

                float smoothness=UnpackSmoothness(_Glossiness);
                float3 diffuseColor=albedo*(1-_Metallic);
                float3 specColor=lerp(lightCol,_Color.rgb,_Metallic*.5);

                float3 finalCol=albedo*atten*lightCol +ambient;
                return float4(finalCol,1);
            }
            ENDCG
        }
        USEPASS "Hidden/ShadowCaster/Main"
    }
}
