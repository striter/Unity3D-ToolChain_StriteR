Shader "Hidden/GBVBase"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Main Texture", 2D) = "white" {}
        [NoScaleOffset]_SSSTex("SSS Texture",2D)= "white" {}
        [NoScaleOffset]_ILMTex("ILM Texture",2D)= "black" {}
        [NoScaleOffset]_DetailTex("Detail Texture",2D)="white"{}
        _AttenuationCutOff("Attenuation Cutoff",Range(0,1))=.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "LightMode"="ForwardBase" "Queue"="Geometry"}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
                float3 tangent:TANGENT;
			    float4 color:COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
			    float4 pos : SV_POSITION;
			    float2 uv : TEXCOORD0;
			    float3 normal:TEXCOORD1;
                float3 tangent:TEXCOORD2;
			    float3 lightDir:TEXCOORD3;
			    float3 viewDir:TEXCOORD4;
			    float3 worldPos:TEXCOORD5;
			    float4 color:TEXCOORD6;
			    SHADOW_COORDS(7)
            };

            sampler2D _MainTex;
            sampler2D _SSSTex;
            sampler2D _ILMTex;
            sampler2D _DetailTex;
            float _AttenuationCutOff;

            v2f vert (appdata v)
            {
			    v2f o;
			    o.pos = UnityObjectToClipPos(v.vertex);
			    o.uv =v.uv;
			    o.normal=normalize( mul((float3x3)unity_ObjectToWorld, v.normal));
			    o.tangent=normalize( mul((float3x3)unity_ObjectToWorld, v.tangent));
			    o.lightDir=WorldSpaceLightDir(v.vertex);
			    o.viewDir=WorldSpaceViewDir(v.vertex);
			    o.worldPos=mul(unity_ObjectToWorld,v.vertex);
			    o.color=v.color;
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 baseCol=tex2D(_MainTex,i.uv);
                float4 sssCol=tex2D(_SSSTex,i.uv);
                float3 detailCol=tex2D(_DetailTex,i.uv);
                float4 lightMask=tex2D(_ILMTex,i.uv);
                float3 lightCol=_LightColor0.rgb;
                float3 ambient=UNITY_LIGHTMODEL_AMBIENT.rgb;
                float4 vertexMask=i.color;

			    float3 normal=normalize(i.normal);
                float3 tangent=normalize(i.tangent);
			    float3 lightDir=normalize(i.lightDir);
			    float3 viewDir = normalize(i.viewDir);
                float3 halfDir=normalize(lightDir+viewDir);

                float NDL=dot(normal,lightDir);
                float NDV=dot(normal,viewDir);
                float NDH=dot(normal,halfDir);
			    UNITY_LIGHT_ATTENUATION(atten,i,i.worldPos)
                NDH*=lightMask.r;
                NDV*=lightMask.b;
                atten=lerp(atten,1,lightMask.g);
                atten=step(_AttenuationCutOff,atten);

                float3 darkenCol=lerp(sssCol*baseCol,sssCol,vertexMask.r);
                float3 lightenCol=baseCol;

                float3 finalCol= ambient+ lightCol*lerp(darkenCol,lightenCol,atten);

	            float specular = pow(NDH*NDV,.6);
                finalCol+=specular*lightCol;
                finalCol*=lightMask.a;
                return float4(finalCol,1);
            }
            ENDCG
        }
        USEPASS "Hidden/ShadowCaster/Main"
    }
}
