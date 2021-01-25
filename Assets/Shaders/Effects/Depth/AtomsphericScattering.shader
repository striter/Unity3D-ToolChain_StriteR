Shader "Unlit/AtomsphericScatteringTest"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "../../BoundingCollision.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal:NORMAL;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal:NORMAL;
                float3 viewDir:TEXCOORD0;
                float3 lightDir:TEXCOORD1;
                float4 screenPos:TEXCOORD2;
                float3 pos:TEXCOORD3;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.viewDir=ObjSpaceViewDir(v.vertex);
                o.lightDir=ObjSpaceLightDir(v.vertex);
                o.normal=v.normal;
                o.pos=v.vertex;
                o.screenPos=ComputeScreenPos(o.vertex);
                return o;
            }

            sampler2D _CameraDepthTexture;

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDir=normalize(i.lightDir);
                float3 viewDir=-normalize(i.viewDir);
                float3 normal=normalize(i.normal);
                
                half viewDst = BSRayDistance(0,.5,i.pos, viewDir).y;
                half worldDepthDst = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r - i.screenPos.w;
                half depthDst = length(mul(unity_WorldToObject, float3(0, worldDepthDst, 0)));
                half maxDst = min(depthDst, viewDst);
                return  maxDst/1.34;
            }
            ENDCG
        }
    }
}
